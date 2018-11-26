using System;
using System.Threading;

namespace OptimaJet.Workflow.Core.Bus
{
    /// <summary>
    /// Represent Event args with a execution result <see cref="ActivityExecutor"/>
    /// </summary>
    public sealed class ExecutionResponseEventArgs : EventArgs
    {
        /// <summary>
        /// Execution result
        /// </summary>
        public ExecutionResponseParameters Parameters { get; }

        /// <summary>
        /// Cancellation token that was passed from public api
        /// </summary>
        public CancellationToken Token { get; }

        /// <summary>
        /// Create ExecutionResponseEventArgs object
        /// </summary>
        /// <param name="parameters">Execution result</param>
        /// <param name="token">Cancellation token that was passed from public api</param>
        public ExecutionResponseEventArgs (ExecutionResponseParameters parameters, CancellationToken token)
        {
            Parameters = parameters;
            Token = token;
        }
    }
}
