using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;

namespace OptimaJet.Workflow.Core.CodeActions
{
    public static class CodeActionUtils
    {
        public static CodeActionsInvoker GetCodeActionsInvokerForAction(string actionName, WorkflowRuntime runtime, ProcessDefinition processDefinition, out bool isGlobal)
        {
            isGlobal = false;
            
            if (runtime.ExecutionSearchOrder.IsLocalPrevaleGlobal())
            {
                var codeActionsInvokerFromScheme = GetCodeActionsInvokerFromScheme(runtime, processDefinition);

                if (codeActionsInvokerFromScheme != null && codeActionsInvokerFromScheme.ExistsAction(actionName))
                {
                    return codeActionsInvokerFromScheme;
                }
                
                if (runtime.GlobalActionsInvoker != null && runtime.GlobalActionsInvoker.ExistsAction(actionName))
                {
                    isGlobal = true;
                    return runtime.GlobalActionsInvoker;
                }

                return null;
            }
            else
            {
                if (runtime.GlobalActionsInvoker != null && runtime.GlobalActionsInvoker.ExistsAction(actionName))
                {
                    isGlobal = true;
                    return runtime.GlobalActionsInvoker;
                }

                var codeActionsInvokerFromScheme = GetCodeActionsInvokerFromScheme(runtime, processDefinition);

                if (codeActionsInvokerFromScheme != null && codeActionsInvokerFromScheme.ExistsAction(actionName))
                {
                    return codeActionsInvokerFromScheme;
                }

                return null;
            }
        }

        public static CodeActionsInvoker GetCodeActionsInvokerForCondition(string conditionName, WorkflowRuntime runtime, ProcessDefinition processDefinition, out bool isGlobal)
        {
            isGlobal = false;

            if (runtime.ExecutionSearchOrder.IsLocalPrevaleGlobal())
            {
                var codeActionsInvokerFromScheme = GetCodeActionsInvokerFromScheme(runtime, processDefinition);

                if (codeActionsInvokerFromScheme != null && codeActionsInvokerFromScheme.ExistsCondition(conditionName))
                {
                    return codeActionsInvokerFromScheme;
                }

                if (runtime.GlobalActionsInvoker != null && runtime.GlobalActionsInvoker.ExistsCondition(conditionName))
                {
                    isGlobal = true;
                    return runtime.GlobalActionsInvoker;
                }

                return null;
            }
            else
            {
                if (runtime.GlobalActionsInvoker != null && runtime.GlobalActionsInvoker.ExistsCondition(conditionName))
                {
                    isGlobal = true;
                    return runtime.GlobalActionsInvoker;
                }

                var codeActionsInvokerFromScheme = GetCodeActionsInvokerFromScheme(runtime, processDefinition);

                if (codeActionsInvokerFromScheme != null && codeActionsInvokerFromScheme.ExistsCondition(conditionName))
                {
                    return codeActionsInvokerFromScheme;
                }

                return null;
            }
        }

        public static CodeActionsInvoker GetCodeActionsInvokerForRuleGet(string ruleGetName, WorkflowRuntime runtime, ProcessDefinition processDefinition, out bool isGlobal)
        {

            isGlobal = false;

            if (runtime.ExecutionSearchOrder.IsLocalPrevaleGlobal())
            {
                var codeActionsInvokerFromScheme = GetCodeActionsInvokerFromScheme(runtime, processDefinition);

                if (codeActionsInvokerFromScheme != null && codeActionsInvokerFromScheme.ExistsRuleGet(ruleGetName))
                {
                    return codeActionsInvokerFromScheme;
                }

                if (runtime.GlobalActionsInvoker != null && runtime.GlobalActionsInvoker.ExistsRuleGet(ruleGetName))
                {
                    isGlobal = true;
                    return runtime.GlobalActionsInvoker;
                }

                return null;
            }
            else
            {

                if (runtime.GlobalActionsInvoker != null && runtime.GlobalActionsInvoker.ExistsRuleGet(ruleGetName))
                {
                    isGlobal = true;
                    return runtime.GlobalActionsInvoker;
                }

                var codeActionsInvokerFromScheme = GetCodeActionsInvokerFromScheme(runtime, processDefinition);

                if (codeActionsInvokerFromScheme != null && codeActionsInvokerFromScheme.ExistsRuleGet(ruleGetName))
                {
                    return codeActionsInvokerFromScheme;
                }

                return null;
            }
        }

        public static CodeActionsInvoker GetCodeActionsInvokerForRuleCheck(string ruleCheckName, WorkflowRuntime runtime, ProcessDefinition processDefinition,  out bool isGlobal)
        {
            isGlobal = false;

            if (runtime.ExecutionSearchOrder.IsLocalPrevaleGlobal())
            {
                var codeActionsInvokerFromScheme = GetCodeActionsInvokerFromScheme(runtime, processDefinition);

                if (codeActionsInvokerFromScheme != null && codeActionsInvokerFromScheme.ExistsRuleCheck(ruleCheckName))
                {
                    return codeActionsInvokerFromScheme;
                }

                if (runtime.GlobalActionsInvoker != null && runtime.GlobalActionsInvoker.ExistsRuleCheck(ruleCheckName))
                {
                    isGlobal = true;
                    return runtime.GlobalActionsInvoker;
                }

                return null;
            }
            else
            {
                if (runtime.GlobalActionsInvoker != null && runtime.GlobalActionsInvoker.ExistsRuleCheck(ruleCheckName))
                {
                    isGlobal = true;
                    return runtime.GlobalActionsInvoker;
                }

                var codeActionsInvokerFromScheme = GetCodeActionsInvokerFromScheme(runtime, processDefinition);

                if (codeActionsInvokerFromScheme != null && codeActionsInvokerFromScheme.ExistsRuleCheck(ruleCheckName))
                {
                    return codeActionsInvokerFromScheme;
                }

                return null;
            }
        }

        private static CodeActionsInvoker GetCodeActionsInvokerFromScheme(WorkflowRuntime runtime, ProcessDefinition processDefinition)
        {
            CodeActionsInvoker codeActionsInvokerFromScheme = null; 
            if (processDefinition.IsSubprocessScheme && processDefinition.RootSchemeId.HasValue)
            {
                //required to get teh root scheme
                var rootProcessDefinition = runtime.Builder.GetProcessScheme(processDefinition.RootSchemeId.Value);
                codeActionsInvokerFromScheme = rootProcessDefinition.CodeActionsInvoker;
            }
            else if (!processDefinition.IsSubprocessScheme)
            {
                codeActionsInvokerFromScheme = processDefinition.CodeActionsInvoker;
            }

            return codeActionsInvokerFromScheme;
        }

        public static string GetClassName(CodeActionDefinition codeActionDefinition)
        {
            return $"{codeActionDefinition.Name}_{codeActionDefinition.Type}_Class".ToValidCSharpIdentifierName();
        }

        public static string GetNamespaceName(string namespacePostfix)
        {
            return $"{NamespaceForCodeActions}_{namespacePostfix}".ToValidCSharpIdentifierName();
        }

        public static string GetMethodName(CodeActionDefinition codeActionDefinition)
        {
            return codeActionDefinition.IsAsync
                ? GetAsyncMethodName(codeActionDefinition.Name, codeActionDefinition.Type)
                : GetMethodName(codeActionDefinition.Name, codeActionDefinition.Type);
        }

        public static string GetMethodName(string name, CodeActionType type)
        {
            return $"{name}_{type}".ToValidCSharpIdentifierName();
        }

        public static string GetAsyncMethodName(string name, CodeActionType type)
        {
            return $"{GetMethodName(name, type)}Async".ToValidCSharpIdentifierName();
        }

        public static string NamespaceForCodeActions = "CodeActions";
    }
}
