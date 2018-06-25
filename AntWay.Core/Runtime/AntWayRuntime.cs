using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow;
using System.Collections.Specialized;
using System.IO;
using AntWay.Core.Model;

namespace AntWay.Core.RunTime
{
    public class AntWayRuntime
    {
        protected WorkflowRuntime WorkflowRuntime { get; set; }


        public AntWayRuntime(WorkflowRuntime workflowRuntime)
        {
            WorkflowRuntime = workflowRuntime;
        }

        public void CreateInstance(string schemeCode, Guid processId)
        {
            WorkflowRuntime.CreateInstance(schemeCode, processId);
        }

        public string DesignerAPI(NameValueCollection pars, Stream filestream = null)
        {
            string result = WorkflowRuntime.DesignerAPI(pars, filestream, true);
            return result;
        }

        public List<AntWayCommand> GetAvailableCommands(Guid processId, string identityId = null)
        {
            var commands = WorkflowRuntime.GetAvailableCommands(processId, identityId ?? string.Empty);

            var result = commands
                            .Select(c => new AntWayCommand(c))
                            .ToList();

            return result;
        }

        public string GetCurrentStateName(Guid processId)
        {
            return WorkflowRuntime.GetCurrentStateName(processId);
        }

        public string GetCurrentActivityName(Guid processId)
        {
            return WorkflowRuntime.GetCurrentActivityName(processId);
        }

        public void ExecuteCommandNext(Guid processId)
        {
            WorkflowRuntimeExtensions.ExecutecommandNext(WorkflowRuntime, processId);
        }

        public bool ExecuteCommand(Guid processId, string commandName,
                                   string identifyId = null)
        {
            return WorkflowRuntimeExtensions.Executecommand(WorkflowRuntime, processId, 
                                                            commandName,
                                                            identifyId); 
        }


        public AntWayProcessInstance GetProcessInstanceAndFillProcessParameters(Guid processId)
        {
            var processInstance = WorkflowRuntime.GetProcessInstanceAndFillProcessParameters(processId);
            return new AntWayProcessInstance(processInstance);
        }
    }
}
