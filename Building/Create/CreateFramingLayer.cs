using System;
using System.Collections.Generic;
using BSoM.LCA.Layers.FramingParts;
using Grasshopper.Kernel;
using Rhino.Geometry;
using BSoM.LCA;
using BSoM.LCA.Layers;

namespace BuildSystemsGH.Building.Create
{
    public class CreateFramingLayer : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreateFramingLayer class.
        /// </summary>
        public CreateFramingLayer()
          : base("Create Framing Layer", "CFL",
              "Creates a Framing Layer, for ex. timber or steel framing.",
              "BuildSystems", "LCA")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // Frame material options
            pManager.AddGenericParameter("Frame material options", "Frame material", "A list of framing materials that could be used interchangeable, without altering the layer properties.", GH_ParamAccess.list);
            // Insulation material options
            pManager.AddGenericParameter("Insulation material options", "Insulation material", "A list of insulating materials that could be used interchangeable, without altering the layer properties.", GH_ParamAccess.list);
            // Frame thickness
            pManager.AddNumberParameter("Frame thickness", "Frame thickness", "Frame thickness in meters. This will dictate the total layer thickness.", GH_ParamAccess.item);
            // Insulation thickness
            pManager.AddNumberParameter("Insulation thickness", "Insulation thickness", "Insulation thickness in meters", GH_ParamAccess.item);
            // Width of framing
            pManager.AddNumberParameter("Frame Width", "Width", "Frame width in meters. The insulation width will be calculated automatically.", GH_ParamAccess.item, 0.1);
            // Spread usually 0.625m for timber framing
            pManager.AddNumberParameter("Spread", "Spread", "Framing spread distance in meters. The standard for timber is 0.625m.", GH_ParamAccess.item, 0.625);
            // Category
            pManager.AddTextParameter("Category", "Category", "Layer category", GH_ParamAccess.item);
            // Description
            pManager.AddTextParameter("Description", "Description", "Layer description", GH_ParamAccess.item);
            // Cost
            pManager.AddNumberParameter("Cost", "Cost", "Layer description", GH_ParamAccess.item);
            
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
            pManager[8].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("BSoM Framing Layer", "Layer", "Framing layer as a BSoM - BuildSystems object Model", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Material> framingOptions = new List<Material>();
            List<Material> insulationOptions = new List<Material>();
            double framingThickness = 0.0;
            double insulationThickness = 0.0;
            double width = 0.0;
            double spread = 0.0;
            string category = string.Empty;
            string desctription = string.Empty;
            double cost = 0;

            if (!DA.GetDataList(0, framingOptions)) return;
            if (!DA.GetDataList(1, insulationOptions)) return;
            if (!DA.GetData(2, ref framingThickness)) return;
            if (!DA.GetData(3, ref insulationThickness)) return;
            if (!DA.GetData(4, ref width)) return;
            if (!DA.GetData(5, ref spread)) return;
            DA.GetData(6, ref category);
            DA.GetData(7, ref desctription);
            DA.GetData(8, ref cost);

            // Create a Frame
            Frame frame = new Frame(framingOptions, framingThickness, width, spread);
            // Create a Layer
            Insulation insulation = new Insulation(insulationOptions, insulationThickness);
            // Create a framing layer object (need a constructor with basic info)
            FramingLayer framingLayer = new FramingLayer(frame, insulation);
            // Add optional inputs
            framingLayer.Category = category;
            framingLayer.Description = desctription;
            framingLayer.Cost = cost;

            // Set data
            DA.SetData(0, framingLayer);
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
            get { return new Guid("B3092D17-63D2-4B34-996C-45AB20EAC7EF"); }
        }
    }
}