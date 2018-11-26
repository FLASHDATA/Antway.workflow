using System.Collections.Generic;

namespace OptimaJet.Workflow.Core.BPMN
{
    public class BPMNEdge
    {
        public string Id { get; internal set; }
        public string bpmnElement;
        public List<Waypoint> waypoints = new List<Waypoint>();

        public void AddWaypoint(string type, string x, string y)
        {
            waypoints.Add(new Waypoint()
            {
                type = type,
                x = int.Parse(x),
                y = int.Parse(y)
            });
        }
    }
}
