using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Grasshopper.Documentation;

namespace BuildSystemsGH.Building.Create
{
    public class CreateWindows : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        /// 

        public void ReplaceGermanCharactersAndSpaces(ref string stringReplace)
        {
            stringReplace = stringReplace.Replace("ä", "ae");
            stringReplace = stringReplace.Replace("ö", "oe");
            stringReplace = stringReplace.Replace("ü", "ue");
            stringReplace = stringReplace.Replace("ß", "ss");
            stringReplace = stringReplace.Replace(" ", "_");
            stringReplace = stringReplace.Replace("(", "");
            stringReplace = stringReplace.Replace(")", "");
            stringReplace = stringReplace.Replace(",", "");
            stringReplace = stringReplace.Replace(".", "");
            stringReplace = stringReplace.Replace(";", "");
            stringReplace = stringReplace.Replace(":", "");
            stringReplace = stringReplace.Replace("!", "");
            stringReplace = stringReplace.Replace("?", "");
            stringReplace = stringReplace.Replace("=", "");
            stringReplace = stringReplace.Replace("+", "");
            stringReplace = stringReplace.Replace("/", "-");

        }

        public double GetDatabaseValue(string materialName, string key, List<string> databaseFileNames)
        {
            double databaseValue = 0;
            for (int i = 0; i < databaseFileNames.Count; i++)
            {
                string mFileNameNoExt = Path.GetFileNameWithoutExtension(databaseFileNames[i]);
                if (mFileNameNoExt.Contains(materialName))
                {
                    string jsonData = File.ReadAllText(databaseFileNames[i]);

                    // Convert json string to a json object
                    JObject jObjectData = JObject.Parse(jsonData);

                    // Get the value of the key
                    string databaseValueString = (string)jObjectData[key];
                    databaseValueString = databaseValueString.Replace(",", ".");
                    databaseValue = Convert.ToDouble(databaseValueString);

                    return databaseValue;
                }
            }
            return 0;
        }

        public CreateWindows()
          : base("Create Windows", "CW",
              "Assemble windows by percentage of wall surface",
              "BuildSystems", "LCA")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Wall surface area [m²]", "Area [m²]", "Area of wall surfarce [m²].", GH_ParamAccess.list);
            pManager.AddNumberParameter("Opening [%]", "Opening [%]", "Percentage of wall surface area to be windows [%].", GH_ParamAccess.item, 50);
            pManager.AddNumberParameter("Glass [%]", "Glass [%]", "Fensterflächenanteil: Percentage of glass [%].", GH_ParamAccess.item, 65);
            pManager.AddNumberParameter("Casement windows [%]", "Casement [%]", "Anteil Flügelfenster: Percentage of casement windows [%].", GH_ParamAccess.item, 50);
            pManager.AddNumberParameter("Sun cover [%]", "Sun cover [%]", "Anteil Sonnenschutz [%].", GH_ParamAccess.item, 100);
            pManager.AddNumberParameter("Height of sill [m]", "Sill height [m]", "Height of windows sill [m].", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Width of windows [m]", "Width [m]", "Windows width [m].", GH_ParamAccess.item, 1.6);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("GWP A1ToA3", "A1ToA3", "GWP for phases A1ToA3 in kg CO2eq.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("GWP C3", "C3", "GWP for phase C3 in kg CO2eq.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("GWP C4", "C4", "GWP for phase C4 in kg CO2eq.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("GWP D1", "D1", "GWP for phase D1 in kg CO2eq.", GH_ParamAccess.tree);
            pManager.AddTextParameter("Material Name", "Name", "Name of material from Ökobilanz.", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Initialize input variables
            List<double> wallSurfaceArea = new List<double>();
            double openingPercentage = 0;
            double glassPercentage = 0; //Fensterflächenanteil
            double casementPercentage = 0; //Anteil Flügelfenster
            double sunCoverPercentage = 0; //Anteil Sonnenschutz
            double sillHeight = 0; // Öffnungshöhe
            double windowsWidth = 0; // Fensterbreite

            // Get input
            if (!DA.GetDataList(0, wallSurfaceArea)) return;
            DA.GetData(1, ref openingPercentage);
            DA.GetData(2, ref glassPercentage);
            DA.GetData(3, ref casementPercentage);
            DA.GetData(4, ref sunCoverPercentage);
            DA.GetData(5, ref sillHeight);
            DA.GetData(6, ref windowsWidth);

            // Initialize work variables
            double openingArea = 0; // rohbauöffnungen
            double glassSurfaceArea = 0; //Fensterglas
            double frameWallLength = 0; //Blendrahmen
            double frameGlassLength = 0; //Flügelrahmen
            double sealingLength = 0; //Fugendichtungsbänder
            double windowsFitings = 0; //Fensterbeschläge
            double windowsHandles = 0; //Fenstergriffe
            double sunCoverSurfaceArea = 0; //Sonnenschutzfläche

            // Initialize database keys
            string keyGWPA1ToA3 = "GWP A1-A3 [kg CO2e]";
            string keyGWPC3 = "GWP C3 [kg CO2e]";
            string keyGWPC4 = "GWP C4 [kg CO2e]";
            string keyGWPD1 = "GWP D1 [kg CO2e]";

            // Get database
            GH_AssemblyInfo info = Grasshopper.Instances.ComponentServer.FindAssembly(new Guid("36538369-6017-4b4c-9973-aee8f072399a"));
            string filePath = info.Location;
            // Get the directory name from the original path.
            string directoryPath = Path.GetDirectoryName(filePath);
            // Combine with the new directory.
            string libPath = Path.Combine(directoryPath, "BuildSystems");
            string[] jsonMaterialPathArray;
            string databaseMaterial = libPath + "\\" + "Material";
            jsonMaterialPathArray = Directory.GetFiles(databaseMaterial, "*.json");

            // Convert to list
            List<string> materialFileNames = new List<string>();
            materialFileNames = jsonMaterialPathArray.ToList();

            // Initialize output variables
            GH_Structure<GH_Number> gwpA1ToA3 = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> gwpC3 = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> gwpC4 = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> gwpD1 = new GH_Structure<GH_Number>();
            GH_Structure<GH_String> materialNameOutput = new GH_Structure<GH_String>();

            // Calculate opening area
            foreach (double area in wallSurfaceArea)
            {
                openingArea += area * openingPercentage / 100;
            }

            //Anzahl Fenster
            double numberOfWindows = openingArea / (windowsWidth * sillHeight);

            // Material

            //Fensterglas
            GH_Path glassPath = new GH_Path(0);
            glassSurfaceArea = openingArea * glassPercentage / 100;
            string mGlass = "Isolierglas 2-Scheiben";
            GH_String gH_MGlass = new GH_String(mGlass);
            materialNameOutput.Append(gH_MGlass, glassPath);
            ReplaceGermanCharactersAndSpaces(ref mGlass);
            double gwpA1ToA3Database = GetDatabaseValue(mGlass, keyGWPA1ToA3, materialFileNames);
            double gwpC3Database = GetDatabaseValue(mGlass, keyGWPC3, materialFileNames);
            double gwpC4Database = GetDatabaseValue(mGlass, keyGWPC4, materialFileNames);
            double gwpD1Database = GetDatabaseValue(mGlass, keyGWPD1, materialFileNames);
            GH_Number gH_A1ToA3 = new GH_Number(gwpA1ToA3Database * glassSurfaceArea);
            GH_Number gH_C3 = new GH_Number(gwpC3Database * glassSurfaceArea);
            GH_Number gH_C4 = new GH_Number(gwpC4Database * glassSurfaceArea);
            GH_Number gH_D1 = new GH_Number(gwpD1Database * glassSurfaceArea);
            gwpA1ToA3.Append(gH_A1ToA3, glassPath);
            gwpC3.Append(gH_C3, glassPath);
            gwpC4.Append(gH_C4, glassPath);
            gwpD1.Append(gH_D1, glassPath);


            // Blendrahmen
            GH_Path frameWallPath = new GH_Path(1);
            frameWallLength = 2 * (numberOfWindows * (windowsWidth + sillHeight));
            string mFrameWall = "Aluminium-Rahmenprofil, pulverbeschichtet";
            GH_String gH_MFrameWall = new GH_String(mFrameWall);
            materialNameOutput.Append(gH_MFrameWall, frameWallPath);
            ReplaceGermanCharactersAndSpaces(ref mFrameWall);
            gwpA1ToA3Database = GetDatabaseValue(mFrameWall, keyGWPA1ToA3, materialFileNames);
            gwpC3Database = GetDatabaseValue(mFrameWall, keyGWPC3, materialFileNames);
            gwpC4Database = GetDatabaseValue(mFrameWall, keyGWPC4, materialFileNames);
            gwpD1Database = GetDatabaseValue(mFrameWall, keyGWPD1, materialFileNames);
            GH_Number gH_A1ToA3FrameWall = new GH_Number(gwpA1ToA3Database * frameWallLength);
            GH_Number gH_C3FrameWall = new GH_Number(gwpC3Database * frameWallLength);
            GH_Number gH_C4FrameWall = new GH_Number(gwpC4Database * frameWallLength);
            GH_Number gH_D1FrameWall = new GH_Number(gwpD1Database * frameWallLength);
            gwpA1ToA3.Append(gH_A1ToA3FrameWall, frameWallPath);
            gwpC3.Append(gH_C3FrameWall, frameWallPath);
            gwpC4.Append(gH_C4FrameWall, frameWallPath);
            gwpD1.Append(gH_D1FrameWall, frameWallPath);

            // Flügelrahmen
            GH_Path frameGlassPath = new GH_Path(2);
            frameGlassLength = frameWallLength * casementPercentage / 100;
            string mFrameGlass = "Aluminium-Flügelrahmenprofil, thermisch getrennt, pulverbeschichtet";
            GH_String gH_MFrameGlass = new GH_String(mFrameGlass);
            materialNameOutput.Append(gH_MFrameGlass, frameGlassPath);
            ReplaceGermanCharactersAndSpaces(ref mFrameGlass);
            gwpA1ToA3Database = GetDatabaseValue(mFrameGlass, keyGWPA1ToA3, materialFileNames);
            gwpC3Database = GetDatabaseValue(mFrameGlass, keyGWPC3, materialFileNames);
            gwpC4Database = GetDatabaseValue(mFrameGlass, keyGWPC4, materialFileNames);
            gwpD1Database = GetDatabaseValue(mFrameGlass, keyGWPD1, materialFileNames);
            GH_Number gH_A1ToA3FrameGlass = new GH_Number(gwpA1ToA3Database * frameGlassLength);
            GH_Number gH_C3FrameGlass = new GH_Number(gwpC3Database * frameGlassLength);
            GH_Number gH_C4FrameGlass = new GH_Number(gwpC4Database * frameGlassLength);
            GH_Number gH_D1FrameGlass = new GH_Number(gwpD1Database * frameGlassLength);
            gwpA1ToA3.Append(gH_A1ToA3FrameGlass, frameGlassPath);
            gwpC3.Append(gH_C3FrameGlass, frameGlassPath);
            gwpC4.Append(gH_C4FrameGlass, frameGlassPath);
            gwpD1.Append(gH_D1FrameGlass, frameGlassPath);

            // Fugendichtungsbänder
            GH_Path sealingPath = new GH_Path(3);
            sealingLength = frameWallLength + frameGlassLength;
            string mSealing = "Fugendichtungsbänder Butyl";
            GH_String gH_MSealing = new GH_String(mSealing);
            materialNameOutput.Append(gH_MSealing, sealingPath);
            ReplaceGermanCharactersAndSpaces(ref mSealing);
            gwpA1ToA3Database = GetDatabaseValue(mSealing, keyGWPA1ToA3, materialFileNames);
            gwpC3Database = GetDatabaseValue(mSealing, keyGWPC3, materialFileNames);
            gwpC4Database = GetDatabaseValue(mSealing, keyGWPC4, materialFileNames);
            gwpD1Database = GetDatabaseValue(mSealing, keyGWPD1, materialFileNames);
            GH_Number gH_A1ToA3Sealing = new GH_Number(gwpA1ToA3Database * sealingLength);
            GH_Number gH_C3Sealing = new GH_Number(gwpC3Database * sealingLength);
            GH_Number gH_C4Sealing = new GH_Number(gwpC4Database * sealingLength);
            GH_Number gH_D1Sealing = new GH_Number(gwpD1Database * sealingLength);
            gwpA1ToA3.Append(gH_A1ToA3Sealing, sealingPath);
            gwpC3.Append(gH_C3Sealing, sealingPath);
            gwpC4.Append(gH_C4Sealing, sealingPath);
            gwpD1.Append(gH_D1Sealing, sealingPath);

            // Fensterbeschläge
            GH_Path windowsFitingsPath = new GH_Path(4);
            windowsFitings = numberOfWindows * casementPercentage / 100;
            string mWindowsFitings = "Fenster-Beschlag für Drehkippfenster (Aluminium)";
            GH_String gH_MWindowsFitings = new GH_String(mWindowsFitings);
            materialNameOutput.Append(gH_MWindowsFitings, windowsFitingsPath);
            ReplaceGermanCharactersAndSpaces(ref mWindowsFitings);
            gwpA1ToA3Database = GetDatabaseValue(mWindowsFitings, keyGWPA1ToA3, materialFileNames);
            gwpC3Database = GetDatabaseValue(mWindowsFitings, keyGWPC3, materialFileNames);
            gwpC4Database = GetDatabaseValue(mWindowsFitings, keyGWPC4, materialFileNames);
            gwpD1Database = GetDatabaseValue(mWindowsFitings, keyGWPD1, materialFileNames);
            GH_Number gH_A1ToA3WindowsFitings = new GH_Number(gwpA1ToA3Database * windowsFitings);
            GH_Number gH_C3WindowsFitings = new GH_Number(gwpC3Database * windowsFitings);
            GH_Number gH_C4WindowsFitings = new GH_Number(gwpC4Database * windowsFitings);
            GH_Number gH_D1WindowsFitings = new GH_Number(gwpD1Database * windowsFitings);
            gwpA1ToA3.Append(gH_A1ToA3WindowsFitings, windowsFitingsPath);
            gwpC3.Append(gH_C3WindowsFitings, windowsFitingsPath);
            gwpC4.Append(gH_C4WindowsFitings, windowsFitingsPath);
            gwpD1.Append(gH_D1WindowsFitings, windowsFitingsPath);

            // Fenstergriffe
            GH_Path windowsHandlesPath = new GH_Path(5);
            windowsHandles = windowsFitings;
            string mWindowsHandles = "Fenstergriff";
            GH_String gH_MWindowsHandles = new GH_String(mWindowsHandles);
            materialNameOutput.Append(gH_MWindowsHandles, windowsHandlesPath);
            ReplaceGermanCharactersAndSpaces(ref mWindowsHandles);
            gwpA1ToA3Database = GetDatabaseValue(mWindowsHandles, keyGWPA1ToA3, materialFileNames);
            gwpC3Database = GetDatabaseValue(mWindowsHandles, keyGWPC3, materialFileNames);
            gwpC4Database = GetDatabaseValue(mWindowsHandles, keyGWPC4, materialFileNames);
            gwpD1Database = GetDatabaseValue(mWindowsHandles, keyGWPD1, materialFileNames);
            GH_Number gH_A1ToA3WindowsHandles = new GH_Number(gwpA1ToA3Database * windowsHandles);
            GH_Number gH_C3WindowsHandles = new GH_Number(gwpC3Database * windowsHandles);
            GH_Number gH_C4WindowsHandles = new GH_Number(gwpC4Database * windowsHandles);
            GH_Number gH_D1WindowsHandles = new GH_Number(gwpD1Database * windowsHandles);
            gwpA1ToA3.Append(gH_A1ToA3WindowsHandles, windowsHandlesPath);
            gwpC3.Append(gH_C3WindowsHandles, windowsHandlesPath);
            gwpC4.Append(gH_C4WindowsHandles, windowsHandlesPath);
            gwpD1.Append(gH_D1WindowsHandles, windowsHandlesPath);

            // Sonnenschutzfläche
            GH_Path sunCoverPath = new GH_Path(6);
            sunCoverSurfaceArea = glassSurfaceArea * sunCoverPercentage / 100;
            string mSunCover = "Rollladen Kunststoff";
            GH_String gH_MSunCover = new GH_String(mSunCover);
            materialNameOutput.Append(gH_MSunCover, sunCoverPath);
            ReplaceGermanCharactersAndSpaces(ref mSunCover);
            gwpA1ToA3Database = GetDatabaseValue(mSunCover, keyGWPA1ToA3, materialFileNames);
            gwpC3Database = GetDatabaseValue(mSunCover, keyGWPC3, materialFileNames);
            gwpC4Database = GetDatabaseValue(mSunCover, keyGWPC4, materialFileNames);
            gwpD1Database = GetDatabaseValue(mSunCover, keyGWPD1, materialFileNames);
            GH_Number gH_A1ToA3SunCover = new GH_Number(gwpA1ToA3Database * sunCoverSurfaceArea);
            GH_Number gH_C3SunCover = new GH_Number(gwpC3Database * sunCoverSurfaceArea);
            GH_Number gH_C4SunCover = new GH_Number(gwpC4Database * sunCoverSurfaceArea);
            GH_Number gH_D1SunCover = new GH_Number(gwpD1Database * sunCoverSurfaceArea);
            gwpA1ToA3.Append(gH_A1ToA3SunCover, sunCoverPath);
            gwpC3.Append(gH_C3SunCover, sunCoverPath);
            gwpC4.Append(gH_C4SunCover, sunCoverPath);
            gwpD1.Append(gH_D1SunCover, sunCoverPath);

            // Set outputs
            DA.SetDataTree(0, gwpA1ToA3);
            DA.SetDataTree(1, gwpC3);
            DA.SetDataTree(2, gwpC4);
            DA.SetDataTree(3, gwpD1);
            DA.SetDataTree(4, materialNameOutput);
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.CreateWindows;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("AD28047B-A691-4A83-8BDA-AC904D5FB25F"); }
        }
    }
}