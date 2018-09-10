using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Antway.Core;
using AntWay.Core.RunTime;
using AntWay.Core.WorkflowObjects;
using AntWay.Persistence.Provider.Model;
using OptimaJet.Workflow.Core.Runtime;

namespace AntWay.Core.WorkflowEngine
{
    public static class WorkflowClient
    {
        private static IWorkflowActionProvider IAntWayActionProvider = null;
        private static ITimerManager ITimerManager = null;

        private static string _DatabaseScheme;
        public static string DataBaseScheme
        {
            get { return _DatabaseScheme;  }
            set
            {
                if (value != _DatabaseScheme)
                {
                    _Runtime = null;
                }
                _DatabaseScheme = value;
            }
        }

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


        public static AntWayProcessView GetAntWayProcess(string localizador, string identifyId = null)
        {
            AntWayProcessView result = WorkflowRuntimeExtensions.GetAntWayProcess(Runtime, localizador, identifyId);
            return result;
        }


        private static WorkflowRuntime _Runtime = null;
        private static WorkflowRuntime Runtime
        {
            get
            {
                if (_Runtime == null)
                {
                    _Runtime = Workflow.InitWorkflowRuntime(
                                 ITimerManager ?? new TimerLazyClientManager(),
                                 IAntWayActionProvider ?? new AntWayActionProvider(),
                                 DataBaseScheme);
                }
                return _Runtime;
            }
        }

        public static AntWayRuntime GetAntWayRunTime(string schemeCode)
        {
            var schemesPersistence = new SchemesPersistence
            {
                IDALSchemes = PersistenceObjectsFactory.GetIDALWFSchemaObject()
            };

            var scheme = schemesPersistence.GetScheme(schemeCode);
            if (scheme == null)
            {
                throw new NotImplementedException("Esquema inexistente");
            }
            DataBaseScheme = scheme.DBSchemeName;

            var result = new AntWayRuntime(Runtime, scheme);
            return result;
        }

        public static AntWayRuntime AntWayRunTime
        {
            get
            {
                if (DataBaseScheme == null)
                {
                    throw new NotImplementedException("Debe especificar el esquema de base de datos. DatabaseScheme");
                }

                var result = new AntWayRuntime(Runtime);
                return result;
            }
        }
    }
}
