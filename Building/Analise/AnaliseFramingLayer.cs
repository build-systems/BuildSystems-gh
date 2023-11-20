using System;
using System.Collections.Generic;
using BSoM.LCA.Layers;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using BSoM.LCA;

namespace BuildSystemsGH.Building.Analise
{
    public class AnaliseSolidLayer : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the AnaliseSolidLayer class.
        /// </summary>
        public AnaliseSolidLayer()
          : base("Analise Framing Layer", "AFL",
              "Analise BSoM Framing Layer (deconstruct it).",
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
            pManager.AddGenericParameter("Frame Material Options", "Frame Materials", "List of possible material options for this layer.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Insulation Material Options", "Insulation Materials", "List of possible material options for this layer.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Frame thickness", "Frame thickness", "Frame thickness in meters.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Insulation thickness", "Insulation thickness", "Insulation thickness in meters.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Frame width", "Frame width", "Frame width in meters.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Frame spread", "Frame spread", "Frame width in meters.", GH_ParamAccess.item);
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
            FramingLayer layer = null;
            if (!DA.GetData(0, ref layer)) return;

            // Convert dictionary into a list, ignoring the option tag
            List<Material> optionsFraming = new List<Material>(layer.Frame.MaterialOptions.Values);
            List<Material> optionsInsulation = new List<Material>(layer.Insulation.MaterialOptions.Values);


            // Convert dictionary into a list, ignoring the option tag

            // Frame options
            DA.SetDataList(0, optionsFraming);
            // Insulation options
            DA.SetDataList(1, optionsInsulation);
            // Frame thickness
            DA.SetData(2, layer.Frame.Thickness);
            // Insulation thickness
            DA.SetData(3, layer.Insulation.Thickness);
            // Frame width
            DA.SetData(4, layer.Frame.Width);
            // Frame spread
            DA.SetData(5, layer.Frame.Spread);
            // Category
            DA.SetData(6, layer.Category);
            // Description
            DA.SetData(7, layer.Description);
            // Cost
            DA.SetData(8, layer.Cost);
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
            get { return new Guid("d748bfb1-2439-4797-b27b-badbe703a895"); }
        }
    }
}