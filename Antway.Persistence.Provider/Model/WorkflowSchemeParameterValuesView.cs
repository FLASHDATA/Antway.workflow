using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntWay.Persistence.Provider.Model
{
    public class WorkflowSchemeParameterValuesView
    {
        public WorkflowSchemeParameterView SchemeParameter { get; set; }
        public string Value { get; set; }

        public string PairParameterValue 
            => $"{SchemeParameter.SchemeParameter}/{Value}";
    }
}
