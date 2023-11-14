using Eto.Drawing;
using Eto.Forms;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace BuildSystemsGH.Urban
{
    //  ██████╗ ██╗   ██╗██╗██╗     ██████╗ ██╗███╗   ██╗ ██████╗         
    //  ██╔══██╗██║   ██║██║██║     ██╔══██╗██║████╗  ██║██╔════╝         
    //  ██████╔╝██║   ██║██║██║     ██║  ██║██║██╔██╗ ██║██║  ███╗        
    //  ██╔══██╗██║   ██║██║██║     ██║  ██║██║██║╚██╗██║██║   ██║        
    //  ██████╔╝╚██████╔╝██║███████╗██████╔╝██║██║ ╚████║╚██████╔╝        
    //  ╚═════╝  ╚═════╝ ╚═╝╚══════╝╚═════╝ ╚═╝╚═╝  ╚═══╝ ╚═════╝         

    //  ██████╗ ██╗   ██╗    ██╗  ██╗███████╗██╗ ██████╗ ██╗  ██╗████████╗
    //  ██╔══██╗╚██╗ ██╔╝    ██║  ██║██╔════╝██║██╔════╝ ██║  ██║╚══██╔══╝
    //  ██████╔╝ ╚████╔╝     ███████║█████╗  ██║██║  ███╗███████║   ██║   
    //  ██╔══██╗  ╚██╔╝      ██╔══██║██╔══╝  ██║██║   ██║██╔══██║   ██║   
    //  ██████╔╝   ██║       ██║  ██║███████╗██║╚██████╔╝██║  ██║   ██║   
    //  ╚═════╝    ╚═╝       ╚═╝  ╚═╝╚══════╝╚═╝ ╚═════╝ ╚═╝  ╚═╝   ╚═╝   
    //                                                                    
    public class BuildingByHeight : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>

        public class Building
        {
            public GH_Structure<GH_Curve> FloorsCurves = new GH_Structure<GH_Curve>(); //Used to calculate a lot of things
            public Brep BrepVolume = new Brep(); //Enclosed volume of the building
            public Brep BrepSkin = new Brep(); //volume - base surfaces. Used to calculate A / V
            public List<double> Heights = new List<double>();

            public Building(List<Curve> buildingBoundaries, List<double> foundationHeight, List<double> firstFloorHeight, List<double> upperFloorsHeight, List<double> parapetHeight, List<double> buildingTotalHeight)
            {

                // match number of items in all lists
                List<double> flatFoundationHeights = new List<double>();
                List<double> flatFirstFloorHeights = new List<double>();
                List<double> flatUpperFloorHeights = new List<double>();
                List<double> flatParapetHeights = new List<double>();
                List<double> flatBuildingHeights = new List<double>();

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

                    if (i < buildingTotalHeight.Count)
                    {
                        double bTHeight = buildingTotalHeight[i];
                        flatBuildingHeights.Add(bTHeight);
                    }
                    else
                    {
                        double bTHeight = buildingTotalHeight[buildingTotalHeight.Count - 1];
                        flatBuildingHeights.Add(bTHeight);
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
                    double oneFloorHeight = flatFoundationHeights[i] + flatFirstFloorHeights[i] + flatUpperFloorHeights[i] + flatParapetHeights[i];
                    if (flatBuildingHeights[i] >= oneFloorHeight)
                    {
                        Curve firstFloorCurve = buildingBoundaries[i].DuplicateCurve();
                        firstFloorCurve.Translate(vectorFoundation + vectorFirstFloor);
                        GH_Curve gH_Curve2 = new GH_Curve(firstFloorCurve);
                        FloorsCurves.Append(gH_Curve2, path);
                    }

                    // add other floors to the data tree
                    double count = flatUpperFloorHeights[i];
                    while (flatBuildingHeights[i] - count - flatFoundationHeights[i] - flatFirstFloorHeights[i] - flatParapetHeights[i] >= flatUpperFloorHeights[i])
                    {
                        Vector3d vectorUpperFloors = new Vector3d(0, 0, count);
                        Curve upperFloorsCurve = buildingBoundaries[i].DuplicateCurve();
                        upperFloorsCurve.Translate(vectorFoundation + vectorFirstFloor + vectorUpperFloors);
                        GH_Curve gH_Curve3 = new GH_Curve(upperFloorsCurve);
                        FloorsCurves.Append(gH_Curve3, path);
                        count += flatUpperFloorHeights[i];
                    }
                }

                // calculate volume and skin
                List<Brep> closedExtrusions = new List<Brep>();
                for (int i = 0; i < buildingBoundaries.Count; i++)
                {
                    double tempHeight = flatBuildingHeights[i];
                    Surface tempClosedExtrusion = Extrusion.Create(buildingBoundaries[i], tempHeight, true);
                    closedExtrusions.Add(tempClosedExtrusion.ToBrep());
                }
                // create a closed brep for volume calculations
                Brep[] unionBrep = Brep.CreateBooleanUnion(closedExtrusions, 0.1);
                BrepVolume = unionBrep[0];

            }

        }

        public BuildingByHeight()
          : base("Building by Height", "BBH",
            "Creates a building volume defined by the total height.",
            "BuildSystems", "Urban")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Building Boundaries", "Boundaries", "Closed curves representind the boundaries of the building.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Parapet height [m]", "Parapet", "Parapet height in meters. One value for each closed curve.", GH_ParamAccess.list, new double[] { 0.0 });
            pManager.AddNumberParameter("Upper floors height [m]", "Upper", "Upper floors height in meters. One value for each closed curve.", GH_ParamAccess.list, new double[] { 3.0 });
            pManager.AddNumberParameter("First floor height [m]", "First", "First floor height in meters. One value for each closed curve.", GH_ParamAccess.list, new double[] { 3.5 });
            pManager.AddNumberParameter("Foundation height [m]", "Foundation", "Foundation height in meters. One value for each closed curve.", GH_ParamAccess.list, new double[] { 0.1 });
            pManager.AddNumberParameter("Total height [m]", "Height", "Total height of the building in meters. One value for each closed curve.", GH_ParamAccess.list, new double[] { 10.0 });
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Floors", "Floors", "All the floors as curves", GH_ParamAccess.tree);
            pManager.AddBrepParameter("Volume", "Volume", "The resulting volume from all boundaries", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> buildingBoundaries = new List<Curve>();
            List<double> parapetHeight = new List<double>();
            List<double> upperFloorsHeight = new List<double>();
            List<double> firstFloorHeight = new List<double>();
            List<double> foundationHeight = new List<double>();
            List<double> totalHeight = new List<double>();

            if (!DA.GetDataList(0, buildingBoundaries)) return;
            DA.GetDataList(1, parapetHeight);
            DA.GetDataList(2, upperFloorsHeight);
            DA.GetDataList(3, firstFloorHeight);
            DA.GetDataList(4, foundationHeight);
            DA.GetDataList(5, totalHeight);


            Building myBuilding = new Building(buildingBoundaries, foundationHeight, firstFloorHeight, upperFloorsHeight, parapetHeight, totalHeight);

            DA.SetDataTree(0, myBuilding.FloorsCurves);
            DA.SetData(1, myBuilding.BrepVolume);

        }

        public override GH_Exposure Exposure => GH_Exposure.primary;


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.BuildingByHeight;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("84df062d-a043-40b9-ae57-c821825aabce");
    }
}