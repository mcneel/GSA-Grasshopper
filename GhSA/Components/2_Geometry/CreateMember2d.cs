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
    /// Component to create new 2D Member
    /// </summary>
    public class CreateMember2d : GH_Component, IGH_PreviewObject
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("df0c2786-9e46-4500-ab63-0c4162a580d4");
        public CreateMember2d()
          : base("Create 2D Member", "Mem2D", "Create GSA Member 2D",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat2())
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override System.Drawing.Bitmap Icon => GSA.Properties.Resources.CreateMem2D;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout


        #endregion

        #region Input and output

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "Planar Brep (non-planar geometry will be automatically converted to an average plane of exterior boundary control points))", GH_ParamAccess.item);
            pManager.AddGenericParameter("2D Property", "PA", "GSA 2D Property. Input either a GSA 2D Property or an Integer to use a Section already defined in model", GH_ParamAccess.item);
            pManager.AddPointParameter("Incl. Point", "iPt", "Inclusion points (will automatically be projected onto Brep)", GH_ParamAccess.list);
            pManager.AddCurveParameter("Incl. Curve", "iCrv", "Inclusion curves (will automatically be made planar and projected onto brep, and converted to Arcs and Lines)", GH_ParamAccess.list);
            pManager.AddNumberParameter("Mesh Size", "Ms", "Targe mesh size", GH_ParamAccess.item, 0);

            pManager.HideParameter(0);
            pManager.HideParameter(2);
            pManager.HideParameter(3);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("2D Member", "Mem2d", "GSA 2D Member", GH_ParamAccess.item);
        }
        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Brep ghbrep = new GH_Brep();
            if (DA.GetData(0, ref ghbrep))
            {
                Brep brep = new Brep();
                if (GH_Convert.ToBrep(ghbrep, ref brep, GH_Conversion.Both))
                {
                    // first import points and curves for inclusion before building member
                    
                    // 2 Points
                    List<Point3d> pts = new List<Point3d>();
                    List<GH_Point> ghpts = new List<GH_Point>();
                    if (DA.GetDataList(2, ghpts))
                    {
                        for (int i = 0; i < ghpts.Count; i++)
                        {
                            Point3d pt = new Point3d();
                            if (GH_Convert.ToPoint3d(ghpts[i], ref pt, GH_Conversion.Both))
                                pts.Add(pt);
                        }
                    }

                    // 3 Curves
                    List<Curve> crvs = new List<Curve>();
                    List<GH_Curve> ghcrvs = new List<GH_Curve>();
                    if (DA.GetDataList(3, ghcrvs))
                    {
                        for (int i = 0; i < ghcrvs.Count; i++)
                        {
                            Curve crv = null;
                            if (GH_Convert.ToCurve(ghcrvs[i], ref crv, GH_Conversion.Both))
                                crvs.Add(crv);
                        }
                    }

                    // now build new member with brep, crv and pts
                    GsaMember2d mem = new GsaMember2d(brep, crvs, pts);

                    // add the rest
                    // 1 section
                    GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
                    GsaProp2d prop2d = new GsaProp2d();
                    if (DA.GetData(1, ref gh_typ))
                    {
                        if (gh_typ.Value is GsaProp2dGoo)
                            gh_typ.CastTo(ref prop2d);
                        else
                        {
                            if (GH_Convert.ToInt32(gh_typ.Value, out int idd, GH_Conversion.Both))
                                prop2d.ID = idd;
                            else
                            {
                                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert PA input to a 2D Property of reference integer");
                                return;
                            }
                        }
                    }
                    else
                        prop2d.ID = 1;
                    mem.Property = prop2d;

                    // 4 mesh size
                    GH_Number ghmsz = new GH_Number();
                    if (DA.GetData(4, ref ghmsz))
                    {
                        GH_Convert.ToDouble(ghmsz, out double m_size, GH_Conversion.Both);
                        mem.Member.MeshSize = m_size;
                    }

                    DA.SetData(0, new GsaMember2dGoo(mem));
                }
            }
        }
    }
}

