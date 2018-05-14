using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimaJet.Workflow.Core.Model
{
    /// <summary>
    /// Type of transition in terms of subprocesses 
    /// </summary>
    public enum TransitionForkType
    {
        /// <summary>
        /// Not fork transition
        /// </summary>
        NotFork,

        /// <summary>
        /// Transition which start a subprocess
        /// </summary>
        ForkStart,

        /// <summary>
        /// Transition which end a subprocess
        /// </summary>
        ForkEnd,

        /// <summary>
        /// Fork transition with unknown status in case of process definition is not marked
        /// </summary>
        ForkUnknown
    }
}
