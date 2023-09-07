using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace BuildSystemsGH.Components.Building.Create
{

    // This GH_Component is a placeholder.
    // It will get a super, main and sub assembly values and simply merging them and adding specific component values.
    // It can use iputs from the json library or from the user.

    public class CreateComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreateComponent class.
        /// </summary>
        public CreateComponent()
          : base("Create Component", "CC",
              "Description",
              "BuildSystems", "LCA")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Properties.Resources.CreateComponent;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("A800D633-13C6-414A-88B3-DD55282A5D46"); }
        }
    }
}