using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

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

            public BuildComponent(List<Surface> surfaces, GH_Structure<GH_String> component)
            {
                /////// Loop through each surfaces ///////
                for (int i = 0; i < surfaces.Count; i++)
                {
                    // Get brep
                    // Initialize empty brep
                    Surface surface = surfaces[i];

                    // Initialize current height
                    double currentHeight = 0;

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

                    // Initialize trimmed curves for insulation
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
                        // Get current path & current material level
                        GH_Path path = iListPaths[j];
                        string layerLevel = "";
                        GH_Convert.ToString(component.get_Branch(path)[1], out layerLevel, GH_Conversion.Primary);
                        char lastlayerLevelChar = layerLevel[layerLevel.Length - 1];
                        int subLayerLevel = Convert.ToInt32(lastlayerLevelChar.ToString());
                        string materialLevelString = "";
                        GH_Convert.ToString(component.get_Branch(path)[2], out materialLevelString, GH_Conversion.Primary);
                        int materialLevel = Convert.ToInt32(materialLevelString);

                        // Get next path & next material level
                        int nextMaterialLevel = new int();
                        if (j != iListPaths.Count - 1)
                        {
                            GH_Path nextPath = iListPaths[j + 1];
                            string nextMaterialLevelString = "";
                            GH_Convert.ToString(component.get_Branch(nextPath)[2], out nextMaterialLevelString, GH_Conversion.Primary);
                            nextMaterialLevel = Convert.ToInt32(nextMaterialLevelString);
                        }

                        // Get thickness from component data tree
                        string thicknessAsString = "";
                        GH_Convert.ToString(component.get_Branch(path)[7], out thicknessAsString, GH_Conversion.Primary);
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

                        // change plane origin based on current height
                        brepPlane.Origin = center + dir;

                        if (materialLevel == 1)
                        {
                            if (nextMaterialLevel != 2) // Do not consider the sub-assembly item in the data tree
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
                        }

                        /////// MaterialLevel 2 ///////
                        // This part takes care of the materials that overlap
                        else
                        {
                            /////// Calculation for framing ///////
                            // There is always a framing material on the first layer
                            if (subLayerLevel == 1)
                            {
                                // Get the frame width and spread from the component data tree
                                string frameWidthString = "";
                                GH_Convert.ToString(component.get_Branch(path)[8], out frameWidthString, GH_Conversion.Primary);
                                frameWidth = Convert.ToDouble(frameWidthString.Replace(",", "."));

                                string spreadString = "";
                                GH_Convert.ToString(component.get_Branch(path)[9], out spreadString, GH_Conversion.Primary);
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
                                // Reverse the paramEnd array to match the paramStart array
                                Array.Reverse(paramEnd);

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
                            }
                            /////// Calculation for insulation ///////
                            else
                            {
                                // Get the insulation width
                                double insulationWidth = (mainCurve.GetLength() - ((spreadNumber + 1) * frameWidth)) / spreadNumber;

                                // Get spread from the component data tree
                                string spreadString = "";
                                GH_Convert.ToString(component.get_Branch(path)[9], out spreadString, GH_Conversion.Primary);
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
                                // First find the insulationPlanes list
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
                                currentHeight += thickness;
                            }
                        }
                    }

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

            // GWP A1ToA3 method
            public GH_Structure<GH_Number> GetGWPA1ToA3(List<List<string>> matDatabase, GH_Structure<GH_Box> ListBoxes, GH_Structure<GH_String> component)
            {

                // Get GH_Path from ListBoxes
                IList<GH_Path> boxesPaths = ListBoxes.Paths;
                GH_Structure<GH_Number> panelGwpA1ToA3 = new GH_Structure<GH_Number>();
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
                            string matID = "";
                            GH_Convert.ToString(component.get_Branch(compPath)[3], out matID, GH_Conversion.Primary);

                            // Loop through the material database and match the material ID
                            for (int j = 0; j < matDatabase.Count; j++)
                            {
                                // Get the material ID
                                string matIDDatabase = matDatabase[j][1];

                                // Check if the material ID matches
                                if (matID == matIDDatabase)
                                {
                                    // Get the GWP A1ToA3 value
                                    string gwpA1ToA3String = matDatabase[j][13];
                                    if (gwpA1ToA3String == "")
                                    {
                                        gwpA1ToA3String = "0";
                                    }
                                    double unitGwpA1ToA3 = Convert.ToDouble(gwpA1ToA3String);

                                    // Calculate the GWP A1ToA3 value
                                    double gwpA1ToA3 = unitGwpA1ToA3 * matVolume;
                                    GH_Number gH_Number = new GH_Number(gwpA1ToA3);
                                    panelGwpA1ToA3.Append(gH_Number, panelPath);
                                }
                            }
                        }
                    }
                }
                return panelGwpA1ToA3;

            }

        }

        public AssembleComponent()
          : base("Assemble Component", "AC",
              "Assemble the component layers.",
              "Build Systems", "Components")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Folder Path", "Path", "Root folder containing the three other folders with the JSON libraries.", GH_ParamAccess.item);
            pManager.AddSurfaceParameter("Building Surfaces", "Surfaces", "Building surfaces to generate the bateil.", GH_ParamAccess.list);
            pManager.AddTextParameter("Component Layers Tree", "Component", "Component layers in the format of GH Data Tree.", GH_ParamAccess.tree);
            // Here could be a value list that is filled automatically
            pManager.AddTextParameter("Calculation Phase", "Phase", "Phases to calculate the GWP (A1ToA3, C3, C4, D1, AToC, AToD). Default is A1ToA3.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBoxParameter("Materials as Boxes", "Boxes", "Representation of materials as Boxes.", GH_ParamAccess.tree);
            pManager.AddColourParameter("Materials' Color RGB", "RGB", "Materials' colors RGB for the Boxes.", GH_ParamAccess.tree);
            pManager.AddTextParameter("Selected Phase PENRT", "PENRT", "PENRT calculated using the material volumes from the Boxes [MJ].", GH_ParamAccess.tree);
            pManager.AddTextParameter("Selected Phase GWP", "GWP", "GWP calculated using the material volumes from the Boxes [kg CO2-eq].", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string filePath = "";
            List<Surface> buildignSurfaces = new List<Surface>();
            GH_Structure<GH_String> buildingComponent = new GH_Structure<GH_String>();
            string calculationPhase = "A1ToA3";

            GH_Structure<GH_Colour> componentColours = new GH_Structure<GH_Colour>();
            GH_Structure<GH_Number> componentPENRT = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> componentGWP = new GH_Structure<GH_Number>();

            if (!DA.GetData(0, ref filePath)) return;
            if (!DA.GetDataList(1, buildignSurfaces)) return;
            if (!DA.GetDataTree(2, out buildingComponent)) return;
            DA.GetData(3, ref calculationPhase);

            // Generate the building component object
            BuildComponent buildComponent = new BuildComponent(buildignSurfaces, buildingComponent);

            // Retrieve the boxes data tree with the ListBoxes method
            GH_Structure<GH_Box> ListBoxes = buildComponent.ListBoxes;

            // Material database to get the GWP next
            List<List<string>> matDatabase = buildComponent.ConvertDatabase(filePath);

            // Create a method to genereate the RGB colour data tree here
            //componentColours = buildComponent.GetRGB;

            // Get the GWP based on the selected phase
            if (calculationPhase == "A1ToA3")
            {
                //componentPENRT = buildComponent.GetPENRTA1ToA3(matDatabase, ListBoxes, Component);
                componentGWP = buildComponent.GetGWPA1ToA3(matDatabase, ListBoxes, buildingComponent);
            }
            else if (calculationPhase == "C3")
            {
                //componentPENRT = buildComponent.GetPENRTC3(matDatabase, ListBoxes, Component);
                //componentGWP = buildComponent.GetGWPC3(matDatabase, ListBoxes, buildComponent);
            }
            else if (calculationPhase == "D1")
            {
                //componentPENRT = buildComponent.GetPENRTD1(matDatabase, ListBoxes, Component);
                //componentGWP = buildComponent.GetGWPD1(matDatabase, ListBoxes, buildComponent);
            }
            else if (calculationPhase == "AToC")
            {
                //componentPENRT = buildComponent.GetPENRTAToC(matDatabase, ListBoxes, Component);
                //componentGWP = buildComponent.GetGWPAToC(matDatabase, ListBoxes, buildComponent);
            }
            else if (calculationPhase == "AToD")
            {

                //componentPENRT = buildComponent.GetPENRTAToD(matDatabase, ListBoxes, Component);
                //componentGWP = buildComponent.GetGWPAToD(matDatabase, ListBoxes, buildComponent);
            }
            else
            {
                // Error here
            }

            DA.SetDataTree(0, ListBoxes);
            DA.SetDataTree(1, componentColours);
            DA.SetDataTree(2, componentPENRT);
            DA.SetDataTree(3, componentGWP);

            //Add a message to the bottom of the component
            //this.Component.Message = Surfaces.Count + " surfaces";

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