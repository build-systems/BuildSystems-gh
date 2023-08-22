using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using BSoM;

namespace BuildSystemsGH.Components
{
    public class ComponentsLibrary : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>

        public GH_Structure<GH_String> MapChildrenToAlpha1(GH_Structure<GH_String> datatree, JToken children, int newIndex, int index)
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

        public GH_Structure<GH_String> DeserializeJSONToDataTree(GH_Structure<GH_String> datatree, JToken children, ref int index)
        {
            int newIndex = 0;
            int sizeChildren = children.Count();

            while (newIndex < sizeChildren)
            {
                while (children[newIndex]["Children"] == null)
                {
                    MapChildrenToAlpha1(datatree, children, newIndex, index);
                    index++;
                    newIndex++;
                    if (newIndex == sizeChildren || children[newIndex]["Children"] != null) break;
                }
                if (newIndex == sizeChildren) break;
                JToken childrenOfChildren = children[newIndex]["Children"];
                DeserializeJSONToDataTree(datatree, childrenOfChildren, ref index);
                newIndex++;
            }
            return datatree;
        }

        public void AppendAssemblyToTree(GH_Structure<GH_String> componentDataTree, string assembly, string[] jsonAssemblyPathArray, string header, ref int index)
        {
            if (assembly != string.Empty)
            {
                // Filter the json Assembly paths list to find the selected Aufbau
                List<string> assemblyPath = jsonAssemblyPathArray.Where(s => s.Contains(assembly)).ToList();

                // Open the json file with path superAufbauPath[0]
                string jsonAssembly = File.ReadAllText(assemblyPath[0]);

                // Convert json string to a json object
                JObject jObjectAssembly = JObject.Parse(jsonAssembly);

                JObject jObjectAssembled = new JObject();
                jObjectAssembled[header] = jObjectAssembly;

                JToken children = jObjectAssembly["Children"];

                // Add the children of the super Aufbau to the bauteilDataTree
                componentDataTree = DeserializeJSONToDataTree(componentDataTree, children, ref index);
            }
        }

        public ComponentsLibrary()
          : base("B-S Components Library", "BSL",
              "Library of selected components.",
              "BuildSystems", "LCA")
        {
        }
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            GH_AssemblyInfo info = Grasshopper.Instances.ComponentServer.FindAssembly(new Guid("36538369-6017-4b4c-9973-aee8f072399a"));
            string filePath = info.Location;
            // Get the directory name from the original path.
            string directoryPath = Path.GetDirectoryName(filePath);
            // Combine with the new directory.
            string libPath = Path.Combine(directoryPath, "BuildSystems");
            pManager.AddTextParameter("Folder Path", "Path", "Root folder containing the three other folders with the JSON libraries.", GH_ParamAccess.item, libPath);
            pManager.AddTextParameter("Building Component ID", "ID", "ID of the building component (Bauteil)", GH_ParamAccess.item);
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
            string libPath = "";
            string componentName = "";
            DA.GetData(0, ref libPath);
            if (!DA.GetData(1, ref componentName)) return;

            // Initialize

            List<string> buildingComponets = new List<string>();
            GH_Structure<GH_String> MaterialList = new GH_Structure<GH_String>();
            string[] jsonComponentPathArray;
            string[] jsonAssemblyPathArray;
            string[] jsonMaterialPathArray;
            // Sanity check
            // Check if the folder path is valid
            string[] requiredFolders = { "Component", "Assembly", "Material" };
            try
            {
                // Get all subdirectories
                string[] subdirectories = Directory.GetDirectories(libPath);

                // Check if the required folders match the subdirectories
                foreach (string requiredFolder in requiredFolders)
                {
                    // Check if the required folder is in the subdirectories
                    if (!subdirectories.Contains(libPath + "\\" + requiredFolder))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The folder path is not valid. The sub-folder '" + requiredFolder + "' is missing.");
                        return;
                    }
                }

                // Make a list of json files in the database directory
                string databaseComponent = libPath + "\\" + "Component";
                jsonComponentPathArray = System.IO.Directory.GetFiles(databaseComponent, "*.json");
                string databaseAssembly = libPath + "\\" + "Assembly";
                jsonAssemblyPathArray = System.IO.Directory.GetFiles(databaseAssembly, "*.json");
                string databaseMaterial = libPath + "\\" + "Material";
                jsonMaterialPathArray = System.IO.Directory.GetFiles(databaseMaterial, "*.json");

                // Convert to list
                buildingComponets = jsonComponentPathArray.ToList();

                // Filter the jsonBauteil list to find the selected component
                List<string> selComponentPath = jsonComponentPathArray.Where(s => s.Contains(componentName)).ToList();
                
                // Get the name of file without the path
                for (int i = 0; i < buildingComponets.Count; i++)
                {
                    buildingComponets[i] = buildingComponets[i].Substring(buildingComponets[i].LastIndexOf("\\") + 1);
                    buildingComponets[i] = buildingComponets[i].Substring(0, buildingComponets[i].LastIndexOf("."));
                }

                // Open the json file with path selBauteilPath[0]
                string jsonComponent = File.ReadAllText(selComponentPath[0]);

                // Convert json string to a json object
                JObject jObjectComponent = JObject.Parse(jsonComponent);

                // Keys on the JSON. They derive from the headers on the Excel database
                string keySuper = "Super-Aufbau";
                string keyMain = "Main-Aufbau";
                string keySub = "Sub-Aufbau";

                // Get the super, main and sub Aufbau from the json object
                string superAssembly = (string)jObjectComponent[keySuper];
                string mainAssembly = (string)jObjectComponent[keyMain];
                string subAssembly = (string)jObjectComponent[keySub];

                // Create a new GH data tree for the bauteil data
                GH_Structure<GH_String> componentDataTree = new GH_Structure<GH_String>();

                int indexDataTree = 0;
                AppendAssemblyToTree(componentDataTree, superAssembly, jsonAssemblyPathArray, keySuper, ref indexDataTree);
                AppendAssemblyToTree(componentDataTree, mainAssembly, jsonAssemblyPathArray, keyMain, ref indexDataTree);
                AppendAssemblyToTree(componentDataTree, subAssembly, jsonAssemblyPathArray, keySub, ref indexDataTree);

                DA.SetDataTree(0, componentDataTree);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "An error occurred: " + ex.Message);
            }
        }

        public override void AddedToDocument(GH_Document document)
        {
            // Find a way to add value list dynamically
            //Add Value List
            int[] stringID = new int[] { 1 };

            for (int i = 0; i < stringID.Length; i++)
            {
                Grasshopper.Kernel.Parameters.Param_String in0str = Params.Input[stringID[i]] as Grasshopper.Kernel.Parameters.Param_String;
                if (in0str == null || in0str.SourceCount > 0 || in0str.PersistentDataCount > 0) return;
                Attributes.PerformLayout();
                int x = (int)in0str.Attributes.Pivot.X - 200;
                int y = (int)in0str.Attributes.Pivot.Y - 11;
                Grasshopper.Kernel.Special.GH_ValueList valList = new Grasshopper.Kernel.Special.GH_ValueList();
                valList.CreateAttributes();
                valList.Attributes.Pivot = new PointF(x, y);
                valList.Attributes.ExpireLayout();
                valList.ListItems.Clear();

                List<string> maualList = new List<string>
                {
                    "TD-tr-HM-A",
                    "TD-tr-HM-B",
                    "TD-tr-HM-C",
                    "TD-tr-HT-A",
                    "TD-tr-HT-B",
                    "TD-tr-HBV-A",
                    "TD-tr-HBV-B",
                    "TD-tr-STB-A",
                    "DD-tr-HM-A",
                    "DD-tr-HM-B",
                    "DD-tr-HM-C",
                    "DD-tr-HT-A",
                    "DD-tr-HT-B",
                    "DD-tr-STB-A",
                    "DD-tr-STB-B",
                    "TW-tr-HM-A",
                    "TW-tr-HM-B",
                    "TW-tr-HM-C",
                    "TW-tr-HM-D",
                    "TW-tr-HAT-A",
                    "TW-tr-HAT-B",
                    "TW-tr-HAT-C",
                    "TW-tr-STB",
                    "TW-tr-MWZ-A",
                    "TW-tr-MWZ-B",
                    "TW-tr-MWKS-A",
                    "TW-tr-MWKS-B",
                    "TW-nt-LB-A",
                    "TW-nt-LB-B",
                    "IW-tr-HM-A",
                    "IW-nt-HT-A",
                    "IW-nt-HT-B",
                    "IW-tr-STB",
                    "IW-nt-MWZ-A",
                    "IW-tr-MWKS-A",
                    "IW-tr-MWKS-B",
                    "IW-nt-MWKS-C",
                    "IW-nt-LB-A",
                    "IW-nt-LB-B",
                    "AW-tr-HM-A",
                    "AW-tr-HM-B",
                    "AW-tr-HM-C",
                    "AW-tr-HM-D",
                    "AW-tr-HM-E",
                    "AW-nt-TF-A",
                    "AW-tr-TF-B",
                    "AW-tr-TF-C",
                    "AW-tr-TF-D",
                    "AW-tr-TF-E",
                    "AW-tr-STB-A",
                    "AW-tr-STB-B",
                    "AW-tr-STB-C",
                    "AW-tr-STB-D",
                    "AW-tr-STB-E",
                    "AW-tr-MWZ-A",
                    "AW-tr-MWZ-B",
                    "AW-tr-MWZ-C",
                    "AW-tr-MWKS-A",
                    "AW-tr-MWKS-B",
                    "AW-tr-MWKS-C",
                    "AW-tr-MWKS-D",
                    "KW-tr-STB-A",
                    "KW-tr-MWZ-A",
                    "KW-tr-MWKS-A",
                    "BP-tr-STB-A"
                };

                List<Grasshopper.Kernel.Special.GH_ValueListItem> componentsAvailable = new List<Grasshopper.Kernel.Special.GH_ValueListItem>();
                    foreach (string component in maualList)
                    {
                    Grasshopper.Kernel.Special.GH_ValueListItem valueItem = new Grasshopper.Kernel.Special.GH_ValueListItem(component, '"' + component + '"');
                        componentsAvailable.Add(valueItem);
                    }

                valList.ListItems.AddRange(componentsAvailable);
                document.AddObject(valList, false);
                in0str.AddSource(valList);
            }
            base.AddedToDocument(document);
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