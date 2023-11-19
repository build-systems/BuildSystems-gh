using BSoM.LCA;
using Grasshopper.Kernel;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Linq;
using Newtonsoft.Json;

namespace BuildSystemsGH.Libraries
{
    // This GH_Component is a placeholder.
    // It will list the materials in the json library.
    // Should have filters for material type, like wood, metal, glass, etc.

    public class MaterialsLibrary : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public MaterialsLibrary()
          : base("B-S Materials Library", "BSML",
              "Description",
              "BuildSystems", "LCA")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Folder Path", "Path", "Root folder containing the three other folders with the JSON libraries.", GH_ParamAccess.item, BSoM.Database.Info.Folder);
            pManager.AddTextParameter("Material name", "Name", "Name of the material", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("BSoM material", "Material", "Material representation as BuildSystems object model", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string libPath = "";
            string materialName = "";
            DA.GetData(0, ref libPath);
            if (!DA.GetData(1, ref materialName)) return;

            // Sanity check
            // Check if the folder path is valid
            string requiredFolder = BSoM.Database.Info.MaterialTag;
            try
            {
                // Get all subdirectories
                string[] subdirectories = Directory.GetDirectories(libPath);

                // Check if the required folder is in the subdirectories
                if (!subdirectories.Contains(libPath + "\\" + requiredFolder))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The folder path is not valid. The sub-folder '" + requiredFolder + "' is missing.");
                    return;
                }

                // List of all the materials available
                List<string> jsonMaterialsFiles = BSoM.Database.Info.MaterialFiles();

                // Filter the buildingComponetsList to find the selected component
                List<string> selectedMaterialPath = jsonMaterialsFiles.Where(s => s.Contains(materialName)).ToList();

                // Read the json file
                string materialAsJson = File.ReadAllText(selectedMaterialPath[0]);

                // Deserialize JSON to C# object
                Material material = JsonConvert.DeserializeObject<Material>(materialAsJson);

                DA.SetData(0, material);
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
            int[] stringName = new int[] { 1 };

            for (int i = 0; i < stringName.Length; i++)
            {
                Grasshopper.Kernel.Parameters.Param_String in0str = Params.Input[stringName[i]] as Grasshopper.Kernel.Parameters.Param_String;
                if (in0str == null || in0str.SourceCount > 0 || in0str.PersistentDataCount > 0) return;
                Attributes.PerformLayout();
                int x = (int)in0str.Attributes.Pivot.X - 200;
                int y = (int)in0str.Attributes.Pivot.Y - 11;
                Grasshopper.Kernel.Special.GH_ValueList valList = new Grasshopper.Kernel.Special.GH_ValueList();
                valList.CreateAttributes();
                valList.Attributes.Pivot = new PointF(x, y);
                valList.Attributes.ExpireLayout();
                valList.ListItems.Clear();

                string libPath = BSoM.Database.Info.Folder;
                // Initiate empty materials list (will fill inside try)
                List<string> materialsList = new List<string>();

                // Sanity check
                // Check if the folder path is valid
                string requiredFolder = BSoM.Database.Info.MaterialTag;
                try
                {
                    // Get all subdirectories
                    string[] subdirectories = Directory.GetDirectories(libPath);

                    // Check if the required folder is in the subdirectories
                    if (!subdirectories.Contains(libPath + "\\" + requiredFolder))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The folder path is not valid. The sub-folder '" + requiredFolder + "' is missing.");
                        return;
                    }

                    // Material list
                    materialsList = BSoM.Database.Info.MaterialFiles();

                    // Get the name of file without the path
                    for (int j = 0; j < materialsList.Count; j++)
                    {
                        // Extract the file name without the path
                        materialsList[j] = materialsList[j].Substring(materialsList[j].LastIndexOf("\\") + 1);
                        // Extract the file name without the extension
                        materialsList[j] = materialsList[j].Substring(0, materialsList[j].LastIndexOf("."));
                    }

                }
                catch (Exception ex)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "An error occurred: " + ex.Message);
                }

                List<Grasshopper.Kernel.Special.GH_ValueListItem> materialsAvailable = new List<Grasshopper.Kernel.Special.GH_ValueListItem>();
                foreach (string material in materialsList)
                {
                    Grasshopper.Kernel.Special.GH_ValueListItem valueItem = new Grasshopper.Kernel.Special.GH_ValueListItem(material, '"' + material + '"');
                    materialsAvailable.Add(valueItem);
                }

                valList.ListItems.AddRange(materialsAvailable);
                document.AddObject(valList, false);
                in0str.AddSource(valList);
            }
            base.AddedToDocument(document);
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.MaterialsLibrary;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("019919CD-53B5-4BBE-B3CB-E68EAE7F1351"); }
        }
    }
}