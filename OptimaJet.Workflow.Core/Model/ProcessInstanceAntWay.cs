using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimaJet.Workflow.Core.Model
{
    public partial class ProcessInstance
    {
        public List<ParameterDefinitionWithValue> GetParameters()
        {
            return ProcessParameters.Select(p => p).ToList();
        }
    }
}
