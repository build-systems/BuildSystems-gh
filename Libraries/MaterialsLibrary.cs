using BSoM.LCA;
using Grasshopper.Kernel;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Linq;

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
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            string filePath = assembly.Location;
            // Get the directory name from the original path.
            string directoryPath = Path.GetDirectoryName(filePath);
            // Combine with the new directory.
            string libPath = Path.Combine(directoryPath, "BuildSystems");
            pManager.AddTextParameter("Folder Path", "Path", "Root folder containing the three other folders with the JSON libraries.", GH_ParamAccess.item, libPath);
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

            // Create one BSoM material with BSoM.LCA
            Material material = new Material();
            // List of all the materials available
            List<string> materialsList = new List<string>();
            string[] jsonMaterialsPathArray;

            // Sanity check
            // Check if the folder path is valid
            string[] requiredFolders = { "Material" };
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
                string databaseMaterials = libPath + "\\" + "Material";
                jsonMaterialsPathArray = Directory.GetFiles(databaseMaterials, "*.json");

                // Convert to list
                materialsList = jsonMaterialsPathArray.ToList();

                // Filter the buildingComponetsList to find the selected component
                List<string> selectedMaterialPath = jsonMaterialsPathArray.Where(s => s.Contains(materialName)).ToList();

                // Get the name of file without the path
                for (int i = 0; i < materialsList.Count; i++)
                {
                    // Extract the file name without the path
                    materialsList[i] = materialsList[i].Substring(materialsList[i].LastIndexOf("\\") + 1);
                    // Extract the file name without the extension
                    materialsList[i] = materialsList[i].Substring(0, materialsList[i].LastIndexOf("."));
                }

                // Read the json file
                string materialJsonFile = File.ReadAllText(selectedMaterialPath[0]);

                // Convert json string to a json object
                JObject materialJsonObj = JObject.Parse(materialJsonFile);

                // JSON keys are hard-coded here
                string keyID = "id";
                string keyName = "name";
                string keyUnit = "unit";
                string keyWeight = "weight_kg";
                string keyDensity = "density";
                string keyDensityArea = "area_density";
                string keyDensityLinear = "linear_density";
                string keyConvFactor = "conversion_factor";
                string keyConvFactorKg = "conversion_factor_kg";
                string keyPenrtA1ToA3 = "penrt_a1toa3";
                string keyPenrtC3 = "penrt_c3";
                string keyPenrtC4 = "penrt_c4";
                string keyPenrtD1 = "penrt_d1";
                string keyGwpA1ToA3 = "gwp_a1toa3";
                string keyGwpC3 = "gwp_c3";
                string keyGwpC4 = "gwp_c4";
                string keyGwpD1 = "gwp_d1";

                // Convert values from json. If empty assign 0 to doubles
                material.ID = (string)materialJsonObj[keyID];
                material.Name = (string)materialJsonObj[keyName];
                material.Unit = (string)materialJsonObj[keyUnit];
                material.Weight = (materialJsonObj[keyWeight] != null) ? (double)materialJsonObj[keyWeight] : 0;
                material.Density = (materialJsonObj[keyDensity] != null) ? (double)materialJsonObj[keyWeight] : 0;
                material.DensityArea = (materialJsonObj[keyDensityArea] != null) ? (double)materialJsonObj[keyWeight] : 0;
                material.DensityLinear = (materialJsonObj[keyDensityLinear] != null) ? (double)materialJsonObj[keyWeight] : 0;
                material.ConversionFactor = (materialJsonObj[keyConvFactor] != null) ? (double)materialJsonObj[keyConvFactor] : 0;
                material.ConversionFactorKg = (materialJsonObj[keyConvFactorKg] != null) ? (double)materialJsonObj[keyConvFactor] : 0;
                material.PENRT_A1ToA3 = (materialJsonObj[keyPenrtA1ToA3] != null) ? (double)materialJsonObj[keyPenrtA1ToA3] : 0;
                material.PENRT_C3 = (materialJsonObj[keyPenrtC3] != null) ? (double)materialJsonObj[keyPenrtC3] : 0;
                material.PENRT_C4 = (materialJsonObj[keyPenrtC4] != null) ? (double)materialJsonObj[keyPenrtC4] : 0;
                material.PENRT_D1 = (materialJsonObj[keyPenrtD1] != null) ? (double)materialJsonObj[keyPenrtD1] : 0;
                material.GWP_A1ToA3 = (materialJsonObj[keyGwpA1ToA3] != null) ? (double)materialJsonObj[keyGwpA1ToA3] : 0;
                material.GWP_C3 = (materialJsonObj[keyGwpC3] != null) ? (double)materialJsonObj[keyGwpC3] : 0;
                material.GWP_C4 = (materialJsonObj[keyGwpC4] != null) ? (double)materialJsonObj[keyGwpC4] : 0;
                material.GWP_D1 = (materialJsonObj[keyGwpD1] != null) ? (double)materialJsonObj[keyGwpD1] : 0;

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

                // Library path
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                string filePath = assembly.Location;
                // Get the directory name from the original path.
                string directoryPath = Path.GetDirectoryName(filePath);
                // Combine with the new directory.
                string libPath = Path.Combine(directoryPath, "BuildSystems");

                // Material list
                List<string> materialsList = new List<string>();
                string[] jsonMaterialsPathArray;

                // Sanity check
                // Check if the folder path is valid
                string[] requiredFolders = { "Material" };
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
                    string databaseMaterials = libPath + "\\" + "Material";
                    jsonMaterialsPathArray = Directory.GetFiles(databaseMaterials, "*.json");

                    // Convert to list
                    materialsList = jsonMaterialsPathArray.ToList();

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


            List<string> maualList = new List<string>
                {
                    "1-1_Kies_2-32",
                    "7-26_Gipskartonplatte_Lochplatte",
                    "8-3_Fassadenfarbe_Voranstrich_Silikat-Dispersion"
                };

                List<Grasshopper.Kernel.Special.GH_ValueListItem> componentsAvailable = new List<Grasshopper.Kernel.Special.GH_ValueListItem>();
                foreach (string component in materialsList)
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