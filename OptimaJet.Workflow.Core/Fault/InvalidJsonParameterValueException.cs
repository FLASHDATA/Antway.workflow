using System;

namespace OptimaJet.Workflow.Core.Fault
{
    public class InvalidJsonParameterValueException : Exception
    {
        public InvalidJsonParameterValueException(string message, params object[] args) : base(string.Format(message, args))
        {
            
        }
    }
}
