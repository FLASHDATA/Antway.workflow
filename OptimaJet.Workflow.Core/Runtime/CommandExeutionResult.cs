using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Runtime
{
    /// <summary>
    /// The result of command execution
    /// </summary>
    public class CommandExeutionResult
    {
        /// <summary>
        /// If true, the command was executed.
        /// </summary>
        public bool WasExecuted { get; set; }
        /// <summary>
        /// The process, after executing the command
        /// </summary>
        public ProcessInstance ProcessInstance { get; set; }
       
    }
}
