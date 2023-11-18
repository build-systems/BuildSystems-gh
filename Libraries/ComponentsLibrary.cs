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
using System.Reflection;

using BSoM.Database;
using BSoM.LCA;

namespace BuildSystemsGH.Libraries
{
    public class ComponentsLibrary : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ComponentsLibrary class.
        /// </summary>

        public GH_Structure<GH_String> MapChildrenToAlpha1(GH_Structure<GH_String> datatree, JToken children, int newIndex, int index)
        {
            // Function to map data to surface tool alpha 1
            string childLOD = (string)children[newIndex]["properties"]["lod"];
            string childLevel = (string)children[newIndex]["properties"]["level"];
            string childBOM = Convert.ToString(Convert.ToInt32(childLOD) - 1);
            string childID = (string)children[newIndex]["option A"]["id"];
            string childMaterial = (string)children[newIndex]["option A"]["material"];
            string childCategory = (string)children[newIndex]["properties"]["category"];
            string childDescription = (string)children[newIndex]["properties"]["description"];
            string childThickness = (string)children[newIndex]["properties"]["thickness"];
            string childWidth = (string)children[newIndex]["properties"]["width"];
            string childSpread = (string)children[newIndex]["properties"]["spread"];

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
                while (children[newIndex]["children"] == null)
                {
                    MapChildrenToAlpha1(datatree, children, newIndex, index);
                    index++;
                    newIndex++;
                    if (newIndex == sizeChildren || children[newIndex]["children"] != null) break;
                }
                if (newIndex == sizeChildren) break;
                JToken childrenOfChildren = children[newIndex]["children"];
                DeserializeJSONToDataTree(datatree, childrenOfChildren, ref index);
                newIndex++;
            }
            return datatree;
        }

        public void AppendAssemblyToTree(GH_Structure<GH_String> componentDataTree, string assembly, List<string> jsonAssemblyPathArray, string header, ref int index)
        {
            if (assembly != string.Empty && assembly != null)
            {
                // Filter the json Assembly paths list to find the selected Aufbau
                List<string> assemblyPath = jsonAssemblyPathArray.Where(s => s.Contains(assembly)).ToList();

                // Open the json file with path superAufbauPath[0]
                string jsonAssembly = File.ReadAllText(assemblyPath[0]);

                // Convert json string to a json object
                JObject jObjectAssembly = JObject.Parse(jsonAssembly);

                JObject jObjectAssembled = new JObject();
                jObjectAssembled[header] = jObjectAssembly;

                JToken children = jObjectAssembly["children"];

                // Add the children of the super Aufbau to the bauteilDataTree
                componentDataTree = DeserializeJSONToDataTree(componentDataTree, children, ref index);
            }
        }

        public ComponentsLibrary()
          : base("B-S Components Library", "BSCL",
              "Library of selected components.",
              "BuildSystems", "LCA")
        {
        }
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Folder Path", "Path", "Root folder containing the three other folders with the JSON libraries.", GH_ParamAccess.item, BSoM.Database.Info.Folder);
            pManager.AddTextParameter("Building Component ID", "ID", "ID of the building component (Bauteil)", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Assembled Component", "Component", "The resulting data representing the layers of the building component", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string databasefolder = "";
            string componentName = "";
            DA.GetData(0, ref databasefolder);
            if (!DA.GetData(1, ref componentName)) return;
            //BSoM.Database.Info database = new BSoM.Database.Info();
            //database.Path = databasefolder;

            // Initialize
            List<string> buildingComponetsList = new List<string>();
            GH_Structure<GH_String> MaterialList = new GH_Structure<GH_String>();
            List<string> jsonComponentsFiles = BSoM.Database.Info.ComponentFiles();
            List<string> jsonAssembliesFiles = BSoM.Database.Info.AssemblyFiles();
            List<string> jsonMaterialsFiles = BSoM.Database.Info.MaterialFiles();

            // Sanity check
            // Check if the folder path is valid
            string[] requiredFolders = { "Component", "Assembly", "Material" };
            try
            {
                // Get all subdirectories
                string[] subdirectories = Directory.GetDirectories(databasefolder);

                // Check if the required folders match the subdirectories
                foreach (string requiredFolder in requiredFolders)
                {
                    // Check if the required folder is in the subdirectories
                    if (!subdirectories.Contains(databasefolder + "\\" + requiredFolder))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The folder path is not valid. The sub-folder '" + requiredFolder + "' is missing.");
                        return;
                    }
                }

                // Filter the buildingComponetsList to find the selected component
                List<string> selectedComponentPath = jsonComponentsFiles.Where(s => s.Contains(componentName)).ToList();

                // Read the json file
                string jsonComponent = File.ReadAllText(selectedComponentPath[0]);

                // Convert json string to a json object
                JObject jObjectComponent = JObject.Parse(jsonComponent);

                // Keys on the JSON. They derive from the headers on the Excel database
                string keySuper = "superAssemblyId";
                string keyMain = "mainAssemblyId";
                string keySub = "subAssemblyId";

                // Get the super, main and sub Aufbau from the json object
                string superAssembly = (string)jObjectComponent[keySuper];
                string mainAssembly = (string)jObjectComponent[keyMain];
                string subAssembly = (string)jObjectComponent[keySub];



                ////// Create assembly using a constructor, as input a filename and a database folder




                // Create a new GH data tree for the bauteil data
                GH_Structure<GH_String> componentDataTree = new GH_Structure<GH_String>();

                int indexDataTree = 0;
                AppendAssemblyToTree(componentDataTree, superAssembly, jsonAssembliesFiles, keySuper, ref indexDataTree);
                AppendAssemblyToTree(componentDataTree, mainAssembly, jsonAssembliesFiles, keyMain, ref indexDataTree);
                AppendAssemblyToTree(componentDataTree, subAssembly, jsonAssembliesFiles, keySub, ref indexDataTree);

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

                string libFolder = BSoM.Database.Info.Folder;

                // Material list
                List<string> componentsList = new List<string>();
                List<string> jsonComponentsFiles = BSoM.Database.Info.ComponentFiles();

                // Sanity check
                // Check if the folder path is valid
                string requiredFolder = BSoM.Database.Info.Component;
                try
                {
                    // Get all subdirectories
                    string[] subdirectories = Directory.GetDirectories(libFolder);
                    // Check if the required folder is in the subdirectories
                    if (!subdirectories.Contains(libFolder + "\\" + requiredFolder))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The folder path is not valid. The sub-folder '" + requiredFolder + "' is missing.");
                        return;
                    }

                    // Get the name of file without the path
                    for (int j = 0; j < jsonComponentsFiles.Count; j++)
                    {
                        // Extract the file name without the path
                        jsonComponentsFiles[j] = jsonComponentsFiles[j].Substring(jsonComponentsFiles[j].LastIndexOf("\\") + 1);
                        // Extract the file name without the extension
                        jsonComponentsFiles[j] = jsonComponentsFiles[j].Substring(0, jsonComponentsFiles[j].LastIndexOf("."));
                    }

                }
                catch (Exception ex)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "An error occurred: " + ex.Message);
                }

                // Add items to value list
                List<Grasshopper.Kernel.Special.GH_ValueListItem> componentsAvailable = new List<Grasshopper.Kernel.Special.GH_ValueListItem>();
                foreach (string component in jsonComponentsFiles)
                {
                    Grasshopper.Kernel.Special.GH_ValueListItem valueItem = new Grasshopper.Kernel.Special.GH_ValueListItem(component, '"' + component + '"');
                    componentsAvailable.Add(valueItem);
                }

                // Add value list to Grasshopper document
                valList.ListItems.AddRange(componentsAvailable);
                document.AddObject(valList, false);
                in0str.AddSource(valList);
            }
            base.AddedToDocument(document);
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Properties.Resources.ComponentsLibrary;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6F9909C5-F87D-4D75-9286-10238A7AAABF"); }
        }
    }
}