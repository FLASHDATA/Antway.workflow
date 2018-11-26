using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Runtime
{
    /// <summary>
    /// Represent a command parameter for use in an application
    /// </summary>
#if !NETCOREAPP
    [Serializable]
#endif
    public class CommandParameter
    {

        /// <summary>
        /// System name of the parameter
        /// </summary>
        public string ParameterName { get; set; }

        /// <summary>
        /// Localized name of the parameter
        /// </summary>
        public string LocalizedName { get; set; }

        /// <summary>
        /// Indicates that the parameter is required
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Value of the parameter
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Default value of the parameter
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// Type of the parameter
        /// </summary>
        public Type Type => Type.GetType(TypeName);


        /// <summary>
        /// Full name of the type of the parameter
        /// </summary>
        public string TypeName { get; set; }

    }

    /// <summary>
    /// Represents a workflow command for use in an application
    /// </summary>
#if !NETCOREAPP
    [Serializable]
#endif
    public sealed class WorkflowCommand
    {
        /// <summary>
        /// Process id for which command is valid
        /// </summary>
        public Guid ProcessId { get; set; }

        /// <summary>
        /// Activity name for which command is valid
        /// </summary>
        public string ValidForActivityName { get; set; }

        /// <summary>
        /// If true mean that command is valid for a subprocess
        /// </summary>
        //public bool ValidForSubprocess { get; set; }

        /// <summary>
        /// Activity name for which command is valid
        /// </summary>
        public string ValidForStateName { get; set; }

        /// <summary>
        /// Transition classifier for command  <see cref="TransitionClassifier"/>
        /// </summary>
        public TransitionClassifier Classifier { get; set; }

        /// <summary>
        /// If true means that the command was recieved from a subprocess
        /// </summary>
        public bool IsForSubprocess { get; set; }

        /// <summary>
        /// List of user ids which can execute the command
        /// </summary>
        public IEnumerable<string> Identities
        {
            get { return _identitiesList; }
        }

        private readonly List<string> _identitiesList = new List<string>();

        /// <summary>
        /// System name of the command
        /// </summary>
        public string CommandName { get; set; }

        /// <summary>
        /// Localized name of the command
        /// </summary>
        public string LocalizedName { get; set; }

        /// <summary>
        /// Parameters list of the command <see cref="CommandParameter"/>
        /// </summary>
        public List<CommandParameter> Parameters = new List<CommandParameter>();

        /// <summary>
        /// Get the parameter value
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <returns>Parameter value</returns>
        public object GetParameter(string name)
        {
            var parameter = Parameters.SingleOrDefault(p => p.ParameterName.Equals(name, StringComparison.Ordinal));
            return parameter?.Value;
        }

        /// <summary>
        /// Set parameter value
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        public void SetParameter(string name, object value)
        {
            var parameter = Parameters.SingleOrDefault(p => p.ParameterName == name);
            if (parameter != null)
            {
                parameter.Value = value;
            }
            else
            {
                Parameters.Add(new CommandParameter() {ParameterName = name,TypeName = value.GetType().AssemblyQualifiedName,Value = value} );
            }
        }

        /// <summary>
        /// Create a workflow command object
        /// </summary>
        /// <param name="processId">Procees id</param>
        /// <param name="transitionDefinition">Command transition <see cref="TransitionDefinition"/></param>
        /// <param name="processDefinition"><see cref="ProcessDefinition"/> object which represent parsed workflow scheme</param>
        /// <param name="deserializer">Function for deserialize InitialValue of Parameter from JSON</param>
        /// <returns><see cref="WorkflowCommand"/> object</returns>
        public static WorkflowCommand Create(Guid processId, TransitionDefinition transitionDefinition,
            ProcessDefinition processDefinition, Func<string,Type,object> deserializer = null )
        {
            if (transitionDefinition.Trigger.Type != TriggerType.Command || transitionDefinition.Trigger.Command == null)
                throw new InvalidOperationException();

            var parametrs = new List<CommandParameter>(transitionDefinition.Trigger.Command.InputParameters.Count);

            parametrs.AddRange(
                transitionDefinition.Trigger.Command.InputParameters.Select(
                    p => new CommandParameter
                    {
                        ParameterName = p.Name,
                        IsRequired = p.IsRequired,
                        TypeName = p.Parameter.Type.AssemblyQualifiedName,
                        LocalizedName =
                            processDefinition.GetLocalizedParameterName(p.Name, CultureInfo.CurrentCulture),
                        DefaultValue = GetDefaultValue(p,deserializer),
                        Value = null
                    }));

            return new WorkflowCommand
            {
                CommandName = transitionDefinition.Trigger.Command.Name,
                LocalizedName =
                    processDefinition.GetLocalizedCommandName(transitionDefinition.Trigger.Command.Name,
                        CultureInfo.CurrentCulture),
                Parameters = parametrs,
                ProcessId = processId,
                ValidForActivityName = transitionDefinition.From.Name,
                ValidForStateName = transitionDefinition.From.State,
                Classifier = transitionDefinition.Classifier
            };
        }

        private static object GetDefaultValue(ParameterDefinitionReference p, Func<string,Type,object> deserializer)
        {
            object defaultValue;

            if (deserializer != null)
            {
                if (!string.IsNullOrEmpty(p.DefaultValue))
                {
                    try
                    {
                        defaultValue = deserializer(p.DefaultValue, p.Parameter.Type);
                    }
                    catch (Exception)
                    {
                        defaultValue = p.DefaultValue;
                    }
                }
                else
                {
                    defaultValue = null;
                }
            }
            else
            {
                defaultValue = p.DefaultValue;
            }
            return defaultValue;
        }

        /// <summary>
        /// Add an identity in Identities collection
        /// </summary>
        /// <param name="identityId"></param>
        public void AddIdentity(string identityId)
        {
            if (!_identitiesList.Contains(identityId))
                _identitiesList.Add(identityId);
        }

        /// <summary>
        /// Validates a command
        /// </summary>
        /// <param name="errorMessage">Errors summary</param>
        /// <returns>true if the command is valid</returns>
        public bool Validate(out string errorMessage)
        {
            var errors = new StringBuilder();
            foreach (var parameter in Parameters)
            {
                if (parameter.IsRequired && parameter.Value == null)
                    errors.AppendFormat("Parameter {0} of {1} command is required", parameter.ParameterName, CommandName);

                if (parameter.Value != null && parameter.Value.GetType() != parameter.Type &&
                    !parameter.Type.GetTypeInfo().IsInstanceOfType(parameter.Value))
                {
                    try
                    {
                        // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                        var res = Convert.ChangeType(parameter.Value, parameter.Type);
                    }
                    catch (Exception)
                    {
                           errors.AppendFormat("Parameter {0} of {1} command has a wrong type. The extected type is {2}. The actual type is {3}", parameter.ParameterName, CommandName,
                        parameter.Type.FullName, parameter.Value.GetType().FullName);
                    }
                }
            }
            errorMessage = errors.ToString();
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets all value of parameters to their default values
        /// </summary>
        public void SetAllParametersToDefault()
        {
            Parameters.ToList().ForEach(p => p.Value = p.DefaultValue);
        }

        /// <summary>
        /// Sets value of parameter with specific name to default value
        /// </summary>
        /// <param name="name">Name of parameter</param>
        public void SetParameterToDefault(string name)
        {
            var parameter = Parameters.SingleOrDefault(p => p.ParameterName == name);
            if (parameter == null)
                throw new InvalidOperationException();
            parameter.Value = parameter.DefaultValue;
        }
    }
}
