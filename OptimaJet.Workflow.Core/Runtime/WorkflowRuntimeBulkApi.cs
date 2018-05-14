using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;

namespace OptimaJet.Workflow.Core.Runtime
{
    /// <summary>
    /// Auxiliary parameters that configure bulk operations.
    /// </summary>
    public class BulkOperationOptions
    {
        /// <summary>
        /// Maximum parallelization of subprocess execution.
        /// </summary>
        public int MaxDegreeOfParallelism { get; set; }

        internal static BulkOperationOptions Default => new BulkOperationOptions {MaxDegreeOfParallelism = Environment.ProcessorCount};
    }
    /// <summary>
    /// Runtime API for bulk operations
    /// </summary>
    public static class WorkflowRuntimeBulkApi
    {
        #region CreateInstances

        /// <summary>
        /// Creates and initializes multiple process instances
        /// </summary>
        /// <param name="runtime">Instance of workflow runtime</param>
        /// <param name="createInstanceParameters">BulkCreateInstancePrams objects</param>
        /// <param name="options">Auxiliary parameters that configure bulk operations</param>
        /// <returns></returns>
        public static void BulkCreateInstance(this WorkflowRuntime runtime, BulkCreateInstancePrams createInstanceParameters, BulkOperationOptions options = null)
        {
            BulkCreateInstanceAsync(runtime, new List<BulkCreateInstancePrams> { createInstanceParameters }, CancellationToken.None, options).Wait();
        }

        /// <summary>
        /// Creates and initializes multiple process instances
        /// </summary>
        /// <param name="runtime">Instance of workflow runtime</param>
        /// <param name="createInstanceParameters">List of BulkCreateInstancePrams objects</param>
        /// <param name="options">Auxiliary parameters that configure bulk operations</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static void BulkCreateInstance(this WorkflowRuntime runtime, List<BulkCreateInstancePrams> createInstanceParameters, BulkOperationOptions options = null)
        {
            BulkCreateInstanceAsync(runtime, createInstanceParameters, CancellationToken.None, options).Wait();
        }

        /// <summary>
        /// Creates and initializes multiple process instances (Async version)
        /// </summary>
        /// <param name="runtime">Instance of workflow runtime</param>
        /// <param name="createInstanceParameters">BulkCreateInstancePrams objects</param>
        /// <param name="options">Auxiliary parameters that configure bulk operations</param>
        /// <returns></returns>
        public static async Task BulkCreateInstanceAsync(this WorkflowRuntime runtime, BulkCreateInstancePrams createInstanceParameters, BulkOperationOptions options = null)
        {
            await BulkCreateInstanceAsync(runtime, new List<BulkCreateInstancePrams> { createInstanceParameters }, CancellationToken.None, options);
        }

        /// <summary>
        /// Creates and initializes multiple process instances (Async version)
        /// </summary>
        /// <param name="runtime">Instance of workflow runtime</param>
        /// <param name="createInstanceParameters">List of BulkCreateInstancePrams objects</param>
        /// <param name="options">Auxiliary parameters that configure bulk operations</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static async Task BulkCreateInstanceAsync(this WorkflowRuntime runtime, List<BulkCreateInstancePrams> createInstanceParameters, BulkOperationOptions options = null)
        {
            await BulkCreateInstanceAsync(runtime, createInstanceParameters, CancellationToken.None, options);
        }

        /// <summary>
        /// Creates and initializes multiple process instances (Async version)
        /// </summary>
        /// <param name="runtime">Instance of workflow runtime</param>
        /// <param name="createInstanceParameters">BulkCreateInstancePrams objects</param>
        /// <param name="options">Auxiliary parameters that configure bulk operations</param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        public static async Task BulkCreateInstanceAsync(this WorkflowRuntime runtime, BulkCreateInstancePrams createInstanceParameters, CancellationToken token, BulkOperationOptions options = null)
        {
            await BulkCreateInstanceAsync(runtime, new List<BulkCreateInstancePrams> {createInstanceParameters}, token, options);
        }

        /// <summary>
        /// Creates and initializes multiple process instances (Async version)
        /// </summary>
        /// <param name="runtime">Instance of workflow runtime</param>
        /// <param name="createInstanceParameters">List of BulkCreateInstancePrams objects</param>
        /// <param name="token">Cancellation token</param>
        /// <param name="options">Auxiliary parameters that configure bulk operations</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static async Task BulkCreateInstanceAsync(this WorkflowRuntime runtime, List<BulkCreateInstancePrams> createInstanceParameters, CancellationToken token, BulkOperationOptions options = null)
        {
            if (token.IsCancellationRequested)
                return;

            if (!runtime.PersistenceProvider.IsBulkOperationsSupported)
                throw new NotSupportedException("Bulk operations is not supported by specified persistence parameter");

            if (options == null)
                options = BulkOperationOptions.Default;
            
            var schemes = createInstanceParameters.ToDictionary(p => p.Id,
                p => runtime.Builder.GetProcessScheme(p.SchemeCode, p.SchemeCreationParameters ?? new Dictionary<string, object>()));

            List<ProcessInstance> requestTimerValueProcessInstances = new List<ProcessInstance>();
            ConcurrentBag<ProcessInstance> executeProcessInstances = new ConcurrentBag<ProcessInstance>();
            ConcurrentBag<ProcessInstance> notExecuteProcessInstances = new ConcurrentBag<ProcessInstance>();
            ConcurrentBag<TimerToRegister> timersToRegister = new ConcurrentBag<TimerToRegister>();

            foreach (var request in createInstanceParameters)
            {
                var scheme = schemes[request.Id];

                var needExececution = scheme.InitialActivity.HaveImplementation ||
                                      scheme.GetAutoTransitionForActivity(scheme.InitialActivity).Any();
                var needRequestTimerValue = false;
                var needReqisterTimer = false;

                List<TimerToRegister> timersFromScheme = null;

                if (runtime.TimerManager != null)
                {
                    needRequestTimerValue = scheme.GetTimerTransitionForActivity(scheme.InitialActivity, ForkTransitionSearchType.Both)
                        .Any(t => t.Trigger.Timer.Value == runtime.TimerManager.ImmediateTimerValue || t.Trigger.Timer.Value == runtime.TimerManager.InfinityTimerValue);
                    needReqisterTimer = scheme.GetTimerTransitionForActivity(scheme.InitialActivity, ForkTransitionSearchType.Both)
                        .Any(t => t.Trigger.Timer.Value != runtime.TimerManager.ImmediateTimerValue && t.Trigger.Timer.Value != runtime.TimerManager.InfinityTimerValue);
                    if (!needRequestTimerValue && needReqisterTimer)
                    {
                        timersFromScheme = runtime.TimerManager.GetTimersToRegister(scheme, scheme.InitialActivity.Name);
                    }
                }

                bool haveProcessSpecificIdentityIds = request.ProcessSpecificIdentityIds != null && request.ProcessSpecificIdentityIds.Any();
                bool haveProcessSpecificImpIdentityIds = request.ProcessSpecificImpersonatedIdentityIds != null && request.ProcessSpecificImpersonatedIdentityIds.Any();

                foreach (var processId in request.ProcessIds)
                {
                    var processInstance = ProcessInstance.Create(scheme.Id, processId, scheme, scheme.IsObsolete, false);
                    processInstance.RootProcessId = processId;
                    //Initial parameters from scheme
                    processInstance.InitPersistenceParametersFromScheme();

                    //Initial parameters from dictionary
                    if (request.InitialProcessParameters != null && request.InitialProcessParameters.Any())
                    {
                        foreach (var parameter in request.InitialProcessParameters)
                        {
                            processInstance.SetParameter(parameter.Key, parameter.Value);
                        }
                    }

                    //Initial parameters from dictionary
                    if (request.ProcessSpecificProcessPrarameters != null && request.ProcessSpecificProcessPrarameters.ContainsKey(processId))
                    {
                        foreach (var parameter in request.ProcessSpecificProcessPrarameters[processId])
                        {
                            processInstance.SetParameter(parameter.Key, parameter.Value);
                        }
                    }

                    SetIdentityIds(processInstance, request, haveProcessSpecificIdentityIds, haveProcessSpecificImpIdentityIds);

                    if (needExececution)
                    {
                        executeProcessInstances.Add(processInstance);
                    }
                    else
                    {
                        if (needRequestTimerValue)
                        {
                            requestTimerValueProcessInstances.Add(processInstance);
                        }
                        else
                        {
                            notExecuteProcessInstances.Add(processInstance);
                            if (needReqisterTimer)
                            {
                                var timers =
                                    timersFromScheme.Select(ts => new TimerToRegister() {ExecutionDateTime = ts.ExecutionDateTime, Name = ts.Name, ProcessId = processId}).ToList();
                                timers.ForEach(timersToRegister.Add);

                            }
                        }
                    }


                }
            }

            if (requestTimerValueProcessInstances.Any())
            {
                var requestTimerActions = requestTimerValueProcessInstances.Select((pi) =>
                    (Action) (() => RequestTimerValue(runtime, pi, executeProcessInstances, notExecuteProcessInstances, timersToRegister))).ToArray();
                Parallel.Invoke(new ParallelOptions() {MaxDegreeOfParallelism = options.MaxDegreeOfParallelism, CancellationToken = token}, requestTimerActions);
//                await Task.WhenAll(
//                    requestTimerValueProcessInstances.Select(
//                        pi => Task.Run(() => RequestTimerValue(runtime, pi, executeProcessInstances, notExecuteProcessInstances, timersToRegister), token)));
            }

            //Now we have sorted process instances
            //Lets create instances which are not required execution
            await runtime.PersistenceProvider.BulkInitProcesses(notExecuteProcessInstances.ToList(), timersToRegister.ToList(), ProcessStatus.Idled, token);

            if (token.IsCancellationRequested)
                return;

            var invokeActivityChanged = runtime.IsProcessActivityChangedSubscribed;
            var invokeStatusChanged = runtime.IsProcessStatusChangedSubscribed;

            if (invokeActivityChanged || invokeStatusChanged)
            {
                var invokeEventsActions = notExecuteProcessInstances.Select((pi) =>
                    (Action) (() =>
                    {
                        pi.CurrentActivityName = pi.ProcessScheme.InitialActivity.Name;
                        pi.CurrentState = pi.ProcessScheme.InitialActivity.State;
                        ExecuteEvents(runtime, pi, invokeStatusChanged, invokeActivityChanged, true);
                    })).ToArray();
                
                Parallel.Invoke(new ParallelOptions() {MaxDegreeOfParallelism = options.MaxDegreeOfParallelism, CancellationToken = token}, invokeEventsActions);
                
//                await Task.WhenAll(
//                    notExecuteProcessInstances.Select(
//                        pi => Task.Run(() =>
//                        {
//                            pi.CurrentActivityName = pi.ProcessScheme.InitialActivity.Name;
//                            pi.CurrentState = pi.ProcessScheme.InitialActivity.State;
//                            ExecuteEvents(runtime, pi, invokeStatusChanged, invokeActivityChanged, true);
//                        }, token)));
            }

            if (token.IsCancellationRequested)
                return;

            //Refresh interval
            if (timersToRegister.Any())
            {
                runtime.TimerManager?.Refresh();
            }

            if (!executeProcessInstances.Any())
                return;

            //Lets create instances which are required execution
            await runtime.PersistenceProvider.BulkInitProcesses(executeProcessInstances.ToList(), ProcessStatus.Initialized, token);

            if (token.IsCancellationRequested)
                return;

            //Fire Initialized status changed event
            if (invokeStatusChanged)
            {
                var invokeEventsActions = executeProcessInstances.Select((pi) =>
                    (Action) (() => { ExecuteEvents(runtime, pi, true, false, false); })).ToArray();
                
                Parallel.Invoke(new ParallelOptions() {MaxDegreeOfParallelism = options.MaxDegreeOfParallelism, CancellationToken = token}, invokeEventsActions);
                
//                await Task.WhenAll(
//                    executeProcessInstances.Select(
//                        pi => Task.Run(() => ExecuteEvents(runtime, pi, true, false, false), token)));
            }


            //Execute created processes

            var actions = executeProcessInstances.Select(pi => (Action) (() => { runtime.ExecuteCreatedProcess(pi, pi.IdentityId, pi.ImpersonatedIdentityId, token).Wait(); }))
                .ToArray();

            Parallel.Invoke(new ParallelOptions() {MaxDegreeOfParallelism = options.MaxDegreeOfParallelism, CancellationToken = token}, actions);

        }

        private static void SetIdentityIds(ProcessInstance processInstance, BulkCreateInstancePrams request, bool haveProcessSpecificIdentityIds, bool haveProcessSpecificImpIdentityIds)
        {
            var processId = processInstance.ProcessId;

            if (!haveProcessSpecificIdentityIds)
            {
                processInstance.IdentityId = request.IdentityId;
            }
            else
            {
                processInstance.IdentityId = request.ProcessSpecificIdentityIds.ContainsKey(processId)
                    ? request.ProcessSpecificIdentityIds[processId]
                    : request.IdentityId;
            }

            if (!haveProcessSpecificImpIdentityIds)
            {
                processInstance.ImpersonatedIdentityId = request.ImpersonatedIdentityId;
            }
            else
            {
                processInstance.ImpersonatedIdentityId = request.ProcessSpecificImpersonatedIdentityIds.ContainsKey(processId)
                    ? request.ProcessSpecificImpersonatedIdentityIds[processId]
                    : request.ImpersonatedIdentityId;
            }

            if (processInstance.ImpersonatedIdentityId == null)
                processInstance.ImpersonatedIdentityId = processInstance.IdentityId;
        }

        private static void ExecuteEvents(WorkflowRuntime runtime, ProcessInstance processInstance, bool invokeStatusChanged, bool invokeActivityChanged,
            bool invokeIdledStatusChanged)
        {
            if (invokeStatusChanged)
            {
                runtime.InvokeProcessStatusChanged(new ProcessStatusChangedEventArgs(processInstance.ProcessId, false, ProcessStatus.NotFound, ProcessStatus.Initialized));
                if (invokeIdledStatusChanged)
                    runtime.InvokeProcessStatusChanged(new ProcessStatusChangedEventArgs(processInstance.ProcessId, false, ProcessStatus.Initialized, ProcessStatus.Idled));
            }
            if (invokeActivityChanged)
            {
                runtime.InvokeProcessActivityChanged(new ProcessActivityChangedEventArgs(processInstance, true));
            }
        }

        private static void RequestTimerValue(WorkflowRuntime runtime, ProcessInstance processInstance, ConcurrentBag<ProcessInstance> executeProcessInstances,
            ConcurrentBag<ProcessInstance> notExecuteProcessInstances, ConcurrentBag<TimerToRegister> timers)
        {

            runtime.TimerManager.RequestTimerValue(processInstance, processInstance.ProcessScheme.InitialActivity);
            var needExecution = runtime.TimerManager.GetTransitionsForImmediateExecution(processInstance, processInstance.ProcessScheme.InitialActivity).Any();
            if (needExecution)
            {
                executeProcessInstances.Add(processInstance);
            }
            else
            {
                notExecuteProcessInstances.Add(processInstance);
                var timersToRegister = runtime.TimerManager.GetTimersToRegister(processInstance, processInstance.ProcessScheme.InitialActivity.Name);
                timersToRegister.ForEach(timers.Add);
            }
        }
        
        #endregion
    }
}
