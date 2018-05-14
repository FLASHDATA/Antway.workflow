using System;
using System.Collections.Generic;

namespace OptimaJet.Workflow.Core.BPMN
{
    public enum TaskType
    {
        Task,
        ServiceTask,
        SendTask,
        ReceiveTask,
        UserTask,
        ManualTask,
        BusinessRuleTask,
        ScriptTask,
        SubProcess,
        AdHocSubProcess,
        Transaction,
        CallActivity
    }

    public class Task
    {
        public string Id;
        public string Name;
        public TaskType Type;
        public List<string> IncomingList = new List<string>();
        public List<string> OutgoingList = new List<string>();
        public List<Task> TasksList = new List<Task>();
        public List<Event> EventsList = new List<Event>();
        public List<Flow> FlowList = new List<Flow>();

        public void AddOutgoing(string value)
        {
            OutgoingList.Add(value);
        }

        public void AddIncoming(string value)
        {
            IncomingList.Add(value);
        }

        public void AddTask(Task task)
        {
            TasksList.Add(task);
        }

        public void AddEvent(Event e)
        {
            EventsList.Add(e);
        }

        public void AddFlow(Flow flow)
        {
            FlowList.Add(flow);
        }
    }
}
