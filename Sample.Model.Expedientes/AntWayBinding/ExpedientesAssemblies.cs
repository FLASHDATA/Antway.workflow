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
    public class ExpedientesAssemblies : IAssemblies
    {
        public void RegisterAssembliesForWorkflowDesigner(WorkflowRuntime runtime)
        {
            var assemblies = GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                runtime.RegisterAssemblyForCodeActions(assembly);
            }
        }

        public void RegisterAssembliesForServiceLocator()
        {
            var assemblies = GetAssemblies();

            foreach(Assembly assembly in assemblies)
            {
                Assembly.Load(assembly.FullName);
            }
        }

        protected List<Assembly> GetAssemblies()
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
