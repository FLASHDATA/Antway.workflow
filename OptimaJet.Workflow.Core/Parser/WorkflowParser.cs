using System.Collections.Generic;
using System.Linq;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Parser
{
    /// <summary>
    /// Base workflow parser, which parses not parsed process scheme <see cref="SchemeDefinition{T}"/> to the object model of a scheme of a process <see cref="ProcessDefinition"/>
    /// </summary>
    /// <typeparam name="TSchemeMedium">Type of not parsed scheme</typeparam>
    public abstract class WorkflowParser<TSchemeMedium> : IWorkflowParser<TSchemeMedium> where TSchemeMedium : class
    {
        /// <summary>
        /// Parses timers from not parsed scheme in theirs object model
        /// </summary>
        /// <param name="schemeMedium">Not parsed scheme</param>
        /// <returns>List of <see cref="TimerDefinition"/> objects</returns>
        public abstract List<TimerDefinition> ParseTimers(TSchemeMedium schemeMedium);

        /// <summary>
        /// Parses actors from not parsed scheme in theirs object model
        /// </summary>
        /// <param name="schemeMedium">Not parsed scheme</param>
        /// <returns>List of <see cref="ActorDefinition"/> objects</returns>
        public abstract List<ActorDefinition> ParseActors(TSchemeMedium schemeMedium);

        /// <summary>
        /// Parses localization items from not parsed scheme in theirs object model
        /// </summary>
        /// <param name="schemeMedium">Not parsed scheme</param>
        /// <returns>List of <see cref="LocalizeDefinition"/> objects</returns>
        public abstract List<LocalizeDefinition> ParseLocalization(TSchemeMedium schemeMedium);

        /// <summary>
        /// Parses parameters from not parsed scheme in theirs object model
        /// </summary>
        /// <param name="schemeMedium">Not parsed scheme</param>
        /// <returns>List of <see cref="ParameterDefinition"/> objects</returns>
        public abstract List<ParameterDefinition> ParseParameters(TSchemeMedium schemeMedium);

        /// <summary>
        /// Parses commands from not parsed scheme in theirs object model
        /// </summary>
        /// <param name="schemeMedium">Not parsed scheme</param>
        /// <param name="parameterDefinitions">List of parsed parameters <see cref="ParameterDefinition"/></param>
        /// <returns>List of <see cref="CommandDefinition"/> objects</returns>
        public abstract List<CommandDefinition> ParseCommands(TSchemeMedium schemeMedium,
            List<ParameterDefinition> parameterDefinitions);

        /// <summary>
        /// Parses activities from not parsed scheme in theirs object model
        /// </summary>
        /// <param name="schemeMedium">Not parsed scheme</param>
        /// <returns>List of <see cref="ActivityDefinition"/> objects</returns>
        public abstract List<ActivityDefinition> ParseActivities(TSchemeMedium schemeMedium);

        /// <summary>
        /// Parses code actions from not parsed scheme in theirs object model
        /// </summary>
        /// <param name="schemeMedium">Not parsed scheme</param>
        /// <returns>List of <see cref="CodeActionDefinition"/> objects</returns>
        public abstract List<CodeActionDefinition> ParseCodeActions(TSchemeMedium schemeMedium);

        /// <summary>
        /// Parses transitions from not parsed scheme in theirs object model
        /// </summary>
        /// <param name="schemeMedium">Not parsed scheme</param>
        /// <param name="actorDefinitions">List of parsed actors <see cref="ActorDefinition"/></param>
        /// <param name="commandDefinitions">List of parsed commands <see cref="CommandDefinition"/></param>
        /// <param name="activityDefinitions">List of parsed activities <see cref="ActivityDefinition"/></param>
        /// <param name="timerDefinitions">List of parsed timers <see cref="TimerDefinition"/></param>
        /// <returns>List of <see cref="TransitionDefinition"/> objects</returns>
        public abstract List<TransitionDefinition> ParseTransitions(TSchemeMedium schemeMedium,
            List<ActorDefinition> actorDefinitions,
            List<CommandDefinition> commandDefinitions,
            List<ActivityDefinition> activityDefinitions,
            List<TimerDefinition> timerDefinitions);

        /// <summary>
        /// Gets the code of the scheme from not parsed scheme 
        /// </summary>
        /// <param name="schemeMedium">Not parsed scheme</param>
        /// <returns>Code of the scheme</returns>
        public abstract string GetSchemeCode(TSchemeMedium schemeMedium);

        /// <summary>
        /// Gets designer settings from not parsed scheme 
        /// </summary>
        /// <param name="schemeMedium">Not parsed scheme</param>
        /// <returns>Designer settings</returns>
        public abstract DesignerSettings GetDesignerSettings(TSchemeMedium schemeMedium);

        /// <summary>
        /// Returns object model of the scheme of a process
        /// </summary>
        /// <param name="scheme">String representation of not parsed sheme</param>
        /// <returns>ProcessDefinition object</returns>
        public abstract ProcessDefinition Parse(string scheme);

        /// <summary>
        /// Serializes object model of the scheme to not parsed scheme
        /// </summary>
        /// <param name="processDefinition">ProcessDefinition object</param>
        /// <returns>Not parsed scheme</returns>
        public abstract TSchemeMedium SerializeToSchemeMedium(ProcessDefinition processDefinition);

        /// <summary>
        /// Serializes object model of the scheme to string
        /// </summary>
        /// <param name="processDefinition">ProcessDefinition object</param>
        /// <returns>String representation of not parsed scheme</returns>
        public virtual string SerializeToString(ProcessDefinition processDefinition)
        {
            return SerializeToSchemeMedium(processDefinition).ToString();
        }

        /// <summary>
        /// Returns object model of the scheme of a process
        /// </summary>
        /// <param name="schemeMedium">Not parsed scheme</param>
        /// <returns>ProcessDefinition object</returns>
        public ProcessDefinition Parse(TSchemeMedium schemeMedium)
        {
            var localization = ParseLocalization(schemeMedium).ToList();
            var actors = ParseActors(schemeMedium).ToList();
            var timers = ParseTimers(schemeMedium).ToList();
            var parameters = ParseParameters(schemeMedium).ToList();
            var commands = ParseCommands(schemeMedium, parameters).ToList();
            var activities = ParseActivities(schemeMedium).ToList();
            var transitions = ParseTransitions(schemeMedium, actors, commands, activities, timers).ToList();
            var designerSettings = GetDesignerSettings(schemeMedium);
            var codeActions = ParseCodeActions(schemeMedium);


            return ProcessDefinition.Create(GetSchemeCode(schemeMedium),
                actors,
                parameters,
                commands,
                timers,
                activities,
                transitions,
                localization,
                codeActions,
                designerSettings);
        }
    }
}
