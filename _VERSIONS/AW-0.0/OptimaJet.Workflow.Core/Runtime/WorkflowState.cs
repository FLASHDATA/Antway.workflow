using System;

namespace OptimaJet.Workflow.Core.Runtime
{
    /// <summary>
    /// Represent a state of a process for use in an application
    /// </summary>
#if !NETCOREAPP
    [Serializable]
#endif
    public class WorkflowState
    {
        /// <summary>
        /// System name of the state
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Localized name of the state
        /// </summary>
        public string VisibleName { get; set; }

        /// <summary>
        /// Code of the scheme of the process
        /// </summary>
        public string SchemeCode { get; set; }
    }
}
