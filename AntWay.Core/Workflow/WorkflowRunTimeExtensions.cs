using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Runtime;

namespace AntWay.Core
{
    public static class WorkflowRuntimeExtensions
    {
        public static bool ExecutecommandNext(WorkflowRuntime worklowRuntime, 
                                       Guid wfProcessGuid,
                                       string identifyId = null)
        {
            return Executecommand(worklowRuntime, wfProcessGuid, "next", identifyId);
        }


        public static bool Executecommand(WorkflowRuntime worklowRuntime,
                                          Guid wfProcessGuid, string commandName,
                                          string identifyId = null)
        {
            WorkflowCommand command = worklowRuntime
                                      .GetAvailableCommands(wfProcessGuid, identifyId ?? string.Empty)
                                      .FirstOrDefault(c => c.CommandName.Trim().ToLower() == commandName.ToLower());

            if (command == null) return false;

            var cmdExecResult = worklowRuntime.ExecuteCommand(command, identifyId ?? string.Empty, string.Empty);

            return cmdExecResult.WasExecuted;
        }
    }
}
