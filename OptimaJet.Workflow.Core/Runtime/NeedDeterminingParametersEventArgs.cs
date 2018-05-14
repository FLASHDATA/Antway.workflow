using System;
using System.Collections.Generic;

namespace OptimaJet.Workflow.Core.Runtime
{
    /// <summary>
    /// Event args for request parameters for creating a process
    /// </summary>
    public class NeedDeterminingParametersEventArgs : EventArgs
    {
        /// <summary>
        /// Process id 
        /// </summary>
        public Guid ProcessId { get; set; }

        /// <summary>
        /// List of parameters for creating the process
        /// </summary>
        public IDictionary<string, object> DeterminingParameters { get; set; }
    }
}
