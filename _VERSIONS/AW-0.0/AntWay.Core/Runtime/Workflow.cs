using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Antway.Core.Persistence;
using AntWay.Core.Manager;
using AntWay.Core.Model;
using AntWay.Persistence.Provider.Model;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.Oracle;

namespace AntWay.Core.Runtime
{
    internal static class Workflow
    {
        internal static ManagerResponse StartWFP(AntWayRuntime antwayRuntime,
                                                string schemeCode, 
                                                string localizadorFieldName,
                                                string localizador,
                                                string actor = null)
        {
            var locatorPersistence = new LocatorPersistence
                            { IDALocators = PersistenceObjectsFactory.GetIDALLocatorsObject() };
            var wfInstance = locatorPersistence.GetWorkflowByLocator(schemeCode, localizador);

            Guid? ProcessId = (wfInstance?.WFProcessGuid == null)
                                ? (Guid?)null
                                : wfInstance.WFProcessGuid;

            if (ProcessId != null)
            {
                var processInstance = antwayRuntime.GetProcessInstance(ProcessId.Value);

                if (processInstance != null)
                {
                    if (antwayRuntime.IsErrorState(processInstance.ProcessId))
                    {
                        var stateName = antwayRuntime.GetCurrentStateName(ProcessId.Value);
                        string activityId = null;

                        if(stateName == Constants.ACTIVITY_ERROR_TITLE)
                        {
                            activityId = $"{processInstance.CurrentActivityName.Split('/')[0]}/{Constants.ACTIVITY_RUNNING_EXCEPTION_ID}";
                        }

                        if (stateName == Constants.CHECKSUM_ERROR_TITLE)
                        {
                            activityId = $"{processInstance.CurrentActivityName.Split('/')[0]}/{Constants.CHECKSUM_ERROR_TITLE}";
                        }

                        return new ManagerResponse
                        {
                            ProcessId = processInstance.ProcessId,
                            ActivityId = activityId,
                            ActivityName = processInstance.CurrentActivityName,
                            Success = false
                        };
                    }

                    bool timeExpired = CheckTimersExpired(antwayRuntime, processInstance);
                    if (timeExpired)
                    {
                        return new ManagerResponse
                        {
                            ProcessId = processInstance.ProcessId,
                            Success = true,
                            TimeExpired = true,
                            ActivityName = antwayRuntime.GetCurrentActivityName(ProcessId.Value),
                            StateName = antwayRuntime.GetCurrentStateName(ProcessId.Value)
                        };
                    }

                    var managerResponse = antwayRuntime.ValidateModel(processInstance);

                    managerResponse.AvailableCommands = antwayRuntime
                                                        .GetAvailableCommands(ProcessId.Value, actor)
                                                        .Select(c => c.CommandName)
                                                        .ToList();
                    managerResponse.ActivityName = antwayRuntime.GetCurrentActivityName(ProcessId.Value);
                    managerResponse.StateName = antwayRuntime.GetCurrentStateName(ProcessId.Value);

                    if (!managerResponse.Success)
                    {
                        antwayRuntime.WorkflowRuntime
                        .SetErrorState(processInstance,
                                      $"{managerResponse.ActivityId}/{Constants.CHECKSUM_ERROR_TITLE}",
                                      managerResponse.ActivityName,
                                      Constants.CHECKSUM_ERROR_TITLE,
                                      managerResponse.ValidationMessages);

                        managerResponse.ActivityId = $"{managerResponse.ActivityId}/{Constants.CHECKSUM_ERROR_TITLE}";
                    }


                    return managerResponse;
                }
            }


            var processPersistenceViewNew = new ProcessPersistenceView
            {
                WFProcessGuid = Guid.NewGuid(),
                LocatorFieldName = localizadorFieldName,
                LocatorValue = localizador,
                SchemeCode = schemeCode,
            };

            var processId = antwayRuntime
                             .CreateInstanceAndPersist(processPersistenceViewNew);

            return new ManagerResponse { Success = true, ProcessId = processId };
        }

        internal static ManagerResponse StartWFPNew(AntWayRuntime antwayRuntime,
                                               string schemeCode,
                                               string localizadorFieldName,
                                               string localizador,
                                               string actor = null)
        {
            var processPersistenceViewNew = new ProcessPersistenceView
            {
                WFProcessGuid = Guid.NewGuid(),
                LocatorFieldName = localizadorFieldName,
                LocatorValue = localizador,
                SchemeCode = schemeCode,
            };

            var processId = antwayRuntime
                             .CreateInstanceAndPersist(processPersistenceViewNew);

            var result = new ManagerResponse
            {
                ProcessId = processId,
                AvailableCommands = antwayRuntime
                                    .GetAvailableCommands(processId, actor)
                                    .Select(c => c.CommandName)
                                    .ToList(),
                ActivityName = antwayRuntime.GetCurrentActivityName(processId),
                StateName = antwayRuntime.GetCurrentStateName(processId),
                Success = true,
            };

            return result;
        }


        internal static bool CheckTimersExpired(AntWayRuntime antWayRuntime,
                                                Guid processId)
        {
            var processInstance = antWayRuntime.GetProcessInstance(processId);
            var result = CheckTimersExpired(antWayRuntime, processInstance);
            return result;
        }

        internal static bool CheckTimersExpired(AntWayRuntime antwayRuntime,
                                                ProcessInstance processInstance)
        {
            string transitionExpiredStateTo = antwayRuntime
                                .GetTransitionExpiredState(processInstance);

            if (transitionExpiredStateTo != null)
            {
                antwayRuntime
                    .SkipToState(processInstance.ProcessId, transitionExpiredStateTo);
                return true;
            }

            return false;
        }


        internal static WorkflowRuntime InitWorkflowRuntime(ITimerManager timerManager,
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
                .WithBus(new AntWayBus())
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
