using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OptimaJet.Workflow.Core.Designer;

namespace OptimaJet.Workflow.Core.Model
{

    /// <summary>
    ///  Represent a parameter in a process scheme vith value
    /// </summary>
    public sealed class ParameterDefinitionWithValue : ParameterDefinition
    {
        /// <summary>
        /// Name of the parameter
        /// </summary>
        public override string Name
        {
            get { return ParameterDefinition.Name; }
            set { ParameterDefinition.Name = value; }
        }

        /// <summary>
        /// Specifies the method of storing parameters <see cref="ParameterPurpose"/>
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public override ParameterPurpose Purpose
        {
            get { return ParameterDefinition.Purpose; }
            set { ParameterDefinition.Purpose = value; }
        }

        /// <summary>
        /// Type of the parameter
        /// </summary>
        [JsonConverter(typeof(StringTypeConverter))]
        public override Type Type
        {
            get { return ParameterDefinition.Type; }
             set { ParameterDefinition.Type = value; }
        }

        internal ParameterDefinition ParameterDefinition { private get; set; }

        /// <summary>
        /// Value of the parameter
        /// </summary>
        public object Value { get; set; }

        public new ParameterDefinitionWithValue Clone()
        {
            return base.Clone() as ParameterDefinitionWithValue;
        }
    }
}
