using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Antway.Core;
using AntWay.Persistence.Provider;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;

namespace AntWay.Core.WorkflowObjects
{
    public class AntWayActionProvider : IWorkflowActionProvider
    {
        protected readonly Dictionary<string, Action<ProcessInstance, WorkflowRuntime, string>>
            _actions = new Dictionary<string, Action<ProcessInstance, WorkflowRuntime, string>>();
        protected readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task>>
            _asyncActions = new Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task>>();
        //protected readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, bool>> _conditions = new Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, bool>>();
        //protected readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task<bool>>> _asyncConditions = new Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task<bool>>>();


        public AntWayActionProvider()
        {
            //Register your actions in _actions
            _actions.Add("InsertarNotificacionAction", InsertarNotificationAction); //sync
        }

        protected virtual void InsertarNotificationAction(ProcessInstance processInstance,
                                                WorkflowRuntime runtime,
                                                string actionParameter)
        {
            try
            {
                var schemePersistence = new ProcessPersistence
                {
                  IDALProcessPersistence = PersistenceObjectsFactory.GetIDALWFLocatorObject(),
                };
                var wfScheme = schemePersistence.GetWorkflowLocatorFromGuid(processInstance.ProcessId);
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

        public bool ExecuteCondition(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExecuteConditionAsync(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public List<string> GetActions()
        {
            return  _actions.Keys.Union(_asyncActions.Keys).ToList();
        }

        public List<string> GetConditions()
        {
            return new List<string>();
        }

        public bool IsActionAsync(string name)
        {
            return _asyncActions.ContainsKey(name);
        }

        public bool IsConditionAsync(string name)
        {
            return false;
        }
        #endregion
    }
}
