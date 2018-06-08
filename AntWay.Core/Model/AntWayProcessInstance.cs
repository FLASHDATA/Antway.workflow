using System;
using System.Collections.Generic;
using OptimaJet.Workflow.Core.Model;

namespace AntWay.Core.Model
{
    public class AntWayProcessInstance
    {
        protected ProcessInstance ProcessInstance { get; set; }

        public AntWayProcessInstance(ProcessInstance processInstance)
        {
            ProcessInstance = processInstance;
        }

        public KeyValuePair<string, object>? GetParameter(string name)
        {
            var pd = ProcessInstance.GetParameter(name);
            if (pd == null) return null;

            var result = new KeyValuePair<string, object>(pd.Name, pd.Value);
            return result;
        }
    }

 

}
