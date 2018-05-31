using System;
using System.Reflection;
using System.Xml.Linq;
using Antway.Core;
using AntWay.Core.WorkflowObjects;
using AntWay.Persistence.Model;
using OptimaJet.Workflow.Core.Runtime;


namespace AntWay.Core.WorkflowEngine
{
    public static class WorkflowClient
    {
        private static IWorkflowActionProvider IAntWayActionProvider = null;
        private static ITimerManager ITimerManager = null;

        public static string SingleDataBaseScheme;

        public static void InitWith(IWorkflowActionProvider _IAntWayActionProvider,
                                    ITimerManager _ITimerManager)
        {
            WithActionProvider(_IAntWayActionProvider);
            WithTimeManager(_ITimerManager);
        }

        public static void WithActionProvider(IWorkflowActionProvider _IAntWayActionProvider)
                                
        {
            IAntWayActionProvider = _IAntWayActionProvider;
        }

        public static void WithTimeManager(ITimerManager _ITimerManager)
        {
            ITimerManager = _ITimerManager;
        }

        public static Guid CreateInstance(ProcessPersistenceView processPersistenceView)
        {
            Guid result = Workflow.CreateInstance(Runtime, processPersistenceView);
            return result;
        }

        public static void ExecutecommandNext(Guid processId)
        {
            WorkflowRuntimeExtensions.ExecutecommandNext(Runtime, processId);
        }


        public static bool Executecommand(Guid processId, string commandName, string identifyId = null)
        {
            bool result = WorkflowRuntimeExtensions.Executecommand(Runtime, processId, commandName, identifyId);
            return result;
        }

        private static WorkflowRuntime _Runtime = null;
        public static WorkflowRuntime Runtime
        {
            get
            {
                if (_Runtime == null)
                {
                    _Runtime = Workflow.InitWorkflowRuntime(
                                 ITimerManager ?? new TimerLazyClientManager(),
                                 IAntWayActionProvider ?? new AntWayActionProvider(),
                                 SingleDataBaseScheme);
                }
                return _Runtime;
            }
        }


    }
}
