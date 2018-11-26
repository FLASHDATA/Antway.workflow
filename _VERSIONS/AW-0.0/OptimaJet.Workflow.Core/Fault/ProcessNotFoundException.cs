using System;

namespace OptimaJet.Workflow.Core.Fault
{
    public class ProcessNotFoundException : Exception
    {
        [Obsolete("Use ProcessNotFoundException(Guid) constructor")]
        public ProcessNotFoundException()
        {
        }

        public ProcessNotFoundException(Guid id) : base(string.Format("Process with the id = {0} not found", id))
        {
        }
    }
}
