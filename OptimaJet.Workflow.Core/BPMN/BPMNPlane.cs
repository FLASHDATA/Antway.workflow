using System.Collections.Generic;

namespace OptimaJet.Workflow.Core.BPMN
{
    public class BPMNPlane
    {
        public string bpmnElement;
        public string Id { get; internal set; }

        public List<BPMNShape> shapes = new List<BPMNShape>();
        public List<BPMNEdge> edges = new List<BPMNEdge>();

        public void AddShape(BPMNShape p)
        {
            if (p != null)
                shapes.Add(p);
        }

        public void AddEdge(BPMNEdge p)
        {
            if (p != null)
                edges.Add(p);
        }
    }
}
