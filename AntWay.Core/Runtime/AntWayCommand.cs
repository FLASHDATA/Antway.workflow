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
            workflowCommand = WorkflowCommand;
        }

        public void SetParameter(string name, object value)
        {
            WorkflowCommand.SetParameter(name, value);
        }

        public string CommandName => WorkflowCommand.CommandName;
    }
}
