using System;
using System.Reflection;
using System.Xml.Linq;
using AntWay.Core.Manager;
using OptimaJet.Workflow.Core.Runtime;

namespace AntWay.Core.Runtime
{
    public class WorkflowServer
    {
        private static IWorkflowActionProvider IAntWayActionProvider = null;
        private static ITimerManager ITimerManager = null;
        private static ICommandsMapping ICommandMapping = null;
        private static IAssemblies IAssemblies = null;
        private string DataBaseScheme;


        public static void WithActionProvider(IWorkflowActionProvider _IAntWayActionProvider)
        {
            IAntWayActionProvider = _IAntWayActionProvider;
        }

        public static void WithAssemblies(IAssemblies _IAssemblies)
        {
            IAssemblies = _IAssemblies;
        }

        //public static ManagerResponse StartWF(string schemeCode, string localizadorFieldName,
        //                             string localizador,
        //                             IAssemblies assemblies,
        //                             IActivityManager activityManager)
        //{
        //    var result = Workflow.StartWF(schemeCode, localizadorFieldName,
        //                                  localizador, assemblies, activityManager);

        //    return result;
        //}

        
        public WorkflowServer(string databaseScheme)
        {
            DataBaseScheme = databaseScheme;
        }

        private WorkflowRuntime _Runtime;
        private WorkflowRuntime Runtime
        {
            get
            {
                if (_Runtime == null)
                {
                    _Runtime = Workflow.InitWorkflowRuntime(
                                         ITimerManager ?? new TimerManager(),
                                         IAntWayActionProvider ?? new AntWayActionProvider(),
                                         ICommandMapping ?? new EmptyCommandMapping(),
                                         IAssemblies ?? new EmptyAsembly(),
                                         DataBaseScheme);
                }
                return _Runtime;
            }
        }

        public void Start()
        {
            Runtime.Start();
        }
    }
}
