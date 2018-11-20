using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using OptimaJet.Workflow.Core.Runtime;
using AntWay.Core.Runtime;

namespace Sample.Model.Expedientes.AntWayBinding
{
    public class ExpedientesAssemblies : AssembliesBase, IAssemblies
    {
        protected override List<Assembly> GetAssemblies()
        {
            var result = new List<Assembly>
            {
                Assembly.GetAssembly(typeof(ActivityEnviarASignar)),
                Assembly.GetAssembly(typeof(ActivityEnviarASignarParametersOutput)),
            };

            return result;
        }
    }
}
