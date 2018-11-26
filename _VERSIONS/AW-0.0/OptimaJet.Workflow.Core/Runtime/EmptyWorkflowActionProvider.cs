using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Runtime
{
    /// <summary>
    /// Empty action provider for system purpose
    /// </summary>
    public class EmptyWorkflowActionProvider : IWorkflowActionProvider
    {
        public void ExecuteAction(string name, ProcessInstance processInstance, WorkflowRuntime runtime,
            string actionParameter)
        {
        }

        public async Task ExecuteActionAsync(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }


        public bool ExecuteCondition(string name, ProcessInstance processInstance, WorkflowRuntime runtime,
            string actionParameter)
        {
            return false;
        }

        public async Task<bool> ExecuteConditionAsync(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }

        public bool IsActionAsync(string name)
        {
            return false;
        }

        public bool IsConditionAsync(string name)
        {
            return false;
        }

        public List<string> GetActions()
        {
            return new List<string>();
        }

        public List<string> GetConditions()
        {
            return new List<string>();
        }
    }
}
