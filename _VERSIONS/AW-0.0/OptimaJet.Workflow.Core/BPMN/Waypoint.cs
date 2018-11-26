namespace OptimaJet.Workflow.Core.BPMN
{
    public class Waypoint
    {
        public int x;
        public int y;
        public string type;

        public Waypoint()
        {

        }

        public Waypoint(string x, string y)
        {
            int.TryParse(x, out this.x);
            int.TryParse(y, out this.y);
            type = "point";
        }

        public Waypoint(int x, int y)
        {
            this.x = x;
            this.y = y;
            type = "point";
        }
    }
}
