using System;
using System.Collections.Generic;

namespace OptimaJet.Workflow.Core.Fault
{
    public class WrongParameterTypeException : Exception
    {
        public List<Exception> ParseExceptions { get; }

        public WrongParameterTypeException(string parameterName, string parameterType, List<Exception> parseExceptions) : base(
            $"Parameter{parameterName} have a wrong type {parameterType}. It is impossible to transfor it to System.Type", parseExceptions.Count > 0 ? parseExceptions[0] : null)
        {
            ParseExceptions = parseExceptions;
        }
    }
}