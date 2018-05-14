using System;
using System.Collections.Generic;
using System.Linq;
using OptimaJet.Workflow.Core.CodeActions;
using OptimaJet.Workflow.Core.Fault;
using System.Globalization;
using System.Xml.Linq;
using Newtonsoft.Json;
using OptimaJet.Workflow.Core.Designer;

namespace OptimaJet.Workflow.Core.Model
{
    public enum ForkTransitionSearchType
    {
        NotFork,
        Fork,
        Both
    }

    /// <summary>
    /// Represents object model of a scheme of a process
    /// </summary>
    public sealed class ProcessDefinition : BaseDefinition
    {
        /// <summary>
        /// Code action invoker with compilled code actions
        /// </summary>
        [JsonIgnore]
        public CodeActionsInvoker CodeActionsInvoker {  get; internal set; }

        /// <summary>
        /// List of actors <see cref="ActorDefinition"/>
        /// </summary>
        public List<ActorDefinition> Actors { get; set; }
        /// <summary>
        /// List of parameters <see cref="ParameterDefinition"/>
        /// </summary>
        public List<ParameterDefinition> Parameters { get; set; }
        /// <summary>
        /// List of commands <see cref="CommandDefinition"/>
        /// </summary>
        public List<CommandDefinition> Commands { get; set; }
        /// <summary>
        /// List of timers <see cref="TimerDefinition"/>
        /// </summary>
        public List<TimerDefinition> Timers { get; set; }
        /// <summary>
        /// List of activities <see cref="ActivityDefinition"/>
        /// </summary>
        public List<ActivityDefinition> Activities { get; set; }
        /// <summary>
        /// List of transitions <see cref="TransitionDefinition"/>
        /// </summary>
        public List<TransitionDefinition> Transitions { get; set; }
        /// <summary>
        /// List of localization items <see cref="LocalizeDefinition"/>
        /// </summary>
        public List<LocalizeDefinition> Localization { get; set; }
        /// <summary>
        /// List of code actions <see cref="CodeActionDefinition"/>
        /// </summary>
        public List<CodeActionDefinition> CodeActions { get; set; }
 
        /// <summary>
        /// Additional parameters used by Workflow Designer
        /// </summary>
        public Dictionary<string, object> AdditionalParams { get; set; }

        public string StartingTransition { get; set; }

        public ProcessDefinition()
        {
            Actors = new List<ActorDefinition>();
            Parameters = new List<ParameterDefinition>();
            Commands = new List<CommandDefinition>();
            Timers = new List<TimerDefinition>();
            Activities = new List<ActivityDefinition>();
            Transitions = new List<TransitionDefinition>();
            Localization = new List<LocalizeDefinition>();
            AdditionalParams = new Dictionary<string, object>();
            CodeActions = new List<CodeActionDefinition>();
        }

        [JsonIgnore]
        /// <summary>
        /// Returns true if the scheme contains subprocesses
        /// </summary>
        public bool ContainsSubprocesses
        {
            get { return Transitions.Any(t => t.IsFork); }
        }

        /// <summary>
        /// Returns initial activity of the scheme
        /// </summary>
        [JsonIgnore]
        public ActivityDefinition InitialActivity
        {
            get
            {
                return Activities.FirstOrDefault(a => a.IsInitial);
            }
        }

        /// <summary>
        /// Returns all parameter definitions except system <see cref="ParameterDefinition.Purpose"/>
        /// </summary>
        public ParameterDefinition[] ParametersForSerialize
        {
            get
            {
                if (Parameters == null)
                    return null;

                return Parameters.Where(p => DefaultDefinitions.DefaultParameters.Count(p1 => p1.Name == p.Name) == 0).ToArray();
            }
        }

        /// <summary>
        /// Returns all persisted parameter definitions <see cref="ParameterDefinition.Purpose"/> 
        /// </summary>
        public IEnumerable<ParameterDefinition> PersistenceParameters
        {
            get { return Parameters.Where(p => p.Purpose == ParameterPurpose.Persistence); }
        }

        /// <summary>
        /// Returns activity with specific name
        /// </summary>
        /// <param name="name">Name of the activity <see cref="ActivityDefinition.Name"/></param>
        /// <returns>ActivityDefinition object</returns>
        public ActivityDefinition FindActivity (string name)
        {
            var activity = Activities.SingleOrDefault(a => a.Name == name);
            if (activity == null)
                throw new ActivityNotFoundException();
            return activity;
        }

        /// <summary>
        /// Returns command with specific name
        /// </summary>
        /// <param name="name">Name of the command <see cref="CommandDefinition.Name"/></param>
        /// <returns>CommandDefinition object</returns>
        public CommandDefinition FindCommand(string name)
        {
            var command = Commands.SingleOrDefault(a => a.Name == name);
            if (command == null)
                throw new CommandNotFoundException();
            return command;
        }


        /// <summary>
        /// Returns actor with specific name
        /// </summary>
        /// <param name="name">Name of the actor <see cref="ActorDefinition.Name"/></param>
        /// <returns>ActorDefinition object</returns>
        public ActorDefinition FindActor(string name)
        {
            var actor = Actors.SingleOrDefault(a => a.Name == name);
            if (actor == null)
                throw new ActorNotFoundException();
            return actor;
        }

        /// <summary>
        /// Returns the parameter specific name 
        /// </summary>
        /// <param name="name">>Name of the parameter <see cref="ParameterDefinition.Name"/></param>
        /// <returns>ParameterDefinition object</returns>
        public ParameterDefinition FindParameterDefinition(string name)
        {
            return Parameters.SingleOrDefault(p => p.Name.Equals(name, StringComparison.Ordinal));
        }

        /// <summary>
        /// Returns transition with specific name
        /// </summary>
        /// <param name="name">Name of the transition <see cref="TransitionDefinition.Name"/></param>
        /// <returns>TransitionDefinition object</returns>
        public TransitionDefinition FindTransition(string name)
        {
            var transition = Transitions.SingleOrDefault(a => a.Name == name);
            if (transition == null)
                throw new TransitionNotFoundException();
            return transition;
        }

        /// <summary>
        /// Returns transitions linked by specified activities
        /// </summary>
        /// <param name="from">From activity <see cref="TransitionDefinition.From"/></param>
        /// <param name="to">To activity <see cref="TransitionDefinition.To"/></param>
        /// <returns>List of TransitionDefinition objects</returns>
        public List<TransitionDefinition> FindTransitions(ActivityDefinition from, ActivityDefinition to)
        {
            return
                Transitions.Where(
                    t =>
                        t.From.Name.Equals(from.Name, StringComparison.Ordinal) &&
                        t.To.Name.Equals(to.Name, StringComparison.Ordinal)).ToList();
        }

        /// <summary>
        /// Returns the list of all outgoing transitions for specific activity
        /// </summary>
        /// <param name="activity">Activity definition</param>
        /// <param name="forkTransitionSearch">Search filter for fork transitions</param>
        /// <returns>List of all outgoing transitions</returns>
        public IEnumerable<TransitionDefinition> GetPossibleTransitionsForActivity(ActivityDefinition activity,
            ForkTransitionSearchType forkTransitionSearch = ForkTransitionSearchType.NotFork)
        {
            return Transitions.Where(t => t.From == activity && ForkFilter(forkTransitionSearch, t));
        }

        /// <summary>
        /// Returns the list of all outgoing command transitions for specific activity <see cref="TriggerType.Command"/>
        /// </summary>
        /// <param name="activity">Activity definition</param>
        ///   /// <param name="forkTransitionSearch">Search filter for fork transitions</param>
        /// <returns>List of all outgoing command transitions</returns>
        public IEnumerable<TransitionDefinition> GetCommandTransitions(ActivityDefinition activity,
            ForkTransitionSearchType forkTransitionSearch = ForkTransitionSearchType.NotFork)
        {
            return Transitions.Where(t =>t.From == activity && t.Trigger.Type == TriggerType.Command && ForkFilter(forkTransitionSearch, t));
        }

        /// <summary>
        /// Returns the list of all outgoing auto transitions for specific activity <see cref="TriggerType.Auto"/>
        /// </summary>
        /// <param name="activity">Activity definition</param>
        /// <param name="forkTransitionSearch">Search filter for fork transitions</param>
        /// <returns>List of all outgoing auto transitions</returns>
        public IEnumerable<TransitionDefinition> GetAutoTransitionForActivity(ActivityDefinition activity,
            ForkTransitionSearchType forkTransitionSearch = ForkTransitionSearchType.NotFork)
        {
            return Transitions.Where(t => t.From == activity && t.Trigger.Type == TriggerType.Auto && ForkFilter(forkTransitionSearch, t));
        }

        /// <summary>
        /// Returns the list of all outgoing command transitions for specific activity <see cref="TriggerType.Command"/> whis specific command name <see cref="CommandDefinition.Name"/>
        /// </summary>
        /// <param name="activity">Activity definition</param>
        /// <param name="commandName">Name of the command</param>
        /// <param name="forkTransitionSearch">Search filter for fork transitions</param>
        /// <returns>List of all outgoing command transitions whis specific command name</returns>
        public IEnumerable<TransitionDefinition> GetCommandTransitionForActivity(ActivityDefinition activity, string commandName,
            ForkTransitionSearchType forkTransitionSearch = ForkTransitionSearchType.NotFork)
        {
            return Transitions.Where(t => t.From == activity && t.Trigger.Type == TriggerType.Command && t.Trigger.Command.Name == commandName && ForkFilter(forkTransitionSearch,t));
        }

        /// <summary>
        /// Returns the list of all outgoing timer transitions for specific activity <see cref="TriggerType.Timer"/>
        /// </summary>
        /// <param name="activity">Activity definition</param>
        /// <param name="forkTransitionSearch">Search filter for fork transitions</param>
        /// <returns>List of all outgoing timer transitions</returns>
        public IEnumerable<TransitionDefinition> GetTimerTransitionForActivity(ActivityDefinition activity,
             ForkTransitionSearchType forkTransitionSearch = ForkTransitionSearchType.NotFork)
        {
            return Transitions.Where(t => t.From == activity && t.Trigger.Type == TriggerType.Timer && ForkFilter(forkTransitionSearch, t));
        }

        
        private static bool ForkFilter(ForkTransitionSearchType forkTransitionSearch, TransitionDefinition t)
        {
            return forkTransitionSearch == ForkTransitionSearchType.Both ||
                   (t.IsFork && forkTransitionSearch == ForkTransitionSearchType.Fork) ||
                   (!t.IsFork && forkTransitionSearch == ForkTransitionSearchType.NotFork);
        }

        /// <summary>
        /// Create ProcessDefinition object
        /// </summary>
        /// <param name="name">Name of the scheme</param>
        /// <param name="actors">List of actors <see cref="ActorDefinition"/></param>
        /// <param name="parameters">List of parameters <see cref="ParameterDefinition"/></param>
        /// <param name="commands">List of commands <see cref="CommandDefinition"/></param>
        /// <param name="timers">List of timers <see cref="TimerDefinition"/></param>
        /// <param name="activities">List of activities <see cref="ActivityDefinition"/></param>
        /// <param name="transitions">List of transitions <see cref="TransitionDefinition"/></param>
        /// <param name="localization">List of localization items <see cref="LocalizeDefinition"/></param>
        /// <param name="codeActions"> List of code actions <see cref="CodeActionDefinition"/></param>
        /// <param name="designerSettigs">Designer settings</param>
        /// <returns>ProcessDefinition object</returns>
        public static ProcessDefinition Create(string name, List<ActorDefinition> actors,
            List<ParameterDefinition> parameters, List<CommandDefinition> commands, List<TimerDefinition> timers,
            List<ActivityDefinition> activities, List<TransitionDefinition> transitions,
            List<LocalizeDefinition> localization,List<CodeActionDefinition> codeActions, DesignerSettings designerSettigs)
        {
            return new ProcessDefinition 
            {
                Activities = activities,
                Actors = actors,
                Commands = commands,
                Timers = timers,
                Name = name,
                Parameters = parameters,
                Transitions = transitions,
                Localization = localization,
                CodeActions = codeActions,
                DesignerSettings = designerSettigs
            };
        }


       
    
        #region Localization

        /// <summary>
        /// Returns localized state name in specific culture
        /// </summary>
        /// <param name="stateName">System state name</param>
        /// <param name="culture">Culture</param>
        /// <returns>Localized state name</returns>
        public string GetLocalizedStateName(string stateName, CultureInfo culture)
        {
            return GetLocalizedName(stateName, culture, LocalizeType.State);
        }

        /// <summary>
        /// Returns localized command name in specific culture
        /// </summary>
        /// <param name="commandName">System command name</param>
        /// <param name="culture">Culture</param>
        /// <returns>Localized command name</returns>
        public string GetLocalizedCommandName(string commandName, CultureInfo culture)
        {
            return GetLocalizedName(commandName, culture, LocalizeType.Command);
        }

        /// <summary>
        /// Returns localized parameter name in specific culture
        /// </summary>
        /// <param name="parameterName">System parameter name</param>
        /// <param name="culture">Culture</param>
        /// <returns>Localized parameter name</returns>
        public string GetLocalizedParameterName(string parameterName, CultureInfo culture)
        {
            return GetLocalizedName(parameterName, culture, LocalizeType.Parameter);
        }

        private string GetLocalizedName(string name, CultureInfo culture, LocalizeType localizeType)
        {
            if (Localization == null)
                return name;                     

            var localize =
                Localization.FirstOrDefault(
                    l =>
                    l.Type == localizeType && String.Compare(l.Culture, culture.Name, StringComparison.OrdinalIgnoreCase) == 0 &&
                    l.ObjectName == name);

            if (localize != null)
                return localize.Value;

            localize =
                Localization.FirstOrDefault(
                    l =>
                    l.Type == localizeType && l.IsDefault &&
                    l.ObjectName == name);

            if (localize != null)
                return localize.Value;

            return name;
        }
        #endregion


        public new ProcessDefinition Clone()
        {
            return Clone(false, false, false);
        }

        /// <summary>
        ///  Clones ProcessDefinition object
        /// </summary>
        /// <param name="doNotCloneActivities">If True activity list will be empty after cloning</param>
        /// <param name="doNotCloneTransitions">If True transition list will be empty after cloning</param>
        /// <param name="doNotCloneCodeActions">If True code actions list will be empty after cloning</param>
        /// <returns>Cloned ProcessDefinition object</returns>
        public ProcessDefinition Clone(bool doNotCloneActivities, bool doNotCloneTransitions, bool doNotCloneCodeActions)
        {
            var newObject = MemberwiseClone() as ProcessDefinition;

            if (newObject == null)
                throw new Exception("Error cloning ProcessDefinition object");

            newObject.Actors = new List<ActorDefinition>();
            Actors.ForEach(a => newObject.Actors.Add(a.Clone()));

            newObject.Parameters = new List<ParameterDefinition>();
            Parameters.ForEach(a => newObject.Parameters.Add(a.Clone()));

            newObject.Timers = new List<TimerDefinition>();
            Timers.ForEach(a => newObject.Timers.Add(a.Clone()));

            newObject.Activities = new List<ActivityDefinition>();
            if (!doNotCloneActivities)
                Activities.ForEach(a => newObject.Activities.Add(a.Clone()));

            newObject.Localization = new List<LocalizeDefinition>();
            Localization.ForEach(a => newObject.Localization.Add(a.Clone()));

            newObject.CodeActions = new List<CodeActionDefinition>();
            if (!doNotCloneCodeActions)
                CodeActions.ForEach(a => newObject.CodeActions.Add(a.Clone()));


            newObject.Commands = new List<CommandDefinition>();
            Commands.ForEach(a => newObject.Commands.Add(a.Clone(newObject.Parameters)));

            newObject.Transitions = new List<TransitionDefinition>();
            if (!doNotCloneTransitions)
                Transitions.ForEach(a => newObject.Transitions.Add(a.Clone(newObject.Actors, newObject.Commands, newObject.Activities, newObject.Timers)));

            return newObject;
        }

        public string DefiningParametersString { get; set; }
        
        public string RootSchemeCode { get; set; }

        public Guid? RootSchemeId { get; set; }

        public bool IsObsolete { get; set; }

        public Guid Id { get; set; }

        public bool IsSubprocessScheme
        {
            get { return RootSchemeId.HasValue && RootSchemeId.Value != Id; }
        }

        public List<string> AllowedActivities { get; set; }

        public string Serialize(){
            return SerializeToXElement().ToString(); 
        }
        public XElement SerializeToXElement()
        {
            var doc = new XElement("Process");
            doc.SetAttributeValue("Name", Name);

            var itemProcessDesigner = new XElement("Designer");
            itemProcessDesigner.SetAttributeValue("X", DesignerSettings.X);
            itemProcessDesigner.SetAttributeValue("Y", DesignerSettings.Y);
            itemProcessDesigner.SetAttributeValue("Scale", DesignerSettings.Scale);
            doc.Add(itemProcessDesigner);

            #region Actors

            if (Actors.Count > 0)
            {
                var actors = new XElement("Actors");
                foreach (var item in Actors)
                {
                    var actor = new XElement("Actor");
                    actor.SetAttributeValue("Name", item.Name);
                    actor.SetAttributeValue("Rule", item.Rule);
                    actor.SetAttributeValue("Value", item.Value);
                    actors.Add(actor);
                }

                doc.Add(actors);
            }
            #endregion

            #region Parameters

            var parametersForSerialized = ParametersForSerialize;
            if (parametersForSerialized.Length > 0)
            {
                var parametrs = new XElement("Parameters");
                foreach (var item in parametersForSerialized)
                {
                    var itemXe = new XElement("Parameter");
                    itemXe.SetAttributeValue("Name", item.Name);
                    itemXe.SetAttributeValue("Type", ParsedType.GetFriendlyName(item.Type));
                    itemXe.SetAttributeValue("Purpose", item.Purpose);
                    if (!string.IsNullOrEmpty(item.InitialValue))
                        itemXe.SetAttributeValue("InitialValue", item.InitialValue);
                    parametrs.Add(itemXe);
                }

                doc.Add(parametrs);
            }

            #endregion

            #region Commands

            if (Commands.Count > 0)
            {
                XElement commands = new XElement("Commands");
                foreach (var item in Commands)
                {
                    var itemXe = new XElement("Command");
                    itemXe.SetAttributeValue("Name", item.Name);

                    if (item.InputParameters.Count > 0)
                    {
                        var itemXeSub = new XElement("InputParameters");
                        foreach (var itemSub in item.InputParameters)
                        {
                            var itemXeSubParam = new XElement("ParameterRef");
                            itemXeSubParam.SetAttributeValue("Name", itemSub.Name);
                            itemXeSubParam.SetAttributeValue("IsRequired", itemSub.IsRequired);
                            itemXeSubParam.SetAttributeValue("DefaultValue", itemSub.DefaultValue);
                            itemXeSubParam.SetAttributeValue("NameRef", itemSub.Parameter.Name);
                            itemXeSub.Add(itemXeSubParam);
                        }
                        itemXe.Add(itemXeSub);
                    }
                    commands.Add(itemXe);
                }
                doc.Add(commands);
            }
            #endregion

            #region Timers

            if (Timers.Count > 0)
            {
                var timers = new XElement("Timers");
                foreach (var item in Timers)
                {
                    var itemXe = new XElement("Timer");
                    itemXe.SetAttributeValue("Name", item.Name);
                    itemXe.SetAttributeValue("Type", item.Type);
                    itemXe.SetAttributeValue("Value", item.Value);
                    itemXe.SetAttributeValue("NotOverrideIfExists", item.NotOverrideIfExists);

                    timers.Add(itemXe);
                }
                doc.Add(timers);
            }
            #endregion


            #region Activities
            if (Activities.Count > 0)
            {
                XElement activities = new XElement("Activities");
                foreach (var item in Activities)
                {
                    XElement itemXe = new XElement("Activity");
                    itemXe.SetAttributeValue("Name", item.Name);
                    if (!string.IsNullOrEmpty(item.State))
                        itemXe.SetAttributeValue("State", item.State);
                    itemXe.SetAttributeValue("IsInitial", item.IsInitial.ToString());
                    itemXe.SetAttributeValue("IsFinal", item.IsFinal.ToString());
                    itemXe.SetAttributeValue("IsForSetState", item.IsForSetState.ToString());
                    itemXe.SetAttributeValue("IsAutoSchemeUpdate", item.IsAutoSchemeUpdate.ToString());

                    if (item.Implementation != null && item.Implementation.Count() > 0)
                    {
                        XElement itemXeSub = new XElement("Implementation");
                        foreach (var itemSub in item.Implementation)
                        {
                            XElement itemXeSubParam = new XElement("ActionRef");
                            itemXeSubParam.SetAttributeValue("Order", itemSub.Order);
                            itemXeSubParam.SetAttributeValue("NameRef", itemSub.ActionName);

                            if (!string.IsNullOrEmpty(itemSub.ActionParameter))
                            {
                                itemXeSubParam.Add(new XElement("ActionParameter"));
                                var xElement = itemXeSubParam.Element("ActionParameter");
                                if (xElement != null)
                                    xElement.ReplaceNodes(new XCData(itemSub.ActionParameter));
                            }

                            itemXeSub.Add(itemXeSubParam);
                        }

                        itemXe.Add(itemXeSub);
                    }

                    if (item.PreExecutionImplementation != null && item.PreExecutionImplementation.Count() > 0)
                    {
                        XElement itemXeSub = new XElement("PreExecutionImplementation");
                        foreach (var itemSub in item.PreExecutionImplementation)
                        {
                            XElement itemXeSubParam = new XElement("ActionRef");
                            itemXeSubParam.SetAttributeValue("Order", itemSub.Order);
                            itemXeSubParam.SetAttributeValue("NameRef", itemSub.ActionName);

                            if (!string.IsNullOrEmpty(itemSub.ActionParameter))
                            {
                                itemXeSubParam.Add(new XElement("ActionParameter"));
                                var xElement = itemXeSubParam.Element("ActionParameter");
                                if (xElement != null)
                                    xElement.ReplaceNodes(new XCData(itemSub.ActionParameter));
                            }

                            itemXeSub.Add(itemXeSubParam);
                        }

                        itemXe.Add(itemXeSub);
                    }

                    activities.Add(itemXe);

                    XElement itemDesigner = new XElement("Designer");
                    itemDesigner.SetAttributeValue("X", item.DesignerSettings.X);
                    itemDesigner.SetAttributeValue("Y", item.DesignerSettings.Y);
                    itemXe.Add(itemDesigner);

                }
                doc.Add(activities);
            }
            #endregion

            #region Transition
            if (Transitions.Count > 0)
            {
                XElement transitions = new XElement("Transitions");
                foreach (var item in Transitions)
                {
                    XElement itemXe = new XElement("Transition");
                    itemXe.SetAttributeValue("Name", item.Name);
                    itemXe.SetAttributeValue("To", item.To.Name);
                    itemXe.SetAttributeValue("From", item.From.Name);
                    itemXe.SetAttributeValue("Classifier", item.Classifier);
                    itemXe.SetAttributeValue("AllowConcatenationType", item.AllowConcatenationType);
                    itemXe.SetAttributeValue("RestrictConcatenationType", item.RestrictConcatenationType);
                    itemXe.SetAttributeValue("ConditionsConcatenationType", item.ConditionsConcatenationType);
                    itemXe.SetAttributeValue("IsFork", item.IsFork);
                    itemXe.SetAttributeValue("MergeViaSetState", item.MergeViaSetState);
                    itemXe.SetAttributeValue("DisableParentStateControl", item.DisableParentStateControl);
                    if (item.Restrictions != null && item.Restrictions.Count > 0)
                    {
                        var itemXeSub = new XElement("Restrictions");
                        foreach (var itemSub in item.Restrictions)
                        {
                            var itemXeSubParam = new XElement("Restriction");
                            itemXeSubParam.SetAttributeValue("Type", itemSub.Type);
                            if (itemSub.Actor != null)
                                itemXeSubParam.SetAttributeValue("NameRef", itemSub.Actor.Name);
                            itemXeSub.Add(itemXeSubParam);
                        }
                        itemXe.Add(itemXeSub);
                    }


                    //if (item.Trigger.Count() > 0)
                    {
                        var itemXeSub = new XElement("Triggers");
                        //foreach (var itemSub in item.Restrictions)
                        {
                            var itemSub = item.Trigger;
                            var itemXeSubParam = new XElement("Trigger");
                            itemXeSubParam.SetAttributeValue("Type", itemSub.Type);

                            string nameRef = itemSub.NameRef;
                            if (!string.IsNullOrWhiteSpace(nameRef))
                                itemXeSubParam.SetAttributeValue("NameRef", nameRef);
                            itemXeSub.Add(itemXeSubParam);
                        }
                        itemXe.Add(itemXeSub);
                    }

                    if (item.Conditions.Any())
                    {
                        var itemXeSub = new XElement("Conditions");
                        foreach (var itemSub in item.Conditions)
                        {

                            var itemXeSubParam = new XElement("Condition");
                            itemXeSubParam.SetAttributeValue("Type", itemSub.Type);
                            if (itemSub.Action != null)
                            {
                                itemXeSubParam.SetAttributeValue("NameRef", itemSub.Action.ActionName);
                                itemXeSubParam.SetAttributeValue("ConditionInversion", itemSub.ConditionInversion);
                            }
                            if (itemSub.ResultOnPreExecution.HasValue)
                                itemXeSubParam.SetAttributeValue("ResultOnPreExecution", itemSub.ResultOnPreExecution);

                            if (itemSub.Action != null && !string.IsNullOrEmpty(itemSub.Action.ActionParameter))
                            {
                                itemXeSubParam.Add(new XElement("ActionParameter"));
                                var xElement = itemXeSubParam.Element("ActionParameter");
                                if (xElement != null)
                                    xElement.ReplaceNodes(new XCData(itemSub.Action.ActionParameter));
                            }

                            itemXeSub.Add(itemXeSubParam);
                        }
                        itemXe.Add(itemXeSub);
                    }

                    transitions.Add(itemXe);

                    XElement itemDesigner = new XElement("Designer");
                    //1.5.7
                    //itemDesigner.SetAttributeValue("Bending", item.DesignerSettings.Bending);
                    itemDesigner.SetAttributeValue("X", item.DesignerSettings.X);
                    itemDesigner.SetAttributeValue("Y", item.DesignerSettings.Y);
                    itemXe.Add(itemDesigner);
                }
                doc.Add(transitions);
            }
            #endregion

            #region CodeActions
            if (CodeActions.Any())
            {
                XElement codeActions = new XElement("CodeActions");
                foreach (var item in CodeActions)
                {
                    if (string.IsNullOrEmpty(item.ActionCode))
                        continue;
                    XElement itemXe = new XElement("CodeAction");
                    itemXe.SetAttributeValue("Name", item.Name);
                    itemXe.SetAttributeValue("Type", item.Type.ToString());
                    itemXe.SetAttributeValue("IsGlobal", item.IsGlobal.ToString());
                    itemXe.SetAttributeValue("IsAsync", item.IsAsync.ToString());
                    itemXe.Add(new XElement("ActionCode"));
                    var xElement = itemXe.Element("ActionCode");
                    if (xElement != null)
                        xElement.ReplaceNodes(new XCData(item.ActionCode));
                    itemXe.Add(new XElement("Usings"));
                    var xElementUsings = itemXe.Element("Usings");
                    if (xElementUsings != null)
                        xElementUsings.ReplaceNodes(new XCData(item.Usings));
                    codeActions.Add(itemXe);
                }

                doc.Add(codeActions);
            }
            #endregion

            #region Localization
            if (Localization.Count > 0)
            {
                XElement localization = new XElement("Localization");
                foreach (var item in Localization)
                {
                    XElement itemXe = new XElement("Localize");
                    itemXe.SetAttributeValue("Type", item.Type);
                    itemXe.SetAttributeValue("IsDefault", item.IsDefault.ToString());
                    itemXe.SetAttributeValue("Culture", item.Culture);
                    itemXe.SetAttributeValue("ObjectName", item.ObjectName);
                    itemXe.SetAttributeValue("Value", item.Value);
                    localization.Add(itemXe);
                }

                doc.Add(localization);
            }
            #endregion

            return doc;
        }
    }
}
