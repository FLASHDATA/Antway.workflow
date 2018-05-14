using System;

namespace OptimaJet.Workflow.Core.Fault
{
    public class ProcessAlreadyExistsException : Exception
    {
        [Obsolete("Use ProcessAlreadyExistsException(Guid) constructor")]
        public ProcessAlreadyExistsException()
        {
        }

        public ProcessAlreadyExistsException(Guid id) : base(string.Format("Process with the id = {0} already exists", id))
        {
        }
    }
}
