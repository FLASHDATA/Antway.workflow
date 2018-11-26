using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OptimaJet.Workflow.Core.Fault
{
    public class ImpossibleToSetStatusException : Exception
    {
        public ImpossibleToSetStatusException() { }
        public ImpossibleToSetStatusException(string message, Exception innerException) : base(message, innerException) { }
    }
}
