using System;
using System.Collections.Generic;
using BSoM.LCA;
using BSoM.LCA.Layers;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace BuildSystemsGH.Building.Create
{
    // This GH_Component is a placeholder.
    // It will take materials from the json library and add layer information to it, like thickness, spread and so on.

    public class CreateSolidLayer : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public CreateSolidLayer()
          : base("Create Layer", "CL",
              "Description",
              "BuildSystems", "LCA")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("BSoM Materials", "Materials", "Material options from BSoM (BuildSystems object Model).", GH_ParamAccess.list);
            pManager.AddNumberParameter("Thickness", "Thickness", "Layer thickness in meters.", GH_ParamAccess.item);
            pManager.AddTextParameter("Category", "Category", "Layer category ex. Timber.", GH_ParamAccess.item);
            pManager.AddTextParameter("Description", "Desctription", "Layer description ex. Insulation.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Cost", "Cost", "Layer cost per squared meters.", GH_ParamAccess.item);

            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("BSoM Solid Layer", "Layer", "Solid layer as a BSoM - BuildSystems object Model", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Material> materialOptions = new List<Material>();
            double thickness = 0.0;
            string category = "";
            string desctription = string.Empty;
            double cost = 0;

            if (!DA.GetDataList(0, materialOptions)) return;
            if (!DA.GetData(1, ref thickness)) return;
            DA.GetData(2, ref category);
            DA.GetData(3, ref desctription);
            DA.GetData(4, ref cost);

            SolidLayer layer = new SolidLayer();
            foreach (Material mat in  materialOptions)
            {
                layer.AddMaterialOption(mat);
            }
            layer.Category = category;
            layer.Description = desctription;
            layer.Thickness = thickness;
            layer.Cost = cost;

            DA.SetData(0, layer);
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.CreateLayer;


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("5169DCB4-ED12-4DE2-853F-C54132200410"); }
        }
    }
}