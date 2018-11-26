using System;
using OptimaJet.Workflow.Core.Runtime;

namespace OptimaJet.Workflow.Core.Model
{
    /// <summary>
    /// Represent a reference on action in a process scheme
    /// </summary>
    public class ActionDefinitionReference
    {
        /// <summary>
        /// Name of the action
        /// </summary>
        public  string ActionName { get; set; }

        /// <summary>
        /// Execution order of the action
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Additional parameter which is passed to the appropriate methods <see cref="IWorkflowActionProvider"/>
        /// </summary>
        public string ActionParameter { get; set; }

        /// <summary>
        /// Create ActionDefinitionReference object
        /// </summary>
        /// <param name="actionName">Name of the action</param>
        /// <param name="order">Execution order of the action</param>
        /// <param name="parameter">Additional parameter which is passed to the appropriate methods <see cref="IWorkflowActionProvider"/></param>
        /// <returns>ActionDefinitionReference object</returns>
        public static ActionDefinitionReference Create(string actionName, string order, string parameter)
        {
            return new ActionDefinitionReference() {ActionName = actionName, Order = Int32.Parse(order), ActionParameter = parameter};
        }

        public ActionDefinitionReference Clone()
        {
            return MemberwiseClone() as ActionDefinitionReference;
        }
    }
}
