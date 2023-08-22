using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace BuildSystemsGH.Components
{
    public class DefineBuildingLayers : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public DefineBuildingLayers()
          : base("Define Building Layers", "DBL",
              "If the Rhino layers are not organized using the BuildSystem standard, then they need to be mapped here.",
              "BuildSystems", "LCA")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Außenwant nicht tragent", "Außenwant nt", "Außenwand nicht tragend (external wall non-structural).", GH_ParamAccess.item);
            pManager.AddTextParameter("Außenwand tragend", "Außenwand t", "Außenwand tragend (external wall structural).", GH_ParamAccess.item);
            pManager.AddTextParameter("Balkon", "Balkon", "Balkon (balcony).", GH_ParamAccess.item);
            pManager.AddTextParameter("Dachdecke", "Dachdecke", "Dachdecke (roofing).", GH_ParamAccess.item);
            pManager.AddTextParameter("Innenwand nicht tragend", "Innenwand nt", "Innenwand nicht tragend (internal wall non-structural).", GH_ParamAccess.item);
            pManager.AddTextParameter("Trenndecke", "Trenndecke", "Trenndecke (partition floor).", GH_ParamAccess.item);
            pManager.AddTextParameter("Trennwand", "Trennwand", "Trennwand (partition wall).", GH_ParamAccess.item);
            pManager.AddTextParameter("Decke über EG", "Decke EG", "Decke über Erdgeschoss (floor above the first floor).", GH_ParamAccess.item);
            pManager.AddTextParameter("Außenwand EG", "Außenwand EG", "Außenwand Erdgeschoss (external wall on the first floor).", GH_ParamAccess.item);
            pManager.AddTextParameter("Trennwand EG", "Trennwand EG", "Trennwand Erdgeschoss (partition wall on the first floor).", GH_ParamAccess.item);
            pManager.AddTextParameter("Wand Erschließung", "Wand E", "Wand Erschließung (wall for circulation).", GH_ParamAccess.item);
            pManager.AddTextParameter("Decke Erschließung", "Decke E", "Decke Erschließung (floor for circulation).", GH_ParamAccess.item);
            pManager.AddTextParameter("Bodenplatte", "Bodenplatte", "Bodenplatte (ground floor).", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Building Layers", "Layers", "Layers grouping building components.", GH_ParamAccess.list);
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
            get { return new Guid("5DB5101E-2ACA-4C49-82D2-C84D52595026"); }
        }
    }
}