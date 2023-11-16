using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using BSoM.LCA;

namespace BuildSystemsGH.Libraries
{
    public class DeconstructMaterial : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public DeconstructMaterial()
          : base("Analise material", "AM",
              "Deconstruct the material object into its parts",
              "BuildSystems", "LCA")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("BSoM material", "Material", "Material representation as BuildSystems object model", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("ID", "ID", "Material ID", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "Name", "Material name", GH_ParamAccess.item);
            pManager.AddTextParameter("Unit", "Unit", "Material unit", GH_ParamAccess.item);
            pManager.AddTextParameter("Weight", "Weight", "Material weight in kg/unit", GH_ParamAccess.item);
            pManager.AddTextParameter("Density", "Density", "Material density in kg/m3", GH_ParamAccess.item);
            pManager.AddTextParameter("Area density", "A-Density", "Material area density in kg/m2", GH_ParamAccess.item);
            pManager.AddTextParameter("Linear density", "L-Density", "Material linear density in kg/m", GH_ParamAccess.item);
            pManager.AddTextParameter("Conversion factor", "Factor", "Material conversion factor for m3", GH_ParamAccess.item);
            pManager.AddTextParameter("Conversion factor per kg", "FactorKg", "Material conversion factor for kg", GH_ParamAccess.item);
            pManager.AddTextParameter("PENRT A1-A3", "P A1-A3", "Energy consumption during phases A1 to A3 in MJ/m3", GH_ParamAccess.item);
            pManager.AddTextParameter("PENRT C3", "P C3", "Energy consumption during phase C3 in MJ/m3", GH_ParamAccess.item);
            pManager.AddTextParameter("PENRT C4", "P C4", "Energy consumption during phase C4 in MJ/m3", GH_ParamAccess.item);
            pManager.AddTextParameter("PENRT D1", "P D1", "Energy consumption during phase D1 in MJ/m3", GH_ParamAccess.item);
            pManager.AddTextParameter("GWP A1-A3", "G A1-A3", "Carbon consumption during phases A1 to A3 in CO2eq/m3", GH_ParamAccess.item);
            pManager.AddTextParameter("GWP C3", "G C3", "Carbon consumption during phase C3 in CO2eq/m3", GH_ParamAccess.item);
            pManager.AddTextParameter("GWP C4", "G C4", "Carbon consumption during phase C4 in CO2eq/m3", GH_ParamAccess.item);
            pManager.AddTextParameter("GWP D1", "G D1", "Carbon consumption during phase D1 in CO2eq/m3", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Material material = null;
            DA.GetData(0, ref material);

            DA.SetData(0, new GH_String(material.ID));
            DA.SetData(1, new GH_String(material.Name));
            DA.SetData(2, new GH_String(material.Unit));
            DA.SetData(3, new GH_Number(material.Weight));
            DA.SetData(4, new GH_Number(material.Density));
            DA.SetData(5, new GH_Number(material.DensityArea));
            DA.SetData(6, new GH_Number(material.DensityLinear));
            DA.SetData(7, new GH_Number(material.ConversionFactor));
            DA.SetData(8, new GH_Number(material.ConversionFactorKg));
            DA.SetData(9, new GH_Number(material.PENRT_A1ToA3));
            DA.SetData(10, new GH_Number(material.PENRT_C3));
            DA.SetData(11, new GH_Number(material.PENRT_C4));
            DA.SetData(12, new GH_Number(material.PENRT_D1));
            DA.SetData(13, new GH_Number(material.GWP_A1ToA3));
            DA.SetData(14, new GH_Number(material.GWP_C3));
            DA.SetData(15, new GH_Number(material.GWP_C4));
            DA.SetData(16, new GH_Number(material.GWP_D1));
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
            get { return new Guid("CBAE26D3-C126-46CC-AAFD-DED0E9227170"); }
        }
    }
}