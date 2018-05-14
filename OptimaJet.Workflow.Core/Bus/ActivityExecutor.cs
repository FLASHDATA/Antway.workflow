using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.CodeActions;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;

namespace OptimaJet.Workflow.Core.Bus
{
    /// <summary>
    /// Provides choice of activity for execution <see cref="TransitionDefinition.Conditions"/> and execution of actions <see cref="ActivityDefinition.Implementation"/> and <see cref="ActivityDefinition.PreExecutionImplementation"/> in chosen activity
    /// </summary>
    public class ActivityExecutor
    {
        private readonly WorkflowRuntime _runtime;
        private readonly Action<ProcessInstance> _beforeExecutionAction;

        /// <summary>
        /// Returns action provider <see cref="IWorkflowActionProvider"/> from associated workflow runtime 
        /// </summary>
        public IWorkflowActionProvider ActionProvider => _runtime.ActionProvider;

        private SemaphoreSlim Semaphore => _runtime.LicenseSemaphore;
        private bool IsPreExecution { get; }


        /// <summary>
        /// Create ActivityExecutor object
        /// </summary>
        /// <param name="runtime">WorkflowRuntime instance which owned executor</param>
        /// <param name="beforeExecutionAction">Action will be called before execution</param>
        public ActivityExecutor(WorkflowRuntime runtime, Action<ProcessInstance> beforeExecutionAction = null)
        {
            _runtime = runtime;
            _beforeExecutionAction = beforeExecutionAction;
            IsPreExecution = false;
        }

        /// <summary>
        /// Create ActivityExecutor object
        /// </summary>
        /// <param name="runtime">WorkflowRuntime instance which owned executor</param>
        /// <param name="isPreExecution">If true then Result Pre Execution will be considered in time of the check of conditions</param>
        ///    /// <param name="beforeExecutionAction">Action will be called before execution</param>
        public ActivityExecutor(WorkflowRuntime runtime, bool isPreExecution, Action<ProcessInstance> beforeExecutionAction = null)
        {
            _runtime = runtime;
            _beforeExecutionAction = beforeExecutionAction;
            IsPreExecution = isPreExecution;
        }

        /// <summary>
        /// Provides choice of activity for execution <see cref="TransitionDefinition.Conditions"/> and execution of actions <see cref="ActivityDefinition.Implementation"/> and <see cref="ActivityDefinition.PreExecutionImplementation"/> in chosen activity
        /// </summary>
        /// <param name="requestParameters">List of execution requests which contains conditions and actions <see cref="ExecutionRequestParameters"/></param>
        /// <returns>Execution result</returns>
        public async Task<ExecutionResponseParameters> Execute(IEnumerable<ExecutionRequestParameters> requestParameters, CancellationToken token)
        {
            Semaphore?.Wait();

            try
            {
                var requestParametersList = requestParameters.ToList();

                var allAlwaysConditions =
                    requestParametersList.Where(rp => rp.IsAlways).ToList();

                if (IsPreExecution)
                {
                    allAlwaysConditions =
                        allAlwaysConditions.Where(
                                // ReSharper disable once PossibleInvalidOperationException
                                rp => !rp.Conditions.First().ResultOnPreExecution.HasValue || rp.Conditions.First().ResultOnPreExecution.Value)
                            .ToList();
                }

                var always = allAlwaysConditions.FirstOrDefault();

                if (always != null)
                {
                    try
                    {
                        return await ExecuteMethod(always, token).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        return GetExecutionResponseErrorParameters(always, ex);
                    }
                }



                foreach (var condition in requestParametersList.Where(rp => rp.IsCondition))
                {
                    try
                    {
                        if (await CheckConditions(condition, token).ConfigureAwait(false))
                        {
                            return await ExecuteMethod(condition, token).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        return GetExecutionResponseErrorParameters(condition, ex);
                    }
                }

                var otherwise = requestParametersList.SingleOrDefault(rp => rp.IsOtherwise);

                if (otherwise != null)
                {
                    try
                    {
                        return await ExecuteMethod(otherwise, token).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        return GetExecutionResponseErrorParameters(otherwise, ex);
                    }
                }

                var executionParameters = ExecutionResponseParameters.Empty;
                executionParameters.ProcessInstance = requestParametersList.First().ProcessInstance;
                return executionParameters;
            }
            finally
            {
                Semaphore?.Release();
            }
        }

        private static ExecutionResponseParameters GetExecutionResponseErrorParameters(ExecutionRequestParameters always,
            Exception ex)
        {
            var errorResponse = string.IsNullOrEmpty(always.TransitionName)
                ? ExecutionResponseParameters.Create(always.ProcessInstance, always.ActivityName, ex)
                : ExecutionResponseParameters.Create(always.ProcessInstance, always.ActivityName,
                    always.TransitionName, ex);

            return errorResponse;
        }

        private async Task<bool> CheckConditions(ExecutionRequestParameters parameters, CancellationToken token)
        {
            bool? result = null;

            foreach (var condition in parameters.Conditions)
            {
                if (condition.Type != ConditionType.Action || condition.Action == null)
                    throw new InvalidOperationException();

                bool conditionResult;
                if (IsPreExecution && condition.ResultOnPreExecution.HasValue)
                    conditionResult = condition.ResultOnPreExecution.Value;
                else
                {
                    FillExecutedActivityState(parameters);

                    var conditionName = condition.Action.ActionName;
                    
                    if (_runtime.ExecutionSearchOrder.IsProviderFirst())
                    {
                        if (ActionProvider.GetConditions().Contains(conditionName))
                        {
                            conditionResult = await ExecuteConditionFromProviderAsync(parameters, token, conditionName, condition).ConfigureAwait(false);
                        }
                        else
                        {
                            var codeActionsInvoker = CodeActionUtils.GetCodeActionsInvokerForCondition(conditionName, _runtime, parameters.ProcessInstance.ProcessScheme, out _);

                            if (codeActionsInvoker == null)
                            {
#pragma warning disable 618
                                if (!_runtime.IgnoreMissingExecutionItems)
                                    throw new NotImplementedException($"Condition with name {conditionName} is not implemented");
                                conditionResult = false;
#pragma warning restore 618  
                                
                            }
                            else
                            {
                                conditionResult = await ExecuteConditionWithInvokerAsync(parameters, token, codeActionsInvoker, conditionName, condition).ConfigureAwait(false);
                            }
                        }
                    }
                    else if (_runtime.ExecutionSearchOrder.IsProviderLast())
                    {
                        var codeActionsInvoker = CodeActionUtils.GetCodeActionsInvokerForCondition(conditionName, _runtime, parameters.ProcessInstance.ProcessScheme, out _);

                        if (codeActionsInvoker != null)
                        {
                            conditionResult = await ExecuteConditionWithInvokerAsync(parameters, token, codeActionsInvoker, conditionName, condition).ConfigureAwait(false);

                        }
                        else if (ActionProvider.GetConditions().Contains(conditionName))
                        {
                            conditionResult = await ExecuteConditionFromProviderAsync(parameters, token, conditionName, condition).ConfigureAwait(false);
                        }
                        else
                        {
#pragma warning disable 618
                            if (!_runtime.IgnoreMissingExecutionItems)
                                throw new NotImplementedException($"Condition with name {conditionName} is not implemented");
                            conditionResult = false;
#pragma warning restore 618  
                        }
                    }
                    else
                    {
                        var codeActionsInvoker =
                            CodeActionUtils.GetCodeActionsInvokerForCondition(conditionName, _runtime, parameters.ProcessInstance.ProcessScheme, out bool isGlobal);

                        var isLocalPrevaleGlobal = _runtime.ExecutionSearchOrder.IsLocalPrevaleGlobal();
                        
                        if (codeActionsInvoker != null && ((isLocalPrevaleGlobal && !isGlobal) || (!isLocalPrevaleGlobal && isGlobal)))
                        {
                            conditionResult = await ExecuteConditionWithInvokerAsync(parameters, token, codeActionsInvoker, conditionName, condition).ConfigureAwait(false);
                        }
                        else if (ActionProvider.GetConditions().Contains(conditionName))
                        {
                            conditionResult = await ExecuteConditionFromProviderAsync(parameters, token, conditionName, condition).ConfigureAwait(false);
                        }
                        else if (codeActionsInvoker != null)
                        {
                            conditionResult = await ExecuteConditionWithInvokerAsync(parameters, token, codeActionsInvoker, conditionName, condition).ConfigureAwait(false);
                        }
                        else
                        {
#pragma warning disable 618
                            if (!_runtime.IgnoreMissingExecutionItems)
                                throw new NotImplementedException($"Condition with name {conditionName} is not implemented");
                            conditionResult = false;
#pragma warning restore 618  
                        }
                    }

                    if (condition.ConditionInversion)
                        conditionResult = !conditionResult;
                }

                if (result.HasValue)
                    result = parameters.ConditionsConcatenationType == ConcatenationType.And ? result.Value && conditionResult : result.Value || conditionResult;
                else
                {
                    result = conditionResult;
                }
            }

            return result.HasValue && result.Value;
        }

        private async Task<bool> ExecuteConditionWithInvokerAsync(ExecutionRequestParameters parameters, CancellationToken token, CodeActionsInvoker codeActionsInvoker, string conditionName,
            ConditionDefinition condition)
        {
            bool conditionResult;
            if (codeActionsInvoker.IsConditionAsync(conditionName))
            {
                conditionResult = await codeActionsInvoker.InvokeConditionAsync(conditionName, parameters.ProcessInstance, _runtime, condition.Action.ActionParameter, token)
                    .ConfigureAwait(false);
            }
            else
            {
                conditionResult = codeActionsInvoker.InvokeCondition(conditionName, parameters.ProcessInstance, _runtime, condition.Action.ActionParameter);
            }

            return conditionResult;
        }

        private async Task<bool> ExecuteConditionFromProviderAsync(ExecutionRequestParameters parameters, CancellationToken token, string conditionName, ConditionDefinition condition)
        {
            bool conditionResult;
            if (ActionProvider.IsConditionAsync(conditionName))
            {
                conditionResult = await ActionProvider.ExecuteConditionAsync(conditionName, parameters.ProcessInstance, _runtime, condition.Action.ActionParameter, token)
                    .ConfigureAwait(false);
            }
            else
            {
                conditionResult = ActionProvider.ExecuteCondition(conditionName, parameters.ProcessInstance, _runtime, condition.Action.ActionParameter);
            }

            return conditionResult;
        }

        private async Task<ExecutionResponseParameters> ExecuteMethod(ExecutionRequestParameters parameters, CancellationToken token)
        {
            var response = string.IsNullOrEmpty(parameters.TransitionName)
                ? ExecutionResponseParameters.Create(parameters.ProcessInstance, parameters.ActivityName)
                : ExecutionResponseParameters.Create(parameters.ProcessInstance, parameters.ActivityName,
                    parameters.TransitionName);

            FillExecutedActivityState(parameters);

            if (!IsPreExecution)
            {
                _beforeExecutionAction?.Invoke(parameters.ProcessInstance);
            }

            foreach (var method in parameters.Methods.OrderBy(m => m.Order))
            {

                if (_runtime.ExecutionSearchOrder.IsProviderFirst())
                {
                    if (ActionProvider.GetActions().Contains(method.ActionName))
                    {
                        await ExecuteActionFromProviderAsync(token, method, response).ConfigureAwait(false);;
                    }
                    else
                    {
                        var codeActionsInvoker = CodeActionUtils.GetCodeActionsInvokerForAction(method.ActionName, _runtime, response.ProcessInstance.ProcessScheme, out _);

                        if (codeActionsInvoker == null)
                        {
#pragma warning disable 618
                            if (!_runtime.IgnoreMissingExecutionItems)
                                throw new NotImplementedException($"Action with name {method.ActionName} is not implemented");
#pragma warning restore 618  
                        }
                        else
                        {
                            await ExecuteActionWithInvokerAsync(token, codeActionsInvoker, method, response).ConfigureAwait(false);
                           ;
                        }
                    }
                }
                else if (_runtime.ExecutionSearchOrder.IsProviderLast())
                {
                    var codeActionsInvoker = CodeActionUtils.GetCodeActionsInvokerForAction(method.ActionName, _runtime, response.ProcessInstance.ProcessScheme, out _);

                    if (codeActionsInvoker != null)
                    {
                        await ExecuteActionWithInvokerAsync(token, codeActionsInvoker, method, response).ConfigureAwait(false);;

                    }
                    else if (ActionProvider.GetActions().Contains(method.ActionName))
                    {
                        await ExecuteActionFromProviderAsync(token, method, response).ConfigureAwait(false);;
                    }
                    else
                    {
#pragma warning disable 618
                        if (!_runtime.IgnoreMissingExecutionItems)
                            throw new NotImplementedException($"Action with name {method.ActionName} is not implemented");
#pragma warning restore 618  
                    }
                }
                else 
                {
                    var codeActionsInvoker = CodeActionUtils.GetCodeActionsInvokerForAction(method.ActionName, _runtime, response.ProcessInstance.ProcessScheme, out bool isGlobal);

                    var isLocalPrevaleGlobal = _runtime.ExecutionSearchOrder.IsLocalPrevaleGlobal();
                    
                    if (codeActionsInvoker != null && ((isLocalPrevaleGlobal && !isGlobal) || (!isLocalPrevaleGlobal && isGlobal)))
                    {
                       await ExecuteActionWithInvokerAsync(token, codeActionsInvoker, method, response).ConfigureAwait(false);;
                    }
                    else if (ActionProvider.GetActions().Contains(method.ActionName))
                    {
                        await ExecuteActionFromProviderAsync(token, method, response).ConfigureAwait(false);;
                    }
                    else if (codeActionsInvoker != null)
                    {
                        await ExecuteActionWithInvokerAsync(token, codeActionsInvoker, method, response).ConfigureAwait(false);;
                    }
                    else
                    {
#pragma warning disable 618
                        if (!_runtime.IgnoreMissingExecutionItems)
                            throw new NotImplementedException($"Action with name {method.ActionName} is not implemented");
#pragma warning restore 618  
                    }
                }
             }

            return response;
        }

        private async Task ExecuteActionWithInvokerAsync(CancellationToken token, CodeActionsInvoker codeActionsInvoker, ActionDefinitionReference method,
            ExecutionResponseParametersComplete response)
        {
            if (codeActionsInvoker.IsActionAsync(method.ActionName))
            {
                await codeActionsInvoker.InvokeActionAsync(method.ActionName, response.ProcessInstance, _runtime, method.ActionParameter, token).ConfigureAwait(false);
            }
            else
            {
                codeActionsInvoker.InvokeAction(method.ActionName, response.ProcessInstance, _runtime, method.ActionParameter);
            }
        }

        private async Task ExecuteActionFromProviderAsync(CancellationToken token, ActionDefinitionReference method, ExecutionResponseParametersComplete response)
        {
            if (ActionProvider.IsActionAsync(method.ActionName))
            {
                await ActionProvider.ExecuteActionAsync(method.ActionName, response.ProcessInstance, _runtime, method.ActionParameter, token).ConfigureAwait(false);
            }
            else
            {
                ActionProvider.ExecuteAction(method.ActionName, response.ProcessInstance, _runtime, method.ActionParameter);
            }
        }

        private static void FillExecutedActivityState(ExecutionRequestParameters parameters)
        {
            var executedActivityStateNameValue = parameters.Activity.State;

            if (string.IsNullOrEmpty(executedActivityStateNameValue))
            {
                executedActivityStateNameValue = parameters.ProcessInstance.CurrentState;
            }

            parameters.ProcessInstance.ExecutedActivityState = executedActivityStateNameValue;

            parameters.ProcessInstance.ExecutedActivity = parameters.Activity;
            if (!string.IsNullOrEmpty(parameters.TransitionName))
                parameters.ProcessInstance.ExecutedTransition =
                    parameters.ProcessInstance.ProcessScheme.FindTransition(parameters.TransitionName);
        }
    }
}
