using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Antway.Core.Persistence;
using AntWay.Core.Manager;
using AntWay.Persistence.Provider.Model;
using OptimaJet.Workflow.Core.Runtime;


namespace AntWay.Core.Runtime
{
    public static class WorkflowClient
    {
        private static IWorkflowActionProvider IAntWayActionProvider = null;
        private static ITimerManager ITimerManager = null;
        private static ICommandsMapping ICommandsMapping = null;
        public static IAssemblies IAssemblies = null;
        public static IActivityManager IActivityManager = null;

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

        public static void WithActionProvider(IWorkflowActionProvider _IAntWayActionProvider)
        {
            IAntWayActionProvider = _IAntWayActionProvider;
        }

        public static void WithTimeManager(ITimerManager _ITimerManager)
        {
            ITimerManager = _ITimerManager;
        }

        public static void WithCommands(ICommandsMapping _ICommandsMapping)
        {
            ICommandsMapping = _ICommandsMapping;
        }

        public static void WithAssemblies(IAssemblies _IAssemblies)
        {
            IAssemblies = _IAssemblies;
        }

        public static void WithActivityManager(IActivityManager _IActivityManager)
        {
            IActivityManager = _IActivityManager;
        }


        public static ManagerResponse StartWF(string schemeCode, string localizadorFieldName,
                                             string localizador,
                                             IAssemblies assemblies,
                                             IActivityManager activityManager,
                                             bool forceNewProcess,
                                             string actor = null)
        {
            WithAssemblies(assemblies);
            WithActivityManager(activityManager);
            GetAntWayRunTime(schemeCode);

            var result = !forceNewProcess
                             ? Workflow.StartWFP(AntWayRunTime,
                                          schemeCode,
                                          localizadorFieldName,
                                          localizador,
                                          actor)
                             : Workflow.StartWFPNew(AntWayRunTime,
                                                  schemeCode,
                                                  localizadorFieldName,
                                                  localizador,
                                                  actor);

            result.Success = (result.Success &&
                              result.ActivityName != null &&
                              result.ActivityName.ToLower().IndexOf("error") < 0);

            return result;
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
                                 ICommandsMapping ?? new EmptyCommandMapping(),
                                 IAssemblies ?? new EmptyAsembly(),
                                 DataBaseScheme);
                }
                return _Runtime;
            }
        }
    }
}
