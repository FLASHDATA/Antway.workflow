using System.Collections.Generic;

namespace OptimaJet.Workflow.Core.BPMN
{
    public enum EventType
    {
        startEvent,
        endEvent,
        intermediateThrowEvent,
        intermediateCatchEvent
    }

    public enum EventSubType
    {
        none,
        timerEventDefinition,
        messageEventDefinition,
        conditionalEventDefinition,
        signalEventDefinition
    }
    public class Event
    {
        public string Id;
        public string Name;
        public EventType Type;
        public List<string> IncomingList = new List<string>();
        public List<string> OutgoingList = new List<string>();

        public EventSubType SubType = EventSubType.none;

        public void AddOutgoing(string value)
        {
            OutgoingList.Add(value);
        }

        public void AddIncoming(string value)
        {
            IncomingList.Add(value);
        }
    }
}
