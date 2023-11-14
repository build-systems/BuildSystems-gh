using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace BuildSystemsGH.Building.Define
{
    public class DefineBuildingLayers : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        /// 


        public void CreateDropDownList(int[] stringID, string[] maualList, GH_Document document)
        {
            for (int i = 0; i < stringID.Length; i++)
            {
                Grasshopper.Kernel.Parameters.Param_String in0str = Params.Input[stringID[i]] as Grasshopper.Kernel.Parameters.Param_String;
                if (in0str == null || in0str.SourceCount > 0 || in0str.PersistentDataCount > 0) return;
                Attributes.PerformLayout();
                int x = (int)in0str.Attributes.Pivot.X - 500;
                int y = (int)in0str.Attributes.Pivot.Y - 11;
                GH_ValueList valList = new GH_ValueList();
                valList.CreateAttributes();
                valList.Attributes.Pivot = new PointF(x, y);
                valList.Attributes.ExpireLayout();
                valList.ListItems.Clear();

                List<GH_ValueListItem> componentsAvailable = new List<GH_ValueListItem>();
                foreach (string component in maualList)
                {
                    GH_ValueListItem valueItem = new GH_ValueListItem(component, '"' + component + '"');
                    componentsAvailable.Add(valueItem);
                }

                valList.ListItems.AddRange(componentsAvailable);
                document.AddObject(valList, false);
                in0str.AddSource(valList);
            }
        }


        public DefineBuildingLayers()
          : base("Define Building Layers", "DBL",
              "If the Rhino layers are not organized using the BuildSystem standard, then they need to be mapped here.",
              "BuildSystems", "LCA")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Außenwant nicht tragent", "Außenwant nt", "Außenwand nicht tragend (external wall non-structural).", GH_ParamAccess.item);
            pManager.AddTextParameter("Außenwand tragend", "Außenwand tr", "Außenwand tragend (external wall structural).", GH_ParamAccess.item);
            pManager.AddTextParameter("Balkon", "Balkon", "Balkon (balcony).", GH_ParamAccess.item);
            pManager.AddTextParameter("Dachdecke", "Dachdecke", "Dachdecke (roofing).", GH_ParamAccess.item);
            pManager.AddTextParameter("Innenwand nicht tragend", "Innenwand nt", "Innenwand nicht tragend (internal wall non-structural).", GH_ParamAccess.item);
            pManager.AddTextParameter("Trenndecke OG", "Trenndecke", "Trenndecke (partition floor).", GH_ParamAccess.item);
            pManager.AddTextParameter("Trennwand OG", "Trennwand", "Trennwand (partition wall).", GH_ParamAccess.item);
            pManager.AddTextParameter("Decke über EG", "Decke EG", "Decke über Erdgeschoss (floor above the first floor).", GH_ParamAccess.item);
            pManager.AddTextParameter("Außenwand EG", "Außenwand EG", "Außenwand Erdgeschoss (external wall on the first floor).", GH_ParamAccess.item);
            pManager.AddTextParameter("Trennwand EG", "Trennwand EG", "Trennwand Erdgeschoss (partition wall on the first floor).", GH_ParamAccess.item);
            pManager.AddTextParameter("Wand Erschließung", "Wand E", "Wand Erschließung (wall for circulation).", GH_ParamAccess.item);
            pManager.AddTextParameter("Decke Erschließung", "Decke E", "Decke Erschließung (floor for circulation).", GH_ParamAccess.item);
            pManager.AddTextParameter("Bodenplatte", "Bodenplatte", "Bodenplatte (ground floor).", GH_ParamAccess.item);
            pManager.AddTextParameter("Wand Keller", "Wand Keller", "Wand Keller (wall in the basement).", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Building Layers", "Layers", "Layers grouping building components.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string außenwandNt = null;
            string außenwandTr = null;
            string balkon = null;
            string dachdecke = null;
            string innenwandNt = null;
            string trenndeckeOG = null;
            string trennwandOG = null;
            string deckeEG = null;
            string außenwandEG = null;
            string trennwandEG = null;
            string wandE = null;
            string deckeE = null;
            string bodenplatte = null;
            string wandKeller = null;
            DA.GetData(0, ref außenwandNt);
            DA.GetData(1, ref außenwandTr);
            DA.GetData(2, ref balkon);
            DA.GetData(3, ref dachdecke);
            DA.GetData(4, ref innenwandNt);
            DA.GetData(5, ref trenndeckeOG);
            DA.GetData(6, ref trennwandOG);
            DA.GetData(7, ref deckeEG);
            DA.GetData(8, ref außenwandEG);
            DA.GetData(9, ref trennwandEG);
            DA.GetData(10, ref wandE);
            DA.GetData(11, ref deckeE);
            DA.GetData(12, ref bodenplatte);
            DA.GetData(13, ref wandKeller);

            List<string> buildingLayers = new List<string>();
            buildingLayers.Add(außenwandNt);
            buildingLayers.Add(außenwandTr);
            buildingLayers.Add(balkon);
            buildingLayers.Add(dachdecke);
            buildingLayers.Add(innenwandNt);
            buildingLayers.Add(trenndeckeOG);
            buildingLayers.Add(trennwandOG);
            buildingLayers.Add(deckeEG);
            buildingLayers.Add(außenwandEG);
            buildingLayers.Add(trennwandEG);
            buildingLayers.Add(wandE);
            buildingLayers.Add(deckeE);
            buildingLayers.Add(bodenplatte);
            buildingLayers.Add(wandKeller);

            DA.SetDataList(0, buildingLayers);
        }


        public override void AddedToDocument(GH_Document document)
        {
            // Find a way to add value list dynamically
            //Add Value List

            List<string> layerNames = new List<string>();
            foreach (Layer layer in Rhino.RhinoDoc.ActiveDoc.Layers)
            {
                if (!layer.IsDeleted)
                {
                    layerNames.Add(layer.Name);
                }
            }

            for (int i = 0; i < 14; i++)
            {
                int[] stringID = new int[] { i };
                string[] maualList = layerNames.ToArray();
                CreateDropDownList(stringID, maualList, document);
            }


            base.AddedToDocument(document);
        }


        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Properties.Resources.DefineBuildingLayers;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("5DB5101E-2ACA-4C49-82D2-C84D52595026"); }
        }
    }
}