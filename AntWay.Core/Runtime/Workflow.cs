using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using AntWay.Core.Providers;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Bus;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.Oracle;

namespace AntWay.Core.Runtime
{
    public static class Workflow
    {
        public static WorkflowRuntime InitWorkflowRuntime(ITimerManager timerManager,
                                                 IWorkflowActionProvider actionProvider,
                                                 ICommandsMapping commandMapping,
                                                 IAssemblies assemblies,
                                                 string databaseScheme = null)
        {
            WorkflowRuntime.RegisterLicense("Flash_Data,_S.L.U.-Rmxhc2hfRGF0YSxfUy5MLlUuOjA1LjA5LjIwMTk6ZXlKTllYaE9kVzFpWlhKUFprRmpkR2wyYVhScFpYTWlPaTB4TENKTllYaE9kVzFpWlhKUFpsUnlZVzV6YVhScGIyNXpJam90TVN3aVRXRjRUblZ0WW1WeVQyWlRZMmhsYldWeklqb3RNU3dpVFdGNFRuVnRZbVZ5VDJaVWFISmxZV1J6SWpvdE1Td2lUV0Y0VG5WdFltVnlUMlpEYjIxdFlXNWtjeUk2TFRGOTpnMGtTZzRGS0FSaGcrQ1ovVEh4NTVxTUVnb0FIbjZBUVpyR1FRTW1NaGVNeVVhTzVJUGJKQlpnRHJrSVpWcDlSd1hxVkhveW1CN1BidC9ScVd3UzFTeWNXbzM3WSsxd1psa0RWdlhvQ2tlZ2Y2SVVwTHM2aXJtaG5ncjFML2RYK1lmcU9OakdPMVdXa211eFJ4WHhPZ1daVXQwNGpadmNWRUoyck5TMFJSWDQ9");

            var connectionString = System.Configuration.ConfigurationManager
                                   .ConnectionStrings["ConnectionString"].ConnectionString;
            var dbProvider = new OracleProvider(connectionString, databaseScheme);

            var builder = new WorkflowBuilder<XElement>(
                dbProvider,
                new OptimaJet.Workflow.Core.Parser.XmlWorkflowParser(),
                dbProvider
            ).WithDefaultCache();

            var runtime = new WorkflowRuntime()
                .WithBuilder(builder)
                .WithPersistenceProvider(dbProvider)
                .WithTimerManager(timerManager)
                .WithBus(new NullBus())
                .WithActionProvider(actionProvider)
                .SwitchAutoUpdateSchemeBeforeGetAvailableCommandsOff();
                //.SwitchAutoUpdateSchemeBeforeGetAvailableCommandsOn();

            //events subscription
            runtime.ProcessStatusChanged += (sender, args) =>
            {
                byte status = args.NewStatus.Id;
            };

            runtime.CommandMapping = commandMapping;

            runtime.RegisterAssemblyForCodeActions(
                    Assembly.GetAssembly(typeof(AntWayRuntime))
            );

            assemblies.RegisterAssembliesForWorkflowDesigner(runtime);
            assemblies.RegisterAssembliesForServiceLocator();

            runtime.Start();

            return runtime;
        }
    }
}
