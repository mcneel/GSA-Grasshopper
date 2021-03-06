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
          : base("Get Model Analysis", "GetAnalysis", "Get Analysis Cases and Tasks from GSA model",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat0())
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override System.Drawing.Bitmap Icon => GSA.Properties.Resources.GetAnalysis;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        #endregion

        #region Input and output

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("GSA Model", "GSA", "GSA model containing some Analysis Cases and Tasks", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Analysis Tasks", "Tasks", "List of analysis tasks in model", GH_ParamAccess.list);
            pManager.AddGenericParameter("Analysis Case Names", "Name", "Analysis case name", GH_ParamAccess.list);
            pManager.AddGenericParameter("Load Case/Combination ID", "LC", "Load cases and combinations list", GH_ParamAccess.list);
            pManager.AddGenericParameter("Analysis Case Description", "Desc", "Analysis case description", GH_ParamAccess.list);
        }
        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GsaModel gsaModel = new GsaModel();
            if (DA.GetData(0, ref gsaModel))
            {
                Model model = gsaModel.Model;

                //Tasks and cases output
                List<string> taskList = new List<string>();
                List<string> descriptionList = new List<string>();
                List<string> caseNameList = new List<string>();
                List<int> analysisIdList = new List<int>();
                
                //Tasks
                foreach (int key in model.AnalysisTasks().Keys)
                {
                    model.AnalysisTasks().TryGetValue(key, out AnalysisTask analTask);
                    taskList.Add(analTask.Name);
                }

                foreach (int key in model.Results().Keys)
                {
                    descriptionList.Add(model.AnalysisCaseDescription(key));
                    caseNameList.Add(model.AnalysisCaseName(key));
                    analysisIdList.Add(key);
                }

                DA.SetDataList(0, taskList);
                DA.SetDataList(1, caseNameList);
                DA.SetDataList(3, descriptionList);
                DA.SetDataList(2, analysisIdList);
            }
        }
    }
}

