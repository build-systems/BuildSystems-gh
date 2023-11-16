using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace BuildSystemsGH.Building.Define
{
    // This GH_Component has a hard coded list of building components separated by the BuildSystems template layers.
    // You can find the Rhino file with the layers in the repos/templates folder on Google Drive.
    // This is not optimal, but this is how the final component should work.
    // Ideally it would consider new layers in the open Rhino file and new components in the library.

    public class DefineBuildingComponents : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DefineBuildingComponents class.
        /// </summary>
        /// 

        public void CreateDropDownList(int[] stringID, string[] maualList, GH_Document document)
        {
            for (int i = 0; i < stringID.Length; i++)
            {
                Grasshopper.Kernel.Parameters.Param_String in0str = Params.Input[stringID[i]] as Grasshopper.Kernel.Parameters.Param_String;
                if (in0str == null || in0str.SourceCount > 0 || in0str.PersistentDataCount > 0) return;
                Attributes.PerformLayout();
                int x = (int)in0str.Attributes.Pivot.X - 200;
                int y = (int)in0str.Attributes.Pivot.Y - 11;
                Grasshopper.Kernel.Special.GH_ValueList valList = new Grasshopper.Kernel.Special.GH_ValueList();
                valList.CreateAttributes();
                valList.Attributes.Pivot = new PointF(x, y);
                valList.Attributes.ExpireLayout();
                valList.ListItems.Clear();

                List<Grasshopper.Kernel.Special.GH_ValueListItem> componentsAvailable = new List<Grasshopper.Kernel.Special.GH_ValueListItem>();
                foreach (string component in maualList)
                {
                    Grasshopper.Kernel.Special.GH_ValueListItem valueItem = new Grasshopper.Kernel.Special.GH_ValueListItem(component, '"' + component + '"');
                    componentsAvailable.Add(valueItem);
                }

                valList.ListItems.AddRange(componentsAvailable);
                document.AddObject(valList, false);
                in0str.AddSource(valList);
            }
        }


        public DefineBuildingComponents()
                  : base("Define Building Components", "DBC",
                      "Defines a component for each building layer.",
                      "BuildSystems", "LCA")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Außenwand nicht tragent", "Außenwand nt", "Außenwand nicht tragend (external wall non-structural).", GH_ParamAccess.item);
            pManager.AddTextParameter("Außenwand tragend", "Außenwand tr", "Außenwand tragend (external wall structural).", GH_ParamAccess.item);
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
            pManager.AddTextParameter("Wand Keller", "Wand Keller", "Wand Keller (wall in the basement).", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Building Components IDs", "IDs", "IDs of building components.", GH_ParamAccess.tree);
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
            string trenndecke = null;
            string trennwand = null;
            string deckeEG = null;
            string außenwandEG = null;
            string trennwandEG = null;
            string wandE = null;
            string deckeE = null;
            string bodenplatte = null;
            string wandKeller = null;
            if (!DA.GetData(0, ref außenwandNt)) return;
            if (!DA.GetData(1, ref außenwandTr)) return;
            if (!DA.GetData(2, ref balkon)) return;
            if (!DA.GetData(3, ref dachdecke)) return;
            if (!DA.GetData(4, ref innenwandNt)) return;
            if (!DA.GetData(5, ref trenndecke)) return;
            if (!DA.GetData(6, ref trennwand)) return;
            if (!DA.GetData(7, ref deckeEG)) return;
            if (!DA.GetData(8, ref außenwandEG)) return;
            if (!DA.GetData(9, ref trennwandEG)) return;
            if (!DA.GetData(10, ref wandE)) return;
            if (!DA.GetData(11, ref deckeE)) return;
            if (!DA.GetData(12, ref bodenplatte)) return;
            if (!DA.GetData(13, ref wandKeller)) return;

            GH_Structure<GH_String> buildingComponents = new GH_Structure<GH_String>();

            GH_String gH_außenwandNt = new GH_String(außenwandNt);
            buildingComponents.Append(gH_außenwandNt, new GH_Path(0));

            GH_String gH_außenwandTr = new GH_String(außenwandTr);
            buildingComponents.Append(gH_außenwandTr, new GH_Path(1));

            GH_String gH_balkon = new GH_String(balkon);
            buildingComponents.Append(gH_balkon, new GH_Path(2));

            GH_String gH_dachdecke = new GH_String(dachdecke);
            buildingComponents.Append(gH_dachdecke, new GH_Path(3));

            GH_String gH_innenwandNt = new GH_String(innenwandNt);
            buildingComponents.Append(gH_innenwandNt, new GH_Path(4));

            GH_String gH_trenndecke = new GH_String(trenndecke);
            buildingComponents.Append(gH_trenndecke, new GH_Path(5));

            GH_String gH_trennwand = new GH_String(trennwand);
            buildingComponents.Append(gH_trennwand, new GH_Path(6));

            GH_String gH_deckeEG = new GH_String(deckeEG);
            buildingComponents.Append(gH_deckeEG, new GH_Path(7));

            GH_String gH_außenwandEG = new GH_String(außenwandEG);
            buildingComponents.Append(gH_außenwandEG, new GH_Path(8));

            GH_String gH_trennwandEG = new GH_String(trennwandEG);
            buildingComponents.Append(gH_trennwandEG, new GH_Path(9));

            GH_String gH_wandE = new GH_String(wandE);
            buildingComponents.Append(gH_wandE, new GH_Path(10));

            GH_String gH_deckeE = new GH_String(deckeE);
            buildingComponents.Append(gH_deckeE, new GH_Path(11));

            GH_String gH_bodenplatte = new GH_String(bodenplatte);
            buildingComponents.Append(gH_bodenplatte, new GH_Path(12));

            GH_String gH_wandKeller = new GH_String(wandKeller);
            buildingComponents.Append(gH_wandKeller, new GH_Path(13));

            DA.SetDataTree(0, buildingComponents);
        }


        public override void AddedToDocument(GH_Document document)
        {
            // Find a way to add value list dynamically
            //Add Value List
            int[] außenwantNtID = new int[] { 0 };
            string[] außenwandNtList =
                {
                "AW-tr-HM-A",
                "AW-tr-HM-B",
                "AW-tr-HM-C",
                "AW-tr-HM-D",
                "AW-tr-HM-E",
                "AW-tr-TF-B",
                "AW-tr-TF-C",
                "AW-tr-TF-D",
                "AW-tr-TF-E",
                "AW-tr-STB-A",
                "AW-tr-STB-B",
                "AW-tr-STB-C",
                "AW-tr-STB-D",
                "AW-tr-STB-E",
                "AW-tr-MWZ-A",
                "AW-tr-MWZ-B",
                "AW-tr-MWZ-C",
                "AW-tr-MWKS-A",
                "AW-tr-MWKS-B",
                "AW-tr-MWKS-C",
                "AW-tr-MWKS-D",
                "AW-nt-TF-A"
                };

            int[] außenwandTrID = new int[] { 1 };
            string[] außenwandTrList =
                {
                "AW-tr-HM-A",
                "AW-tr-HM-B",
                "AW-tr-HM-C",
                "AW-tr-HM-D",
                "AW-tr-HM-E",
                "AW-tr-TF-B",
                "AW-tr-TF-C",
                "AW-tr-TF-D",
                "AW-tr-TF-E",
                "AW-tr-STB-A",
                "AW-tr-STB-B",
                "AW-tr-STB-C",
                "AW-tr-STB-D",
                "AW-tr-STB-E",
                "AW-tr-MWZ-A",
                "AW-tr-MWZ-B",
                "AW-tr-MWZ-C",
                "AW-tr-MWKS-A",
                "AW-tr-MWKS-B",
                "AW-tr-MWKS-C",
                "AW-tr-MWKS-D"
                };

            int[] balkonID = new int[] { 2 };
            string[] balkonList = { "TD-tr-STB-A" };

            int[] dachdeckeID = new int[] { 3 };
            string[] dachdeckeList =
                {
                "DD-tr-HM-A",
                "DD-tr-HM-B",
                "DD-tr-HM-C",
                "DD-tr-HT-A",
                "DD-tr-HT-B",
                "DD-tr-STB-A",
                "DD-tr-STB-B",
            };

            int[] innenwandNtID = new int[] { 4 };
            string[] innenwandNtList =
                {
                "IW-tr-HM-A",
                "IW-nt-HT-A",
                "IW-nt-HT-B",
                "IW-tr-STB",
                "IW-nt-MWZ-A",
                "IW-tr-MWKS-A",
                "IW-tr-MWKS-B",
                "IW-nt-MWKS-C",
                "IW-nt-LB-A",
                "IW-nt-LB-B"
            };

            int[] trenndeckeID = new int[] { 5 };
            string[] trenndeckeList =
                {
                "TD-tr-HM-A",
                "TD-tr-HM-B",
                "TD-tr-HM-C",
                "TD-tr-HT-A",
                "TD-tr-HT-B",
                "TD-tr-HBV-A",
                "TD-tr-HBV-B",
                "TD-tr-STB-A"
                };

            int[] trennwandID = new int[] { 6 };
            string[] trennwandList =
                {
                "TW-tr-HM-A",
                "TW-tr-HM-B",
                "TW-tr-HM-C",
                "TW-tr-HM-D",
                "TW-tr-HAT-A",
                "TW-tr-HAT-B",
                "TW-tr-HAT-C",
                "TW-tr-STB",
                "TW-tr-MWZ-A",
                "TW-tr-MWZ-B",
                "TW-tr-MWKS-A",
                "TW-tr-MWKS-B",
                "TW-nt-LB-A",
                "TW-nt-LB-B"
                };

            int[] deckeEgID = new int[] { 7 };
            string[] deckeEgList = { "TD-tr-STB-A" };

            int[] außenwandEGID = new int[] { 8 };
            string[] außenwandEGList =
                {
                "AW-tr-STB-A",
                "AW-tr-STB-B",
                "AW-tr-STB-C",
                "AW-tr-STB-D"
                };

            int[] trennwandEGID = new int[] { 9 };
            string[] trennwandEGList = { "TW-tr-STB" };

            int[] wandEID = new int[] { 10 };
            string[] wandEList = { "TW-tr-STB" };

            int[] deckeEID = new int[] { 11 };
            string[] deckeEList = { "TD-tr-STB-A" };

            int[] bodenplatteID = new int[] { 12 };
            string[] bodenplatteList = { "BP-tr-STB-A" };

            int[] wandKID = new int[] { 13 };
            string[] wandKList =
                {
                "KW-tr-STB-A",
                "KW-tr-MWZ-A",
                "KW-tr-MWKS-A"
                };

            CreateDropDownList(außenwantNtID, außenwandNtList, document);
            CreateDropDownList(außenwandTrID, außenwandTrList, document);
            CreateDropDownList(balkonID, balkonList, document);
            CreateDropDownList(dachdeckeID, dachdeckeList, document);
            CreateDropDownList(innenwandNtID, innenwandNtList, document);
            CreateDropDownList(trenndeckeID, trenndeckeList, document);
            CreateDropDownList(trennwandID, trennwandList, document);
            CreateDropDownList(deckeEgID, deckeEgList, document);
            CreateDropDownList(außenwandEGID, außenwandEGList, document);
            CreateDropDownList(trennwandEGID, trennwandEGList, document);
            CreateDropDownList(wandEID, wandEList, document);
            CreateDropDownList(deckeEID, deckeEList, document);
            CreateDropDownList(bodenplatteID, bodenplatteList, document);
            CreateDropDownList(wandKID, wandKList, document);


            base.AddedToDocument(document);
        }

        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Properties.Resources.DefineBuildingComponents;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("C723FD34-7C9F-428F-A345-A6F0D52351FD"); }
        }
    }
}