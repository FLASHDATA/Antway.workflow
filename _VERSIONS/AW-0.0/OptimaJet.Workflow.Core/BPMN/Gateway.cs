using System.Collections.Generic;

namespace OptimaJet.Workflow.Core.BPMN
{
    public enum GatewayType
    {
        ExclusiveGateway,
        InclusiveGateway,
        ParallelGateway,
        ComplexGateway,
        EventBasedGateway
    }

    public class Gateway
    {
        public string Id;
        public string Name;
        public GatewayType Type;
        public List<string> IncomingList = new List<string>();
        public List<string> OutgoingList = new List<string>();

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
