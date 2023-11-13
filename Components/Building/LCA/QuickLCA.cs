using System;
using System.Collections.Generic;
using System.IO;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.UI;
using Rhino;
using System.Linq;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using Newtonsoft.Json.Linq;
using BSoM;
using Eto.Forms;

namespace BuildSystemsGH.Components.Building.LCA
{
    public class QuickLCA : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        /// 

        public class BuildingComponent
        {
            public string Name { get; set; }
            public string RhinoLayerIndex { get; set; }
            public string RhinoLayer { get; set; }
            public string Component { get; set; }
            public List<Brep> ComponentSurfaces { get; set; }
            public double Area { get; set; }
            public double PENRT { get; set; }
            public double GWP { get; set; }


        }


        public class BuildingLayer
        {
            public int LayerIndex { get; set; }
            public string Layer { get; set; }
            public string Component { get; set; }
            public double Area { get; set; }
            public double PENRT { get; set; }
            public double GWP { get; set; }
        }

        public Brep GetSurface(Brep brep)
        {
            List<Brep> faces = new List<Brep>();

            for (int i = 0; i <= brep.Faces.Count - 1; i++)
            {
                //Extract the faces
                int[] iList = { i };
                Brep face = brep.DuplicateSubBrep(iList);
                faces.Add(face);
            }

            // Sort the faces by area
            faces.Sort((x, y) => x.GetArea().CompareTo(y.GetArea()));

            // Get the two largest faces
            Brep[] twoLargestFaces = { faces[faces.Count - 1], faces[faces.Count - 2] };

            // Calculate the distance between the center of the largest face to the closest point on the second largest face
            Point3d center = twoLargestFaces[0].GetBoundingBox(false).Center;
            Point3d closestPoint = twoLargestFaces[1].ClosestPoint(center);
            // Move largest face to the middle in between the two largest faces
            Vector3d vector = new Vector3d(closestPoint - center);
            twoLargestFaces[0].Translate(vector);

            // Return the moved face
            return twoLargestFaces[0];
        }

        public List<BuildingLayer> CalculateLCA(string libPath, GH_Structure<GH_String> treeIds, List<GH_Brep> ghBreps)
        {
            // Building layers
            BuildingLayer außenwandNt = new BuildingLayer();
            BuildingLayer außenwandTr = new BuildingLayer();
            BuildingLayer balkon = new BuildingLayer();
            BuildingLayer dachdecke = new BuildingLayer();
            BuildingLayer innenwandNt = new BuildingLayer();
            BuildingLayer trenndeckeOG = new BuildingLayer();
            BuildingLayer trennwandOG = new BuildingLayer();
            BuildingLayer deckeEG = new BuildingLayer();
            BuildingLayer außenwandEG = new BuildingLayer();
            BuildingLayer trennwandEG = new BuildingLayer();
            BuildingLayer wandE = new BuildingLayer();
            BuildingLayer deckeE = new BuildingLayer();
            BuildingLayer bodenplatte = new BuildingLayer();
            BuildingLayer wandKeller = new BuildingLayer();


            // Get surfaces
            //foreach (GH_Brep ghBrep in ghBreps)
            for (int i = 0; i < ghBreps.Count; i++)
            {
                // Convert GH_Brep to Brep
                Brep brep = new Brep();
                GH_Convert.ToBrep(ghBreps[i], ref brep, GH_Conversion.Both);
                Brep surface = new Brep();

                // If brep is solid, then use GetSurface to get the largest surface
                if (brep.IsSolid)
                {
                    surface = GetSurface(brep);
                    //surfaces.Add(surface);
                }
                // If brep is not solid, then add the brep to the list of surfaces
                else
                {
                    surface = brep;
                    //surfaces.Add(brep);
                }

                //int layerIndex = layerIndexes[i];

                //    if (layerIndex == außenwandNt.LayerIndex)
                //    {
                //        außenwandNt.Area += surface.GetArea();
                //    }
                //    else if (layerIndex == außenwandTr.LayerIndex)
                //    {
                //        außenwandTr.Area += surface.GetArea();
                //    }
                //    else if (layerIndex == balkon.LayerIndex)
                //    {
                //        balkon.Area += surface.GetArea();
                //    }
                //    else if (layerIndex == dachdecke.LayerIndex)
                //    {
                //        dachdecke.Area += surface.GetArea();
                //    }
                //    else if (layerIndex == innenwandNt.LayerIndex)
                //    {
                //        innenwandNt.Area += surface.GetArea();
                //    }
                //    else if (layerIndex == trenndeckeOG.LayerIndex)
                //    {
                //        trenndeckeOG.Area += surface.GetArea();
                //    }
                //    else if (layerIndex == trennwandOG.LayerIndex)
                //    {
                //        trennwandOG.Area += surface.GetArea();
                //    }
                //    else if (layerIndex == deckeEG.LayerIndex)
                //    {
                //        deckeEG.Area += surface.GetArea();
                //    }
                //    else if (layerIndex == außenwandEG.LayerIndex)
                //    {
                //        außenwandEG.Area += surface.GetArea();
                //    }
                //    else if (layerIndex == trennwandEG.LayerIndex)
                //    {
                //        trennwandEG.Area += surface.GetArea();
                //    }
                //    else if (layerIndex == wandE.LayerIndex)
                //    {
                //        wandE.Area += surface.GetArea();
                //    }
                //    else if (layerIndex == deckeE.LayerIndex)
                //    {
                //        deckeE.Area += surface.GetArea();
                //    }
                //    else if (layerIndex == bodenplatte.LayerIndex)
                //    {
                //        bodenplatte.Area += surface.GetArea();
                //    }
                //    else if (layerIndex == wandKeller.LayerIndex)
                //    {
                //        wandKeller.Area += surface.GetArea();
                //    }
            }

            //// Now get the building component, GWP and PENRT ////

            // Make a list of json files in the database directory
            string[] jsonComponentPathArray;

            string databaseComponent = libPath + "\\" + "Component";
            jsonComponentPathArray = Directory.GetFiles(databaseComponent, "*.json");

            // Convert to list
            List<string> buildingComponets = new List<string>();
            buildingComponets = jsonComponentPathArray.ToList();

            // Get the name of file without the path
            for (int i = 0; i < buildingComponets.Count; i++)
            {
                buildingComponets[i] = buildingComponets[i].Substring(buildingComponets[i].LastIndexOf("\\") + 1);
                buildingComponets[i] = buildingComponets[i].Substring(0, buildingComponets[i].LastIndexOf("."));
            }

            // Keys on the JSON. They derive from the headers on the Excel database
            string keyPENRTA1A3 = "PENRT A1-A3";
            string keyPENRTC3 = "PENRT C3";
            string keyPENRTC4 = "PENRT C4";
            string keyPENRTD1 = "PENRT D1";
            string keyGWPA1A3 = "GWP A1-A3";
            string keyGWPC3 = "GWP C3";
            string keyGWPC4 = "GWP C4";
            string keyGWPD1 = "GWP D1";

            // Loop through treeIds
            for (int i = 0; i < treeIds.Branches.Count; i++)
            {
                // Get the branch
                List<GH_String> branch = treeIds.Branches[i];

                // Get the first item in the branch
                GH_String ghString = branch[0];

                // Get the value of the first item in the branch
                string componentName = ghString.Value;


                // Filter the jsonBauteil list to find the selected component
                List<string> selComponentPath = jsonComponentPathArray.Where(s => s.Contains(componentName)).ToList();

                // Open the json file with path selBauteilPath[0]
                string jsonComponent = File.ReadAllText(selComponentPath[0]);

                // Convert json string to a json object
                JObject jObjectComponent = JObject.Parse(jsonComponent);

                // Get the super, main and sub Aufbau from the json object
                string stringPENRTA1A3 = (string)jObjectComponent[keyPENRTA1A3];
                string stringPENRTC3 = (string)jObjectComponent[keyPENRTC3];
                string stringPENRTC4 = (string)jObjectComponent[keyPENRTC4];
                string stringPENRTD1 = (string)jObjectComponent[keyPENRTD1];
                string stringGWPA1A3 = (string)jObjectComponent[keyGWPA1A3];
                string stringGWPC3 = (string)jObjectComponent[keyGWPC3];
                string stringGWPC4 = (string)jObjectComponent[keyGWPC4];
                string stringGWPD1 = (string)jObjectComponent[keyGWPD1];

                stringPENRTA1A3 = stringPENRTA1A3.Replace(",", ".");
                stringPENRTC3 = stringPENRTC3.Replace(",", ".");
                stringPENRTC4 = stringPENRTC4.Replace(",", ".");
                stringPENRTD1 = stringPENRTD1.Replace(",", ".");
                stringGWPA1A3 = stringGWPA1A3.Replace(",", ".");
                stringGWPC3 = stringGWPC3.Replace(",", ".");
                stringGWPC4 = stringGWPC4.Replace(",", ".");
                stringGWPD1 = stringGWPD1.Replace(",", ".");

                // Convert the strings to doubles
                double doublePENRTA1A3 = Convert.ToDouble(stringPENRTA1A3);
                double doublePENRTC3 = Convert.ToDouble(stringPENRTC3);
                double doublePENRTC4 = Convert.ToDouble(stringPENRTC4);
                double doublePENRTD1 = Convert.ToDouble(stringPENRTD1);
                double doubleGWPA1A3 = Convert.ToDouble(stringGWPA1A3);
                double doubleGWPC3 = Convert.ToDouble(stringGWPC3);
                double doubleGWPC4 = Convert.ToDouble(stringGWPC4);
                double doubleGWPD1 = Convert.ToDouble(stringGWPD1);

                double doublePENRTAToD = doublePENRTA1A3 + doublePENRTC3 + doublePENRTC4 + doublePENRTD1;
                double doubleGWPAToD = doubleGWPA1A3 + doubleGWPC3 + doubleGWPC4 + doubleGWPD1;

                if (i == 0)
                {
                    außenwandNt.Component = componentName;
                    außenwandNt.PENRT = doublePENRTAToD * außenwandNt.Area;
                    außenwandNt.GWP = doubleGWPAToD * außenwandNt.Area;
                }
                else if (i == 1)
                {
                    außenwandTr.Component = componentName;
                    außenwandTr.PENRT = doublePENRTAToD * außenwandTr.Area;
                    außenwandTr.GWP = doubleGWPAToD * außenwandTr.Area;
                }
                else if (i == 2)
                {
                    balkon.Component = componentName;
                    balkon.PENRT = doublePENRTAToD * balkon.Area;
                    balkon.GWP = doubleGWPAToD * balkon.Area;
                }
                else if (i == 3)
                {
                    dachdecke.Component = componentName;
                    dachdecke.PENRT = doublePENRTAToD * dachdecke.Area;
                    dachdecke.GWP = doubleGWPAToD * dachdecke.Area;
                }
                else if (i == 4)
                {
                    innenwandNt.Component = componentName;
                    innenwandNt.PENRT = doublePENRTAToD * innenwandNt.Area;
                    innenwandNt.GWP = doubleGWPAToD * innenwandNt.Area;
                }
                else if (i == 5)
                {
                    trenndeckeOG.Component = componentName;
                    trenndeckeOG.PENRT = doublePENRTAToD * trenndeckeOG.Area;
                    trenndeckeOG.GWP = doubleGWPAToD * trenndeckeOG.Area;
                }
                else if (i == 6)
                {
                    trennwandOG.Component = componentName;
                    trennwandOG.PENRT = doublePENRTAToD * trennwandOG.Area;
                    trennwandOG.GWP = doubleGWPAToD * trennwandOG.Area;
                }
                else if (i == 7)
                {
                    deckeEG.Component = componentName;
                    deckeEG.PENRT = doublePENRTAToD * deckeEG.Area;
                    deckeEG.GWP = doubleGWPAToD * deckeEG.Area;
                }
                else if (i == 8)
                {
                    außenwandEG.Component = componentName;
                    außenwandEG.PENRT = doublePENRTAToD * außenwandEG.Area;
                    außenwandEG.GWP = doubleGWPAToD * außenwandEG.Area;
                }
                else if (i == 9)
                {
                    trennwandEG.Component = componentName;
                    trennwandEG.PENRT = doublePENRTAToD * trennwandEG.Area;
                    trennwandEG.GWP = doubleGWPAToD * trennwandEG.Area;
                }
                else if (i == 10)
                {
                    wandE.Component = componentName;
                    wandE.PENRT = doublePENRTAToD * wandE.Area;
                    wandE.GWP = doubleGWPAToD * wandE.Area;
                }
                else if (i == 11)
                {
                    deckeE.Component = componentName;
                    deckeE.PENRT = doublePENRTAToD * deckeE.Area;
                    deckeE.GWP = doubleGWPAToD * deckeE.Area;
                }
                else if (i == 12)
                {
                    bodenplatte.Component = componentName;
                    bodenplatte.PENRT = doublePENRTAToD * bodenplatte.Area;
                    bodenplatte.GWP = doubleGWPAToD * bodenplatte.Area;
                }
                else if (i == 13)
                {
                    wandKeller.Component = componentName;
                    wandKeller.PENRT = doublePENRTAToD * wandKeller.Area;
                    wandKeller.GWP = doubleGWPAToD * wandKeller.Area;
                }
            }

            List<BuildingLayer> layers = new List<BuildingLayer>();

            // Add non-empty layers to the list
            if (außenwandNt.Area > 0) layers.Add(außenwandNt);
            if (außenwandTr.Area > 0) layers.Add(außenwandTr);
            if (balkon.Area > 0) layers.Add(balkon);
            if (dachdecke.Area > 0) layers.Add(dachdecke);
            if (innenwandNt.Area > 0) layers.Add(innenwandNt);
            if (trenndeckeOG.Area > 0) layers.Add(trenndeckeOG);
            if (trennwandOG.Area > 0) layers.Add(trennwandOG);
            if (deckeEG.Area > 0) layers.Add(deckeEG);
            if (außenwandEG.Area > 0) layers.Add(außenwandEG);
            if (trennwandEG.Area > 0) layers.Add(trennwandEG);
            if (wandE.Area > 0) layers.Add(wandE);
            if (deckeE.Area > 0) layers.Add(deckeE);
            if (bodenplatte.Area > 0) layers.Add(bodenplatte);
            if (wandKeller.Area > 0) layers.Add(wandKeller);

            return layers;
        }

        public QuickLCA()
          : base("Quick LCA", "QLCA",
              "Uses the average PENRT and GWP per square meter to calculate the LCA.",
              "BuildSystems", "LCA")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Building Geometry", "Geometry", "Either a list of surfaces or list of closed breps.", GH_ParamAccess.list);
            pManager.AddTextParameter("Building Components IDs", "IDs", "IDs of building components.", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Layers", "Layers", "Names of rhino layers.", GH_ParamAccess.tree);
            pManager.AddTextParameter("Components", "Components", "Names of building components.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Areas", "Areas", "Areas of building layers.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("PENRT", "PENRT", "Primary energy non-renewable total.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("GWP", "GWP", "Global warming potential.", GH_ParamAccess.tree);
            //pManager.AddBrepParameter("brep", "brep", "brep", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Path
            GH_AssemblyInfo info = Grasshopper.Instances.ComponentServer.FindAssembly(new Guid("36538369-6017-4b4c-9973-aee8f072399a"));
            string filePath = info.Location;
            // Get the directory name from the original path.
            string directoryPath = Path.GetDirectoryName(filePath);
            // Combine with the new directory.
            string libPath = Path.Combine(directoryPath, "BuildSystems");

            // Get inputs
            List<GH_Brep> ghBreps = new List<GH_Brep>();
            if (!DA.GetDataList(0, ghBreps)) return;
            if (!DA.GetDataTree(1, out GH_Structure<GH_String> componentIDs)) return;

            // Get Rhino document
            RhinoDoc doc = RhinoDoc.ActiveDoc;

            //// First get the layer id, layer, and area ////
            // List to store Layer index information
            List<int> layerIndexes = new List<int>();

            // List to store surfaces
            List<Brep> surfaces = new List<Brep>();

            foreach (GH_Brep ghBrep in ghBreps)
            {
                // Get layer index
                Guid id = new Guid();
                try
                {
                    id = ghBrep.ReferenceID;
                }
                catch
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Please check the input geometry.");
                    return;
                }
                int layerIndex = doc.Objects.Find(id).Attributes.LayerIndex;
                layerIndexes.Add(layerIndex);
            }

            // Get unique layer indexes
            IEnumerable<int> uniqueLayerIndexes = layerIndexes.Distinct().OrderBy(n => n);

            // All building components to be considered. Should have a layer pair in Rhino.
            string[] buildingComponentsList =
                { "Außenwand nt",
                "Außenwand tr",
                "Balkon", "Dachdecke",
                "Innenwand nt",
                "Trenndecke OG",
                "Trennwand OG",
                "Decke EG",
                "Außenwand EG",
                "Trennwand EG",
                "Wand E",
                "Decke E",
                "Bodenplatte",
                "Wand Keller"
                };

            //foreach (int layerIndex in uniqueLayerIndexes)
            //{
            //    Layer layer = doc.Layers[layerIndex];

            //    string layerName = layer.Name;

            //    if (layerName.Contains("Außenwand nt"))
            //    {
            //        außenwandNt.LayerIndex = layerIndex;
            //        außenwandNt.Layer = layerName;
            //    }
            //    else if (layerName.Contains("Außenwand tr"))
            //    {
            //        außenwandTr.LayerIndex = layerIndex;
            //        außenwandTr.Layer = layerName;
            //    }
            //    else if (layerName.Contains("Balkon"))
            //    {
            //        balkon.LayerIndex = layerIndex;
            //        balkon.Layer = layerName;
            //    }
            //    else if (layerName.Contains("Dachdecke"))
            //    {
            //        dachdecke.LayerIndex = layerIndex;
            //        dachdecke.Layer = layerName;
            //    }
            //    else if (layerName.Contains("Innenwand nt"))
            //    {
            //        innenwandNt.LayerIndex = layerIndex;
            //        innenwandNt.Layer = layerName;
            //    }
            //    else if (layerName.Contains("Trenndecke OG"))
            //    {
            //        trenndeckeOG.LayerIndex = layerIndex;
            //        trenndeckeOG.Layer = layerName;
            //    }
            //    else if (layerName.Contains("Trennwand OG"))
            //    {
            //        trennwandOG.LayerIndex = layerIndex;
            //        trennwandOG.Layer = layerName;
            //    }
            //    else if (layerName.Contains("Decke über EG"))
            //    {
            //        deckeEG.LayerIndex = layerIndex;
            //        deckeEG.Layer = layerName;
            //    }
            //    else if (layerName.Contains("Außenwand EG"))
            //    {
            //        außenwandEG.LayerIndex = layerIndex;
            //        außenwandEG.Layer = layerName;
            //    }
            //    else if (layerName.Contains("Trennwand EG"))
            //    {
            //        trennwandEG.LayerIndex = layerIndex;
            //        trennwandEG.Layer = layerName;
            //    }
            //    else if (layerName.Contains("Wand E"))
            //    {
            //        wandE.LayerIndex = layerIndex;
            //        wandE.Layer = layerName;
            //    }
            //    else if (layerName.Contains("Decke E"))
            //    {
            //        deckeE.LayerIndex = layerIndex;
            //        deckeE.Layer = layerName;
            //    }
            //    else if (layerName.Contains("Bodenplatte"))
            //    {
            //        bodenplatte.LayerIndex = layerIndex;
            //        bodenplatte.Layer = layerName;
            //    }
            //    else if (layerName.Contains("Wand Keller"))
            //    {
            //        wandKeller.LayerIndex = layerIndex;
            //        wandKeller.Layer = layerName;
            //    }
            //    // Return grasshopper error "unknown layer"
            //    else
            //    {
            //        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input geometry with unknow layer: " + layerName);
            //    }

            //}



            //// OLD CODE ////

            List<BuildingLayer> buildingLayers = CalculateLCA(libPath, componentIDs, ghBreps);

            // If there is a null on layers, then throw an exception

            if (buildingLayers == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not assign a property of BuildingLayer.");
                return;
            }

            GH_Structure<GH_String> layers = new GH_Structure<GH_String>();
            GH_Structure<GH_String> components = new GH_Structure<GH_String>();
            GH_Structure<GH_Number> areas = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> PENRT = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> GWP = new GH_Structure<GH_Number>();

            for (int i = 0; i < buildingLayers.Count; i++)
            {
                BuildingLayer building = buildingLayers[i];
                GH_Path path = new GH_Path(building.LayerIndex);

                layers.Append(new GH_String(building.Layer), path);
                components.Append(new GH_String(building.Component), path);
                areas.Append(new GH_Number(building.Area), path);
                PENRT.Append(new GH_Number(building.PENRT), path);
                GWP.Append(new GH_Number(building.GWP), path);

            }

            DA.SetDataTree(0, layers);
            DA.SetDataTree(1, components);
            DA.SetDataTree(2, areas);
            DA.SetDataTree(3, PENRT);
            DA.SetDataTree(4, GWP);
        }

        public override GH_Exposure Exposure => GH_Exposure.quarternary;

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                //return Properties.Resources.QuickLCA;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7A5BB0CD-9768-4F5F-B13E-B08F1BCFDE31"); }
        }
    }
}