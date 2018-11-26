using System;
using System.Collections.Generic;
using System.Linq;
using OptimaJet.Workflow.Core.Runtime;

namespace AntWay.Core.Model
{

    public class AntWayCommandParameter
    {
        public static AntWayCommandParameter Single
                                => new AntWayCommandParameter();

        public CommandParameter NewCommandParameter(string parameterName, object value,
                                      bool isRequired = false)
        {
            var result = new CommandParameter
            {
                ParameterName = parameterName,
                Value = value,
                IsRequired = isRequired
            };

            return result;
        }
    }
}
