using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace BuildSystemsGH.Urban
{
    public class BuildingByFloorNumber : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        /// 

        public class Building
        {
            public GH_Structure<GH_Curve> FloorsCurves = new GH_Structure<GH_Curve>(); //Used to calculate a lot of things
            public Brep BrepVolume = new Brep(); //Enclosed volume of the building
            public Brep BrepSkin = new Brep(); //volume - base surfaces. Used to calculate A / V

            public Building(List<Curve> buildingBoundaries, List<double> foundationHeight, List<double> firstFloorHeight, List<double> upperFloorsHeight, List<double> parapetHeight, List<int> numberFloors)
            {

                // match number of items in all lists
                List<double> flatFoundationHeights = new List<double>();
                List<double> flatFirstFloorHeights = new List<double>();
                List<double> flatUpperFloorHeights = new List<double>();
                List<double> flatParapetHeights = new List<double>();
                List<int> flatNumberFloors = new List<int>();

                // if there are more curves than heights, repeat the last height
                for (int i = 0; i < buildingBoundaries.Count; i++)
                {
                    if (i < foundationHeight.Count)
                    {
                        double fHeight = foundationHeight[i];
                        flatFoundationHeights.Add(fHeight);
                    }
                    else
                    {
                        double fHeight = foundationHeight[foundationHeight.Count - 1];
                        flatFoundationHeights.Add(fHeight);
                    }

                    if (i < firstFloorHeight.Count)
                    {
                        double fFHeight = firstFloorHeight[i];
                        flatFirstFloorHeights.Add(fFHeight);
                    }
                    else
                    {
                        double fFHeight = firstFloorHeight[firstFloorHeight.Count - 1];
                        flatFirstFloorHeights.Add(fFHeight);
                    }

                    if (i < upperFloorsHeight.Count)
                    {
                        double uFHeight = upperFloorsHeight[i];
                        flatUpperFloorHeights.Add(uFHeight);
                    }
                    else
                    {
                        double uFHeight = upperFloorsHeight[upperFloorsHeight.Count - 1];
                        flatUpperFloorHeights.Add(uFHeight);
                    }

                    if (i < parapetHeight.Count)
                    {
                        double pHeight = parapetHeight[i];
                        flatParapetHeights.Add(pHeight);
                    }
                    else
                    {
                        double pHeight = parapetHeight[parapetHeight.Count - 1];
                        flatParapetHeights.Add(pHeight);
                    }

                    if (i < numberFloors.Count)
                    {
                        int nFloor = numberFloors[i];
                        flatNumberFloors.Add(nFloor);
                    }
                    else
                    {
                        int nFloor = numberFloors[numberFloors.Count - 1];
                        flatNumberFloors.Add(nFloor);
                    }

                    // Make sure the boundary has the right orientation
                    CurveOrientation boundaryOrientation = buildingBoundaries[i].ClosedCurveOrientation(Plane.WorldXY);
                    if (boundaryOrientation == CurveOrientation.Clockwise)
                    {
                        buildingBoundaries[i].Reverse();
                    }
                }


                // add floor plans to the list
                for (int i = 0; i < buildingBoundaries.Count; i++)
                {
                    //Data tree path
                    GH_Path path = new GH_Path(i);

                    // create vector3d for each height
                    Vector3d vectorFoundation = new Vector3d(0, 0, flatFoundationHeights[i]);
                    Vector3d vectorFirstFloor = new Vector3d(0, 0, flatFirstFloorHeights[i]);

                    // add foundation to the data tree
                    Curve foundationCurve = buildingBoundaries[i].DuplicateCurve();
                    foundationCurve.Translate(vectorFoundation);
                    GH_Curve gH_Curve = new GH_Curve(foundationCurve);
                    FloorsCurves.Append(gH_Curve, path);

                    // add first floor to the data tree
                    if (flatNumberFloors[i] >= 2)
                    {
                        Curve firstFloorCurve = buildingBoundaries[i].DuplicateCurve();
                        firstFloorCurve.Translate(vectorFoundation + vectorFirstFloor);
                        GH_Curve gH_Curve2 = new GH_Curve(firstFloorCurve);
                        FloorsCurves.Append(gH_Curve2, path);
                    }

                    // add other floors to the data tree
                    if (flatNumberFloors[i] > 2)
                    {
                        for (int floorNumber = 3; floorNumber <= flatNumberFloors[i]; floorNumber++)
                        {
                            Vector3d vectorUpperFloors = new Vector3d(0, 0, (floorNumber - 2) * flatUpperFloorHeights[i]);
                            Curve upperFloorsCurve = buildingBoundaries[i].DuplicateCurve();
                            upperFloorsCurve.Translate(vectorFoundation + vectorFirstFloor + vectorUpperFloors);
                            GH_Curve gH_Curve3 = new GH_Curve(upperFloorsCurve);
                            FloorsCurves.Append(gH_Curve3, path);
                        }
                    }
                }

                // calculate volume
                List<Brep> closedExtrusions = new List<Brep>();
                for (int i = 0; i < buildingBoundaries.Count; i++)
                {
                    double tempHeight = flatFoundationHeights[i] + flatFirstFloorHeights[i] + (flatNumberFloors[i] - 1) * flatUpperFloorHeights[i] + flatParapetHeights[i];
                    Surface tempClosedExtrusion = Extrusion.Create(buildingBoundaries[i], tempHeight, true);
                    closedExtrusions.Add(tempClosedExtrusion.ToBrep());
                }
                // create a closed brep for volume calculations
                Brep[] unionBrep = Brep.CreateBooleanUnion(closedExtrusions, 0.1);
                this.BrepVolume = unionBrep[0];

            }

        }

        public BuildingByFloorNumber()
          : base("Building by Floor Number", "BBF",
              "Creates a building volume defined by the number of floors.",
              "Build Systems", "Urban")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Building Boundaries", "Boundaries", "Closed curves representind the boundaries of the building.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Parapet height [m]", "Parapet", "Parapet height in meters. One value for each closed curve.", GH_ParamAccess.list, new double[] { 0.0 });
            pManager.AddNumberParameter("Upper floors height [m]", "Upper", "Upper floors height. One value for each closed curve.", GH_ParamAccess.list, new double[] { 3.0 });
            pManager.AddNumberParameter("First floor height [m]", "First", "First floor height. One value for each closed curve.", GH_ParamAccess.list, new double[] { 3.5 });
            pManager.AddNumberParameter("Foundation height [m]", "Foundation", "Foundation height. One value for each closed curve.", GH_ParamAccess.list, new double[] { 0.1 });
            pManager.AddIntegerParameter("Number of floors   ", "Floors", "Total height of the building. One value for each closed curve.", GH_ParamAccess.list, new int[] { 3 });
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Floors", "Floors", "All the floors as curves", GH_ParamAccess.tree);
            pManager.AddBrepParameter("Volume", "Volume", "The resulting volume from all boundaries", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> buildingBoundaries = new List<Curve>();
            List<double> parapetHeight = new List<double>();
            List<double> upperFloorsHeight = new List<double>();
            List<double> firstFloorHeight = new List<double>();
            List<double> foundationHeight = new List<double>();
            List<int> totalFloors = new List<int>();

            if (!DA.GetDataList(0, buildingBoundaries)) return;
            DA.GetDataList(1, parapetHeight);
            DA.GetDataList(2, upperFloorsHeight);
            DA.GetDataList(3, firstFloorHeight);
            DA.GetDataList(4, foundationHeight);
            DA.GetDataList(5, totalFloors);


            Building myBuilding = new Building(buildingBoundaries, foundationHeight, firstFloorHeight, upperFloorsHeight, parapetHeight, totalFloors);

            DA.SetDataTree(0, myBuilding.FloorsCurves);
            DA.SetData(1, myBuilding.BrepVolume);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Properties.Resources.BuildingByFloorNumber;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8A123D45-B59E-48B4-9AA8-2C32138A9F31"); }
        }
    }
}