﻿using System;
using System.Reflection;
using System.Xml.Linq;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Bus;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.Oracle;

namespace WorkFlowEngine
{
    //public static class Workflow
    //{
    //    public static string DataBaseScheme;
    //    public static IWorkflowActionProvider ActionProvider;

    //    private static readonly Lazy<WorkflowRuntime> LazyRuntime =
    //                        new Lazy<WorkflowRuntime>(InitWorkflowRuntime);
    //    public static WorkflowRuntime Runtime
    //    {
    //        get { return LazyRuntime.Value; }
    //    }

    //    public static void Start()
    //    {
    //        Runtime.Start();
    //    }

    //    private static WorkflowRuntime InitWorkflowRuntime()
    //    {
    //        WorkflowRuntime.RegisterLicense("Flash_Data,_S.L.U.-Rmxhc2hfRGF0YSxfUy5MLlUuOjA1LjA5LjIwMTk6ZXlKTllYaE9kVzFpWlhKUFprRmpkR2wyYVhScFpYTWlPaTB4TENKTllYaE9kVzFpWlhKUFpsUnlZVzV6YVhScGIyNXpJam90TVN3aVRXRjRUblZ0WW1WeVQyWlRZMmhsYldWeklqb3RNU3dpVFdGNFRuVnRZbVZ5VDJaVWFISmxZV1J6SWpvdE1Td2lUV0Y0VG5WdFltVnlUMlpEYjIxdFlXNWtjeUk2TFRGOTpnMGtTZzRGS0FSaGcrQ1ovVEh4NTVxTUVnb0FIbjZBUVpyR1FRTW1NaGVNeVVhTzVJUGJKQlpnRHJrSVpWcDlSd1hxVkhveW1CN1BidC9ScVd3UzFTeWNXbzM3WSsxd1psa0RWdlhvQ2tlZ2Y2SVVwTHM2aXJtaG5ncjFML2RYK1lmcU9OakdPMVdXa211eFJ4WHhPZ1daVXQwNGpadmNWRUoyck5TMFJSWDQ9");

    //        var connectionString = System.Configuration.ConfigurationManager
    //                               .ConnectionStrings["ConnectionString"].ConnectionString;
    //        var dbProvider = new OracleProvider(connectionString, DataBaseScheme);

    //        var builder = new WorkflowBuilder<XElement>(
    //            dbProvider,
    //            new OptimaJet.Workflow.Core.Parser.XmlWorkflowParser(),
    //            dbProvider
    //        ).WithDefaultCache();

    //        var runtime = new WorkflowRuntime()
    //            .WithBuilder(builder)
    //            .WithPersistenceProvider(dbProvider)
    //            .WithBus(new NullBus())
    //            .WithTimerManager(new TimerManager())
    //            //.WithActionProvider(new ActionProvider())
    //            //.EnableCodeActions()
    //            .SwitchAutoUpdateSchemeBeforeGetAvailableCommandsOn();

    //        if (ActionProvider != null)
    //        {
    //            runtime.WithActionProvider(ActionProvider);
    //        }

    //        //events subscription
    //        runtime.ProcessActivityChanged += (sender, args) => { };
    //        runtime.ProcessStatusChanged += (sender, args) => { };

    //        runtime.RegisterAssemblyForCodeActions(
    //                Assembly.GetAssembly(typeof(System.Net.Http.HttpClient))
    //            );

    //        runtime.Start();

    //        return runtime;
    //    }

    //}
}
