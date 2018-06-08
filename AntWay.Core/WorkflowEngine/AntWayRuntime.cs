using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow;
using System.Collections.Specialized;
using System.IO;
using Antway.Core;
using OptimaJet.Workflow.Core.Model;

namespace AntWay.Core.WorkflowEngine
{
    public class AntWayCommandExeutionResult: CommandExeutionResult
    {

    }

    public class AntWayProcessInstance
    {
        protected ProcessInstance ProcessInstance { get; set; }

        public AntWayProcessInstance(ProcessInstance processInstance)
        {
            ProcessInstance = processInstance;
        }

        public KeyValuePair<string, object>? GetParameter(string name)
        {
            var pd = ProcessInstance.GetParameter(name);
            if (pd == null) return null;

            var result = new KeyValuePair<string, object>(pd.Name, pd.Value);
            return result;
        }
    }

    public class AntWayCommand
    {
        public WorkflowCommand WorkflowCommand { get; set; }

        public AntWayCommand(WorkflowCommand workflowCommand)
        {
            workflowCommand = WorkflowCommand;
        }

        public string CommandName => WorkflowCommand.CommandName;
    }

    public class AntWayRuntime
    {
        public WorkflowRuntime WorkflowRuntime { get; set; }
        
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

        public AntWayCommandExeutionResult ExecuteCommand(AntWayCommand command, string identityId, string impersonatedIdentityId)
        {
            var commandExeutionResult =  WorkflowRuntime
                                            .ExecuteCommand(command.WorkflowCommand, identityId, impersonatedIdentityId);

            var result = new AntWayCommandExeutionResult
            {
                ProcessInstance = commandExeutionResult.ProcessInstance,
                WasExecuted = commandExeutionResult.WasExecuted
            };

            return result;
        }

        public AntWayProcessInstance GetProcessInstanceAndFillProcessParameters(Guid processId)
        {
            var processInstance = WorkflowRuntime.GetProcessInstanceAndFillProcessParameters(processId);
            return new AntWayProcessInstance(processInstance);
        }
    }
}
