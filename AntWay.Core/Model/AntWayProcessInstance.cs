using System;
using System.Collections.Generic;
using OptimaJet.Workflow.Core.Model;

namespace AntWay.Core.Model
{
    //TODO: VALORAR ELIMINAR ESTA CLASE
    public class AntWayProcessInstance
    {
        public ProcessInstance ProcessInstance { get; set; }

        public AntWayProcessInstance(ProcessInstance processInstance)
        {
            ProcessInstance = processInstance;
        }

        public KeyValuePair<string, object>? GetParameter(string name)
        {
            var pd = ProcessInstance.GetParameter(name);
            if (pd == null) return null;

            var cleanValue = pd?.Value.ToString() ?? "";
            cleanValue = cleanValue.Replace("\"", "");

            var result = new KeyValuePair<string, object>(pd.Name, cleanValue);

            return result;
        }
    }
}
