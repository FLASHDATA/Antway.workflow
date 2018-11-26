using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OptimaJet.Workflow.Core.Model
{
    /// <summary>
    /// Represent a transition's restiction in a process scheme
    /// </summary>
    public class RestrictionDefinition
    {
        /// <summary>
        /// Type of the restriction <see cref="RestrictionType"/>
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public RestrictionType Type { get; set; }

        /// <summary>
        /// Actor which allowed or restricted to trigger a transition
        /// </summary>
        public ActorDefinition Actor { get; set; }

        /// <summary>
        /// Create RestrictionDefinition object
        /// </summary>
        /// <param name="type">Type of the condition <see cref="ConditionType"/></param>
        /// <param name="actor"> Actor which allowed or restricted to trigger a transition</param>
        /// <returns>RestrictionDefinition object</returns>
        public static RestrictionDefinition Create (string type, ActorDefinition actor)
        {
            RestrictionType parsedType;
            Enum.TryParse(type, true, out parsedType);

            return new RestrictionDefinition() { Actor = actor, Type = parsedType };
        }

        /// <summary>
        /// Create RestrictionDefinition object
        /// </summary>
        /// <param name="type">Type of the condition <see cref="ConditionType"/></param>
        /// <param name="actor"> Actor which allowed or restricted to trigger a transition</param>
        /// <returns>RestrictionDefinition object</returns>
        public static RestrictionDefinition Create(RestrictionType type, ActorDefinition actor)
        {
            return new RestrictionDefinition() { Actor = actor, Type = type };
        }

        public RestrictionDefinition Clone()
        {
            var newObject = MemberwiseClone() as RestrictionDefinition;

            if (newObject == null)
                throw new Exception("Error cloning RestrictionDefinition object");

            if (Actor != null)
                newObject.Actor = Actor.Clone();

            return newObject;
        }

        /// <summary>
        /// Clones RestrictionDefinition object replacing references to the references to the specific objects
        /// </summary>
        /// <param name="actorDefinitions">List of the actors to replace</param>
        /// <returns>Cloned RestrictionDefinition object</returns>
        public RestrictionDefinition Clone(List<ActorDefinition> actorDefinitions)
        {
            var newObject = MemberwiseClone() as RestrictionDefinition;

            if (newObject == null)
                throw new Exception("Error cloning RestrictionDefinition object");

            if (Actor != null)
                newObject.Actor =
                    actorDefinitions.First(a => a.Name.Equals(Actor.Name, StringComparison.Ordinal));

            return newObject;
        }
    }

    /// <summary>
    /// Type of transition's restriction
    /// </summary>
    public enum RestrictionType
    {
        /// <summary>
        /// Transition is allowed to specific actor
        /// </summary>
        Allow,
        /// <summary>
        /// Transition is restricted to specific actor
        /// </summary>
        Restrict
    }
}
