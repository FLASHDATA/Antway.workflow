using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;

namespace OptimaJet.Workflow.Core.CodeActions
{
    /// <summary>
    /// Provides invoke of code actions by name
    /// </summary>
    public sealed class CodeActionsInvoker
    {
        private readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, bool>> _conditions =
            new Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, bool>>();

        private readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task<bool>>> _asyncConditions =
            new Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task<bool>>>();

        private readonly Dictionary<string, Action<ProcessInstance, WorkflowRuntime, string>> _actions =
            new Dictionary<string, Action<ProcessInstance, WorkflowRuntime, string>>();

        private readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task>> _asyncActions =
            new Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task>>();

        private readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, IEnumerable<string>>>
            _getRules =
                new Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, IEnumerable<string>>>();


        private readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, string, bool>> _checkRules =
            new Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, string, bool>>();

        public void AddCompilledType(Type compilledType)
        {
            var methodInfos = compilledType.GetMethods(BindingFlags.Static | BindingFlags.Public);

            foreach (var method in methodInfos)
            {
                MethodInfo m = method;
                var isAsync = false;
                var value = m.Name.Split('_').Last();
                if (value.EndsWith("Async",StringComparison.Ordinal))
                {
                    isAsync = true;
                    value = value.Remove(value.Length - 5, 5);
                }

                var type = (CodeActionType) Enum.Parse(typeof (CodeActionType), value, true);

                switch (type)
                {
                    case CodeActionType.Action:
                        if (isAsync)
                        {
                            _asyncActions.Add(m.Name, (pi, r, p, t) => GetActionTask(pi, r, p, t, m));
                        }
                        else
                        {
                            _actions.Add(m.Name, (pi, r, p) => InvokeAction(pi, r, p, m));
                        }
                        break;
                    case CodeActionType.Condition:
                        if (isAsync)
                        {
                            _asyncConditions.Add(m.Name, (pi, r, p, t) => GetConditionTask(pi, r, p, t, m));
                        }
                        else
                        {
                            _conditions.Add(m.Name, (pi, r, p) => InvokeCondition(pi, r, p, m));
                        }
                        break;
                    case CodeActionType.RuleCheck:
                        _checkRules.Add(m.Name, (pid, r, iid, p) => InvokeRuleCheck(pid, r, iid, p, m));
                        break;
                    case CodeActionType.RuleGet:
                        _getRules.Add(m.Name, (pid, r, p) => InvokeRuleGet(pid, r, p, m));
                        break;
                }
            }
        }

        public bool InvokeCondition(string name, ProcessInstance processInstance, WorkflowRuntime runtime,
            string parameter)
        {
            return _conditions[CodeActionUtils.GetMethodName(name, CodeActionType.Condition)].Invoke(processInstance, runtime, parameter);
        }

        public async Task<bool> InvokeConditionAsync(string name, ProcessInstance processInstance, WorkflowRuntime runtime,
            string parameter, CancellationToken token)
        {
            return await _asyncConditions[CodeActionUtils.GetAsyncMethodName(name, CodeActionType.Condition)].Invoke(processInstance, runtime, parameter, token).ConfigureAwait(false);
        }

        public void InvokeAction(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string parameter)
        {
            _actions[CodeActionUtils.GetMethodName(name, CodeActionType.Action)].Invoke(processInstance, runtime, parameter);
        }

        public async Task InvokeActionAsync(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string parameter, CancellationToken token)
        {
            await _asyncActions[CodeActionUtils.GetAsyncMethodName(name, CodeActionType.Action)].Invoke(processInstance, runtime, parameter, token).ConfigureAwait(false);
        }

        public IEnumerable<string> InvokeRuleGet(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string parameter)
        {
            return _getRules[CodeActionUtils.GetMethodName(name, CodeActionType.RuleGet)].Invoke(processInstance, runtime, parameter);
        }

        public bool InvokeRuleCheck(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string identityId, string parameter)
        {
            return _checkRules[CodeActionUtils.GetMethodName(name, CodeActionType.RuleCheck)].Invoke(processInstance, runtime, identityId,
                parameter);
        }

        public bool ExistsCondition(string name)
        {
            return _conditions.ContainsKey(CodeActionUtils.GetMethodName(name, CodeActionType.Condition)) || _asyncConditions.ContainsKey(CodeActionUtils.GetAsyncMethodName(name,CodeActionType.Condition));
        }

        public bool ExistsAction(string name)
        {
            return _actions.ContainsKey(CodeActionUtils.GetMethodName(name, CodeActionType.Action)) || _asyncActions.ContainsKey(CodeActionUtils.GetAsyncMethodName(name, CodeActionType.Action));
        }

        public bool ExistsRuleGet(string name)
        {
            return _getRules.ContainsKey(CodeActionUtils.GetMethodName(name,CodeActionType.RuleGet));
        }

        public bool ExistsRuleCheck(string name)
        {
            return _checkRules.ContainsKey(CodeActionUtils.GetMethodName(name, CodeActionType.RuleCheck));
        }

        public bool IsConditionAsync(string name)
        {
            return _asyncConditions.ContainsKey(CodeActionUtils.GetAsyncMethodName(name, CodeActionType.Condition));
        }

        public bool IsActionAsync(string name)
        {
            return _asyncActions.ContainsKey(CodeActionUtils.GetAsyncMethodName(name, CodeActionType.Action));
        }

        private bool InvokeCondition(ProcessInstance processInstance, WorkflowRuntime runtime, string parameter,
            MethodInfo methodInfo)
        {
            return (bool) methodInfo.Invoke(null, new object[] {processInstance, runtime, parameter});
        }

        private Task<bool> GetConditionTask(ProcessInstance processInstance, WorkflowRuntime runtime, string parameter, CancellationToken token,
            MethodInfo methodInfo)
        {
            return (Task<bool>)methodInfo.Invoke(null, new object[] { processInstance, runtime, parameter, token });
        }

        private void InvokeAction(ProcessInstance processInstance, WorkflowRuntime runtime, string parameter,
            MethodInfo methodInfo)
        {
            methodInfo.Invoke(null, new object[] {processInstance, runtime, parameter});
        }

        private Task GetActionTask(ProcessInstance processInstance, WorkflowRuntime runtime, string parameter, CancellationToken token,
            MethodInfo methodInfo)
        {
            return (Task)methodInfo.Invoke(null, new object[] { processInstance, runtime, parameter, token });
        }

        private IEnumerable<string> InvokeRuleGet(ProcessInstance processInstance, WorkflowRuntime runtime, string parameter, MethodInfo methodInfo)
        {
            return (IEnumerable<string>)methodInfo.Invoke(null, new object[] { processInstance, runtime, parameter });
        }

        private bool InvokeRuleCheck(ProcessInstance processInstance, WorkflowRuntime runtime, string identityId, string parameter, MethodInfo methodInfo)
        {
            return (bool)methodInfo.Invoke(null, new object[] { processInstance, runtime, identityId, parameter });
        }
    }
}
