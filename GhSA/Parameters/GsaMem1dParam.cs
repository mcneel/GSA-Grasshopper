﻿using System;
using System.Collections.Generic;
using System.Linq;

using GsaAPI;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino;
using GhSA.Util.Gsa;
using Grasshopper.Documentation;
using Rhino.Collections;

namespace GhSA.Parameters
{
    /// <summary>
    /// Member1d class, this class defines the basic properties and methods for any Gsa Member 1d
    /// </summary>
    public class GsaMember1d

    {
        public Member Member
        {
            get { return m_member; }
            set { m_member = value; }
        }
        public PolyCurve PolyCurve
        {
            get { return m_crv; }
            set { m_crv = Util.GH.Convert.ConvertCurve(value); }
        }
        public int ID
        {
            get { return m_id; }
            set { m_id = value; }
        }

        public List<Point3d> Topology
        {
            get { return m_topo; }
            set { m_topo = value; }
        }

        public List<string> TopologyType
        {
            get { return m_topoType; }
            set { m_topoType = value; }
        }

        public GsaBool6 ReleaseStart
        {
            get { return m_rel1; }
            set { m_rel1 = value; }
        }
        public GsaBool6 ReleaseEnd
        {
            get { return m_rel2; }
            set { m_rel2 = value; }
        }

        public GsaSection Section
        {
            get { return m_section; }
            set { m_section = value; }
        }



        #region fields
        private Member m_member;
        private int m_id = 0;

        private PolyCurve m_crv; //Polyline for visualisation /member1d/member2d
        private List<Point3d> m_topo; // list of topology points for visualisation /member1d/member2d
        private List<string> m_topoType; //list of polyline curve type (arch or line) for member1d/2d
        private GsaBool6 m_rel1;
        private GsaBool6 m_rel2;
        private GsaSection m_section;
        #endregion

        #region constructors
        public GsaMember1d()
        {
            m_member = new Member();
            m_crv = new PolyCurve();
        }

        public GsaMember1d(List<Point3d> topology, List<string> topo_type = null)
        {
            m_member = new Member
            {
                Type = MemberType.GENERIC_1D
            };
            m_crv = Util.GH.Convert.BuildCurve(topology, topo_type);
            m_topo = topology;
            m_topoType = topo_type;

            Topology = m_topo;
            TopologyType = m_topoType;
        }

        public GsaMember1d(Curve crv, int prop = 1)
        {
            m_member = new Member
            {
                Type = MemberType.GENERIC_1D,
                Property = prop
            };
            Tuple<PolyCurve, List<Point3d>, List<string>> convertCrv = Util.GH.Convert.ConvertPolyCrv(crv);
            m_crv = convertCrv.Item1;
            m_topo = convertCrv.Item2;
            m_topoType = convertCrv.Item3;

            m_section = new GsaSection();

            Topology = m_topo;
            TopologyType = m_topoType;
        }


        public GsaMember1d Duplicate()
        {
            GsaMember1d dup = new GsaMember1d
            {
                m_member = m_member //add clone or duplicate if available
            };

            if (m_crv != null)
                dup.m_crv = m_crv.DuplicatePolyCurve();

            Point3dList point3Ds = new Point3dList(m_topo);
            dup.Topology = new List<Point3d>(point3Ds.Duplicate());
            dup.TopologyType = m_topoType.ToList();

            dup.ID = m_id;

            if (m_rel1 != null)
                dup.ReleaseStart = m_rel1.Duplicate();
            if (m_rel2 != null)
                dup.ReleaseEnd = m_rel2.Duplicate();
            if (m_section != null)
                dup.Section = m_section.Duplicate();

            return dup;
        }
        #endregion

        #region properties
        public bool IsValid
        {
            get
            {
                if (m_crv == null)
                    return false;
                return true;
            }
        }
        

        #endregion

        #region methods
        public override string ToString()
        {
            string idd = " " + ID.ToString();
            if (ID == 0) { idd = ""; }
            string typeTxt = "GSA " + m_member.Type.ToString() + " Member" + idd;

            return typeTxt;
        }

        #endregion
    }

    /// <summary>
    /// GsaMember Goo wrapper class, makes sure GsaMember can be used in Grasshopper.
    /// </summary>
    public class GsaMember1dGoo : GH_GeometricGoo<GsaMember1d>, IGH_PreviewData
    {
        #region constructors
        public GsaMember1dGoo()
        {
            this.Value = new GsaMember1d();
        }
        public GsaMember1dGoo(GsaMember1d member)
        {
            if (member == null)
                member = new GsaMember1d();
            this.Value = member;
        }

        public override IGH_GeometricGoo DuplicateGeometry()
        {
            return DuplicateGsaMember1d();
        }
        public GsaMember1dGoo DuplicateGsaMember1d()
        {
            return new GsaMember1dGoo(Value == null ? new GsaMember1d() : Value.Duplicate());
        }
        #endregion

        #region properties
        public override bool IsValid
        {
            get
            {
                if (Value == null) { return false; }
                if (Value.PolyCurve == null) { return false; }
                return true;
            }
        }
        public override string IsValidWhyNot
        {
            get
            {
                //if (Value == null) { return "No internal GsaMember instance"; }
                if (Value.IsValid) { return string.Empty; }
                return Value.IsValid.ToString(); //Todo: beef this up to be more informative.
            }
        }
        public override string ToString()
        {
            if (Value == null)
                return "Null Member1D";
            else
                return Value.ToString();
        }
        public override string TypeName
        {
            get { return ("Member 1D"); }
        }
        public override string TypeDescription
        {
            get { return ("GSA 1D Member"); }
        }

        public override BoundingBox Boundingbox
        {
            get
            {
                if (Value == null) { return BoundingBox.Empty; }
                if (Value.PolyCurve == null) { return BoundingBox.Empty; }
                return Value.PolyCurve.GetBoundingBox(false) ;
            }
        }
        public override BoundingBox GetBoundingBox(Transform xform)
        {
            if (Value == null) { return BoundingBox.Empty; }
            if (Value.PolyCurve == null) { return BoundingBox.Empty; }
            return Value.PolyCurve.GetBoundingBox(xform);
        }
        #endregion

        #region casting methods
        public override bool CastTo<Q>(out Q target)
        {
            // This function is called when Grasshopper needs to convert this 
            // instance of GsaMember into some other type Q.            


            if (typeof(Q).IsAssignableFrom(typeof(GsaMember1d)))
            {
                if (Value == null)
                    target = default;
                else
                    target = (Q)(object)Value;
                return true;
            }

            if (typeof(Q).IsAssignableFrom(typeof(Member)))
            {
                if (Value == null)
                    target = default;
                else
                    target = (Q)(object)Value.Member;
                return true;
            }

            //Cast to Curve
            if (typeof(Q).IsAssignableFrom(typeof(Curve)))
            {
                if (Value == null)
                    target = default;
                else
                    target = (Q)(object)Value.PolyCurve;
                return true;
            }
            if (typeof(Q).IsAssignableFrom(typeof(GH_Curve)))
            {
                if (Value == null)
                    target = default;
                else
                {
                    target = (Q)(object)new GH_Curve(Value.PolyCurve);
                    if (Value.PolyCurve == null)
                        return false;
                }
                   
                return true;
            }

            if (typeof(Q).IsAssignableFrom(typeof(PolyCurve)))
            {
                if (Value == null)
                    target = default;
                else
                {
                    target = (Q)(object)Value.PolyCurve;
                    if (Value.PolyCurve == null)
                        return false;
                }
                    
                return true;
            }

            if (typeof(Q).IsAssignableFrom(typeof(Polyline)))
            {
                if (Value == null)
                    target = default;
                else
                {
                    target = (Q)(object)Value.PolyCurve;
                    if (Value.PolyCurve == null)
                        return false;
                }
                    
                return true;
            }
            if (typeof(Q).IsAssignableFrom(typeof(Line)))
            {
                if (Value == null)
                    target = default;
                else
                {
                    target = (Q)(object)Value.PolyCurve.ToPolyline(0.05, 5, 0, 0);
                    if (Value.PolyCurve == null)
                        return false;
                }
                    
                return true;
            }


            target = default;
            return false;
        }
        public override bool CastFrom(object source)
        {
            // This function is called when Grasshopper needs to convert other data 
            // into GsaMember.


            if (source == null) { return false; }

            //Cast from GsaMember
            if (typeof(GsaMember1d).IsAssignableFrom(source.GetType()))
            {
                Value = (GsaMember1d)source;
                return true;
            }

            //Cast from GsaAPI Member
            if (typeof(Member).IsAssignableFrom(source.GetType()))
            {
                Value.Member = (Member)source;
                return true;
            }

            
            //Cast from Curve
            Curve crv = null;

            if (GH_Convert.ToCurve(source, ref crv, GH_Conversion.Both))
            {
                GsaMember1d member = new GsaMember1d(crv);
                this.Value = member;
                return true;
            }
            

            return false;
        }
        #endregion

        #region transformation methods
        public override IGH_GeometricGoo Transform(Transform xform)
        {
            if (Value == null) { return null; }
            if (Value.PolyCurve == null) { return null; }

            GsaMember1d mem = Value.Duplicate();

            List<Point3d> pts = Value.Topology;
            Point3dList xpts = new Point3dList(pts);
            xpts.Transform(xform);
            mem.Topology = xpts.ToList();
            mem.TopologyType = Value.TopologyType;
            
            if (Value.PolyCurve != null)
            {
                PolyCurve crv = Value.PolyCurve.DuplicatePolyCurve();
                crv.Transform(xform);
                mem.PolyCurve = crv;
            }
            
            return new GsaMember1dGoo(mem);
        }

        public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
        {
            if (Value == null) { return null; }
            if (Value.PolyCurve == null) { return null; }

            GsaMember1d mem = new GsaMember1d
            {
                Member = Value.Member
            };

            List<Point3d> pts = Value.Topology;
            for (int i = 0; i < pts.Count; i++)
                pts[i] = xmorph.MorphPoint(pts[i]);
            mem.Topology = pts;

            if (Value.PolyCurve != null)
            {
                PolyCurve crv = Value.PolyCurve.DuplicatePolyCurve();
                xmorph.Morph(crv);
                mem.PolyCurve = crv;
            }

            return new GsaMember1dGoo(mem);
        }

        #endregion

        #region drawing methods
        public BoundingBox ClippingBox
        {
            get { return Boundingbox; }
        }
        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            // no meshes to be drawn
        }
        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            if (Value == null) { return; }

            //Draw lines
            if (Value.PolyCurve != null)
            {
                if (args.Color == System.Drawing.Color.FromArgb(255, 150, 0, 0)) // this is a workaround to change colour between selected and not
                    args.Pipeline.DrawCurve(Value.PolyCurve, UI.Colour.Member1d, 2);
                else
                    args.Pipeline.DrawCurve(Value.PolyCurve, UI.Colour.Member1dSelected, 2);
            }

            //Draw points.
            if (Value.Topology != null)
            {
                List<Point3d> pts = Value.Topology;
                for (int i = 0; i < pts.Count; i++)
                {
                    if (args.Color == System.Drawing.Color.FromArgb(255, 150, 0, 0)) // this is a workaround to change colour between selected and not
                    {
                        if (i == 0 | i == pts.Count - 1) // draw first point bigger
                            args.Pipeline.DrawPoint(pts[i], Rhino.Display.PointStyle.RoundSimple, 3, UI.Colour.Member1dNode);
                        else
                            args.Pipeline.DrawPoint(pts[i], Rhino.Display.PointStyle.RoundSimple, 2, UI.Colour.Member1dNode);
                    }
                    else
                    {
                        if (i == 0 | i == pts.Count - 1) // draw first point bigger
                            args.Pipeline.DrawPoint(pts[i], Rhino.Display.PointStyle.RoundControlPoint, 3, UI.Colour.Member1dNodeSelected);
                        else
                            args.Pipeline.DrawPoint(pts[i], Rhino.Display.PointStyle.RoundControlPoint, 2, UI.Colour.Member1dNodeSelected);
                    }
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// This class provides a Parameter interface for the Data_GsaMember1d type.
    /// </summary>
    public class GsaMember1dParameter : GH_PersistentGeometryParam<GsaMember1dGoo>, IGH_PreviewObject
    {
        public GsaMember1dParameter()
          : base(new GH_InstanceDescription("GSA 1D Member", "Member 1D", "Maintains a collection of GSA 1D Member data.", GhSA.Components.Ribbon.CategoryName.Name(), GhSA.Components.Ribbon.SubCategoryName.Cat9()))
        {
        }

        public override Guid ComponentGuid => new Guid("0392a5a0-7762-4214-8c30-fb395365056e");

        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        protected override System.Drawing.Bitmap Icon => GSA.Properties.Resources.GsaMem1D;

        //We do not allow users to pick parameter, 
        //therefore the following 4 methods disable all this ui.
        protected override GH_GetterResult Prompt_Plural(ref List<GsaMember1dGoo> values)
        {
            return GH_GetterResult.cancel;
        }
        protected override GH_GetterResult Prompt_Singular(ref GsaMember1dGoo value)
        {
            return GH_GetterResult.cancel;
        }
        protected override System.Windows.Forms.ToolStripMenuItem Menu_CustomSingleValueItem()
        {
            System.Windows.Forms.ToolStripMenuItem item = new System.Windows.Forms.ToolStripMenuItem
            {
                Text = "Not available",
                Visible = false
            };
            return item;
        }
        protected override System.Windows.Forms.ToolStripMenuItem Menu_CustomMultiValueItem()
        {
            System.Windows.Forms.ToolStripMenuItem item = new System.Windows.Forms.ToolStripMenuItem
            {
                Text = "Not available",
                Visible = false
            };
            return item;
        }

        #region preview methods
        public BoundingBox ClippingBox
        {
            get
            {
                return Preview_ComputeClippingBox();
            }
        }
        public void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            //Use a standard method to draw gunk, you don't have to specifically implement this.
            Preview_DrawMeshes(args);
        }
        public void DrawViewportWires(IGH_PreviewArgs args)
        {
            //Use a standard method to draw gunk, you don't have to specifically implement this.
            Preview_DrawWires(args);
        }

        private bool m_hidden = false;
        public bool Hidden
        {
            get { return m_hidden; }
            set { m_hidden = value; }
        }
        public bool IsPreviewCapable
        {
            get { return true; }
        }
        #endregion
    }

}
