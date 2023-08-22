using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace BuildSystemsGH.Components
{
    public class QuickLCA : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public QuickLCA()
          : base("Quick LCA", "QLCA",
              "Uses the average PENRT and GWP per square meter to calculate the LCA.",
              "BuildSystems", "LCA")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Building Surfaces", "Surfaces", "Building surfaces to generate the bateil.", GH_ParamAccess.list);
            pManager.AddTextParameter("Building Components IDs", "IDs", "IDs of building components.", GH_ParamAccess.list);
            pManager.AddTextParameter("Building Layer Names", "Layers", "Names of building layers.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("PENRT", "PENRT", "Primary energy non-renewable total.", GH_ParamAccess.item);
            pManager.AddNumberParameter("GWP", "GWP", "Global warming potential.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
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
            get { return new Guid("7A5BB0CD-9768-4F5F-B13E-B08F1BCFDE31"); }
        }
    }
}