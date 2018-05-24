using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;

namespace WorkFlowEngine
{
    public class ActionProvider : OptimaJet.Workflow.Core.Runtime.IWorkflowActionProvider
    {
        private readonly Dictionary<string, Action<ProcessInstance, WorkflowRuntime, string>> _actions = new Dictionary<string, Action<ProcessInstance, WorkflowRuntime, string>>();

        private readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task>> _asyncActions = new Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task>>();

        private readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, bool>> _conditions = new Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, bool>>();

        private readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task<bool>>> _asyncConditions = new Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task<bool>>>();

        public ActionProvider()
        {
            //Register your actions in _actions and _asyncActions dictionaries
            _actions.Add("MyAction", MyAction); //sync
            _actions.Add("MyAction2", MyAction2); //sync
            //_asyncActions.Add("MyAsyncAction", MyAsyncAction); //async

            //Register your conditions in _conditions and _asyncConditions dictionaries
            _conditions.Add("ConditionalEval", ConditionalParser); //sync
            //_asyncConditions.Add("MyAsyncCondition", MyAsyncCondition); //async
        }

        private void MyAction(ProcessInstance processInstance, WorkflowRuntime runtime,
            string actionParameter)
        {
            //Execute your synchronous code here

        }

        private void MyAction2(ProcessInstance processInstance, WorkflowRuntime runtime,
            string actionParameter)
        {
            //Execute your synchronous code here

        }

        private async Task MyAsyncAction(ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
        {
            //Execute your asynchronous code here. You can use await in your code.
        }

        private bool ConditionalParser(ProcessInstance processInstance, WorkflowRuntime runtime,
                                      string actionParameter)
        {
            return false;

            ////var price = processInstance.GetParameter<decimal>("price");
            //dynamic myObject = JsonConvert.DeserializeObject<dynamic>(actionParameter);
            //string eval = myObject.eval.ToString(); // $"[price]>10";

            //foreach (var p in  processInstance.ProcessParameters)
            //{
            //    if (eval.IndexOf(p.Name) >= 0)
            //    {
            //        if (p.Type.Name.ToLower() == "decimal")
            //        {
            //            eval = eval.Replace($"[{p.Name}]",
            //                                Convert.ToDecimal(p.Value)
            //                                .ToString(CultureInfo.CreateSpecificCulture("en-GB")));
            //        }
            //    }
            //}

            ////Execute your code here
            //bool result = (bool)new DataTable().Compute(eval, null);

            //return result;
        }

        private async Task<bool> MyAsyncCondition(ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
        {
            //Execute your asynchronous code here. You can use await in your code.
            return false;
        }

        #region Implementation of IWorkflowActionProvider

        public void ExecuteAction(string name, ProcessInstance processInstance, WorkflowRuntime runtime,
            string actionParameter)
        {
            if (_actions.ContainsKey(name))
                _actions[name].Invoke(processInstance, runtime, actionParameter);
            else
                throw new NotImplementedException($"Action with name {name} isn't implemented");
        }

        public async Task ExecuteActionAsync(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
        {
            //token.ThrowIfCancellationRequested(); // You can use the transferred token at your discretion
            if (_asyncActions.ContainsKey(name))
                await _asyncActions[name].Invoke(processInstance, runtime, actionParameter, token);
            else
                throw new NotImplementedException($"Async Action with name {name} isn't implemented");
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

        public bool IsActionAsync(string name)
        {
            return _asyncActions.ContainsKey(name);
        }

        public bool IsConditionAsync(string name)
        {
            return _asyncConditions.ContainsKey(name);
        }

        public List<string> GetActions()
        {
            return _actions.Keys.Union(_asyncActions.Keys).ToList();
        }

        public List<string> GetConditions()
        {
            return _conditions.Keys.Union(_asyncConditions.Keys).ToList();
        }
        #endregion
    }
}
