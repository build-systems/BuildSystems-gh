using System;
using System.Collections.Generic;
using BSoM.LCA.Layers;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace BuildSystemsGH.Building.Analise
{
    public class AnaliseSolidLayer : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the AnaliseSolidLayer class.
        /// </summary>
        public AnaliseSolidLayer()
          : base("Analise Layer", "AL",
              "Analise BSoM Layer (deconstruct it).",
              "BuildSystems", "LCA")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("BSoM Layer", "Layer", "BuildSystems Layer object.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // Update first to accept a list
            pManager.AddGenericParameter("BSoM Material Options", "Material", "List of possible material options for this layer.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Thickness", "Thickness", "Layer thickness in meters.", GH_ParamAccess.item);
            pManager.AddTextParameter("Category", "Category", "Layer category ex. Timber.", GH_ParamAccess.item);
            pManager.AddTextParameter("Description", "Desctription", "Layer description ex. Insulation.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Cost", "Cost", "Layer cost per squared meters.", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            SolidLayer layer = new SolidLayer();
            if (!DA.GetData(0, ref layer)) return;

            // Convert dictionary into a list, ignoring the option tag
            List<BSoM.LCA.Material> options = new List<BSoM.LCA.Material> (layer.MaterialOptions.Values);

            // Update later to list
            DA.SetDataList(0, options);
            DA.SetData(1, layer.Thickness);
            DA.SetData(2, layer.Category);
            DA.SetData(3, layer.Description);
            DA.SetData(4, layer.Cost);
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
            get { return new Guid("72C4797D-6318-408F-AF23-E2F37E272E7C"); }
        }
    }
}