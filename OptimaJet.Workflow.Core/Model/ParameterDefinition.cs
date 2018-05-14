using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OptimaJet.Workflow.Core.Designer;
using OptimaJet.Workflow.Core.Fault;

namespace OptimaJet.Workflow.Core.Model
{

    /// <summary>
    /// Represent a parameter in a process scheme
    /// </summary>
    public class ParameterDefinition : BaseDefinition
    {
        /// <summary>
        /// Type of the parameter
        /// </summary>
        [JsonConverter(typeof(StringTypeConverter))]
        public virtual Type Type { get;  set; }

        /// <summary>
        /// Specifies the method of storing parameters <see cref="ParameterPurpose"/>
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public virtual ParameterPurpose Purpose { get; set; }

        /// <summary>
        /// Serialized default value of the parameter
        /// </summary>
        public virtual string InitialValue { get; set; }

        /// <summary>
        /// Create ParameterDefinition object
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="type">Type of the parameter</param>
        /// <param name="purpose">Specifies the method of storing parameters <see cref="ParameterPurpose"/></param>
        /// <param name="initialValue">Serialized default value of the parameter</param>
        /// <returns>ParameterDefinition object</returns>
        public static ParameterDefinition Create(string name, string type, string purpose, string initialValue)
        {
            ParameterPurpose parsedPurpose;
            Type clrType = null;
            Enum.TryParse(purpose, true, out parsedPurpose);
            try
            {
                clrType = ParsedType.Parse(type).ConvertToType();
            }
            catch (Exception ex)
            {
                try
                {
                    clrType = Type.GetType(type);
                }
                catch (Exception ex1)
                {
                    throw new WrongParameterTypeException(name, type, new List<Exception> {ex,ex1});
                }
             }
            
            return new ParameterDefinition
            {
                Name = name,
                Type = clrType,
                Purpose = parsedPurpose,
                InitialValue = initialValue
            };
        }

        /// <summary>
        /// Create ParameterDefinition object
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="type">Type of the parameter</param>
        /// <param name="purpose">Specifies the method of storing parameters <see cref="ParameterPurpose"/></param>
        /// <param name="initialValue">Serialized default value of the parameter</param>
        /// <returns>ParameterDefinition object</returns>
        public static ParameterDefinition Create(string name, Type type, ParameterPurpose purpose,
            string initialValue = null)
        {
            return new ParameterDefinition
            {
                Name = name,
                Type = type,
                Purpose = purpose,
                InitialValue = initialValue
            };
        }

        /// <summary>
        /// Create ParameterDefinition object
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="type">Type of the parameter</param>
        /// <param name="initialValue">Serialized default value of the parameter</param>
        /// <returns>ParameterDefinition object</returns>
        public static ParameterDefinition Create(string name, string type, string initialValue)
        {
            return Create(name, type, ParameterPurpose.Temporary.ToString("G"), initialValue);
        }


        /// <summary>
        /// Create ParameterDefinitionWithValue object 
        /// </summary>
        /// <param name="parameterDefinition">Parameter object</param>
        /// <param name="value">Value of the parameter</param>
        public static ParameterDefinitionWithValue Create(ParameterDefinition parameterDefinition, object value)
        {
            if (value != null && parameterDefinition.Type != typeof(UnknownParameterType) && value.GetType() != parameterDefinition.Type &&
                !parameterDefinition.Type.GetTypeInfo().IsInstanceOfType(value))
            {
                try
                {
                    var converted = Convert.ChangeType(value, parameterDefinition.Type);
                    return new ParameterDefinitionWithValue {ParameterDefinition = parameterDefinition, Value = converted};
                }
                catch (Exception)
                {
                    throw new InvalidParameterValueException("Parameter {0} has a wrong type. The extected type is {1}. The actual type is {2}", parameterDefinition.Name,
                        parameterDefinition.Type.FullName, value.GetType().FullName);
                }
            }
            return new ParameterDefinitionWithValue {ParameterDefinition = parameterDefinition, Value = value};
        }

        public new ParameterDefinition Clone()
        {
            return base.Clone() as ParameterDefinition;
        }
    }

}
