﻿using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Attributes;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;
using Grasshopper.Kernel;
using Grasshopper;
using Rhino.Geometry;
using System.Windows.Forms;
using Grasshopper.Kernel.Types;
using GsaAPI;
using GhSA.Parameters;


namespace GhSA.Components
{
    /// <summary>
    /// Component to retrieve non-geometric objects from a GSA model
    /// </summary>
    public class GetProperties : GH_Component
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("fa497db7-8bdd-438d-888f-83a85d6cd48a");
        public GetProperties()
          : base("Get Model Properties", "GetProps", "Get Sections, 2D Properties and Springs from GSA model",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat0())
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        //protected override Bitmap Icon => Resources.CrossSections;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        #endregion

        #region Input and output

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("GSA Model", "GSA", "GSA model containing some properties", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Sections", "PB", "Nodes from GSA Model", GH_ParamAccess.list);
            pManager.AddGenericParameter("2D Properties", "PA", "GSA 1D Elements from GSA Model", GH_ParamAccess.list);
            pManager.AddGenericParameter("Springs", "PS", "GSA 2D Elements from GSA Model", GH_ParamAccess.list);
        }
        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GsaModel gsaModel = new GsaModel();
            if (DA.GetData(0, ref gsaModel))
            {
                Model model = new Model();
                model = gsaModel.Model;


                List<GsaSection> sections = Util.Gsa.GsaImport.GsaGetSections(model);
                List<GsaProp2d> prop2Ds = Util.Gsa.GsaImport.GsaGetProp2ds(model);
                // spring import missing in GsaAPI. To be implemented

                DA.SetDataList(0, sections);
                DA.SetDataList(1, prop2Ds);
            }
        }
    }
}

