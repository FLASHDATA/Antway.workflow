using System;
using System.Collections.Generic;
using System.Linq;

namespace OptimaJet.Workflow.Core.Model
{

    /// <summary>
    /// Represent a command in a process scheme
    /// </summary>
    public class CommandDefinition : BaseDefinition
    {
        /// <summary>
        /// List of references on parameters <see cref="ParameterDefinitionReference"/>
        /// </summary>
        public List<ParameterDefinitionReference> InputParameters { get; set; }

        /// <summary>
        /// Create CommandDefinition object
        /// </summary>
        /// <param name="name">Name of the command</param>
        /// <returns></returns>
        public static CommandDefinition Create(string name)
        {
            return new CommandDefinition() {Name = name, InputParameters = new List<ParameterDefinitionReference>()};
        }

        /// <summary>
        /// Add the reference on the parameter to InputParameters collection
        /// </summary>
        /// <param name="name">Name of the reference</param>
        /// <param name="isRequired">Indicates that the parameter is required</param>
        /// <param name="defaultValue">JSON serialized default value or string</param>
        /// <param name="parameter">Parameter definition</param>
        public void AddParameterRef(string name, bool isRequired, string defaultValue, ParameterDefinition parameter)
        {
            InputParameters.Add(new ParameterDefinitionReference {Name = name, IsRequired = isRequired, DefaultValue = defaultValue, Parameter = parameter});
        }

        /// <summary>
        /// Add the reference on the parameter to InputParameters collection
        /// </summary>
        /// <param name="name">Name of the reference</param>
        /// <param name="parameter">Parameter definition</param>
        [Obsolete("Use AddParameterRef (string name, bool isRequired, string defaultValue, ParameterDefinition parameter). It going to be removed since 1.6.6 version.")]
        public void AddParameterRef(string name, ParameterDefinition parameter)
        {
            InputParameters.Add(new ParameterDefinitionReference { Name = name, IsRequired = false, Parameter = parameter });
        }

        public new CommandDefinition Clone()
        {
            var newObject = base.Clone() as CommandDefinition;

            if (newObject == null)
                throw new Exception("Cloning of CommandDefinition fails");

            newObject.InputParameters = new List<ParameterDefinitionReference>();

            InputParameters.ForEach(p => newObject.InputParameters.Add(p.Clone()));

            return newObject;
        }

        public CommandDefinition Clone(List<ParameterDefinition> parameters)
        {
            var newObject = base.Clone() as CommandDefinition;

            if (newObject == null)
                throw new Exception("Cloning of CommandDefinition fails");

            newObject.InputParameters = new List<ParameterDefinitionReference>();

            foreach (var inputParameter in InputParameters)
            {
                var newParameterDefinition =
                    parameters.First(
                        p => p.Name.Equals(inputParameter.Parameter.Name, StringComparison.Ordinal));

                newObject.AddParameterRef(inputParameter.Name, inputParameter.IsRequired, inputParameter.DefaultValue, newParameterDefinition);
            }

            return newObject;
        }
    }

    /// <summary>
    ///  Represents a named reference on a parameter in a process scheme
    /// </summary>
    public class ParameterDefinitionReference
    {
        /// <summary>
        /// Name of the reference
        /// </summary>
        public string Name {get; set;}
        /// <summary>
        /// Indicates that the parameter is required
        /// </summary>
        public bool IsRequired { get; set; }
        /// <summary>
        /// JSON serialized default value or string
        /// </summary>
        public string DefaultValue { get; set; }
        /// <summary>
        /// Parameter definition
        /// </summary>
        public ParameterDefinition Parameter{get;set;}

        public ParameterDefinitionReference Clone()
        {
            var newObject = MemberwiseClone() as ParameterDefinitionReference;

            if (newObject == null)
                throw new Exception("Error cloning ParameterDefinitionReference object");

            newObject.Parameter = Parameter.Clone();

            return newObject;
        }
    }

}
