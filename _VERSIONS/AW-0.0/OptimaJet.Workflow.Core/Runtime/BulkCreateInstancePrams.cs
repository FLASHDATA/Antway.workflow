using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimaJet.Workflow.Core.Runtime
{
    /// <summary>
    /// Object that represents information which is required for mass creation of processes
    /// </summary>
    public class BulkCreateInstancePrams
    {
        internal Guid Id { get; }

        /// <summary>
        /// Code of the scheme
        /// </summary>
        public string SchemeCode { get; set; }

        /// <summary>
        /// The parameters for creating scheme of process (defining parameters)
        /// </summary>
        public IDictionary<string, object> SchemeCreationParameters { get; set; }

        /// <summary>
        /// List of process IDs to create
        /// </summary>
        public List<Guid> ProcessIds { get; set; }

        /// <summary>
        /// Parameters to be passed to the process as the initial. Common for all creted processes.
        /// </summary>
        public IDictionary<string, object> InitialProcessParameters { get; set; }

        /// <summary>
        /// Process specific process parameters
        /// </summary>
        public Dictionary<Guid, IDictionary<string, object>> ProcessSpecificProcessPrarameters { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="processIds">List of process IDs to create</param>
        public BulkCreateInstancePrams(string schemeCode, List<Guid> processIds)
        {
            SchemeCode = schemeCode;
            ProcessIds = processIds;
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// The user id which execute initial command if command is available.  Common for all creted processes.
        /// </summary>
        public string IdentityId { get; set; }

        /// <summary>
        /// The user id for whom executes initial command if command is available.  Common for all creted processes.
        /// </summary>
        public string ImpersonatedIdentityId { get; set; }

        /// <summary>
        /// The user id which execute initial command if command is available.  Process specific values
        /// </summary>
        public Dictionary<Guid, string> ProcessSpecificIdentityIds { get; set; }

        /// <summary>
        /// The user id for whom executes initial command if command is available. Process specific values
        /// </summary>
        public Dictionary<Guid, string> ProcessSpecificImpersonatedIdentityIds { get; set; }
    }
}
