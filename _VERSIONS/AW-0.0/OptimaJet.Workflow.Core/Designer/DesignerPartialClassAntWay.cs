using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.Core.CodeActions;
using OptimaJet.Workflow.Core.Fault;
using Newtonsoft.Json;
using OptimaJet.Workflow.Core.Designer;
using OptimaJet.Workflow.Core.Subprocess;

namespace OptimaJet.Workflow
{
    public static partial class Designer
    {
        private static string NewScheme(WorkflowRuntime runtime, 
                                        string schemeid, string processid,
                                        List<CommandDefinition> commands = null)
        {
            ProcessDefinition pd = GetDefaultProcessDifinition(runtime, schemeid, processid);

            if (commands != null)
            {
                pd.Commands.AddRange(commands);
            }

            return JsonConvert.SerializeObject(pd);
        }

        private static ProcessDefinition GetDefaultProcessDifinition(WorkflowRuntime runtime, string schemeid, string processid)
        {
            List<CodeActionDefinition> globalActions;
            ProcessDefinition pd;

            globalActions =
                runtime.PersistenceProvider.LoadGlobalParameters<CodeActionDefinition>(
                    WorkflowRuntime.CodeActionsGlobalParameterName);

            pd = InitializeProcessDefinition(runtime, null, schemeid, processid);
            pd.CodeActions.AddRange(globalActions);

            pd.CodeActions = pd.CodeActions.Select(ca => ca.Encode()).ToList();

            pd.Commands = new List<CommandDefinition>()
            {
                new CommandDefinition
                {
                    Name = "next",
                    InputParameters = new List<ParameterDefinitionReference>(),
                },
            };

            return pd;
        }
    }
}
