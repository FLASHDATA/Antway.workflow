using System;
using System.Reflection;
using System.Xml.Linq;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Bus;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.Oracle;

namespace WorkFlowEngine
{
    public class Workflow
    {
        public static string SingleDataBaseScheme;
        private static WorkflowRuntime _SingleRuntime = null;
        public static WorkflowRuntime SingleRuntime
        {
            get
            {
                if (_SingleRuntime == null)
                {
                    _SingleRuntime = new Workflow(SingleDataBaseScheme)
                                    .InitWorkflowRuntime();
                }
                return _SingleRuntime;
            }
        }


        public string DataBaseScheme;
        public IWorkflowActionProvider ActionProvider;

        public Workflow(string databaseScheme)
        {
            DataBaseScheme = databaseScheme;
        }

        private WorkflowRuntime _RuntimeServer;
        public WorkflowRuntime RuntimeServer
        {
            get
            {
                if (_RuntimeServer == null)
                {
                    _RuntimeServer = InitWorkflowRuntime(new TimerManager());
                }
                return _RuntimeServer;
            }
        }

        public void Start()
        {
            RuntimeServer.Start();
        }

        private WorkflowRuntime InitWorkflowRuntime(ITimerManager timeManager = null)
        {
            WorkflowRuntime.RegisterLicense("Flash_Data,_S.L.U.-Rmxhc2hfRGF0YSxfUy5MLlUuOjA1LjA5LjIwMTk6ZXlKTllYaE9kVzFpWlhKUFprRmpkR2wyYVhScFpYTWlPaTB4TENKTllYaE9kVzFpWlhKUFpsUnlZVzV6YVhScGIyNXpJam90TVN3aVRXRjRUblZ0WW1WeVQyWlRZMmhsYldWeklqb3RNU3dpVFdGNFRuVnRZbVZ5VDJaVWFISmxZV1J6SWpvdE1Td2lUV0Y0VG5WdFltVnlUMlpEYjIxdFlXNWtjeUk2TFRGOTpnMGtTZzRGS0FSaGcrQ1ovVEh4NTVxTUVnb0FIbjZBUVpyR1FRTW1NaGVNeVVhTzVJUGJKQlpnRHJrSVpWcDlSd1hxVkhveW1CN1BidC9ScVd3UzFTeWNXbzM3WSsxd1psa0RWdlhvQ2tlZ2Y2SVVwTHM2aXJtaG5ncjFML2RYK1lmcU9OakdPMVdXa211eFJ4WHhPZ1daVXQwNGpadmNWRUoyck5TMFJSWDQ9");

            var connectionString = System.Configuration.ConfigurationManager
                                   .ConnectionStrings["ConnectionString"].ConnectionString;
            var dbProvider = new OracleProvider(connectionString, DataBaseScheme);

            var builder = new WorkflowBuilder<XElement>(
                dbProvider,
                new OptimaJet.Workflow.Core.Parser.XmlWorkflowParser(),
                dbProvider
            ).WithDefaultCache();

            var runtime = new WorkflowRuntime()
                .WithBuilder(builder)
                .WithPersistenceProvider(dbProvider)
                .WithBus(new NullBus())
                .SwitchAutoUpdateSchemeBeforeGetAvailableCommandsOn();

            if (timeManager!=null)
            {
                runtime.WithTimerManager(timeManager);
            }

            if (ActionProvider != null)
            {
                runtime.WithActionProvider(ActionProvider);
            }

            //events subscription
            runtime.ProcessActivityChanged += (sender, args) => { };
            runtime.ProcessStatusChanged += (sender, args) => { };

            runtime.RegisterAssemblyForCodeActions(
                    Assembly.GetAssembly(typeof(System.Net.Http.HttpClient))
                );

            runtime.Start();

            return runtime;
        }

    }
}
