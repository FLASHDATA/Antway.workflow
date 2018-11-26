using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OptimaJet.Workflow.Core.Fault
{
    public class SchemeNotValidException : Exception
    {
        public SchemeNotValidException(string msg) : base(msg) { }
    }
}
