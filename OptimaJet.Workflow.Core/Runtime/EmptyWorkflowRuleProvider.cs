using OptimaJet.Workflow.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OptimaJet.Workflow.Core.Runtime
{
    /// <summary>
    /// Empty rule provider for system purpose
    /// </summary>
    public class EmptyWorkflowRuleProvider : IWorkflowRuleProvider
    {
        public List<string> GetRules()
        {
            return new List<string>();
        }

        public bool Check(ProcessInstance processInstance, WorkflowRuntime runtime, string identityId, string ruleName, string parameter)
        {
            return false;
        }

        public IEnumerable<string> GetIdentities(ProcessInstance processInstance, WorkflowRuntime runtime, string ruleName, string parameter)
        {
            return new List<string>();
        }
    }
}
