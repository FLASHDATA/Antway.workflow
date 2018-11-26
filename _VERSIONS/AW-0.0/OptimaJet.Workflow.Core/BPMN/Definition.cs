using System;
using System.Collections.Generic;
using System.Linq;
using OptimaJet.Workflow.Core.Model;
using System.Xml.Linq;

namespace OptimaJet.Workflow.Core.BPMN
{
    public class Definition
    {
        public string Name { get; set; }
        public List<Process> processList = new List<Process>();
        public List<Collaboration> collaborationList = new List<Collaboration>();
        public BPMNDiagram diagram = new BPMNDiagram();

        /// <summary>
        /// The constructor creates Definition object and loads BPMN2 scheme
        /// </summary>
        /// <param name="bpmn">BPMN2 scheme in XML-format</param>
        public Definition(string bpmn)
        {
            XElement xml = XElement.Parse(bpmn);
            var nameAtt = xml.Attribute("id");
            if (nameAtt != null)
                Name = nameAtt.Value;

            foreach (XElement el in xml.Elements())
            {
                switch (el.Name.LocalName.ToLower())
                {
                    case "process":
                        var ps = ParseProcess(el);
                        if (ps != null)
                            processList.Add(ps);
                        break;
                    case "collaboration":
                        var collaboration = ParseCollaboration(el);
                        if (collaboration != null)
                            collaborationList.Add(collaboration);
                        break;
                    case "bpmndiagram":
                        diagram = ParseDiagram(el);
                        break;
                }
            }
        }

        /// <summary>
        /// The constructor creates Definition object and loads information from ProcessDefinition object
        /// </summary>
        /// <param name="pd">ProcessDefinition object for loading</param>
        public Definition(ProcessDefinition pd)
        {
            Name = pd.Name;
            if (string.IsNullOrWhiteSpace(Name))
                Name = "Unnamed";

            var process = new Process();
            processList.Add(process);

            diagram = new BPMNDiagram();
            BPMNPlane plane = new BPMNPlane();
            plane.bpmnElement = Name;
            diagram.AddPlane(plane);

            #region Activities
            foreach (var a in pd.Activities)
            {
                bool isTask = false;
                if (a.IsInitial || a.IsFinal)
                {
                    process.AddEvent(new Event()
                    {
                        Id = a.Name,
                        Name = a.State,
                        Type = a.IsInitial ? EventType.startEvent : EventType.endEvent,
                        SubType = EventSubType.none,
                        IncomingList = pd.Transitions.Where(tr => tr.From.Name == a.Name).Select(tr => tr.Name).ToList(),
                        OutgoingList = pd.Transitions.Where(tr => tr.To.Name == a.Name).Select(tr => tr.Name).ToList()
                    });
                }
                else
                {
                    #region for future
                    //if (pd.Transitions.Exists(c => c.From.Name == a.Name && c.IsFork))
                    //{
                    //    process.AddGateway(new Gateway()
                    //    {
                    //        Id = a.Name,
                    //        Name = a.State,
                    //        Type = GatewayType.ParallelGateway,
                    //        IncomingList = pd.Transitions.Where(tr => tr.From.Name == a.Name).Select(tr => tr.Name).ToList(),
                    //        OutgoingList = pd.Transitions.Where(tr => tr.To.Name == a.Name).Select(tr => tr.Name).ToList()
                    //    });
                    //}
                    //else if (pd.Transitions.Exists(c => c.To.Name == a.Name && c.IsOtherwiseTransition))
                    //{
                    //    process.AddGateway(new Gateway()
                    //    {
                    //        Id = a.Name,
                    //        Name = a.State,
                    //        Type = GatewayType.EventBasedGateway,
                    //        IncomingList = pd.Transitions.Where(tr => tr.From.Name == a.Name).Select(tr => tr.Name).ToList(),
                    //        OutgoingList = pd.Transitions.Where(tr => tr.To.Name == a.Name).Select(tr => tr.Name).ToList()
                    //    });
                    //}
                    //else if(pd.Transitions.Where(c => c.To.Name == a.Name && c.Trigger.Type == TriggerType.Command).Count() > 1)
                    //{
                    //    process.AddEvent(new Event()
                    //    {
                    //        Id = a.Name,
                    //        Name = a.State,
                    //        Type = EventType.intermediateThrowEvent,
                    //        SubType = EventSubType.messageEventDefinition,
                    //        IncomingList = pd.Transitions.Where(tr => tr.From.Name == a.Name).Select(tr => tr.Name).ToList(),
                    //        OutgoingList = pd.Transitions.Where(tr => tr.To.Name == a.Name).Select(tr => tr.Name).ToList()
                    //    });
                    //}
                    //else 
                    #endregion

                    {
                        process.AddTask(new Task()
                        {
                            Id = a.Name,
                            Name = a.State,
                            Type = TaskType.Task,
                            IncomingList = pd.Transitions.Where(tr => tr.From.Name == a.Name).Select(tr => tr.Name).ToList(),
                            OutgoingList = pd.Transitions.Where(tr => tr.To.Name == a.Name).Select(tr => tr.Name).ToList()
                        });
                        isTask = true;
                    }
                }
                
                plane.AddShape(new BPMNShape()
                {
                    bpmnElement = a.Name,
                    bounds = CreateBoundsByDesignerSettings(a.DesignerSettings, isTask)
                });
            }
            #endregion

            #region Transitions
            foreach (var t in pd.Transitions)
            {
                if (t.Trigger.Type == TriggerType.Timer)
                {
                    #region timer
                    string timerEventId = t.Name + "_Timer";
                    string timerIn = t.Name;
                    string timerOut = t.Name + "_Out";

                    process.AddEvent(new Event()
                    {
                        Id = timerEventId,
                        Type = EventType.intermediateThrowEvent,
                        SubType = EventSubType.timerEventDefinition,
                        Name = t.Trigger.Timer.Value,
                        IncomingList = new List<string>() { timerIn },
                        OutgoingList = new List<string>() { timerOut }
                    });

                    process.AddFlow(new Flow()
                    {
                        Id = timerIn,
                        sourceRef = t.From.Name,
                        targetRef = timerEventId
                    });

                    process.AddFlow(new Flow()
                    {
                        Id = timerOut,
                        sourceRef = timerEventId,
                        targetRef = t.To.Name
                    });

                    var timerbounds = CreateBoundsByDesignerSettings(t.DesignerSettings, false, false);
                    timerbounds.x -= timerbounds.width / 2;
                    timerbounds.y -= timerbounds.height / 2;

                    plane.AddShape(new BPMNShape()
                    {
                        bpmnElement = timerEventId,
                        bounds = timerbounds
                    });
                    
                    var w1 = plane.shapes
                        .Where(c => c.bpmnElement == t.From.Name)
                        .Select(c => c.bounds.GetWaypoint()).FirstOrDefault();
                    var w2 = timerbounds.GetWaypoint();
                    var w3 = plane.shapes
                        .Where(c => c.bpmnElement == t.To.Name)
                        .Select(c => c.bounds.GetWaypoint()).FirstOrDefault();

                    var waypoints = new List<Waypoint>();
                    waypoints.Add(w1);
                    waypoints.Add(w2);

                    plane.AddEdge(new BPMNEdge()
                    {
                        bpmnElement = timerIn,
                        waypoints = waypoints
                    });

                    waypoints = new List<Waypoint>();
                    waypoints.Add(w2);
                    waypoints.Add(w3);

                    plane.AddEdge(new BPMNEdge()
                    {
                        bpmnElement = timerOut,
                        waypoints = waypoints
                    });
                    #endregion
                }
                else
                {
                    if (t.Trigger.Type == TriggerType.Command)
                    {
                        process.AddFlow(new Flow()
                        {
                            Id = t.Name,
                            Name = t.Trigger.Command.Name,
                            sourceRef = t.From.Name,
                            targetRef = t.To.Name
                        });
                    }
                    else
                    {
                        process.AddFlow(new Flow()
                        {
                            Id = t.Name,
                            sourceRef = t.From.Name,
                            targetRef = t.To.Name
                        });
                    }

                    var waypoints = new List<Waypoint>();
                    waypoints.Add(plane.shapes
                        .Where(c => c.bpmnElement == t.From.Name)
                        .Select(c=> c.bounds.GetWaypoint()).FirstOrDefault());

                    if (!string.IsNullOrWhiteSpace(t.DesignerSettings.X) && !string.IsNullOrWhiteSpace(t.DesignerSettings.Y))
                    {
                        waypoints.Add(new Waypoint(t.DesignerSettings.X, t.DesignerSettings.Y));
                    }
                    waypoints.Add(plane.shapes
                        .Where(c => c.bpmnElement == t.To.Name)
                        .Select(c => c.bounds.GetWaypoint()).FirstOrDefault());

                    plane.AddEdge(new BPMNEdge()
                    {
                        bpmnElement = t.Name,
                        waypoints = waypoints
                    });
                }
            }
            #endregion

            NormalizePlane(plane, process);
        }

        private void NormalizePlane(BPMNPlane plane, Process process)
        {
            foreach(var e in plane.edges)
            {
                if (e.waypoints.Count < 2)
                    continue;

                var obj = process.flows.Where(f => f.Id == e.bpmnElement).FirstOrDefault();
                var source = plane.shapes.Where(s => s.bpmnElement == obj.sourceRef).FirstOrDefault();
                var target = plane.shapes.Where(s => s.bpmnElement == obj.targetRef).FirstOrDefault();

                var sw2 = source.bounds.width / 2; var sh2 = source.bounds.height / 2;
                var tw2 = target.bounds.width / 2; var th2 = target.bounds.height / 2;
                var dsize = 25;
                var ascx = source.bounds.x + sw2;
                var ascy = source.bounds.y + sh2;
                var aecx = target.bounds.x + tw2;
                var aecy = target.bounds.y + th2;
               
                int[] direction = new int[] { 0, 0 };

                Waypoint start = new Waypoint();
                Waypoint end = new Waypoint();
                Waypoint middle = e.waypoints.Count > 2 ? e.waypoints[1] : null;

                if (source.bpmnElement == target.bpmnElement)
                {
                    start.x = ascx + sw2;
                    start.y = ascy - sh2 + 14;

                    end.x = aecx + tw2 - 25;
                    end.y = aecx + aecy - th2;

                    direction[1] = 1;
                    if (middle == null)
                        middle = new Waypoint(ascx + sw2 + sh2, ascy - 2 * sh2);
                }
                else
                {
                    var recalcMiddle = false;
                    if (middle == null)
                    {
                        recalcMiddle = true;
                        middle = new Waypoint((ascx + aecx) / 2, (ascy + aecy) / 2);
                    }

                    int xs = ascx, ys = ascy, xe = aecx, ye = aecy;
                    
                    if (ascy - sh2 - dsize > middle.y && aecy - th2 - dsize > middle.y)
                    {
                        ys = ascy - sh2;
                        ye = aecy - th2;
                        direction[0] = 1;
                        direction[1] = 1;
                    }
                    else if (ascy + sh2 + dsize < middle.y && aecy + th2 + dsize < middle.y)
                    {
                        ys = ascy + sh2;
                        ye = aecy + th2;
                        direction[0] = 1;
                        direction[1] = 1;
                    }
                    else if (ascx - sw2 - dsize > middle.x && aecx - tw2 - dsize > middle.x)
                    {
                        xs = ascx - sw2;
                        xe = aecx - tw2;
                    }
                    else if (ascx + sw2 + dsize < middle.x && aecx + tw2 + dsize < middle.x)
                    {
                        xs = ascx + sw2;
                        xe = aecx + tw2;
                    }
                    else
                    {
                        //calculate start point
                        if (ascx + sw2 + dsize < middle.x) xs += sw2;
                        else if (ascx - sw2 - dsize > middle.x) xs -= sw2;
                        else if (ascy + sh2 + dsize < middle.y)
                        {
                            ys += sh2;
                            direction[0] = 1;
                        }
                        else if (ascy - sh2 - dsize > middle.y)
                        {
                            ys -= sh2;
                            direction[0] = 1;
                        }
                        else
                        {
                            if (xs <= middle.x)
                                xs += sw2;
                            else
                            {
                                xs -= sw2;
                                //direction[0] = 1;
                            }
                        }

                        //calculate end point
                        if (aecx + tw2 + dsize < middle.x) xe += tw2;
                        else if (aecx - tw2 - dsize > middle.x) xe -= tw2;
                        else if (aecy + th2 + dsize < middle.y)
                        {
                            ye += th2;
                            direction[1] = 1;
                        }
                        else if (aecy - th2 - dsize > middle.y)
                        {
                            ye -= th2;
                            direction[1] = 1;
                        }
                        else
                        {
                            if (ys >= middle.y)
                                ye += th2;
                            else
                            {
                                ye -= th2;
                            }
                            direction[1] = 1;
                        }
                    }
                    
                    start = new Waypoint(xs, ys);
                    end = new Waypoint(xe, ye);

                    if (recalcMiddle)
                    {
                        middle = new Waypoint((xs + xe) / 2, (ys + ye) / 2 );
                    }

                    //Correct start/end points
                    if (direction[0] == 0)
                    {
                        if (middle.y > ys + sh2 - 7)
                            ys += sh2 - 7;
                        else if (middle.y < ys - sh2 + 7)
                            ys -= sh2 - 7;
                        else
                            ys = middle.y;
                    }
                    else
                    {
                        if (middle.x > xs + sw2 - 10)
                            xs += sw2 - 10;
                        else if (middle.x < xs - sw2 + 10)
                            xs -= sw2 - 10;
                        else
                            xs = middle.x;
                    }

                    if (direction[1] == 0)
                    {
                        if (middle.y > ye + th2 - 7)
                            ye += th2 - 7;
                        else if (middle.y < ye - th2 + 7)
                            ye -= th2 - 7;
                        else
                            ye = middle.y;
                    }
                    else
                    {
                        if (middle.x > xe + tw2 - 10)
                            xe += tw2 - 10;
                        else if (middle.x < xe - tw2 + 10)
                            xe -= tw2 - 10;
                        else
                            xe = middle.x;
                    }

                    start = new Waypoint(xs, ys);
                    end = new Waypoint(xe, ye);

                    if (recalcMiddle)
                    {
                        middle = new Waypoint(end.x, start.y);
                    }
                }

                List<Waypoint> points = new List<Waypoint>{ start, middle, end };
                e.waypoints = GetPoints(points, direction);
            }
        }

        private List<Waypoint> GetPoints(List<Waypoint> points, int[] direction)
        {
            // one line
            if (((points[0].x == points[1].x) && (points[1].x == points[2].x)) || 
                ((points[0].y == points[1].y) && (points[1].y == points[2].y)))
                return points;

            var res = new List<Waypoint>();

            if (points[0].x == points[1].x || points[0].y == points[1].y)
            {
                res.Add(points[0]);
                res.Add(points[1]);
            }
            else
            {
                res.Add(points[0]);
               
                if (direction[0] == 0)
                    res.Add(new Waypoint(points[1].x, points[0].y));
                else
                    res.Add(new Waypoint(points[0].x, points[1].y));

                res.Add(points[1]);
            }

            if (direction[1] == 0 && res[res.Count - 1].x != points[2].x)
            {
                res.Add(new Waypoint(res[res.Count - 1].x, points[2].y));
            }
            else if (direction[1] == 1 && res[res.Count - 1].y != points[2].y)
            {
                res.Add(new Waypoint(points[2].x, res[res.Count - 1].y));
            }

            res.Add(points[2]);
            return res;
        }
        
        private Bounds CreateBoundsByDesignerSettings(DesignerSettings ds, bool isTask = false, bool adoptSize = true)
        {
            int DefaultActivityWidth = 190;
            int DefaultActivityHeight = 60;

            int x = 0;
            int y = 0;
            int.TryParse(ds.X, out x);
            int.TryParse(ds.Y, out y);

            int w = isTask ? 100 : 36;
            int h = isTask ? 80 : 36;

            if (adoptSize)
            {
                x += (DefaultActivityWidth - w) / 2;
                y += (DefaultActivityHeight - h) / 2;
            }

            return new Bounds(x, y, w, h);
        }

        #region Convertion to ProcessDefinition
        /// <summary>
        /// Gets a list of ProcessDefinition from BPMN format 
        /// </summary>
        /// <returns></returns>
        public List<ProcessDefinition> ConvertToProcessDefinitions()
        {
            List<ProcessDefinition> pds = new List<ProcessDefinition>();
            foreach (var p in processList)
            {
                ProcessDefinition pd = new ProcessDefinition();
                Fill(pd, p);
                pds.Add(pd);
            }

            return pds;
        }

        public ProcessDefinition ConvertToComplexProcessDefinitions()
        {
            ProcessDefinition pd = new ProcessDefinition();
            foreach (var p in processList)
            {
                Fill(pd, p);
            }

            foreach (var p in collaborationList)
            {
                Fill(pd, p);
            }

            return pd;
        }

        private void Fill(ProcessDefinition pd, Process p)
        {
            List<string> objectsForOptimization = new List<string>();
            List<Task> subprocessList = new List<Task>();

            foreach (var c in p.events)
            {
                if (c.SubType == EventSubType.timerEventDefinition)
                {
                    objectsForOptimization.Add(c.Id);
                }

                var a = ActivityDefinition.Create(c.Id, c.Name,
                    c.Type == EventType.startEvent,
                    c.Type == EventType.endEvent,
                    true, true);

                a.DesignerSettings = GetPosition(c.Id, diagram);
                pd.Activities.Add(a);
            }

            #region Tasks
            foreach (var c in p.tasks)
            {
                if (c.EventsList.Count > 0)// Subprocess
                {
                    subprocessList.Add(c);
                }

                var a = ActivityDefinition.Create(c.Id, c.Name,
                    false,
                    false,
                    true, true);

                a.DesignerSettings = GetPosition(c.Id, diagram);
                pd.Activities.Add(a);
            }
            #endregion

            #region Gateways
            foreach (var c in p.gateways)
            {
                objectsForOptimization.Add(c.Id);

                var a = ActivityDefinition.Create(c.Id, c.Name,
                    false,
                    false,
                    true, true);

                a.DesignerSettings = GetPosition(c.Id, diagram);
                pd.Activities.Add(a);
            }

            foreach (var c in p.flows)
            {
                FillFlows(pd, p, c);
            }
            #endregion

            #region  Subprocess
            foreach(var subprocess in subprocessList)
            {
                var activity = pd.Activities.FirstOrDefault(c => c.Name == subprocess.Id);
                if (activity == null)
                    continue;

                var transitionsTo = pd.Transitions.Where(c => c.To.Name == activity.Name).ToList();
                var transitionsFrom = pd.Transitions.Where(c => c.From.Name == activity.Name).ToList();

                ActivityDefinition activityStart = null;
                ActivityDefinition activityEnd = null;

                foreach(var c in subprocess.EventsList)
                {
                    var a = ActivityDefinition.Create(c.Id, c.Name,
                        false,
                        false,
                        true, true);

                    a.DesignerSettings = GetPosition(c.Id, diagram);
                    if (c.Type == EventType.startEvent)
                    {
                        activityStart = a;
                        objectsForOptimization.Add(a.Name);
                    }
                    else if (c.Type == EventType.endEvent)
                    {
                        activityEnd = a;
                        objectsForOptimization.Add(a.Name);
                    }

                    pd.Activities.Add(a);
                }

                foreach (var c in subprocess.TasksList)
                {
                    var a = ActivityDefinition.Create(c.Id, c.Name,
                        false,
                        false,
                        true, true);

                    a.DesignerSettings = GetPosition(c.Id, diagram);
                    pd.Activities.Add(a);
                }

                foreach (var c in subprocess.FlowList)
                {
                    FillFlows(pd, p, c);
                }


                if (activityStart != null)
                {
                    transitionsTo.ForEach(c => {
                        c.To = activityStart;
                        c.IsFork = true;
                    });
                }
                else
                {
                    transitionsTo.ForEach(c => pd.Transitions.Remove(c));
                }
                if (activityEnd != null)
                {
                    transitionsFrom.ForEach(c => {
                        c.From = activityEnd;
                        c.IsFork = true;
                    });
                }
                else
                {
                    transitionsFrom.ForEach(c => pd.Transitions.Remove(c));
                }
                pd.Activities.Remove(activity);
            }
            #endregion

            #region Optimization
            foreach (var item in objectsForOptimization)
            {
                var activity = pd.Activities.Where(c => c.Name == item).FirstOrDefault();
                if (activity == null)
                    continue;

                var transFrom = pd.Transitions.Where(c => c.From == activity).ToList();
                var transTo = pd.Transitions.Where(c => c.To == activity).ToList();

                if (transFrom.Count > 1 && transTo.Count > 1)
                    continue;

                var transForDelete = new List<TransitionDefinition>();
                if(transFrom.All(c=> c.Trigger.Type == TriggerType.Auto))
                {
                    transFrom.ForEach(t =>
                    {
                        transForDelete.Add(t);
                        transTo.ForEach(a => a.To = t.To);

                        if (t.IsFork)
                            transTo.ForEach(a => a.IsFork = true);
                    });
                }
                else if(transTo.All(c => c.Trigger.Type == TriggerType.Auto))
                {
                    transTo.ForEach(t =>
                    {
                        transForDelete.Add(t);
                        transFrom.ForEach(a => a.From = t.From);

                        if (t.IsFork)
                            transFrom.ForEach(a => a.IsFork = true);
                    });
                }
                
                if (transForDelete.Count > 0)
                {
                    pd.Activities.Remove(activity);
                    foreach (var td in transForDelete)
                        pd.Transitions.Remove(td);
                }
            }
            #endregion
        }

        private void FillFlows(ProcessDefinition pd, Process p, Flow c)
        {
            var from = pd.Activities.Where(a => a.Name == c.sourceRef).FirstOrDefault();
            var to = pd.Activities.Where(a => a.Name == c.targetRef).FirstOrDefault();

            if (from == null)
                throw new Exception(string.Format("Not found {0} object for {1} flow",
                    c.sourceRef, c.Id));

            if (to == null)
                throw new Exception(string.Format("Not found {0} object for {1} flow",
                    c.targetRef, c.Id));

            var t = TransitionDefinition.Create(from, to);
            t.Name = c.Id;
            t.Classifier = TransitionClassifier.Direct;

            //Timer Trigger
            var toEvent = p.events.Where(e => e.Id == c.targetRef).FirstOrDefault();
            if (toEvent != null && toEvent.SubType == EventSubType.timerEventDefinition)
            {
                var timer = pd.Timers.Where(tobj => tobj.Name == toEvent.Id).FirstOrDefault();
                if (timer == null)
                {
                    timer = new TimerDefinition()
                    {
                        Name = toEvent.Id,
                        Type = TimerType.Interval
                    };
                    pd.Timers.Add(timer);
                }
                timer.Value = toEvent.Name;
                t.Trigger = new TriggerDefinition(TriggerType.Timer);
                t.Trigger.Timer = timer;
            }
            else
            {
                //Command Trigger
                if (!string.IsNullOrWhiteSpace(c.Name))
                {
                    var command = pd.Commands.Where(com => com.Name == c.Name).FirstOrDefault();
                    if (command == null)
                    {
                        command = CommandDefinition.Create(c.Name);
                        pd.Commands.Add(command);
                    }
                    t.Trigger = new TriggerDefinition(TriggerType.Command);
                    t.Trigger.Command = command;
                }
            }

            pd.Transitions.Add(t);
        }

        private void Fill(ProcessDefinition pd, Collaboration collaboration)
        {
            foreach (var c in collaboration.MessageFlowList)
            {
                var from = pd.Activities.Where(a => a.Name == c.sourceRef).FirstOrDefault();
                var to = pd.Activities.Where(a => a.Name == c.targetRef).FirstOrDefault();

                if (from == null)
                    throw new Exception(string.Format("Not found {0} object for {1} MessageFlow",
                        c.sourceRef, c.Id));

                if (to == null)
                    throw new Exception(string.Format("Not found {0} object for {1} MessageFlow",
                        c.targetRef, c.Id));

                var t = TransitionDefinition.Create(from, to);
                t.Name = c.Id;
                t.Classifier = TransitionClassifier.NotSpecified;
                pd.Transitions.Add(t);
            }
        }

        private DesignerSettings GetPosition(string id, BPMNDiagram diagram)
        {
            foreach (var plane in diagram.planes)
            {
                foreach (var shape in plane.shapes)
                {
                    if (shape.bpmnElement == id)
                    {
                        if (shape.bounds != null)
                            return new DesignerSettings() { X = shape.bounds.x.ToString(), Y = shape.bounds.y.ToString() };
                        else
                            return new DesignerSettings();
                    }
                }
            }
            return new DesignerSettings();
        }
        #endregion

        #region Diagram section
        private BPMNDiagram ParseDiagram(XElement el)
        {
            var d = new BPMNDiagram();

            foreach (XElement child in el.Elements())
            {
                switch (child.Name.LocalName)
                {
                    case "BPMNPlane":
                        d.AddPlane(ParsePlane(child));
                        break;
                }
            }

            return d;
        }

        private BPMNPlane ParsePlane(XElement el)
        {
            var p = new BPMNPlane();

            var idAtt = el.Attribute("id");
            if (idAtt != null)
                p.Id = idAtt.Value;

            var bpmnElementAtt = el.Attributes().Where(a => a.Name.LocalName.ToLower() == "bpmnelement").FirstOrDefault();
            if (bpmnElementAtt != null)
                p.bpmnElement = bpmnElementAtt.Value;

            foreach (XElement child in el.Elements())
            {
                switch (child.Name.LocalName.ToLower())
                {
                    case "bpmnshape":
                        p.AddShape(ParseShape(child));
                        break;
                    case "bpmnedge":
                        p.AddEdge(ParseEdge(child));
                        break;
                }
            }

            return p;
        }

        private BPMNShape ParseShape(XElement el)
        {
            var s = new BPMNShape();

            var idAtt = el.Attribute("id");
            if (idAtt != null)
                s.Id = idAtt.Value;

            var bpmnElementAtt = el.Attributes().Where(a => a.Name.LocalName.ToLower() == "bpmnelement").FirstOrDefault();
            if (bpmnElementAtt != null)
                s.bpmnElement = bpmnElementAtt.Value;

            foreach (XElement child in el.Elements())
            {
                switch (child.Name.LocalName.ToLower())
                {
                    case "bounds":
                        s.bounds = ParseBounds(child);
                        break;
                    case "bpmnlabel":
                        s.Label = ParseLabel(child);
                        break;
                }
            }

            return s;
        }

        private Bounds ParseBounds(XElement el)
        {
            int x = 0, y = 0, w = 0, h = 0;
            var xAtt = el.Attribute("x");
            if (xAtt != null)
                int.TryParse(xAtt.Value, out x);

            var yAtt = el.Attribute("y");
            if (yAtt != null)
                int.TryParse(yAtt.Value, out y);

            var heightAtt = el.Attribute("height");
            if (heightAtt != null)
                int.TryParse(heightAtt.Value, out h);
            
            var widthAtt = el.Attribute("width");
            if (widthAtt != null)
                int.TryParse(widthAtt.Value, out w);

            return new Bounds(x, y, w, h);
        }

        private BPMNLabel ParseLabel(XElement el)
        {
            var p = new BPMNLabel();

            foreach (XElement child in el.Elements())
            {
                switch (child.Name.LocalName)
                {
                    case "Bounds":
                        p.bounds = ParseBounds(child);
                        break;
                }
            }

            return p;
        }

        private BPMNEdge ParseEdge(XElement el)
        {
            var p = new BPMNEdge();

            var idAtt = el.Attribute("id");
            if (idAtt != null)
                p.Id = idAtt.Value;

            var bpmnElementAtt = el.Attributes().Where(a => a.Name.LocalName.ToLower() == "bpmnelement").FirstOrDefault();
            if (bpmnElementAtt != null)
                p.bpmnElement = bpmnElementAtt.Value;

            foreach (XElement child in el.Elements())
            {
                switch (child.Name.LocalName)
                {
                    case "waypoint":
                        p.AddWaypoint(
                            child.Attribute("type")?.Value,
                            child.Attribute("x")?.Value,
                            child.Attribute("y")?.Value);
                        break;
                }
            }
            return p;
        }
        #endregion

        #region Process section
        private Process ParseProcess(XElement el)
        {
            var ps = new Process();
            foreach (XElement child in el.Elements())
            {
                switch (child.Name.LocalName.ToLower())
                {
                    //Events
                    case "startevent":
                        ps.AddEvent(ParseEvent(child, EventType.startEvent));
                        break;
                    case "endevent":
                        ps.AddEvent(ParseEvent(child, EventType.endEvent));
                        break;
                    case "intermediatethrowevent":
                        ps.AddEvent(ParseEvent(child, EventType.intermediateThrowEvent));
                        break;
                    case "intermediatecatchevent":
                        ps.AddEvent(ParseEvent(child, EventType.intermediateCatchEvent));
                        break;
                    //Flow
                    case "sequenceflow":
                        ps.AddFlow(ParseFlow(child));
                        break;
                    //Gateways
                    case "exclusivegateway":
                        ps.AddGateway(ParseGateway(child, GatewayType.ExclusiveGateway));
                        break;
                    case "inclusivegateway":
                        ps.AddGateway(ParseGateway(child, GatewayType.InclusiveGateway));
                        break;
                    case "parallelgateway":
                        ps.AddGateway(ParseGateway(child, GatewayType.ParallelGateway));
                        break;
                    case "complexgateway":
                        ps.AddGateway(ParseGateway(child, GatewayType.ComplexGateway));
                        break;
                    case "eventbasedgateway":
                        ps.AddGateway(ParseGateway(child, GatewayType.EventBasedGateway));
                        break;
                    //Tasks
                    case "task":
                        ps.AddTask(ParseTask(child, TaskType.Task));
                        break;
                    case "usertask":
                        ps.AddTask(ParseTask(child, TaskType.UserTask));
                        break;
                    case "adhocsubprocess":
                        ps.AddTask(ParseTask(child, TaskType.AdHocSubProcess));
                        break;
                    case "businessruletask":
                        ps.AddTask(ParseTask(child, TaskType.BusinessRuleTask));
                        break;
                    case "callactivity":
                        ps.AddTask(ParseTask(child, TaskType.CallActivity));
                        break;
                    case "manualtask":
                        ps.AddTask(ParseTask(child, TaskType.ManualTask));
                        break;
                    case "receivetask":
                        ps.AddTask(ParseTask(child, TaskType.ReceiveTask));
                        break;
                    case "scripttask":
                        ps.AddTask(ParseTask(child, TaskType.ScriptTask));
                        break;
                    case "sendtask":
                        ps.AddTask(ParseTask(child, TaskType.SendTask));
                        break;
                    case "servicetask":
                        ps.AddTask(ParseTask(child, TaskType.ServiceTask));
                        break;
                    case "subprocess":
                        ps.AddTask(ParseTask(child, TaskType.SubProcess));
                        break;
                    case "transaction":
                        ps.AddTask(ParseTask(child, TaskType.Transaction));
                        break;

                }
            }

            return ps;
        }

        private Task ParseTask(XElement el, TaskType type)
        {
            Task e = new Task();
            e.Type = type;
            var idAtt = el.Attribute("id");
            if (idAtt != null)
                e.Id = idAtt.Value;

            var nameAtt = el.Attribute("name");
            if (nameAtt != null)
                e.Name = nameAtt.Value;

            foreach (XElement child in el.Elements())
            {
                switch (child.Name.LocalName.ToLower())
                {
                    case "incoming":
                        e.AddIncoming(child.Value);
                        break;
                    case "outgoing":
                        e.AddOutgoing(child.Value);
                        break;
                    case "startevent":
                        e.AddEvent(ParseEvent(child, EventType.startEvent));
                        break;
                    case "endevent":
                        e.AddEvent(ParseEvent(child, EventType.endEvent));
                        break;
                    case "task":
                        e.AddTask(ParseTask(child, TaskType.Task));
                        break;
                    case "sequenceflow":
                        e.AddFlow(ParseFlow(child));
                        break;
                }
            }

            return e;
        }

        private Gateway ParseGateway(XElement el, GatewayType type)
        {
            Gateway g = new Gateway();
            g.Type = type;

            var idAtt = el.Attribute("id");
            if (idAtt != null)
                g.Id = idAtt.Value;

            var nameAtt = el.Attribute("name");
            if (nameAtt != null)
                g.Name = nameAtt.Value;

            foreach (XElement child in el.Elements())
            {
                switch (child.Name.LocalName)
                {
                    case "incoming":
                        g.AddIncoming(child.Value);
                        break;
                    case "outgoing":
                        g.AddOutgoing(child.Value);
                        break;
                }
            }

            return g;
        }

        private Flow ParseFlow(XElement el)
        {
            Flow f = new Flow();

            var idAtt = el.Attribute("id");
            if (idAtt != null)
                f.Id = idAtt.Value;

            var nameAtt = el.Attribute("name");
            if (nameAtt != null)
                f.Name = nameAtt.Value;


            var sourceRefAtt = el.Attributes().Where(a => a.Name.LocalName.ToLower() == "sourceref").FirstOrDefault();
            if (sourceRefAtt != null)
                f.sourceRef = sourceRefAtt.Value;

            var targetRefAtt = el.Attributes().Where(a => a.Name.LocalName.ToLower() == "targetref").FirstOrDefault();
            if (targetRefAtt != null)
                f.targetRef = targetRefAtt.Value;

            return f;
        }

        private Event ParseEvent(XElement el, EventType type)
        {
            Event e = new Event();
            e.Type = type;
            var idAtt = el.Attribute("id");
            if (idAtt != null)
                e.Id = idAtt.Value;

            var nameAtt = el.Attribute("name");
            if (nameAtt != null)
                e.Name = nameAtt.Value;

            foreach (XElement child in el.Elements())
            {
                switch (child.Name.LocalName.ToLower())
                {
                    case "incoming":
                        e.AddIncoming(child.Value);
                        break;
                    case "outgoing":
                        e.AddOutgoing(child.Value);
                        break;
                    case "timereventdefinition":
                        e.SubType = EventSubType.timerEventDefinition;
                        break;
                    case "messageeventdefinition":
                        e.SubType = EventSubType.messageEventDefinition;
                        break;
                }
            }

            return e;
        }
        #endregion

        #region Collaboration section
        private Collaboration ParseCollaboration(XElement el)
        {
            var collabration = new Collaboration();
            foreach (XElement child in el.Elements())
            {
                switch (child.Name.LocalName.ToLower())
                {
                    case "participant":
                        collabration.AddParticipant(ParseParticipant(child));
                        break;
                    case "messageflow":
                        collabration.AddMessageFlow(ParseMessageFlow(child));
                        break;
                }
            }

            return collabration;
        }

        private MessageFlow ParseMessageFlow(XElement el)
        {
            MessageFlow f = new MessageFlow();

            var idAtt = el.Attribute("id");
            if (idAtt != null)
                f.Id = idAtt.Value;

            var nameAtt = el.Attribute("name");
            if (nameAtt != null)
                f.Name = nameAtt.Value;

            var sourceRefAtt = el.Attribute("sourceRef");
            if (sourceRefAtt != null)
                f.sourceRef = sourceRefAtt.Value;

            var targetRefAtt = el.Attribute("targetRef");
            if (targetRefAtt != null)
                f.targetRef = targetRefAtt.Value;

            return f;
        }

        private Participant ParseParticipant(XElement el)
        {
            Participant p = new Participant();

            var idAtt = el.Attribute("id");
            if (idAtt != null)
                p.Id = idAtt.Value;

            var nameAtt = el.Attribute("name");
            if (nameAtt != null)
                p.Name = nameAtt.Value;

            var processRefAtt = el.Attribute("processRef");
            if (processRefAtt != null)
                p.ProcessRef = processRefAtt.Value;

            return p;
        }
        #endregion

        #region Serialize

        private Dictionary<string, string> tags = new Dictionary<string, string>()
        {
            { "xsi", "http://www.w3.org/2001/XMLSchema-instance" },
            { "bpmn", "http://www.omg.org/spec/BPMN/20100524/MODEL" },
            {"bpmndi", "http://www.omg.org/spec/BPMN/20100524/DI"},
            {"dc", "http://www.omg.org/spec/DD/20100524/DC"},
            {"di", "http://www.omg.org/spec/DD/20100524/DI"}
        };

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>XML</returns>
        public string Serialize()
        {
            var doc = new XElement(CreateXName("definitions", "bpmn"),
                new XAttribute(XNamespace.Xmlns + "xsi", tags["xsi"]),
                new XAttribute(XNamespace.Xmlns + "bpmn", tags["bpmn"]),
                new XAttribute(XNamespace.Xmlns + "bpmndi", tags["bpmndi"]),
                new XAttribute(XNamespace.Xmlns + "dc", tags["dc"]),
                new XAttribute(XNamespace.Xmlns + "di", tags["di"]));

            string currentName = Name;
            doc.SetAttributeValue("id", "Definition_" + currentName);

            foreach(var process in processList)
            {
                XElement processTag = new XElement(CreateXName("process", "bpmn"));
                string id = Name;
                if(processList.Count > 1)
                {
                    id += "_" + processList.IndexOf(process);
                }

                processTag.SetAttributeValue("id", id);
                processTag.Add(process.events.Select(e => SerializeEvent(e)).ToArray());
                processTag.Add(process.flows.Select(e => SerializeFlow(e)).ToArray());
                processTag.Add(process.gateways.Select(e => SerializeGateway(e)).ToArray());
                processTag.Add(process.tasks.Select(e => SerializeTask(e)).ToArray());

                doc.Add(processTag);
            }
            
            XElement diagramTag = new XElement(CreateXName("BPMNDiagram", "bpmndi"));
            diagramTag.SetAttributeValue("id", "BPMNDiagram");
            diagramTag.Add(diagram.planes.Select(e => SerializePlane(e)).ToArray());
            doc.Add(diagramTag);
            return doc.ToString();
        }

        

        private XElement SerializePlane(BPMNPlane plane)
        {
            XElement tag = new XElement(CreateXName("BPMNPlane", "bpmndi"));
            tag.SetAttributeValue("bpmnElement", plane.bpmnElement);
            tag.Add(plane.shapes.Select(e => SerializeShape(e)).ToArray());
            tag.Add(plane.edges.Select(e => SerializeEdge(e)).ToArray());
            return tag;
        }

        private XElement SerializeEdge(BPMNEdge e)
        {
            XElement el = new XElement(CreateXName("BPMNEdge", "bpmndi"));
            el.SetAttributeValue("id", e.Id);
            el.SetAttributeValue("bpmnElement", e.bpmnElement);

            el.Add(e.waypoints.Select(c => SerializeWayPoint(c)));
            return el;
        }

        private XElement SerializeWayPoint(Waypoint c)
        {
            XElement el = new XElement(CreateXName("waypoint", "di"));
            el.SetAttributeValue("type", c.type);
            el.SetAttributeValue("x", c.x);
            el.SetAttributeValue("y", c.y);
            return el;
        }

        private XElement SerializeShape(BPMNShape e)
        {
            XElement el = new XElement(CreateXName("BPMNShape", "bpmndi"));
            el.SetAttributeValue("id", e.Id);
            el.SetAttributeValue("bpmnElement", e.bpmnElement);

            el.Add(SerializeBounds(e.bounds));

            if (e.Label != null)
            {
                el.Add(SerializeLabel(e.Label));
            }
            return el;
        }

        private XElement SerializeLabel(BPMNLabel label)
        {
            XElement el = new XElement(CreateXName("BPMNLabel", "bpmndi"));
            el.Add(SerializeBounds(label.bounds));
            return el;
        }

        private XElement SerializeBounds(Bounds e)
        {
            XElement el = new XElement(CreateXName("Bounds", "dc"));
            el.SetAttributeValue("x", e.x);
            el.SetAttributeValue("y", e.y);
            el.SetAttributeValue("width", e.width);
            el.SetAttributeValue("height", e.height);
            return el;
        }

        private object SerializeTask(Task e)
        {
            XElement el = new XElement(CreateXName(e.Type.ToString(), "bpmn"));
            el.SetAttributeValue("id", e.Id);
            el.SetAttributeValue("name", e.Name);

            el.Add(e.IncomingList.Select(c => new XElement(CreateXName("incoming", "bpmn")) { Value = c }));
            el.Add(e.OutgoingList.Select(c => new XElement(CreateXName("outgoing", "bpmn")) { Value = c }));

            el.Add(e.EventsList.Select(c => SerializeEvent(c)));
            el.Add(e.TasksList.Select(c => SerializeTask(c)));
            el.Add(e.FlowList.Select(c => SerializeFlow(c)));
            return el;
        }

        private XElement SerializeGateway(Gateway e)
        {
            XElement el = new XElement(CreateXName(e.Type.ToString(), "bpmn"));
            el.SetAttributeValue("id", e.Id);
            el.SetAttributeValue("name", e.Name);

            el.Add(e.IncomingList.Select(c => new XElement(CreateXName("incoming", "bpmn")) { Value = c }));
            el.Add(e.OutgoingList.Select(c => new XElement(CreateXName("outgoing", "bpmn")) { Value = c }));
            return el;
        }

        private XElement SerializeFlow(Flow e)
        {
            XElement el = new XElement(CreateXName("sequenceFlow", "bpmn"));
            el.SetAttributeValue("id", e.Id);
            el.SetAttributeValue("name", e.Name);
            el.SetAttributeValue("sourceRef", e.sourceRef);
            el.SetAttributeValue("targetRef", e.targetRef);
            return el;
        }

        private XElement SerializeEvent(Event e)
        {
            XElement el = new XElement(CreateXName(e.Type.ToString(), "bpmn"));
            el.SetAttributeValue("id", e.Id);
            el.SetAttributeValue("name", e.Name);

            el.Add(e.IncomingList.Select(c => new XElement(CreateXName("incoming", "bpmn")) { Value = c }));
            el.Add(e.OutgoingList.Select(c => new XElement(CreateXName("outgoing", "bpmn")) { Value = c }));

            if(e.SubType != EventSubType.none)
            {
                var elSub = new XElement(CreateXName(e.SubType.ToString(), "bpmn"));
                el.Add(elSub);
            }

            return el;
        }

        private XName CreateXName(string name, string ns)
        {
            return (XNamespace)tags[ns] + name;
        }
        #endregion
    }
}