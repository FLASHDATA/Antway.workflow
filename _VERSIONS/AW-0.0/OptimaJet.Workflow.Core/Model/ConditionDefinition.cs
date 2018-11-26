using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OptimaJet.Workflow.Core.Runtime;

namespace OptimaJet.Workflow.Core.Model
{

    /// <summary>
    /// Represent a transition's trigger in a process scheme
    /// </summary>
    public class TriggerDefinition
    {
        /// <summary>
        /// Type of the trigger <see cref="TriggerType"/>
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public  TriggerType Type { get; set; }

        /// <summary>
        /// Create TriggerDefinition object
        /// </summary>
        /// <param name="type">Type of the trigger <see cref="TriggerType"/></param>
        public TriggerDefinition(TriggerType type)
        {
            Type = type;
        }

        /// <summary>
        /// Returns the name of the object in scheme which start the trigger
        /// </summary>
        public string NameRef
        {
            get
            {
                string res;
                switch (Type)
                {                    
                    case TriggerType.Command:
                        res = (Command == null ? string.Empty : Command.Name);
                        break;
                    case TriggerType.Timer:
                        res = (Timer == null ? string.Empty : Timer.Name);
                        break;
                    default:
                        res = string.Empty;
                        break;
                }
                return res;
            }
        }

        /// <summary>
        /// Command which start the trigger if the type of the trigger is command <see cref="TriggerType.Command"/>
        /// </summary>
        public CommandDefinition Command { get; set; }

        /// <summary>
        /// Timer which start the trigger if the type of the trigger is timer <see cref="TriggerType.Timer"/>
        /// </summary>
        public TimerDefinition Timer { get; set; }

        /// <summary>
        /// Create the instance of TriggerDefinition object with auto trigger type <see cref="TriggerType.Auto"/>
        /// </summary>
        public static TriggerDefinition Auto
        {
            get { return new TriggerDefinition(TriggerType.Auto);}
        }

        /// <summary>
        /// Create TriggerDefinition object
        /// </summary>
        /// <param name="type">Type of the trigger <see cref="TriggerType"/></param>
        /// <returns>TriggerDefinition object</returns>
        public static TriggerDefinition Create(string type)
        {
            return new TriggerDefinition((TriggerType) Enum.Parse(typeof(TriggerType),type));
        }

        public TriggerDefinition Clone()
        {
            var newObject = MemberwiseClone() as TriggerDefinition;

            if (newObject == null)
                throw new Exception("Error cloning TriggerDefinition object");
            
            if (Command != null)
                newObject.Command = Command.Clone();
         
            if (Timer != null)
                newObject.Timer = Timer.Clone();

            return newObject;
        }

        /// <summary>
        /// Clones TriggerDefinition object replacing references to the references to the specific objects
        /// </summary>
        /// <param name="commandDefinitions">List of the commands to replace</param>
        /// <param name="timerDefinitions">List of the timers to replace</param>
        /// <returns>Cloned TriggerDefinition object</returns>
        public TriggerDefinition Clone(List<CommandDefinition> commandDefinitions,
            List<TimerDefinition> timerDefinitions)
        {
            var newObject = MemberwiseClone() as TriggerDefinition;

            if (newObject == null)
                throw new Exception("Error cloning TriggerDefinition object");

            if (Command != null)
                newObject.Command =
                    commandDefinitions.First(
                        c => c.Name.Equals(Command.Name, StringComparison.Ordinal));

            if (Timer != null)
                newObject.Timer =
                    timerDefinitions.First(
                        c => c.Name.Equals(Timer.Name, StringComparison.Ordinal));

            return newObject;
        }
    }

    /// <summary>
    /// Type of impact that lead to the triggering of a transition
    /// </summary>
    public enum TriggerType
    {
        /// <summary>
        /// Transition starts by a command
        /// </summary>
        Command,
        /// <summary>
        /// Transition starts automatically, without any impact
        /// </summary>
        Auto,
        /// <summary>
        /// Transition starts by a timer
        /// </summary>
        Timer,

        TimerExpired,
    }


    /// <summary>
    /// Represent a transition's condition in a process scheme
    /// </summary>
    public sealed class ConditionDefinition
    {
        private ConditionDefinition()
        {
        }

        /// <summary>
        /// Type of the condition <see cref="ConditionType"/>
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ConditionType Type { get; set; }
        /// <summary>
        /// Referensce on the action if condition type is action <see cref="ConditionType.Action"/> 
        /// </summary>
        public ActionDefinitionReference Action { get; set; }
        /// <summary>
        /// In pre-execution mode if not null the result of the condition will be overriden by the value
        /// </summary>
        public bool? ResultOnPreExecution { get; set; }
        /// <summary>
        /// If true invert condition result if condition type is action <see cref="ConditionType.Action"/> 
        /// </summary>
        public bool ConditionInversion { get; set; }

       /// <summary>
       /// Create ConditionDefinition object
       /// </summary>
       /// <param name="type">Type of the condition <see cref="ConditionType"/></param>
       /// <param name="resultOnPreExecution">In pre-execution mode if not null the result of the condition will be overriden by the value</param>
        /// <returns>ConditionDefinition object</returns>
        public static ConditionDefinition Create(string type, string resultOnPreExecution)
        {
            return Create(type, null, null, resultOnPreExecution);
        }

        /// <summary>
        /// Create ConditionDefinition object
        /// </summary>
        /// <param name="type">Type of the condition <see cref="ConditionType"/></param>
        /// <param name="action">Referensce on the action if condition type is action <see cref="ConditionType.Action"/> </param>
        /// <param name="conditionInversion">If true invert condition result if condition type is action <see cref="ConditionType.Action"/> </param>
        /// <param name="resultOnPreExecution">In pre-execution mode if not null the result of the condition will be overriden by the value</param>
        /// <returns>ConditionDefinition object</returns>
        public static ConditionDefinition Create(string type, ActionDefinitionReference action, string conditionInversion,
            string resultOnPreExecution)
        {
            ConditionType parsedType;
            Enum.TryParse(type, true, out parsedType);

            return new ConditionDefinition()
            {
                Action = action,
                Type = parsedType,
                ConditionInversion = !string.IsNullOrEmpty(conditionInversion) && bool.Parse(conditionInversion),
                ResultOnPreExecution =
                    string.IsNullOrEmpty(resultOnPreExecution) ? (bool?) null : bool.Parse(resultOnPreExecution)
            };
        }


        /// <summary>
        /// Create the instance of ConditionDefinition object with always type <see cref="ConditionType.Always"/>
        /// </summary>
        public static ConditionDefinition Always
        {
           get { return new ConditionDefinition() {Type = ConditionType.Always}; }
        }

        public ConditionDefinition Clone()
        {
            var newObject = MemberwiseClone() as ConditionDefinition;

            if (newObject == null)
                throw new Exception("Error cloning ConditionDefinition object");

            if (Action != null)
                newObject.Action = Action.Clone();

            return newObject;
        }
    }

    /// <summary>
    /// Type of condition for triggering of transition
    /// </summary>
    public enum ConditionType
    {
        /// <summary>
        /// Transition triggering if executed action returns true <see cref="IWorkflowActionProvider.ExecuteCondition"/>
        /// </summary>
        Action,
        /// <summary>
        /// Transition starts always in priority order
        /// </summary>
        Always,
        /// <summary>
        /// Transition starts always in lowest order
        /// </summary>
        Otherwise

    }

    
}
