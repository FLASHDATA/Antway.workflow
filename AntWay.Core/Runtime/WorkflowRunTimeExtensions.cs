using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antway.Core;
using AntWay.Persistence.Model;
using AntWay.Persistence.Provider;
using OptimaJet.Workflow.Core.Runtime;

namespace AntWay.Core.RunTime
{
    internal static class WorkflowRuntimeExtensions
    {
        public static bool ExecutecommandNext(WorkflowRuntime runtime, 
                                       Guid wfProcessGuid,
                                       string identifyId = null,
                                       string impersonatedIdentifyId = null)
        {
            return Executecommand(runtime, wfProcessGuid, "next", identifyId, impersonatedIdentifyId);
        }


        public static bool Executecommand(WorkflowRuntime runtime,
                                          Guid wfProcessGuid, string commandName,
                                          string identifyId = null,
                                          string impersonatedIdentifyId = null)
        {
            WorkflowCommand command = runtime
                                      .GetAvailableCommands(wfProcessGuid, identifyId ?? string.Empty)
                                      .FirstOrDefault(c => c.CommandName.Trim().ToLower() == commandName.ToLower());

            if (command == null) return false;

            var cmdExecResult = runtime.ExecuteCommand(command,
                                                       identifyId ?? string.Empty,
                                                       impersonatedIdentifyId ?? string.Empty);

            return cmdExecResult.WasExecuted;
        }

        public static AntWayProcessView GetAntWayProcess(WorkflowRuntime runtime, 
                                                   string localizador, string identifyId = null)
        {
            var result = new AntWayProcessView();

            var processPersistence = new ProcessPersistence
            {
                IDALProcessPersistence = PersistenceObjectsFactory.GetIDALWFLocatorObject(),
            };
            var wfInstance = processPersistence.GetWorkflowLocator(localizador);

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
