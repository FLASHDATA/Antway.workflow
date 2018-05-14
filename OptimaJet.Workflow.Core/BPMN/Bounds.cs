namespace OptimaJet.Workflow.Core.BPMN
{
    public class Bounds
    {
        public int x;
        public int y;
        public int width;
        public int height;

        public Bounds() { }

        public Bounds(int x, int y, int w, int h)
        {
            this.x = x;
            this.y = y;
            this.width = w;
            this.height = h;
        }

        public Waypoint GetWaypoint()
        {
            return new Waypoint(x, y);
        }
    }
}
