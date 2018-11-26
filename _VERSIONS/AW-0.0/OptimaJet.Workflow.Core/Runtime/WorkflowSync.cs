using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OptimaJet.Workflow.Core.Persistence;

namespace OptimaJet.Workflow.Core.Runtime
{
    public sealed class WorkflowSync : IDisposable
    {
        private bool _isDisposed;

        private readonly WorkflowRuntime _runtime;

        private readonly Guid _processId;

        private readonly AutoResetEvent _handle;

        private List<ProcessStatus> _statusesForWaiting;

        private bool _wasSet;

        public WorkflowSync(WorkflowRuntime runtime, Guid processId)
        {
            if (runtime == null) throw new ArgumentNullException("runtime");
            if (processId == Guid.Empty) throw new ArgumentOutOfRangeException("processId");

            _isDisposed = false;
            _runtime = runtime;
            _processId = processId;
            _handle = new AutoResetEvent(false);
        }

        public void StatrtWaitingFor(IEnumerable<ProcessStatus> statuses)
        {
            _handle.Reset();
            _wasSet = false;

            _statusesForWaiting = statuses.ToList();

            _runtime.ProcessStatusChanged += RuntimeProcessStatusChanged;

            if (!_wasSet)
            {
                var currentStatus = _runtime.GetProcessStatus(_processId);
                if (_statusesForWaiting.Contains(currentStatus))
                    _handle.Set();
            }
        }

        private void RuntimeProcessStatusChanged(object sender, ProcessStatusChangedEventArgs e)
        {
            if (_statusesForWaiting.Contains(e.NewStatus))
            {
                _handle.Set();
                _wasSet = true;
            }
        }

        public void Wait (TimeSpan timeout)
        {
            _handle.WaitOne(timeout);
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _runtime.ProcessStatusChanged -= RuntimeProcessStatusChanged;
                _isDisposed = true;
            }
        }
    }
}
