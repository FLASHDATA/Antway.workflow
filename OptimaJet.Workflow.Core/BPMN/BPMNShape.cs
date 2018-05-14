namespace OptimaJet.Workflow.Core.BPMN
{
    public class BPMNShape
    {
        public Bounds bounds;
        internal string bpmnElement;

        public string Id { get; internal set; }
        public BPMNLabel Label { get; internal set; }
    }
}
