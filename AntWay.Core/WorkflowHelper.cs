using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.CodeActions;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.Core.Subprocess;

namespace AntWay.Core
{
    public static class WorkflowHelper
    {
        private static List<string> _registeredTypeNames = new List<string>
        {
            "String", "Char",
            "Byte", "Int16", "Int32", "Int64",
            "Single", "Double", "Decimal",
            "DateTime"
        };


        public static ProcessDefinition GetProcessDefinition(this WorkflowRuntime runtime,
                                                            string schemecode)
        {
            List<CodeActionDefinition> globalActions;
            ProcessDefinition pd;
            globalActions =
                runtime.PersistenceProvider.LoadGlobalParameters<CodeActionDefinition>("CodeAction");
            pd = InitializeProcessDefinition(runtime, schemecode, "", "");
            pd.CodeActions.AddRange(globalActions);

            pd.CodeActions = pd.CodeActions.Select(ca => ca).ToList();

            return pd;
        }

        private static ProcessDefinition InitializeProcessDefinition(this WorkflowRuntime runtime, string schemecode, string schemeid, string processid)
        {
            ProcessDefinition pd = null;
            if (!string.IsNullOrWhiteSpace(processid))
            {
                var processId = new Guid(processid);
                var pi = runtime.Builder.GetProcessInstance(processId);
                runtime.PersistenceProvider.FillSystemProcessParameters(pi);
                pd = pi.ProcessScheme.Clone();
                pd.AdditionalParams.Clear();
                if (!pd.AdditionalParams.ContainsKey("ProcessParameters"))
                    pd.AdditionalParams.Add("ProcessParameters", pi.ProcessParameters);
                var subprocessActivities = new List<string>();
                var processTree = runtime.GetProcessInstancesTree(processId);
                if (processTree != null)
                {
                    void AddSubprocessActivity(ProcessInstancesTree tree, List<string> acc, bool includeCurrent)
                    {
                        if (includeCurrent)
                            acc.Add(runtime.GetCurrentActivityName(tree.Id));
                        foreach (var child in tree.Children)
                        {
                            AddSubprocessActivity(child, acc, true);
                        }
                    }

                    AddSubprocessActivity(processTree, subprocessActivities, false);
                }

                if (pd.AdditionalParams.ContainsKey("SubprocessCurrentActivities"))
                    pd.AdditionalParams.Remove("SubprocessCurrentActivities");
                pd.AdditionalParams.Add("SubprocessCurrentActivities", subprocessActivities);



            }
            else if (!string.IsNullOrWhiteSpace(schemeid))
            {
                pd = runtime.Builder.GetProcessScheme(new Guid(schemeid));
                pd.AdditionalParams.Clear();
            }
            else if (!string.IsNullOrEmpty(schemecode))
            {
                pd = runtime.Builder.GetProcessSchemeForDesigner(schemecode);
                pd.AdditionalParams.Clear();
            }
            else
            {
                pd = new ProcessDefinition();
            }

            InitializeAdditionalParams(runtime, pd);
            return pd;
        }

        private static void InitializeAdditionalParams(WorkflowRuntime runtime, ProcessDefinition pd)
        {
            if (!pd.AdditionalParams.ContainsKey("Rules"))
                pd.AdditionalParams.Add("Rules", runtime.RuleProvider.GetRules());
            if (!pd.AdditionalParams.ContainsKey("TimerTypes"))
                pd.AdditionalParams.Add("TimerTypes", Enum.GetValues(typeof(TimerType)).Cast<TimerType>().Select(t => t.ToString()));

            if (!pd.AdditionalParams.ContainsKey("Conditions"))
            {
                List<string> conditions;
                try //Do not transfer try/catch to Java Version
                {
                    conditions = runtime.ActionProvider.GetConditions();
                }
                catch (NotImplementedException)
                {
                    conditions = runtime.ActionProvider.GetActions();
                }

                pd.AdditionalParams.Add("Conditions", conditions);
            }

            if (!pd.AdditionalParams.ContainsKey("Actions"))
                pd.AdditionalParams.Add("Actions", runtime.ActionProvider.GetActions());

            if (!pd.AdditionalParams.ContainsKey("Namespaces"))
                pd.AdditionalParams.Add("Usings", CodeActionsCompiller.Usings);
            if (!pd.AdditionalParams.ContainsKey("Types"))
                pd.AdditionalParams.Add("Types", _registeredTypeNames);
        }
    }

}
