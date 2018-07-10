using System;
using System.Collections.Generic;
using System.Linq;
using OptimaJet.Workflow.Core.Runtime;


namespace AntWay.Core.Model
{
    public class AntWayCommand
    {
        public WorkflowCommand WorkflowCommand { get; set; }

        public AntWayCommand(WorkflowCommand workflowCommand)
        {
            WorkflowCommand = workflowCommand;
        }

        public void SetParameter(string name, object value)
        {
            WorkflowCommand.SetParameter(name, value);
        }

        public string CommandName => WorkflowCommand.CommandName;
    }

    public class AntWayCommandParameter
    {
        public AntWayCommandParameter(string parameterName, object value)
        {
            ParameterName = parameterName;
            Value = value;
        }

        public AntWayCommandParameter(string parameterName, object value,
                                      bool isRequired, Type type)
        {
            ParameterName = parameterName;
            Value = value;
            IsRequired = isRequired;
            Type = type;
        }

        public string ParameterName { get; set; }
        public bool IsRequired { get; set; }
        public object Value { get; set; }
        //public object DefaultValue { get; set; }
        public Type Type { get; }
        //public string TypeName { get; set; }
    }
}
