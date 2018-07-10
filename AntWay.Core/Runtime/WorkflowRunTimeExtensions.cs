using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antway.Core;
using AntWay.Persistence.Provider.Model;
using OptimaJet.Workflow.Core.Runtime;

namespace AntWay.Core.RunTime
{
    internal static class WorkflowRuntimeExtensions
    {
        public static bool ExecutecommandNext(WorkflowRuntime runtime, 
                                       Guid processId,
                                       string identifyId = null,
                                       string impersonatedIdentifyId = null)
        {
            WorkflowCommand command = runtime
                                         .GetAvailableCommands(processId, identifyId ?? string.Empty)
                                        .FirstOrDefault(c => c.CommandName.Trim().ToLower() == "next");


            return ExecuteCommand(runtime, command, identifyId, impersonatedIdentifyId);
        }

        public static bool ExecuteCommand(WorkflowRuntime runtime,
                                          WorkflowCommand command,
                                          string identifyId = null,
                                          string impersonatedIdentifyId = null)
        {
            var cmdExecResult = runtime.ExecuteCommand(command,
                                           identifyId ?? string.Empty,
                                           impersonatedIdentifyId ?? string.Empty);

            return cmdExecResult.WasExecuted;
        }

        public static AntWayProcessView GetAntWayProcess(WorkflowRuntime runtime, 
                                                   string localizador, string identifyId = null)
        {
            var result = new AntWayProcessView();

            var locatorPersistence = new LocatorPersistence
            {
                IDALocators = PersistenceObjectsFactory.GetIDALLocatorsObject(),
            };
            var wfInstance = locatorPersistence.GetWorkflowLocator(localizador);

            if (wfInstance == null) return result;

            var commands1 = runtime.GetAvailableCommands(wfInstance.WFProcessGuid, identifyId ?? string.Empty);

            result.AvailableCommands = commands1
                                        .Select(c => c.CommandName.ToLower().Replace(" ", ""))
                                        .ToList();
            result.CurrentActivityName = runtime.GetCurrentActivityName(wfInstance.WFProcessGuid);
            result.CurrentActivityState = runtime.GetCurrentStateName(wfInstance.WFProcessGuid);

            return result;
        }
    }
}
