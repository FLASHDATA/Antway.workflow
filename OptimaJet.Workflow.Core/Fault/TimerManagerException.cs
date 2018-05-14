using System;

namespace OptimaJet.Workflow.Core.Fault
{
    public class TimerManagerException : Exception
    {
        public TimerManagerException(string message) : base(message)
        {
        }
    }
}
