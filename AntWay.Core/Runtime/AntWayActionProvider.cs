using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Antway.Core.Persistence;
using AntWay.Core.Activity;
using AntWay.Core.Mapping;
using AntWay.Core.Model;
using AntWay.Persistence.Provider.Model;
using Newtonsoft.Json;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;

namespace AntWay.Core.Providers
{
    public class AntWayActionProvider : IWorkflowActionProvider
    {
        protected readonly Dictionary<string, Action<ProcessInstance, WorkflowRuntime, string>>
            _actions = new Dictionary<string, Action<ProcessInstance, WorkflowRuntime, string>>();
        protected readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task>>
            _asyncActions = new Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task>>();

        protected readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, bool>> 
            _conditions = new Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, bool>>();
        protected readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task<bool>>>
            _asyncConditions = new Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task<bool>>>();


        public AntWayActionProvider()
        {
            _actions.Add("RunActivity", RunAntWayActivity);
            _actions.Add("RunActivityAsync", RunAntWayActivityAsync);

            _actions.Add("CallService", CallService);
            _actions.Add("CallServiceAsync", CallServiceAsync);

            //_conditions.Add("ParameterValueEqualTo", ParameterValueEqualTo); //sync
            _conditions.Add("CurrentActivityExecutionSucceed", CurrentActivityExecutionSucceed);
            _conditions.Add("LastExecutionFromActivitySucceed", LastExecutionFromActivitySucceed);

            //Register your actions in _actions
            //_actions.Add("InsertarNotificacionAction", InsertarNotificationAction); //sync
        }

        protected virtual void RunAntWayActivity(ProcessInstance processInstance,
                                              WorkflowRuntime runtime,
                                              string actionParameter)
        {
            var activityName = processInstance.ExecutedTransition.To.Name;
            GetActivityAndRun(processInstance, runtime, activityName, false);
        }


        protected virtual void RunAntWayActivityAsync(ProcessInstance processInstance,
                                              WorkflowRuntime runtime,
                                              string actionParameter)
        {
            var activityName = processInstance.ExecutedTransition.To.Name;
            GetActivityAndRun(processInstance, runtime, activityName, true);
        }


        protected void GetActivityAndRun(ProcessInstance processInstance,
                                         WorkflowRuntime runtime,
                                         string activityName, bool runAsync)
        {
            var currenteActivity = processInstance
                                   .ProcessScheme
                                   .Activities.FirstOrDefault(a => a.Name == activityName);

            //Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            IAntWayRuntimeActivity awRuntimeActivity = AntWayActivityActivator
                                                       .GetAntWayObjectFromActivity(currenteActivity.Id);

            if (awRuntimeActivity == null)
            {
                return;
            }

            awRuntimeActivity.ActivityId = currenteActivity.Id;
            awRuntimeActivity.ActivityName = currenteActivity.Name;
            if (runAsync)
            {
                awRuntimeActivity.RunAsync(processInstance, runtime);
            }
            else
            {
                awRuntimeActivity.Run(processInstance, runtime);
            }
        }


        protected virtual void CallService(ProcessInstance processInstance,
                                    WorkflowRuntime runtime,
                                    string actionParameter)
        {
            var jsonObj = JsonConvert
                      .DeserializeObject<ActionCallServiceJson>(actionParameter);
            CallService(processInstance, runtime, jsonObj.url, false);
        }

        protected virtual void CallServiceAsync(ProcessInstance processInstance,
                                    WorkflowRuntime runtime,
                                    string actionParameter)
        {
            var jsonObj = JsonConvert.DeserializeObject<ActionCallServiceJson>(actionParameter);
            CallService(processInstance, runtime, jsonObj.url, true);
        }

        protected void CallService(ProcessInstance processInstance, WorkflowRuntime runtime,
                                   string url, bool runAsync = false)
        {
            var activityName = processInstance.ExecutedTransition.To.Name;

            var currenteActivity = processInstance
                       .ProcessScheme
                       .Activities.FirstOrDefault(a => a.Name == activityName);


            IAntWayRuntimeActivity awRuntimeActivity = new AntWayActivityService
            {
                //ActivityId = typeof(AntWayActivityService)
                //             .GetAttributeValue((ActivityAttribute a) => a.Id)
                ActivityId = currenteActivity.Id,
                ActivityName = currenteActivity.Name,
            };


            var paremeters = new object[] { url  };

            if (runAsync)
            {
                awRuntimeActivity.RunAsync(processInstance, runtime, paremeters);
            }
            else
            {
                awRuntimeActivity.Run(processInstance, runtime, paremeters);
            }
        }


        protected virtual bool CurrentActivityExecutionSucceed(ProcessInstance processInstance,
                                                            WorkflowRuntime runtime,
                                                            string actionParameter)
        {
            var activityName = processInstance.CurrentActivity.Name;
            var currenteActivity = processInstance
                       .ProcessScheme
                       .Activities.FirstOrDefault(a => a.Name == activityName);

            var value = processInstance
                        .GetParameter($"{currenteActivity.Id}/{AntWayProcessParameters.ACTIVITY_EXECUTION_SUCCEED}")
                        ?.Value
                        .ToString();

            bool success = Convert.ToBoolean(value.Replace("\"", ""));

            return success;
        }

        protected virtual bool LastExecutionFromActivitySucceed(ProcessInstance processInstance,
                                                                WorkflowRuntime runtime,
                                                                string actionParameter)
        {
            var jsonObj = JsonConvert.DeserializeObject<ActivityIdJson>(actionParameter);

            var activity = processInstance
                          .ProcessScheme
                          .Activities.FirstOrDefault(a => a.Id == jsonObj.activityId);

            var value = processInstance
                        .GetParameter($"{activity.Id}/{AntWayProcessParameters.ACTIVITY_EXECUTION_SUCCEED}")
                        .Value
                        .ToString();

            bool success = Convert.ToBoolean(value.Replace("\"", ""));

            return success;
        }

        //protected virtual bool ParameterValueEqualTo(ProcessInstance processInstance,
        //                        WorkflowRuntime runtime,
        //                        string actionParameter)
        //{
        //    var kvp = JsonConvert.DeserializeObject<KeyValuePair<string, string>>(actionParameter);

        //    var currentValue = processInstance.GetParameter(kvp.Key).Value.ToString();

        //    bool match = (currentValue.Replace("\"", "") == kvp.Value.ToString());

        //    return match;
        //}


        //TEMA NOTIFICACIONES: DE MOMENTO EN STAND BY (4/10/2018)
        protected virtual void InsertarNotificationAction(ProcessInstance processInstance,
                                                WorkflowRuntime runtime,
                                                string actionParameter)
        {
            try
            {
                var locatorPersistence = new LocatorPersistence
                {
                  IDALocators = PersistenceObjectsFactory.GetIDALLocatorsObject()
                };
                var locatorView = locatorPersistence.GetWorkflowLocatorFromGuid(processInstance.ProcessId);
                //TODO:
                //Override this function, to Insert your code for Notifications
            }
            catch (Exception ex)
            {
                processInstance.SetParameter("InsertarNotificacionAction_error",
                                             ex.Message,
                                             ParameterPurpose.Persistence);
            }
        }



        public void ExecuteAction(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter)
        {
            if (_actions.ContainsKey(name))
                _actions[name].Invoke(processInstance, runtime, actionParameter);
            else
                throw new NotImplementedException($"Action with name {name} isn't implemented");
        }


        #region Not_Used/Not_Implemented
        //NOT IMPLEMENTED
        public Task ExecuteActionAsync(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
        {
            throw new NotImplementedException();
        }


        public bool ExecuteCondition(string name, ProcessInstance processInstance, WorkflowRuntime runtime,
                                    string actionParameter)
        {
            if (_conditions.ContainsKey(name))
                return _conditions[name].Invoke(processInstance, runtime, actionParameter);

            throw new NotImplementedException($"Condition with name {name} isn't implemented");
        }

        public async Task<bool> ExecuteConditionAsync(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
        {
            //token.ThrowIfCancellationRequested(); // You can use the transferred token at your discretion
            if (_asyncConditions.ContainsKey(name))
                return await _asyncConditions[name].Invoke(processInstance, runtime, actionParameter, token);

            throw new NotImplementedException($"Async Condition with name {name} isn't implemented");
        }

        public List<string> GetActions()
        {
            return  _actions.Keys.Union(_asyncActions.Keys).ToList();
        }

        public List<string> GetConditions()
        {
            return _conditions.Keys.Union(_asyncConditions.Keys).ToList();
        }

        public bool IsActionAsync(string name)
        {
            return _asyncActions.ContainsKey(name);
        }

        public bool IsConditionAsync(string name)
        {
            return _asyncConditions.ContainsKey(name); ;
        }
        #endregion
    }

    public class ActionCallServiceJson
    {
        public string url { get; set; }
    }

    public class ActivityIdJson
    {
        public string activityId { get; set; }
    }
}
