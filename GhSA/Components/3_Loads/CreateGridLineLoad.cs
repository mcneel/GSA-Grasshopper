﻿using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using GsaAPI;
using Rhino.Geometry;
using GhSA.Parameters;

namespace GhSA.Components
{
    public class CreateGridLineLoad : GH_Component
    {
        #region Name and Ribbon Layout
        public CreateGridLineLoad()
            : base("Create Grid Line Load", "LineLoad", "Create GSA Grid Line Load",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat3())
        { this.Hidden = true; } // sets the initial state of the component to hidden
        public override Guid ComponentGuid => new Guid("fdd95021-9193-4565-b56b-130f22ab13de");
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override System.Drawing.Bitmap Icon => GSA.Properties.Resources.GridLineLoad;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        #endregion

        #region input and output
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Load case", "LC", "Load case number (default 1)", GH_ParamAccess.item, 1);
            pManager.AddCurveParameter("PolyLine", "Ln", "PolyLine. If you input grid plane below only x and y coordinate positions will be used from this polyline, but if not a new Grid Plane Surface (best-fit plane) will be created from PolyLine control points.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Grid Plane Surface", "GPS", "Grid Plane Surface or Plane (optional). If no input here then the point's z-coordinate will be used for an xy-plane at that elevation.", GH_ParamAccess.item);
            pManager.AddTextParameter("Direction", "Di", "Load direction (default z)." +
                    System.Environment.NewLine + "Accepted inputs are:" +
                    System.Environment.NewLine + "x" +
                    System.Environment.NewLine + "y" +
                    System.Environment.NewLine + "z", GH_ParamAccess.item, "z");
            pManager.AddIntegerParameter("Axis", "Ax", "Load axis (default Global). " +
                    System.Environment.NewLine + "Accepted inputs are:" +
                    System.Environment.NewLine + "0 : Global" +
                    System.Environment.NewLine + "-1 : Local", GH_ParamAccess.item, 0);
            pManager.AddBooleanParameter("Projected", "Pj", "Projected (default not)", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("Value Start (" + Util.GsaUnit.Force + "/" + Util.GsaUnit.LengthLarge + ")", "V1", "Load Value (" + Util.GsaUnit.Force + "/" + Util.GsaUnit.LengthLarge + ") at Start of Line", GH_ParamAccess.item);
            pManager.AddNumberParameter("Value End (" + Util.GsaUnit.Force + "/" + Util.GsaUnit.LengthLarge + ")", "V2", "Load Value (" + Util.GsaUnit.Force + "/" + Util.GsaUnit.LengthLarge + ") at End of Line (default : Start Value)", GH_ParamAccess.item);

            pManager[0].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[7].Optional = true;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid Line Load", "Load", "GSA Grid Line Load", GH_ParamAccess.item);
        }
        #endregion
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GsaGridLineLoad gridlineload = new GsaGridLineLoad();

            // 0 Load case
            int lc = 1;
            GH_Integer gh_lc = new GH_Integer();
            if (DA.GetData(0, ref gh_lc))
                GH_Convert.ToInt32(gh_lc, out lc, GH_Conversion.Both);
            gridlineload.GridLineLoad.Case = lc;

            // Do plane input first as to see if we need to project polyline onto grid plane
            // 2 Plane 
            Plane pln = Plane.Unset;
            GsaGridPlaneSurface grdplnsrf = new GsaGridPlaneSurface();
            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(2, ref gh_typ))
            {
                if (gh_typ.Value is GsaGridPlaneSurface)
                {
                    GsaGridPlaneSurface temppln = new GsaGridPlaneSurface();
                    gh_typ.CastTo(ref temppln);
                    grdplnsrf = temppln.Duplicate();
                    pln = grdplnsrf.Plane;
                }
                else
                {
                    gh_typ.CastTo(ref pln);
                    grdplnsrf = new GsaGridPlaneSurface(pln);
                }
            }
            
            // we wait setting the gridplanesurface until we have run the polyline input

            // 1 Polyline
            Polyline ln = new Polyline();
            GH_Curve gh_crv = new GH_Curve();
            if (DA.GetData(1, ref gh_crv))
            {
                Curve crv = null;
                GH_Convert.ToCurve(gh_crv, ref crv, GH_Conversion.Both);

                //convert to polyline
                if (crv.TryGetPolyline(out ln))
                {
                    // get control points
                    List<Point3d> ctrl_pts = ln.ToList();
                    
                    // plane
                    if (pln == Plane.Unset)
                    {
                        // calculate best fit plane:
                        Plane.FitPlaneToPoints(ctrl_pts, out pln);

                        // create grid plane surface from best fit plane
                        grdplnsrf = new GsaGridPlaneSurface(pln);
                    }
                    else
                    {
                        // project original curve onto grid plane
                        crv = Curve.ProjectToPlane(crv, pln);

                        // convert to polyline again
                        crv.TryGetPolyline(out ln);

                        //get control points again
                        ctrl_pts = ln.ToList();
                    }

                    // string to write polyline description to
                    string desc = "";

                    // loop through all points
                    for (int i = 0; i < ctrl_pts.Count; i++)
                    {
                        if (i > 0)
                            desc += " ";
                        
                        // get control points in local plane coordinates
                        Point3d temppt = new Point3d();
                        pln.RemapToPlaneSpace(ctrl_pts[i], out temppt);

                        // write point to string
                        // format accepted by GSA: (0,0) (0,1) (1,2) (3,4) (4,0)(m)
                        desc += "(" + temppt.X + "," + temppt.Y + ")";
                    }
                    // add units to the end
                    desc += "(" + Util.GsaUnit.LengthLarge + ")";

                    // set polyline in grid line load
                    gridlineload.GridLineLoad.PolyLineDefinition = desc;
                }
                else
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not convert Curve to Polyline");
            }

            // now we can set the gridplanesurface:
            gridlineload.GridPlaneSurface = grdplnsrf;


            // 3 direction
            string dir = "Z";
            Direction direc = Direction.Z;

            GH_String gh_dir = new GH_String();
            if (DA.GetData(3, ref gh_dir))
                GH_Convert.ToString(gh_dir, out dir, GH_Conversion.Both);
            dir = dir.ToUpper();
            if (dir == "X")
                direc = Direction.X;
            if (dir == "Y")
                direc = Direction.Y;

            gridlineload.GridLineLoad.Direction = direc;

            // 4 Axis
            int axis = 0;
            gridlineload.GridLineLoad.AxisProperty = 0; 
            GH_Integer gh_ax = new GH_Integer();
            if (DA.GetData(4, ref gh_ax))
            {
                GH_Convert.ToInt32(gh_ax, out axis, GH_Conversion.Both);
                if (axis == 0 || axis == -1)
                    gridlineload.GridLineLoad.AxisProperty = axis;
            }

            // 6 load value
            double load1 = 0;
            if (DA.GetData(6, ref load1))
                load1 *= -1000; //convert to kN
            gridlineload.GridLineLoad.ValueAtStart = load1;

            // 7 load value
            double load2 = load1;
            if (DA.GetData(7, ref load2))
                load2 *= -1000; //convert to kN
            gridlineload.GridLineLoad.ValueAtEnd = load2;

            // convert to goo
            GsaLoad gsaLoad = new GsaLoad(gridlineload);
            DA.SetData(0, new GsaLoadGoo(gsaLoad));
        }
    }
}
