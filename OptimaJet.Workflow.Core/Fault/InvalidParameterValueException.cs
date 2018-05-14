using System;

namespace OptimaJet.Workflow.Core.Fault
{
    public class InvalidParameterValueException: Exception
    {
        public InvalidParameterValueException(string message, params object[] args)
            : base(string.Format(message, args))
        {
            
        }
    }
}
