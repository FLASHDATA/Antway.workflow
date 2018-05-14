using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OptimaJet.Workflow.Core.Model
{
    /// <summary>
    /// Represent a timer in a process scheme
    /// </summary>
    public sealed class TimerDefinition : BaseDefinition
    {
        /// <summary>
        /// Type of the timer <see cref="TimerType"/>
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public TimerType Type { get; set; }

        /// <summary>
        /// Specifies the timer time depend on timer type 
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        ///If true specifies that the old timer time will not be overriden if a timer with same name exists in outgoing transitions of a new current activity
        /// </summary>
        public bool NotOverrideIfExists { get; set; }

        /// <summary>
        /// Create TimerDefinition object
        /// </summary>
        /// <param name="name">Name of the timer</param>
        /// <param name="type">Type of the timer <see cref="TimerType"/></param>
        /// <param name="value">Specifies the timer time depend on timer type </param>
        /// <param name="notOverrideIfExists">If true specifies that the old timer time will not be overriden if a timer with same name exists in outgoing transitions of a new current activity</param>
        /// <returns>TimerDefinition object</returns>
        public static TimerDefinition Create(string name, string type, string value, string notOverrideIfExists)
        {
            TimerType parsedType;
            Enum.TryParse(type, true, out parsedType);

            bool notOverride;
            bool.TryParse(notOverrideIfExists, out notOverride);

            return new TimerDefinition
            {
                Name = name,
                Type = parsedType,
                Value = value,
                NotOverrideIfExists = notOverride
            };
        }

        public new TimerDefinition Clone()
        {
            return base.Clone() as TimerDefinition;
        }
    }
}
