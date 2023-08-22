using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;
using System.Drawing;

namespace BuildSystemsGH.Components
{
    public class AssembleComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>

        public class BuildComponent
        {
            public List<string> ListPaths = new List<string>();
            public GH_Structure<GH_Box> ListBoxes = new GH_Structure<GH_Box>();

            // Component indexes to map from the Components Library
            public const int indexLod = 0;
            public const int indexLevel = 1;
            public const int indexBom = 2;
            public const int indexMatId = 3;
            public const int indexMatName = 4;
            public const int indexMatCategory = 5;
            public const int indexMatDescription = 6;
            public const int indexMatThickness = 7;
            public const int indexMatWidth = 8;
            public const int indexMatSpread = 9;

            // Composite materials - This is used because the composite data in the column width is actually used as percentage
            // This whole AssembleComponent needs a rework after the BSoM is finished
            public List<string> compositeMaterials = new List<string>
            {
                "Stahlbeton",
                "Mauerwerk",
                "Beton"
            };

            public BuildComponent(List<Surface> surfaces, GH_Structure<GH_String> component, int bomRoot)
            {
                /////// Loop through each surfaces ///////
                for (int i = 0; i < surfaces.Count; i++)
                {
                    // Get brep
                    // Initialize empty brep
                    Surface surface = surfaces[i];

                    // Initialize current height
                    double currentHeight = 0;

                    double prevThickness = 0;

                    // Create a brep from the surface
                    Brep baseBrep = Brep.CreateFromSurface(surface);

                    // Go through each branch in the component datatree and create a brep using CreateFromOffsetFace starting in the current plane origin and stacking using the height data in the component data tree
                    IList<GH_Path> iListPaths = component.Paths;

                    // Get the normal vector of the surface
                    Vector3d normalVector = surface.NormalAt(0.5, 0.5);

                    // Get the bounding box
                    Point3d center = baseBrep.GetBoundingBox(true).Center;

                    // Initialize the spread number
                    int spreadNumber = 0;

                    // Initialize trimmed curves for insulation (also for framing)
                    Curve mainTrimmedCurve = new LineCurve();
                    Curve parallelTrimmedCurve = new LineCurve();

                    // Initialize frame width
                    double frameWidth = 0;

                    // If horizontal, then get the largest side to do the spread calculation
                    // If vertical, then get the lowest side to do the spread calculation

                    // Get the brep edges for frame calculation
                    Curve[] brepEdges = baseBrep.DuplicateEdgeCurves(true);

                    // Initialize the main curve, its parallel and the secondary curve
                    Curve mainCurve = new LineCurve();
                    Curve parallelCurve = new LineCurve();
                    Curve secondaryCurve = new LineCurve();
                    double mainCurveLength = 0;
                    double mainCurveHeight = 100000000;
                    double secondaryCurveLength = 100000000;

                    /////// Define values based on panel direction ///////
                    /////// Horizontal surface ///////
                    if (1 - (Math.Abs(normalVector.Z)) < 0.02) // 0.02 is the tolerance
                    {
                        // Surface is horizontal, so get the largest side to do the spread calculation
                        // Loop through the brepEdges list and find the largest edge
                        foreach (Curve curve in brepEdges)
                        {
                            if (curve.GetLength() > mainCurveLength)
                            {
                                mainCurve = curve.DuplicateCurve();
                                mainCurveLength = curve.GetLength();
                            }
                            if (curve.GetLength() < secondaryCurveLength)
                            {
                                secondaryCurve = curve.DuplicateCurve();
                                secondaryCurveLength = curve.GetLength();
                            }
                        }
                        // Find the parallel curve (parallel to the main curve)
                        Vector3d mainCurveVector = mainCurve.PointAtEnd - mainCurve.PointAtStart;
                        foreach (Curve curve in brepEdges)
                        {
                            Vector3d curveVector = curve.PointAtStart - curve.PointAtEnd;
                            if (curveVector.IsParallelTo(mainCurveVector, 0.1) == 1)
                            {
                                parallelCurve = curve.DuplicateCurve();
                            }
                        }
                    }

                    /////// Vertical surface ///////
                    else
                    {
                        // Surface is vertical, so get the lowest side to do the spread calculation
                        // Loop through the brepEdges list and find the lowest edge
                        List<Curve> tempBrepEdgesList = new List<Curve>();
                        foreach (Curve curve in brepEdges)
                        {
                            double curveHeight = curve.PointAtNormalizedLength(0.5).Z;
                            if (curveHeight < mainCurveHeight)
                            {
                                mainCurve = curve.DuplicateCurve();
                                mainCurveLength = curve.GetLength();
                                mainCurveHeight = curveHeight;
                            }
                        }
                        // Find the parallel curve(parallel to the main curve)
                        Vector3d mainCurveVector = mainCurve.PointAtEnd - mainCurve.PointAtStart;
                        foreach (Curve curve in brepEdges)
                        {
                            Vector3d curveVector = curve.PointAtStart - curve.PointAtEnd;
                            if (curveVector.IsParallelTo(mainCurveVector, 0.1) == 1)
                            {
                                parallelCurve = curve.DuplicateCurve();
                            }

                            //if (curveVector.IsPerpendicularTo(mainCurveVector, 1) == false)
                            //{
                            //  secondaryCurve = curve.DuplicateCurve();
                            //  secondaryCurveLength = curve.GetLength();
                            //}
                        }
                        foreach (Curve curve in brepEdges)
                        {
                            // If curve is not parallel to the main curve, then it is the secondary curve
                            Vector3d curveVector = curve.PointAtEnd - curve.PointAtStart;
                            if (curveVector.IsPerpendicularTo(mainCurveVector, 1) == true)
                            {
                                secondaryCurve = curve.DuplicateCurve();
                                secondaryCurveLength = curve.GetLength();
                            }

                        }
                    }

                    /////// Loop through the component data tree ///////
                    for (int j = 0; j < iListPaths.Count; j++)
                    {
                        // Get current path
                        GH_Path path = iListPaths[j];
                        string layerLevel = "";
                        GH_Convert.ToString(component.get_Branch(path)[indexLevel], out layerLevel, GH_Conversion.Primary);
                        char lastlayerLevelChar = layerLevel[layerLevel.Length - 1];
                        int subLayerLevel = Convert.ToInt32(lastlayerLevelChar.ToString());
                        
                        // Get current BOM (legacy)
                        string bomString = "";
                        GH_Convert.ToString(component.get_Branch(path)[indexBom], out bomString, GH_Conversion.Primary);
                        int bomCurrent = Convert.ToInt32(bomString);

                        // Get category
                        string categoryCurrent = "";
                        GH_Convert.ToString(component.get_Branch(path)[indexMatCategory], out categoryCurrent, GH_Conversion.Primary);

                        // Get next path & next LOD
                        int bomNext = new int();
                        if (j != iListPaths.Count - 1)
                        {
                            GH_Path nextPath = iListPaths[j + 1];
                            string lodNextString = "";
                            GH_Convert.ToString(component.get_Branch(nextPath)[indexBom], out lodNextString, GH_Conversion.Primary);
                            bomNext = Convert.ToInt32(lodNextString);
                        }

                        // Get thickness from component data tree
                        string thicknessAsString = "";
                        GH_Convert.ToString(component.get_Branch(path)[indexMatThickness], out thicknessAsString, GH_Conversion.Primary);
                        // Convert string to double and fix any commas
                        thicknessAsString = thicknessAsString.Replace(",", ".");
                        double thickness = Convert.ToDouble(thicknessAsString);

                        // Add to the path list
                        ListPaths.Add(iListPaths[j].ToString());


                        /////// MaterialLevel 1 ///////
                        //Find moving direction
                        Vector3d dir = normalVector * currentHeight;

                        // Get X point for plane. Project a point to main curve
                        double centerParam = 0;
                        mainCurve.ClosestPoint(center, out centerParam);
                        Point3d planeX = mainCurve.PointAt(centerParam);

                        // Get Y point for plane. Starting point of main curve
                        Point3d planeY = mainCurve.PointAtStart;

                        // Define plane
                        Plane brepPlane = new Plane(center, planeX, planeY);

                        // Change plane origin based on current height
                        brepPlane.Origin = center + dir;

                        if (bomCurrent == bomRoot)
                        {

                            // Define intervals
                            Interval intervalX = new Interval(-secondaryCurveLength / 2, secondaryCurveLength / 2);
                            Interval intervalY = new Interval(-mainCurveLength / 2, mainCurveLength / 2);
                            Interval intervalZ = new Interval(0, -thickness);

                            // 	Box(Plane, Interval, Interval, Interval)
                            Box fullMatBox = new Box(brepPlane, intervalX, intervalY, intervalZ);
                            GH_Box gH_fullMatBox = new GH_Box(fullMatBox);

                            // Path
                            GH_Path pathSurfMat = new GH_Path(i, j);

                            ListBoxes.Append(gH_fullMatBox, pathSurfMat);

                            // Move the brep to the current plane origin
                            currentHeight += thickness;
                            
                        }
                        

                        // For now we have a composite situation in which I multiply the thickness by the percentage of material
                        // The nice way would be to have the full composite thickness and calculate the material percentage in another method
                        // Then in the GWP for example we can just multiply by the calculated material totals
                        else if (compositeMaterials.Contains(categoryCurrent) == true)
                        {
                            string frameWidthString = "";
                            GH_Convert.ToString(component.get_Branch(path)[indexMatWidth], out frameWidthString, GH_Conversion.Primary);
                            frameWidth = Convert.ToDouble(frameWidthString.Replace(",", "."));

                            // Define intervals
                            Interval intervalX = new Interval(-secondaryCurveLength / 2, secondaryCurveLength / 2);
                            Interval intervalY = new Interval(-mainCurveLength / 2, mainCurveLength / 2);
                            Interval intervalZ = new Interval(0, -(thickness * frameWidth));

                            // 	Box(Plane, Interval, Interval, Interval)
                            Box fullMatBox = new Box(brepPlane, intervalX, intervalY, intervalZ);
                            GH_Box gH_fullMatBox = new GH_Box(fullMatBox);

                            // Path
                            GH_Path pathSurfMat = new GH_Path(i, j);

                            ListBoxes.Append(gH_fullMatBox, pathSurfMat);

                            // Move the brep to the current plane origin
                            currentHeight += (thickness * frameWidth);
                            
                        }


                        /////// MaterialLevel 2 ///////
                        // This part takes care of the materials that overlap
                        // if categoryCurrent is not in the list of composite materials
                        else if (compositeMaterials.Contains(categoryCurrent) == false)
                        {
                            /////// Calculation for framing ///////
                            // There is always a framing material on the first layer
                            if (subLayerLevel == 1)
                            {
                                // Get the frame width from the component data tree
                                string frameWidthString = "";
                                GH_Convert.ToString(component.get_Branch(path)[indexMatWidth], out frameWidthString, GH_Conversion.Primary);
                                frameWidth = Convert.ToDouble(frameWidthString.Replace(",", "."));

                                // Get the frame spread from the component data tree
                                string spreadString = "";
                                GH_Convert.ToString(component.get_Branch(path)[indexMatSpread], out spreadString, GH_Conversion.Primary);
                                double spread = Convert.ToDouble(spreadString.Replace(",", "."));

                                // Trim the surface edges to account for the frame width
                                Curve mainCurveUtilFraming = mainCurve.Trim(CurveEnd.Both, frameWidth / 2);
                                Curve secCurveUtilFraming = parallelCurve.Trim(CurveEnd.Both, frameWidth / 2);

                                // Define trimmed curves
                                mainTrimmedCurve = mainCurve.Trim(CurveEnd.Both, frameWidth);
                                parallelTrimmedCurve = parallelCurve.Trim(CurveEnd.Both, frameWidth);

                                // Divide the curve into segments
                                spreadNumber = Convert.ToInt32(Math.Round(mainCurveUtilFraming.GetLength() / spread));
                                double[] paramStart = mainCurveUtilFraming.DivideByCount(spreadNumber, true);
                                double[] paramEnd = secCurveUtilFraming.DivideByCount(spreadNumber, true);
                                Array.Reverse(paramEnd); // Reverse the paramEnd array to match the paramStart array

                                // Get the points from the parameters
                                List<Point3d> pointsStart = new List<Point3d>();
                                List<Point3d> pointsEnd = new List<Point3d>();
                                List<double> framingLengths = new List<double>();
                                foreach (double param in paramStart)
                                {
                                    Point3d point = mainCurveUtilFraming.PointAt(param);
                                    pointsStart.Add(point);
                                }
                                foreach (double param in paramEnd)
                                {
                                    Point3d point = secCurveUtilFraming.PointAt(param);
                                    pointsEnd.Add(point);
                                }
                                // Get frame lengths (distance from start to end)
                                for (int k = 0; k < pointsStart.Count; k++)
                                {
                                    double frameLength = pointsStart[k].DistanceTo(pointsEnd[k]);
                                    framingLengths.Add(frameLength);
                                }

                                // Define the boxes
                                // First find the framingPlanes list
                                for (int k = 0; k < framingLengths.Count; k++)
                                {
                                    // Define frame plane
                                    Point3d averagePoint = (pointsStart[k] + pointsEnd[k]) / 2;
                                    Vector3d frameVector = pointsStart[k] - pointsEnd[k];
                                    Plane framePlane = brepPlane;
                                    framePlane.Origin = pointsStart[k] + dir;

                                    // Define intervals
                                    Interval intervalY = new Interval(-frameWidth / 2, frameWidth / 2);
                                    Interval intervalX = new Interval(0, -framingLengths[k]);
                                    Interval intervalZ = new Interval(0, -thickness);

                                    // 	Box(Plane, Interval, Interval, Interval)
                                    Box frameBox = new Box(framePlane, intervalX, intervalY, intervalZ);
                                    GH_Box gH_frameBox = new GH_Box(frameBox);

                                    // Path
                                    GH_Path pathSurfMat = new GH_Path(i, j);

                                    // Add the box to the list
                                    ListBoxes.Append(gH_frameBox, pathSurfMat);
                                }
                                //currentHeight += thickness;
                            }
                            /////// Calculation for insulation ///////
                            else 
                            {
                                // Get the insulation width
                                mainCurveLength = mainCurve.GetLength();
                                double insulationWidth = (mainCurveLength - ((spreadNumber + 1) * frameWidth)) / spreadNumber;

                                // Get spread from the component data tree
                                string spreadString = "";
                                GH_Convert.ToString(component.get_Branch(path)[indexMatSpread], out spreadString, GH_Conversion.Primary);
                                double spread = Convert.ToDouble(spreadString.Replace(",", "."));

                                // Trim the surface edges to account for the frame width
                                Curve mainTrimCurveUtilInsulation = mainTrimmedCurve.Trim(CurveEnd.Both, insulationWidth / 2);
                                Curve paralellTrimCurveUtilInsulation = parallelTrimmedCurve.Trim(CurveEnd.Both, insulationWidth / 2);

                                // Divide the curve into segments
                                double[] paramStart = mainTrimCurveUtilInsulation.DivideByCount(spreadNumber - 1, true);
                                double[] paramEnd = paralellTrimCurveUtilInsulation.DivideByCount(spreadNumber - 1, true);
                                // Reverse the paramEnd array to match the paramStart array
                                Array.Reverse(paramEnd);

                                // Get the points from the parameters
                                List<Point3d> pointsStart = new List<Point3d>();
                                List<Point3d> pointsEnd = new List<Point3d>();
                                List<double> insulationLengths = new List<double>();
                                foreach (double param in paramStart)
                                {
                                    Point3d point = mainTrimCurveUtilInsulation.PointAt(param);
                                    pointsStart.Add(point);
                                }
                                foreach (double param in paramEnd)
                                {
                                    Point3d point = paralellTrimCurveUtilInsulation.PointAt(param);
                                    pointsEnd.Add(point);
                                }
                                // Get frame lengths (distance from start to end)
                                for (int k = 0; k < pointsStart.Count; k++)
                                {
                                    double insulationLength = pointsStart[k].DistanceTo(pointsEnd[k]);
                                    insulationLengths.Add(insulationLength);
                                }

                                // Define the boxes
                                brepPlane.Origin = center + dir;
                                for (int k = 0; k < insulationLengths.Count; k++)
                                {
                                    // Define insulation plane
                                    Point3d averagePoint = (pointsStart[k] + pointsEnd[k]) / 2;

                                    // Find the origin plane
                                    Plane insulationPlane = brepPlane;
                                    insulationPlane.Origin = averagePoint + dir;

                                    // Define intervals
                                    Interval intervalY = new Interval(-insulationWidth / 2, insulationWidth / 2);
                                    Interval intervalZ = new Interval(0, -thickness);
                                    Interval intervalX = new Interval(-insulationLengths[k] / 2, insulationLengths[k] / 2);

                                    // 	Box(Plane, Interval, Interval, Interval)
                                    Box insulationBox = new Box(insulationPlane, intervalX, intervalY, intervalZ);
                                    GH_Box gH_insulationBox = new GH_Box(insulationBox);

                                    Brep brepBox = insulationBox.ToBrep();

                                    // Path
                                    GH_Path pathSurfMat = new GH_Path(i, j);

                                    // Add insulation to brep tree
                                    ListBoxes.Append(gH_insulationBox, pathSurfMat);
                                }
                                currentHeight += prevThickness;
                            }
                        }

                    // Previous thickness to calculate correct insulation location in the gap
                    prevThickness = thickness;
                    }
                }
            }

            // GWP A1ToA3 method
            public GH_Structure<GH_Number> GetMaterialData(List<List<string>> matDatabase, int matIdIndexDatabase, int propertyIndexDatabase, int indexUmrechnungsfaktor, GH_Structure<GH_String> component, int matIdIndexComponent, GH_Structure<GH_Box> ListBoxes)
            {

                // Get GH_Path from ListBoxes
                IList<GH_Path> boxesPaths = ListBoxes.Paths;
                GH_Structure<GH_Number> panelProperty = new GH_Structure<GH_Number>();
                foreach (GH_Path boxPath in boxesPaths)
                {
                    double matVolume = 0;
                    int[] arrayPath = boxPath.Indices;
                    int lastIndex = arrayPath[arrayPath.Length - 1];
                    int firstIndex = arrayPath[0];
                    GH_Path panelPath = new GH_Path(firstIndex);
                    List<GH_Box> boxes = (List<GH_Box>)ListBoxes.get_Branch(boxPath);

                    // Get the box volume
                    foreach (GH_Box box in boxes)
                    {
                        double boxVolume = box.Brep().GetVolume();
                        matVolume += boxVolume;
                    }

                    // Loop through the component data tree and match the last index
                    for (int i = 0; i < component.Branches.Count; i++)
                    {
                        // Get the path
                        GH_Path compPath = component.get_Path(i);

                        // Check if the last index of the path matches the last index of the boxPath
                        if (lastIndex == i)
                        {
                            // Get material ID from the component datatree
                            string matIdComponent = "";
                            GH_Convert.ToString(component.get_Branch(compPath)[matIdIndexComponent], out matIdComponent, GH_Conversion.Primary);

                            // Initialize the variables
                            double databaseProperty = 0;
                            double databaseConversionFactor = 1;

                            // Loop through the material database
                            for (int j = 0; j < matDatabase.Count; j++)
                            {

                                // Get the material ID on database
                                string matIdDatabase = matDatabase[j][matIdIndexDatabase];
                                // Check if the material ID from component matches the one from database
                                if (matIdComponent == matIdDatabase)
                                {
                                    // Get the property value (for example gwpA1ToA3)
                                    string databasePropertyString = matDatabase[j][propertyIndexDatabase];
                                    databasePropertyString = databasePropertyString.Replace(",", ".");
                                    if (!double.TryParse(databasePropertyString, out databaseProperty))
                                    {
                                        databaseProperty = 0;  // Assign 0 if conversion is not successful.
                                    }

                                    //double databaseProperty = Convert.ToDouble(databasePropertyString.Replace(",", "."));

                                    // Get the conversion factor (from kg to m3)
                                    string databaseConversionFactorString = matDatabase[j][indexUmrechnungsfaktor];
                                    databaseConversionFactorString = databaseConversionFactorString.Replace(",", ".");
                                    if (!double.TryParse(databaseConversionFactorString, out databaseConversionFactor))
                                    {
                                        databaseConversionFactor = 1;  // Assign 0 if conversion is not successful.
                                    }
                                    //databaseConversionFactor = Convert.ToDouble(databaseConversionFactorString.Replace(",", "."));
                                }

                            }
                            // Calculate the Property total
                            double propertyTotal = (databaseProperty * matVolume) * databaseConversionFactor;
                            GH_Number gH_propertyTotal = new GH_Number(propertyTotal);
                            panelProperty.Append(gH_propertyTotal, boxPath);
                        }
                    }
                }
                return panelProperty;

            }

        }

        public List<List<string>> ConvertDatabase(string componentDatabase)
        {
            //Split the database into rows. Now we have a list of strings.
            List<string> rows = componentDatabase.Split('\n').ToList();

            // Split the rows into lists of strings. Now we have a list of lists of strings.
            List<List<string>> databaseLists = rows.Select(s => s.Split(';').ToList()).ToList();

            return databaseLists;
        }

        // First approach and less efficient. Convert all the json files into a list of lists.
        public List<List<string>> ConvertJsonToMatDatabaseAlpha1(string filePath)
            {

                string databaseMaterialPath = filePath + "\\" + "Material";
                string[] jsonComponentPathArray = System.IO.Directory.GetFiles(databaseMaterialPath, "*.json");
                
                List<List<string>> matDatabase = new List<List<string>>();

                // Loop through each JSON
                foreach (string path in jsonComponentPathArray)
                {
                    List<string> matJson = new List<string>();
                    string jsonAssembly = File.ReadAllText(path);

                    // Parse the JSON
                    JObject jObjectAssembly = JObject.Parse(jsonAssembly);
                    // Loop through each JSON key and add the values to the list
                    foreach (JProperty property in jObjectAssembly.Properties())
                    {
                        matJson.Add(property.Value.ToString());
                    }
                    matDatabase.Add(matJson);
                }
                return matDatabase;
            }

        public AssembleComponent()
          : base("Assemble Component", "AC",
              "Assemble the component layers.",
              "BuildSystems", "LCA")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            GH_AssemblyInfo info = Grasshopper.Instances.ComponentServer.FindAssembly( new Guid ("36538369-6017-4b4c-9973-aee8f072399a") );
            string filePath = info.Location;
            // Get the directory name from the original path.
            string directoryPath = Path.GetDirectoryName(filePath);
            // Combine with the new directory.
            string libPath = Path.Combine(directoryPath, "BuildSystems");
            pManager.AddTextParameter("Folder Path", "Path", "Root folder containing the three sub-folders with JSON libraries.", GH_ParamAccess.item, libPath);
            pManager.AddSurfaceParameter("Building Surfaces", "Surfaces", "Building surfaces to generate the bateil.", GH_ParamAccess.list);
            pManager.AddTextParameter("Component Layers Tree", "Component", "Component layers in the format of GH Data Tree.", GH_ParamAccess.tree);
            // Here could be a value list that is filled automatically
            pManager.AddTextParameter("Calculation Phase", "Phase", "Phases to calculate the GWP and PENRT (A1ToA3, C3, C4, D1, AToC, AToD). Default is A1ToA3.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBoxParameter("Materials as Boxes", "Boxes", "Representation of materials as Boxes.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Selected Phase PENRT", "PENRT", "PENRT calculated using the material volumes from the Boxes [MJ].", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Selected Phase GWP", "GWP", "GWP calculated using the material volumes from the Boxes [kg CO2-eq].", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string libPath = "";
            List<Surface> buildignSurfaces = new List<Surface>();
            GH_Structure<GH_String> buildingComponent = new GH_Structure<GH_String>();
            string calculationPhase = "";

            GH_Structure<GH_Colour> componentColours = new GH_Structure<GH_Colour>();
            GH_Structure<GH_Number> componentPenrt = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> componentGwp = new GH_Structure<GH_Number>();

            DA.GetData(0, ref libPath);
            if (!DA.GetDataList(1, buildignSurfaces))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No building surfaces were provided.");
                return;
            }
            if (!DA.GetDataTree(2, out buildingComponent))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No components were provided.");
                return;
            }
            DA.GetData(3, ref calculationPhase);

            // Sanity check
            // Check if the folder path is valid
            string[] requiredFolders = { "Component", "Assembly", "Material" };
            try
            {
                // Get all subdirectories
                string[] subdirectories = Directory.GetDirectories(libPath);
                foreach (string requiredFolder in requiredFolders)
                {
                    // Check if the required folder is in the subdirectories
                    if (!subdirectories.Contains(libPath + "\\" + requiredFolder))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The folder path is not valid. The sub-folder '" + requiredFolder + "' is missing.");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "An error occurred: " + ex.Message);
            }


            // Generate the building component object
            BuildComponent buildComponent = new BuildComponent(buildignSurfaces, buildingComponent, 2);

            // Retrieve the boxes data tree with the ListBoxes method
            GH_Structure<GH_Box> ListBoxes = buildComponent.ListBoxes;

            // Material database to get the GWP next
            List<List<string>> matDatabase = ConvertJsonToMatDatabaseAlpha1(libPath);

            // Create a method to genereate the RGB colour data tree here
            //componentColours = buildComponent.GetRGB;

            // Indexes of material database. The index 0 for example gets values on the first column of the database.
            const int indexUmrechnungsfaktor = 3;
            const int indexPenrtA1ToA3 = 9;
            const int indexPenrtC3 = 10;
            const int indexPenrtC4 = 11;
            const int indexPenrtD1 = 12;
            const int indexGwpA1ToA3 = 13;
            const int indexGwpC3 = 14;
            const int indexGwpC4 = 15;
            const int indexGwpD1 = 16;
            const int indexMatIdDatabase = 0;

            // Index of material ID on component list. (The component datatree is filtered into a single list inside the GetMaterialData method).
            const int matIdIndexComponent = 3;

            // Get the GWP based on the selected phase
            if (calculationPhase == "A1ToA3")
            {
                componentPenrt = buildComponent.GetMaterialData(matDatabase, indexMatIdDatabase, indexPenrtA1ToA3, indexUmrechnungsfaktor, buildingComponent, matIdIndexComponent, ListBoxes);
                componentGwp = buildComponent.GetMaterialData(matDatabase, indexMatIdDatabase, indexGwpA1ToA3, indexUmrechnungsfaktor, buildingComponent, matIdIndexComponent, ListBoxes);
            }
            else if (calculationPhase == "C3")
            {
                componentPenrt = buildComponent.GetMaterialData(matDatabase, indexMatIdDatabase, indexPenrtC3, indexUmrechnungsfaktor, buildingComponent, matIdIndexComponent, ListBoxes);
                componentGwp = buildComponent.GetMaterialData(matDatabase, indexMatIdDatabase, indexGwpC3, indexUmrechnungsfaktor, buildingComponent, matIdIndexComponent, ListBoxes);
            }            
            else if (calculationPhase == "C4")
            {
                componentPenrt = buildComponent.GetMaterialData(matDatabase, indexMatIdDatabase, indexPenrtC4, indexUmrechnungsfaktor, buildingComponent, matIdIndexComponent, ListBoxes);
                componentGwp = buildComponent.GetMaterialData(matDatabase, indexMatIdDatabase, indexGwpC4, indexUmrechnungsfaktor, buildingComponent, matIdIndexComponent, ListBoxes);
            }
            else if (calculationPhase == "D1")
            {
                componentPenrt = buildComponent.GetMaterialData(matDatabase, indexMatIdDatabase, indexPenrtD1, indexUmrechnungsfaktor, buildingComponent, matIdIndexComponent, ListBoxes);
                componentGwp = buildComponent.GetMaterialData(matDatabase, indexMatIdDatabase, indexGwpD1, indexUmrechnungsfaktor, buildingComponent, matIdIndexComponent, ListBoxes);
            }
            else if (calculationPhase == "AToC")
            {
                GH_Structure<GH_Number> componentPenrtA1ToA3 = buildComponent.GetMaterialData(matDatabase, indexMatIdDatabase, indexPenrtA1ToA3, indexUmrechnungsfaktor, buildingComponent, matIdIndexComponent, ListBoxes);
                GH_Structure<GH_Number> componentPenrtC3 = buildComponent.GetMaterialData(matDatabase, indexMatIdDatabase, indexPenrtC3, indexUmrechnungsfaktor, buildingComponent, matIdIndexComponent, ListBoxes);
                GH_Structure<GH_Number> componentPenrtC4 = buildComponent.GetMaterialData(matDatabase, indexMatIdDatabase, indexPenrtC4, indexUmrechnungsfaktor, buildingComponent, matIdIndexComponent, ListBoxes);
                
                GH_Structure<GH_Number> componentGwpA1ToA3 = buildComponent.GetMaterialData(matDatabase, indexMatIdDatabase, indexGwpA1ToA3, indexUmrechnungsfaktor, buildingComponent, matIdIndexComponent, ListBoxes);
                GH_Structure<GH_Number> componentGwpC3 = buildComponent.GetMaterialData(matDatabase, indexMatIdDatabase, indexGwpC3, indexUmrechnungsfaktor, buildingComponent, matIdIndexComponent, ListBoxes);
                GH_Structure<GH_Number> componentGwpC4 = buildComponent.GetMaterialData(matDatabase, indexMatIdDatabase, indexGwpC4, indexUmrechnungsfaktor, buildingComponent, matIdIndexComponent, ListBoxes);
                foreach (GH_Path path in componentPenrtA1ToA3.Paths)
                {
                    for (int i = 0; i < componentPenrtA1ToA3.get_Branch(path).Count; i++)
                    {
                        double gH_tempPenrtA1ToA3 = 0;
                        GH_Convert.ToDouble(componentPenrtA1ToA3.get_Branch(path)[i], out gH_tempPenrtA1ToA3, GH_Conversion.Primary);
                        double gH_tempPenrtC3 = 0;
                        GH_Convert.ToDouble(componentPenrtC3.get_Branch(path)[i], out gH_tempPenrtC3, GH_Conversion.Primary);
                        double gH_tempPenrtC4 = 0;
                        GH_Convert.ToDouble(componentPenrtC4.get_Branch(path)[i], out gH_tempPenrtC4, GH_Conversion.Primary);
                        double totalSum = gH_tempPenrtA1ToA3 + gH_tempPenrtC3 + gH_tempPenrtC4;
                        GH_Number gH_totalSum = new GH_Number(totalSum);
                        componentPenrt.Append(gH_totalSum, path);
                    }
                    for (int i = 0; i < componentGwpA1ToA3.get_Branch(path).Count; i++)
                    {
                        double gH_tempGwpA1ToA3 = 0;
                        GH_Convert.ToDouble(componentGwpA1ToA3.get_Branch(path)[i], out gH_tempGwpA1ToA3, GH_Conversion.Primary);
                        double gH_tempGwpC3 = 0;
                        GH_Convert.ToDouble(componentGwpC3.get_Branch(path)[i], out gH_tempGwpC3, GH_Conversion.Primary);
                        double gH_tempGwpC4 = 0;
                        GH_Convert.ToDouble(componentGwpC4.get_Branch(path)[i], out gH_tempGwpC4, GH_Conversion.Primary);
                        double totalSum = gH_tempGwpA1ToA3 + gH_tempGwpC3 + gH_tempGwpC4;
                        GH_Number gH_totalSum = new GH_Number(totalSum);
                        componentGwp.Append(gH_totalSum, path);
                    }
                }
            }
            else if (calculationPhase == "AToD")
            {
                GH_Structure<GH_Number> componentPenrtA1ToA3 = buildComponent.GetMaterialData(matDatabase, indexMatIdDatabase, indexPenrtA1ToA3, indexUmrechnungsfaktor, buildingComponent, matIdIndexComponent, ListBoxes);
                GH_Structure<GH_Number> componentPenrtC3 = buildComponent.GetMaterialData(matDatabase, indexMatIdDatabase, indexPenrtC3, indexUmrechnungsfaktor, buildingComponent, matIdIndexComponent, ListBoxes);
                GH_Structure<GH_Number> componentPenrtC4 = buildComponent.GetMaterialData(matDatabase, indexMatIdDatabase, indexPenrtC4, indexUmrechnungsfaktor, buildingComponent, matIdIndexComponent, ListBoxes);
                GH_Structure<GH_Number> componentPenrtD1 = buildComponent.GetMaterialData(matDatabase, indexMatIdDatabase, indexPenrtD1, indexUmrechnungsfaktor, buildingComponent, matIdIndexComponent, ListBoxes);

                GH_Structure<GH_Number> componentGwpA1ToA3 = buildComponent.GetMaterialData(matDatabase, indexMatIdDatabase, indexGwpA1ToA3, indexUmrechnungsfaktor, buildingComponent, matIdIndexComponent, ListBoxes);
                GH_Structure<GH_Number> componentGwpC3 = buildComponent.GetMaterialData(matDatabase, indexMatIdDatabase, indexGwpC3, indexUmrechnungsfaktor, buildingComponent, matIdIndexComponent, ListBoxes);
                GH_Structure<GH_Number> componentGwpC4 = buildComponent.GetMaterialData(matDatabase, indexMatIdDatabase, indexGwpC4, indexUmrechnungsfaktor, buildingComponent, matIdIndexComponent, ListBoxes);
                GH_Structure<GH_Number> componentGwpD1 = buildComponent.GetMaterialData(matDatabase, indexMatIdDatabase, indexGwpD1, indexUmrechnungsfaktor, buildingComponent, matIdIndexComponent, ListBoxes);
                foreach (GH_Path path in componentPenrtA1ToA3.Paths)
                {
                    for (int i = 0; i < componentPenrtA1ToA3.get_Branch(path).Count; i++)
                    {
                        double gH_tempPenrtA1ToA3 = 0;
                        GH_Convert.ToDouble(componentPenrtA1ToA3.get_Branch(path)[i], out gH_tempPenrtA1ToA3, GH_Conversion.Primary);
                        double gH_tempPenrtC3 = 0;
                        GH_Convert.ToDouble(componentPenrtC3.get_Branch(path)[i], out gH_tempPenrtC3, GH_Conversion.Primary);
                        double gH_tempPenrtC4 = 0;
                        GH_Convert.ToDouble(componentPenrtC4.get_Branch(path)[i], out gH_tempPenrtC4, GH_Conversion.Primary);
                        double gH_tempPenrtD1 = 0;
                        GH_Convert.ToDouble(componentPenrtD1.get_Branch(path)[i], out gH_tempPenrtD1, GH_Conversion.Primary);
                        double totalSum = gH_tempPenrtA1ToA3 + gH_tempPenrtC3 + gH_tempPenrtC4 + gH_tempPenrtD1;
                        GH_Number gH_totalSum = new GH_Number(totalSum);
                        componentPenrt.Append(gH_totalSum, path);
                    }
                    for (int i = 0; i < componentGwpA1ToA3.get_Branch(path).Count; i++)
                    {
                        double gH_tempGwpA1ToA3 = 0;
                        GH_Convert.ToDouble(componentGwpA1ToA3.get_Branch(path)[i], out gH_tempGwpA1ToA3, GH_Conversion.Primary);
                        double gH_tempGwpC3 = 0;
                        GH_Convert.ToDouble(componentGwpC3.get_Branch(path)[i], out gH_tempGwpC3, GH_Conversion.Primary);
                        double gH_tempGwpC4 = 0;
                        GH_Convert.ToDouble(componentGwpC4.get_Branch(path)[i], out gH_tempGwpC4, GH_Conversion.Primary);
                        double gH_tempGwpD1 = 0;
                        GH_Convert.ToDouble(componentGwpD1.get_Branch(path)[i], out gH_tempGwpD1, GH_Conversion.Primary);
                        double totalSum = gH_tempGwpA1ToA3 + gH_tempGwpC3 + gH_tempGwpC4 + gH_tempGwpD1;
                        GH_Number gH_totalSum = new GH_Number(totalSum);
                        componentGwp.Append(gH_totalSum, path);
                    }
                }
            }
            else
            {
                // Error here
            }

            DA.SetDataTree(0, ListBoxes);
            DA.SetDataTree(1, componentPenrt);
            DA.SetDataTree(2, componentGwp);

            //Add a message to the bottom of the component
            //this.Component.Message = Surfaces.Count + " surfaces";

        }


        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);

            //Add Value List
            int[] stringID = new int[] { 3 };

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
                    "A1ToA3",
                    "C3",
                    "C4",
                    "D1",
                    "AToC",
                    "AToD"
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
            get { return new Guid("E3462118-3EFA-4313-839C-74276AD491DC"); }
        }
    }
}