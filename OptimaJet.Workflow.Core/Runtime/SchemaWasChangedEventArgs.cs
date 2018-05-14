using System;

namespace OptimaJet.Workflow.Core.Runtime
{
    /// <summary>
    /// Represent information about changed scheme of a process 
    /// </summary>
    public class SchemaWasChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Id of the process whose scheme was changed
        /// </summary>
        public Guid ProcessId { get; set; }

        /// <summary>
        /// Actual scheme id
        /// </summary>
        public Guid SchemeId { get; set; }

        /// <summary>
        /// True if the scheme of the process was change due to scheme was obsolete
        /// </summary>
        public bool SchemaWasObsolete { get; set; }

        /// <summary>
        /// True if the scheme of the procee was changed due to process creating parameters was changed
        /// </summary>
        public bool DeterminingParametersWasChanged { get; set; }
    }
}
