using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Geometry.Collections;

namespace BuildSystemsGH.Urban
{
    public class GetBuildingsProperties : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>

        public class GlobalProperties
        {
            public double GRF = 0; // Grundstuecksflaeche (square meters of the site)
            public double GR = 0; // Grundflaeche (the square meters of the outline of your building)
            public double GF = 0; // Geschossflaeche (the sum of all square meters of all your floors)
            public double BM = 0; // Baumasse (building volume)
            public double GRZ = 0; // Grundflaechenzahl (GR / GRF)
            public double GFZ = 0; // Geschossflaechenzahl (GF / GRF)
            public double BMZ = 0; // Baumassenzahl (BM / GRF)
            public double skinAreas = 0; // Surface area exposed to the weather
            public double AV = 0; // A/V-Verhältnis (Wärmeschutznachweis)
                                  //public string AllProperties = "";
            public List<string> AllProperties = new List<string>();

            public List<Curve> TempList = new List<Curve>();

            public GlobalProperties(Curve terrainBoundary, GH_Structure<GH_Curve> floors, List<Brep> volumes)
            {
                // flatten GH_Structure floors
                List<Curve> floorsList = new List<Curve>();
                foreach (GH_Curve ghCurve in floors.FlattenData())
                {
                    floorsList.Add(ghCurve.Value);
                }

                // GRF (square meters of the site)
                AreaMassProperties ampGRF = AreaMassProperties.Compute(terrainBoundary);
                this.GRF = ampGRF.Area;

                //// GR (the square meters of the outline of your building)
                //foreach (double bGR in buildingGR)
                //{
                //  this.GR += bGR;
                //}

                ///////////////////
                // get the skin of the volume for A / V calculation
                List<Brep> allSkins = new List<Brep>();
                List<Brep> allGrdFloors = new List<Brep>();

                foreach (Brep volume in volumes)
                {
                    Brep slectedSkin = new Brep();
                    List<Brep> skinBrepFaces = new List<Brep>();
                    List<Brep> allBrepFaces = new List<Brep>();
                    BrepFaceList allFacesList = volume.DuplicateBrep().Faces; // here I had to duplicate to not change original brep
                                                                              // create a new list with all brepfaces
                    for (int i = 0; i < allFacesList.Count; i++)
                    {
                        allBrepFaces.Add(allFacesList.ExtractFace(i));
                    }
                    // remove the brepfaces close to the ground
                    for (int i = 0; i < allBrepFaces.Count; i++)
                    {
                        AreaMassProperties areaMp = AreaMassProperties.Compute(allBrepFaces[i]);
                        Point3d tempCentroid = areaMp.Centroid;
                        if (tempCentroid.Z > 0.1)
                        {
                            skinBrepFaces.Add(allBrepFaces[i]);
                        }
                        else
                        {
                            // GR (the square meters of the outline of your building)
                            GR += areaMp.Area;
                        }
                    }
                    slectedSkin = Brep.JoinBreps(skinBrepFaces, 0.1)[0];
                    allSkins.Add(slectedSkin);
                }
                ///////////////////

                // GF (the sum of all square meters of all your floors)
                foreach (Curve curve in floorsList)
                {
                    AreaMassProperties ampGF = AreaMassProperties.Compute(curve);
                    this.GF += ampGF.Area;
                }

                // BM (building volume)
                foreach (Brep vol in volumes)
                {
                    VolumeMassProperties vmpBM = VolumeMassProperties.Compute(vol);
                    this.BM += vmpBM.Volume;
                }

                // GRZ Grundflaechenzahl (GR / GRF)
                this.GRZ = GR / GRF;

                // GFZ Geschossflaechenzahl (GF / GRF)
                this.GFZ = GF / GRF;

                // BMZ Baumassenzahl (BM / GRF)
                this.BMZ = BM / GRF;

                // A/V-Verhältnis
                foreach (Brep skin in allSkins)
                {
                    AreaMassProperties ampSkin = AreaMassProperties.Compute(skin);
                    skinAreas += ampSkin.Area;
                }
                this.AV = skinAreas / this.BM;

                // string with all properties
                this.AllProperties.Add("GRF: " + Math.Round(this.GRF, 2));
                this.AllProperties.Add("GR: " + Math.Round(this.GR, 2));
                this.AllProperties.Add("GF: " + Math.Round(this.GF, 2));
                this.AllProperties.Add("BM: " + Math.Round(this.BM, 2));
                this.AllProperties.Add("GRZ: " + Math.Round(this.GRZ, 3));
                this.AllProperties.Add("GFZ: " + Math.Round(this.GFZ, 3));
                this.AllProperties.Add("BMZ: " + Math.Round(this.BMZ, 3));
                this.AllProperties.Add("A: " + Math.Round(this.skinAreas, 2));
                this.AllProperties.Add("A/V: " + Math.Round(this.AV, 3));
            }
        }

        public GetBuildingsProperties()
          : base("Get Buildings Properties", "GBP",
              "Calculate the urban ratios for all buildings.",
              "BuildSystems", "Urban")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Terrain Boundary", "Boundary", "Closed curve representing the terrain boundary.", GH_ParamAccess.item);
            pManager.AddCurveParameter("All Buildings Floors", "Floors", "Closed curves representing the buildings floors.", GH_ParamAccess.tree);
            pManager.AddBrepParameter("Closed Volume of Buildings", "Volumes", "Closed breps representing the buildings volumes.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Buildings Properties", "Properties", "Text with the total enterprise properties.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve boundary = null;
            GH_Structure<GH_Curve> floors = new GH_Structure<GH_Curve>();
            List<Brep> volumes = new List<Brep>();

            if (!DA.GetData(0, ref boundary)) return;
            if (!DA.GetDataTree(1, out floors)) return;
            if (!DA.GetDataList(2, volumes)) return;

            GlobalProperties gProperties = new GlobalProperties(boundary, floors, volumes);

            DA.SetDataList(0, gProperties.AllProperties);
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Properties.Resources.GetBuildingsProperties;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("DADD504D-ADE0-464B-B97D-111837F08C88"); }
        }
    }
}