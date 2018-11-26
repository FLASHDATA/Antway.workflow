using System.Collections.Generic;

namespace OptimaJet.Workflow.Core.BPMN
{
    public class BPMNDiagram
    {
        public List<BPMNPlane> planes = new List<BPMNPlane>();
        public void AddPlane(BPMNPlane p)
        {
            if (p != null)
                planes.Add(p);
        }
    }
}
