using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Collections;
using Rhino.Geometry.Collections;

namespace BuildSystemsGH.Components.Building.Utilities
{
    public class GetMainSurface : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        /// 

        public Brep GetMiddleSurface(Brep brep)
        {
            List<Brep> faces = new List<Brep>();

            // Shrink trimmed surfaces
            BrepFaceList brepFacesList = brep.Faces;
            brepFacesList.ShrinkFaces();

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

        public GetMainSurface()
          : base("Get Main Surface", "GMS",
              "Gets the main surfarce from a brep.",
              "BuildSystems", "LCA")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Breps", "Brep", "Brep to get the main surface from.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Surface", "Surface", "Main surface from the brep.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Brep> breps = new List<Brep>();

            if (!DA.GetDataList(0, breps)) return;

            List<Brep> surfaces = new List<Brep>();

            foreach (Brep brep in breps)
            {
                surfaces.Add(GetMiddleSurface(brep));
            }

            DA.SetDataList(0, surfaces);
        }

        public override GH_Exposure Exposure => GH_Exposure.quinary;

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GetMainSurface;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("DCF50B10-8744-4556-A5A5-3D7554C7AAD6"); }
        }
    }
}