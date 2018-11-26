using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AntWay.Core.Activity;
using OptimaJet.Workflow.Core.Runtime;

namespace AntWay.Core.Runtime
{
    public interface IAssemblies
    {
        void RegisterAssembliesForWorkflowDesigner(WorkflowRuntime runtime);
        void RegisterAssembliesForServiceLocator();
        List<Type> GetTypes();
    }

    public abstract class AssembliesBase
    {
        public virtual void RegisterAssembliesForWorkflowDesigner(WorkflowRuntime runtime)
        {
            var assemblies = GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                runtime.RegisterAssemblyForCodeActions(assembly);
            }
        }

        public virtual void RegisterAssembliesForServiceLocator()
        {
            var assemblies = GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                Assembly.Load(assembly.FullName);
            }
        }

        public virtual List<Type> GetTypes()
        {
            var types = new List<Type>
            {
                typeof(AntWayActivityService),
            };

            var currentAssembly = this.GetType().GetTypeInfo().Assembly;

            var activityTypeInfoList = currentAssembly.DefinedTypes
                                        .Where(type => type.ImplementedInterfaces
                                                       .Any(inter => inter == typeof(IAntWayRuntimeActivity)))
                                        .ToList();

            foreach (var t in activityTypeInfoList)
            {
                Type myType1 = Type.GetType($"{t.FullName}, {t.Assembly.ManifestModule.Name.Replace(".dll", "")}");
                if (myType1 == null) continue;

                types.Add(myType1);
            }

            return types;
        }

        protected virtual List<Assembly> GetAssemblies()
        {
            throw new NotImplementedException();
        }
    }
}
