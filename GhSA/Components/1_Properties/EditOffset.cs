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
using System.Resources;

namespace GhSA.Components
{
    /// <summary>
    /// Component to edit an Offset and ouput the information
    /// </summary>
    public class EditOffset : GH_Component
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("1e094fcd-8f5f-4047-983c-e0e57a83ae52");
        public EditOffset()
          : base("Edit Offset", "OffsetEdit", "Modify GSA Offset or just get information about existing",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat1())
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;

        protected override System.Drawing.Bitmap Icon => GSA.Properties.Resources.EditOffset;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout

        #endregion

        #region Input and output

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Offset", "Off", "GSA Offset", GH_ParamAccess.item);
            pManager.AddNumberParameter("Offset X1", "X1", "Set X1 - Start axial offset (" + Util.GsaUnit.LengthLarge + ")", GH_ParamAccess.item);
            pManager.AddNumberParameter("Offset X2", "X2", "Set X2 - End axial offset (" + Util.GsaUnit.LengthLarge + ")", GH_ParamAccess.item);
            pManager.AddNumberParameter("Offset Y", "Y", "Set Y Offset (" + Util.GsaUnit.LengthLarge + ")", GH_ParamAccess.item);
            pManager.AddNumberParameter("Offset Z", "Z", "Set Z Offset (" + Util.GsaUnit.LengthLarge + ")", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Offset", "Off", "GSA Offset", GH_ParamAccess.item);
            pManager.AddNumberParameter("Offset X1", "X1", "X1 - Start axial offset (" + Util.GsaUnit.LengthLarge + ")", GH_ParamAccess.item);
            pManager.AddNumberParameter("Offset X2", "X2", "X2 - End axial offset (" + Util.GsaUnit.LengthLarge + ")", GH_ParamAccess.item);
            pManager.AddNumberParameter("Offset Y", "Y", "Y Offset (" + Util.GsaUnit.LengthLarge + ")", GH_ParamAccess.item);
            pManager.AddNumberParameter("Offset Z", "Z", "Z Offset (" + Util.GsaUnit.LengthLarge + ")", GH_ParamAccess.item);
        }
        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GsaOffset offset = new GsaOffset();
            if (DA.GetData(0, ref offset))
            {
                if (offset != null)
                {
                    //inputs
                    double x1 = 0;
                    if (DA.GetData(1, ref x1))
                        offset.X1 = x1;
                    double x2 = 0;
                    if (DA.GetData(2, ref x2))
                        offset.X2 = x2;
                    double y = 0;
                    if (DA.GetData(3, ref y))
                        offset.Y = y;
                    double z = 0;
                    if (DA.GetData(4, ref z))
                        offset.Z = z;

                    //outputs
                    DA.SetData(0, new GsaOffsetGoo(offset));
                    DA.SetData(1, offset.X1);
                    DA.SetData(2, offset.X2);
                    DA.SetData(3, offset.Y);
                    DA.SetData(4, offset.Z);
                }
            }
        }
    }
}

