using System.Collections.Generic;

namespace OptimaJet.Workflow.Core.BPMN
{
    public class Process
    {
        public List<Event> events = new List<Event>();
        public List<Flow> flows = new List<Flow>();
        public List<Gateway> gateways = new List<Gateway>();
        public List<Task> tasks = new List<Task>();

        public void AddEvent(Event p)
        {
            if(p != null)
                events.Add(p);
        }

        public void AddFlow(Flow p)
        {
            if (p != null)
                flows.Add(p);
        }

        public void AddGateway(Gateway p)
        {
            if (p != null)
                gateways.Add(p);
        }

        public void AddTask(Task p)
        {
            if (p != null)
                tasks.Add(p);
        }
    }
}
