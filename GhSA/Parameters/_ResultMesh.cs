﻿using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System.IO;
using System.Linq;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.InteropServices;

using Rhino.DocObjects;
using Rhino.Collections;
using GH_IO;
using GH_IO.Serialization;
using Rhino.Display;

namespace GhSA.Parameters
{
    public class ResultMesh : GH_GeometricGoo<Mesh>, IGH_PreviewData
    {
        public ResultMesh(Mesh mesh, List<double> results)
        : base(mesh) 
        { 
            m_results = results;
        }

        private List<double> m_results;

        public override string ToString()
        {
            return string.Format("MeshResult: V:{0:0}, F{1:0.0}, R{2:0.0}", Value.Vertices.Count, Value.Faces.Count, m_results.Count);
        }
        public override string TypeName
        {
            get { return "Result Mesh"; }
        }
        public override string TypeDescription
        {
            get { return "A GSA result mesh type."; }
        }

        public override IGH_GeometricGoo DuplicateGeometry()
        {
            return new ResultMesh(Value, m_results);
        }
        public override BoundingBox Boundingbox
        {
            get
            {
                return Value.GetBoundingBox(false);
            }
        }
        public override BoundingBox GetBoundingBox(Transform xform)
        {
            Mesh m = Value;
            m.Transform(xform);
            return m.GetBoundingBox(false);
        }
        public override IGH_GeometricGoo Transform(Transform xform)
        {
            Mesh m = Value.DuplicateMesh();
            m.Transform(xform);
            return new ResultMesh(m, m_results);
        }
        public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
        {
            Mesh m = Value.DuplicateMesh();
            xmorph.Morph(m);
            return new ResultMesh(m, m_results);
        }

        public override object ScriptVariable()
        {
            return Value;
        }
        public override bool CastTo<TQ>(out TQ target)
        {
            if (typeof(TQ).IsAssignableFrom(typeof(Mesh)))
            {
                target = (TQ)(object)Value;
                return true;
            }

            if (typeof(TQ).IsAssignableFrom(typeof(GH_Mesh)))
            {
                target = (TQ)(object)new GH_Mesh(Value);
                return true;
            }

            target = default(TQ);
            return false;
        }
        public override bool CastFrom(object source)
        {
            if (source == null) return false;
            if (source is Mesh)
            {
                Value = (Mesh)source;
                return true;
            }
            GH_Mesh meshGoo = source as GH_Mesh;
            if (meshGoo != null)
            {
                Value = meshGoo.Value;
                return true;
            }

            Mesh m = new Mesh();
            if (GH_Convert.ToMesh(source, ref m, GH_Conversion.Both))
            {
                Value = m;
                return true;
            }

            return false;
        }

        public BoundingBox ClippingBox
        {
            get { return Boundingbox; }
        }
        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            if (Value == null) { return; }

            //Draw mesh edges
            if (args.Color == System.Drawing.Color.FromArgb(255, 150, 0, 0)) // this is a workaround to change colour between selected and not
            {
                for (int i = 0; i < Value.TopologyEdges.Count; i++)
                    args.Pipeline.DrawLine(Value.TopologyEdges.EdgeLine(i), UI.Colour.Element2dEdge, 1);
            }
            else
            {
                for (int i = 0; i < Value.TopologyEdges.Count; i++)
                    args.Pipeline.DrawLine(Value.TopologyEdges.EdgeLine(i), UI.Colour.Element2dEdgeSelected, 2);
            }
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args) 
        {
            // draw coloured mesh
            args.Pipeline.DrawMeshFalseColors(Value);
        }
    }
}
