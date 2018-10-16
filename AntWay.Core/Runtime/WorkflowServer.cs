using System;
using System.Reflection;
using System.Xml.Linq;
using AntWay.Core.Providers;
using OptimaJet.Workflow.Core.Runtime;

namespace AntWay.Core.Runtime
{
    public class WorkflowServer
    {
        private static IWorkflowActionProvider IAntWayActionProvider = null;
        private static ITimerManager ITimerManager = null;
        private static ICommandsMapping ICommandMapping = null;
        private static IAssemblies IAssemblies = null;


        public static void WithActionProvider(IWorkflowActionProvider _IAntWayActionProvider)
        {
            IAntWayActionProvider = _IAntWayActionProvider;
        }

        public static void WithAssemblies(IAssemblies _IAssemblies)
        {
            IAssemblies = _IAssemblies;
        }

        private string DataBaseScheme;

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
