using System;
using System.Reflection;
using System.Xml.Linq;
using AntWay.Core.WorkflowEngine;
using AntWay.Core.WorkflowObjects;
using OptimaJet.Workflow.Core.Runtime;

namespace AntWay.Core.WorkflowEngine
{
    public class WorkflowServer
    {
        private static IWorkflowActionProvider IAntWayActionProvider = null;
        private static ITimerManager ITimerManager = null;

        public static void WithActionProvider(IWorkflowActionProvider _IAntWayActionProvider)
        {
            IAntWayActionProvider = _IAntWayActionProvider;
        }

        private string DataBaseScheme;

        public WorkflowServer(string databaseScheme)
        {
            DataBaseScheme = databaseScheme;
        }

        private WorkflowRuntime _Runtime;
        public WorkflowRuntime Runtime
        {
            get
            {
                if (_Runtime == null)
                {
                    _Runtime = Workflow.InitWorkflowRuntime(
                                         ITimerManager ?? new TimerManager(),
                                        IAntWayActionProvider ?? new AntWayActionProvider(),
                                        DataBaseScheme);
                }
                return _Runtime;
            }
        }

        //public AntWayRuntime AntWayRunTime
        //{
        //    get
        //    {
        //        var result = new AntWayRuntime
        //        {
        //            WorkflowRuntime = Runtime
        //        };

        //        return result;
        //    }
        //}


        public void Start()
        {
            Runtime.Start();
        }


    }
}
