using System.Collections.Generic;

namespace OptimaJet.Workflow.Core.BPMN
{
    public class Collaboration
    {
        public string Id;
        public List<Participant> ParticipantList = new List<Participant>();
        public List<MessageFlow> MessageFlowList = new List<MessageFlow>();

        public void AddParticipant(Participant p)
        {
            ParticipantList.Add(p);
        }
        public void AddMessageFlow(MessageFlow p)
        {
            MessageFlowList.Add(p);
        }
    }

    public class Participant
    {
        public string Id;
        public string Name;
        public string ProcessRef;
    }

    public class MessageFlow
    {
        public string Id;
        public string Name;
        public string ProcessRef;
        public string sourceRef;
        public string targetRef;
    }
}
