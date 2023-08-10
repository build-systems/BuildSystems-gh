using Rhino;
using Rhino.Collections;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Runtime;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Collections;
using System.ComponentModel;
using System.Data;

namespace BuildSystemsGH.Components
{
    public class ComponentsLibrary : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>

        public GH_Structure<GH_String> mapToAlpha1(GH_Structure<GH_String> datatree, JToken children, int newIndex, int index)
        {
            // Function to map data to surface tool alpha 1
            string childLOD = (string)children[newIndex]["Properties"]["LOD"];
            string childLevel = (string)children[newIndex]["Properties"]["Level"];
            string childBOM = Convert.ToString(Convert.ToInt32(childLOD) - 1);
            string childID = (string)children[newIndex]["Option A"]["ID"];
            string childMaterial = (string)children[newIndex]["Option A"]["Material"];
            string childCategory = (string)children[newIndex]["Properties"]["Category"];
            string childDescription = (string)children[newIndex]["Properties"]["Description"];
            string childThickness = (string)children[newIndex]["Properties"]["Thickness"];
            string childWidth = (string)children[newIndex]["Properties"]["Width"];
            string childSpread = (string)children[newIndex]["Properties"]["Spread"];

            GH_String gH_childLOD = new GH_String(childLOD);
            GH_String gH_childLevel = new GH_String(childLevel);
            GH_String gH_childBOM = new GH_String(childBOM);
            GH_String gH_childID = new GH_String(childID);
            GH_String gH_childMaterial = new GH_String(childMaterial);
            GH_String gH_childCategory = new GH_String(childCategory);
            GH_String gH_childDescription = new GH_String(childDescription);
            GH_String gH_childThickness = new GH_String(childThickness);
            GH_String gH_childWidth = new GH_String(childWidth);
            GH_String gH_childSpread = new GH_String(childSpread);

            GH_Path path = new GH_Path(0, index);
            datatree.Append(gH_childLOD, path);
            datatree.Append(gH_childLevel, path);
            datatree.Append(gH_childBOM, path);
            datatree.Append(gH_childID, path);
            datatree.Append(gH_childMaterial, path);
            datatree.Append(gH_childCategory, path);
            datatree.Append(gH_childDescription, path);
            datatree.Append(gH_childThickness, path);
            datatree.Append(gH_childWidth, path);
            datatree.Append(gH_childSpread, path);

            return datatree;
        }

        public GH_Structure<GH_String> deserializeJSONToDataTree(GH_Structure<GH_String> datatree, JToken children, ref int index)
        {
            int newIndex = 0;
            int sizeChildren = children.Count();

            while (newIndex < sizeChildren)
            {
                while (children[newIndex]["Children"] == null)
                {
                    mapToAlpha1(datatree, children, newIndex, index);
                    index++;
                    newIndex++;
                    if (newIndex == sizeChildren || children[newIndex]["Children"] != null) break;
                }
                if (newIndex == sizeChildren) break;
                JToken childrenOfChildren = children[newIndex]["Children"];
                deserializeJSONToDataTree(datatree, childrenOfChildren, ref index);
                newIndex++;
                //return datatree;
            }
            return datatree;
        }

        public ComponentsLibrary()
          : base("B-S Components Library", "BSL",
              "Library of selected components.",
              "Build Systems", "Components")
        {
        }
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // Could have an automatic way to find the necessry folder
            pManager.AddTextParameter("Folder Path", "Path", "Root folder containing the three other folders with the JSON libraries.", GH_ParamAccess.item);
            pManager.AddTextParameter("Building Component", "Component", "ID of the building component (Bauteil)", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Assembled Component", "Component", "The resulting data representing the layers of the building component", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string filePath = "";
            string componentName = "";
            if (!DA.GetData(0, ref filePath)) return;
            if (!DA.GetData(1, ref componentName)) return;

            //filePath = "C:\\Users\\danie\\AppData\\Roaming\\Grasshopper\\Libraries\\Build Systems\\";
            //filePath = "C:\Users\danie\AppData\Roaming\Grasshopper\Libraries\Build Systems\" ;
            //componentName = "DD-tr-HM-A";

            List<string> buildingComponets = new List<string>();
            GH_Structure<GH_String> MaterialList = new GH_Structure<GH_String>();

            // Make a list of json files in the database directory
            string databaseBauteil = filePath + "Bauteil";
            string[] jsonBauteilPathArray = System.IO.Directory.GetFiles(databaseBauteil, "*.json");
            string databaseAssembly = filePath + "Assembly";
            string[] jsonAssemblyPathArray = System.IO.Directory.GetFiles(databaseAssembly, "*.json");
            string databaseMaterial = filePath + "Material";
            string[] jsonMaterialPathArray = System.IO.Directory.GetFiles(databaseMaterial, "*.json");

            // Convert to list
            buildingComponets = jsonBauteilPathArray.ToList();
            // Get the name of file without the path
            for (int i = 0; i < buildingComponets.Count; i++)
            {
                buildingComponets[i] = buildingComponets[i].Substring(buildingComponets[i].LastIndexOf("\\") + 1);
                buildingComponets[i] = buildingComponets[i].Substring(0, buildingComponets[i].LastIndexOf("."));
            }

            // Filter the jsonBauteil list to find the selected component
            List<string> selBauteilPath = jsonBauteilPathArray.Where(s => s.Contains(componentName)).ToList();

            // Open the json file with path selBauteilPath[0]
            string jsonBauteil = File.ReadAllText(selBauteilPath[0]);

            // Convert json string to a json object
            JObject jObjectBauteil = JObject.Parse(jsonBauteil);

            // Declare super, main and sub Aufbau
            string superAufbau = "";
            string mainAufbau = "";
            string subAufbau = "";

            // Get the super, main and sub Aufbau from the json object
            superAufbau = (string)jObjectBauteil["Super-Aufbau"];
            mainAufbau = (string)jObjectBauteil["Main-Aufbau"];
            subAufbau = (string)jObjectBauteil["Sub-Aufbau"];

            // Get the path of the json files for super, main and sub Aufbau
            List<string> superAufbauPath = new List<string>();
            List<string> mainAufbauPath = new List<string>();
            List<string> subAufbauPath = new List<string>();

            JObject jObjectSuperAufbau = new JObject();
            JObject jObjectMainAufbau = new JObject();
            JObject jObjectSubAufbau = new JObject();

            JObject jObjectAssembled = new JObject();

            // Create a new GH data tree for the bauteil data
            GH_Structure<GH_String> bauteilDataTree = new GH_Structure<GH_String>();

            int indexList = 0;
            if (superAufbau != string.Empty)
            {
                // Filter the json Assembly paths list to find the selected Aufbau
                superAufbauPath = jsonAssemblyPathArray.Where(s => s.Contains(superAufbau)).ToList();

                // Open the json file with path superAufbauPath[0]
                string jsonSuperAufbau = File.ReadAllText(superAufbauPath[0]);

                // Convert json string to a json object
                jObjectSuperAufbau = JObject.Parse(jsonSuperAufbau);
                jObjectAssembled["Super-Aufbau"] = jObjectSuperAufbau;

                JToken children = jObjectSuperAufbau["Children"];

                // Add the children of the super Aufbau to the bauteilDataTree
                bauteilDataTree = deserializeJSONToDataTree(bauteilDataTree, children, ref indexList);
            }

            if (mainAufbau != string.Empty)
            {
                // Filter the json Assembly paths list to find the selected Aufbau
                mainAufbauPath = jsonAssemblyPathArray.Where(s => s.Contains(mainAufbau)).ToList();

                // Open the json file with path mainAufbauPath[0]
                string jsonMainAufbau = File.ReadAllText(mainAufbauPath[0]);

                // Convert json string to a json object
                jObjectMainAufbau = JObject.Parse(jsonMainAufbau);
                jObjectAssembled["Main-Aufbau"] = jObjectMainAufbau;

                JToken children = jObjectMainAufbau["Children"];

                // Add the children of the super Aufbau to the bauteilDataTree
                bauteilDataTree = deserializeJSONToDataTree(bauteilDataTree, children, ref indexList);
            }

            if (subAufbau != string.Empty)
            {
                subAufbauPath = jsonAssemblyPathArray.Where(s => s.Contains(subAufbau)).ToList();

                // Open the json file with path subAufbauPath[0]
                string jsonSubAufbau = File.ReadAllText(subAufbauPath[0]);

                // Convert json string to a json object
                jObjectSubAufbau = JObject.Parse(jsonSubAufbau);
                jObjectAssembled["Sub-Aufbau"] = jObjectSubAufbau;

                JToken children = jObjectSubAufbau["Children"];

                // Add the children of the super Aufbau to the bauteilDataTree
                bauteilDataTree = deserializeJSONToDataTree(bauteilDataTree, children, ref indexList);
            }

            DA.SetDataList(0, bauteilDataTree);
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
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6F9909C5-F87D-4D75-9286-10238A7AAABF"); }
        }
    }
}