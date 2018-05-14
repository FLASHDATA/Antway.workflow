namespace OptimaJet.Workflow.Core.BPMN
{
    public class Flow
    {
        public string sourceRef;
        public string targetRef;

        public string Id { get; internal set; }
        public string Name { get; internal set; }
    }
}
