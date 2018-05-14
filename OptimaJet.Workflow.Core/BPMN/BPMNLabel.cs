namespace OptimaJet.Workflow.Core.BPMN
{
    public class BPMNLabel
    {
        public Bounds bounds;

        public BPMNLabel()
        {
            bounds = new Bounds();
        }

        public BPMNLabel(int x, int y, int w, int h)
        {
            bounds = new Bounds(x, y, w, h);
        }
    }
}
