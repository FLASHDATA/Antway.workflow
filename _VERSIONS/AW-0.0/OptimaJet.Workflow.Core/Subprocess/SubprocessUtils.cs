using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Subprocess
{
    /// <summary>
    /// Algorithms for subprocesses branches
    /// </summary>
    public static class SubprocessUtils
    {
        /// <summary>
        /// Markups scheme, sets nesting level for each Activity
        /// </summary>
        /// <param name="processDefinition"></param>
        /// <returns>Process definition with marked nesting levels an subprocesses ids</returns>
        public static ProcessDefinition MarkupSubprocesses(this ProcessDefinition processDefinition)
        {
            if (!processDefinition.ContainsSubprocesses)
            {
                processDefinition.Activities.ForEach(a => a.NestingLevel = 0);
                return processDefinition;
            }

            foreach (var activity in processDefinition.Activities)
            {
                activity.NestingLevel = null;
            }
            
            var initialActivity = processDefinition.InitialActivity;
            initialActivity.NestingLevel = 0;

            var allTransitions = GetTransitionsForMarkup(processDefinition, initialActivity);

            if (!allTransitions.Any())
                return processDefinition;

            Markup(processDefinition, allTransitions, initialActivity);

            return processDefinition;
        }

        private static void Markup(ProcessDefinition processDefinition, List<TransitionDefinition> allTransitions, ActivityDefinition initialActivity)
        {
            allTransitions.Select(
                t =>
                    t.From == initialActivity
                        ? new {t.From, t.To, t.IsFork} as dynamic
                        : new {From = t.To, To = t.From, t.IsFork} as dynamic)
                .ToList()
                .ForEach(t => MarkupSubprocessNestingLevel(t, processDefinition));
        }

        private static List<TransitionDefinition> GetTransitionsForMarkup(ProcessDefinition processDefinition, ActivityDefinition initialActivity)
        {
            return processDefinition.Transitions.Where(t => t.From == initialActivity || t.To == initialActivity).ToList();
        }

        private static void MarkupSubprocessNestingLevel(dynamic transition, ProcessDefinition processDefinition)
        {
            var from = transition.From as ActivityDefinition;
            var to = transition.To as ActivityDefinition;

            if (!from.NestingLevel.HasValue)
            {
                throw new WrongSubprocessMarkupException(from, "Activity nesting level not marked.");
            }

            var nestinglevel = from.NestingLevel.Value + (transition.IsFork ? 1 : 0);

            if (!to.NestingLevel.HasValue)
                to.NestingLevel = nestinglevel;
            else if (Math.Abs(to.NestingLevel.Value - nestinglevel) == 1)
                throw new WrongSubprocessMarkupException(to,
                    "The intersection of processes with different levels of nesting.");
            else if (to.NestingLevel.Value <= nestinglevel)
                return;
            else
                to.NestingLevel = nestinglevel;

            var allTransitions = GetTransitionsForMarkup(processDefinition, to);

            if (!allTransitions.Any())
                return;

            Markup(processDefinition, allTransitions, to);
        }

        public static ProcessDefinition GetSubprocessDefinition(this ProcessDefinition processDefinition,
            TransitionDefinition startingTransition)
        {
            if (startingTransition.ForkType != TransitionForkType.ForkStart)
                throw new WrongSubprocessMarkupException(startingTransition, "Starting transition must be Virtual and start a subprocess.");

            var subprocess = processDefinition.Clone(true, true, true);

            var initialActivity = startingTransition.To.Clone();
            initialActivity.IsInitial = true;

            var activities = new List<ActivityDefinition> {initialActivity};

            var transitions = new List<TransitionDefinition>();

            var finalActivities = new List<string>();

            var cutoffNestingLevel = startingTransition.From.NestingLevel.Value;

            foreach (var transition in processDefinition.GetPossibleTransitionsForActivity(startingTransition.To, ForkTransitionSearchType.Both))
            {
                AddActivitiesAndTransitions(processDefinition, activities, transitions, transition, finalActivities, cutoffNestingLevel);
            }

            subprocess.Activities = activities;

            transitions.ForEach(t=>subprocess.Transitions.Add(t.Clone(subprocess.Actors,subprocess.Commands,subprocess.Activities,subprocess.Timers)));

            if (startingTransition.DisableParentStateControl)
            {
                return subprocess;
            }

            var activitiesToAnalyze = processDefinition.Activities.Where(a => a.NestingLevel <= cutoffNestingLevel).Select(a=>a.Name).ToList();

            var allowedStates = GetAllowedStates(processDefinition, startingTransition.From.Name, finalActivities,
                activitiesToAnalyze);
            subprocess.AllowedActivities = allowedStates;
            return subprocess;
        }

        private static void AddActivitiesAndTransitions(ProcessDefinition processDefinition,
            List<ActivityDefinition> activities, List<TransitionDefinition> transitions,
            TransitionDefinition startingTransition, List<string> finalActivities, int cutoffNestingLevel)
        {
            if (transitions.Any(t => t.Name.Equals(startingTransition.Name, StringComparison.Ordinal)))
                return;

            var to = startingTransition.To;

            if (startingTransition.ForkType == TransitionForkType.ForkEnd &&
                to.NestingLevel <= cutoffNestingLevel)
            {
                var newTransition = startingTransition.Clone();
                newTransition.IsFork = false;
                transitions.Add(newTransition);

                if (!activities.Any(a => a.Name.Equals(to.Name, StringComparison.Ordinal)))
                {
                    var newActivity = ActivityDefinition.Create(to.Name, to.State, false, true, false, false);
                    activities.Add(newActivity);
                    finalActivities.Add(to.Name);
                }
                return;
            }

            transitions.Add(startingTransition);
            if (!activities.Any(a => a.Name.Equals(to.Name, StringComparison.Ordinal)))
                activities.Add(to.Clone());

            foreach (var transition in processDefinition.GetPossibleTransitionsForActivity(startingTransition.To,ForkTransitionSearchType.Both))
            {
                AddActivitiesAndTransitions(processDefinition, activities, transitions, transition, finalActivities,
                    cutoffNestingLevel);
            }

        }

        private static List<string> GetAllowedStates(this ProcessDefinition processDefinition, string startActivity,
            List<string> finalActivities, List<string> activitiesToAnalyze)
        {
            if (finalActivities.Count > 1)
            {
                var transitionClassifiers = new List<TransitionClassifier>
                {
                    TransitionClassifier.Direct,
                    TransitionClassifier.NotSpecified
                };

                //Stage 1. False finals clear
                //Stage 1.1 Ranging finals by distance from start
                var finalsWithDistance = new List<dynamic>();

                foreach (var final in finalActivities)
                {
                    var distance = processDefinition.GetMinDistance(startActivity, final, activitiesToAnalyze,
                        transitionClassifiers);
                    finalsWithDistance.Add(new {Name = final, Distance = distance});
                }

                var rangedFinalActivities =
                    finalsWithDistance.OrderBy(f => f.Distance).Select(f => f.Name).Cast<string>().ToList();

                //Stage 1.2 Removing false finals

                var tempList = new List<string>();
                tempList.AddRange(rangedFinalActivities);

                foreach (var final in rangedFinalActivities)
                {
                    var final1 = final;
                    foreach (var finalToCompare in tempList.Where(t => t != final1))
                    {
                        var distance = processDefinition.GetMinDistance(final, finalToCompare, activitiesToAnalyze,
                            transitionClassifiers);
                        if (distance < int.MaxValue)
                        {
                            tempList.Remove(final);
                            break;
                        }
                    }
                }

                finalActivities = tempList;
            }


            var result = new List<string>();

            //Stage 2 Getting allowed states for subprocess
            GetAllowedStates(processDefinition, startActivity, finalActivities, activitiesToAnalyze, result);

            return result;
        }

        private static void GetAllowedStates(this ProcessDefinition processDefinition, string activityName,
            List<string> finalActivities, List<string> activitiesToAnalyze, List<string> result)
        {
            if (!activitiesToAnalyze.Contains(activityName))
                return;

            activitiesToAnalyze.Remove(activityName);
            result.Add(activityName);

            if (finalActivities.Contains(activityName))
            {
                return;
            }

            var activity = processDefinition.FindActivity(activityName);
            var transitions =
                processDefinition.GetPossibleTransitionsForActivity(activity,ForkTransitionSearchType.Both)
                    .Where(t => t.Classifier != TransitionClassifier.Reverse && activitiesToAnalyze.Contains(t.To.Name));

            foreach (var transition in transitions)
            {
                GetAllowedStates(processDefinition, transition.To.Name, finalActivities, activitiesToAnalyze, result);
            }

        }

        public static int GetMinDistance(this ProcessDefinition processDefinition, string fromActivityName,
            string toActivityName, List<string> activitiesToAnalyze, List<TransitionClassifier> classifiers)
        {
            var graphDesc = new Dictionary<string, dynamic>();

            foreach (var activity in activitiesToAnalyze)
            {
                dynamic expando = new ExpandoObject();
                expando.Mark = int.MaxValue;
                expando.Visited = false;
                graphDesc.Add(activity,expando);
            }
      
            graphDesc[fromActivityName].Mark = 0;

            var getWithMinMark = new Func<Dictionary<string, dynamic>, string>((desc) =>
            {
                return desc.Where(d=>!d.Value.Visited).OrderBy(d => d.Value.Mark).First().Key;
            });

            var setVisited = new Action<string,Dictionary<string, dynamic>>((name,desc) =>
            {
                desc[name].Visited = true;
            });

            var setMark = new Action<string, int, Dictionary<string, dynamic>>((name,mark, desc) =>
            {
                if (desc[name].Mark > mark)
                    desc[name].Mark = mark;
            });

            var getMark = new Func<string, Dictionary<string, dynamic>, int>((name,desc) => desc[name].Mark);

            var isFinished = new Func<Dictionary<string, dynamic>, bool>((desc) =>
            {
                return desc.All(d => d.Value.Visited);
            });

            var getNeighbors = new Func<string, Dictionary<string, dynamic>, List<string>>((name,desc) =>
            {
                var activity = processDefinition.FindActivity(name);
                var visited = desc.Where(d=>d.Value.Visited).Select(d=>d.Key).ToList();
                var transitions =
                    processDefinition.GetPossibleTransitionsForActivity(activity,ForkTransitionSearchType.Both)
                        .Where(t => classifiers.Contains(t.Classifier) && activitiesToAnalyze.Contains(t.To.Name) && !visited.Contains(t.To.Name));
                return transitions.Select(t => t.To.Name).ToList();
            });

            while (!isFinished(graphDesc))
            {
                var current = getWithMinMark(graphDesc);

                var currentMark = getMark(current, graphDesc);

                if (currentMark == int.MaxValue)
                {
                    setVisited(current, graphDesc);
                    continue;
                }

                var neighbors = getNeighbors(current, graphDesc);

                foreach (var neighbor in neighbors)
                {
                    setMark(neighbor, currentMark + 1, graphDesc);
                }

                setVisited(current, graphDesc);
            }

            return graphDesc[toActivityName].Mark;

        }
    }
}
