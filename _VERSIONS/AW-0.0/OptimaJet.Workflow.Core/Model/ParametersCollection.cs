using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using OptimaJet.Workflow.Core.Runtime;

namespace OptimaJet.Workflow.Core.Model
{
    /// <summary>
    /// Represent a collection of process's parameters
    /// </summary>
    public class ParametersCollection : IEnumerable<ParameterDefinitionWithValue>
    {
        protected List<ParameterDefinitionWithValue> Parameters { get; set; }

        public ParametersCollection(List<ParameterDefinitionWithValue> parameters)
        {
            Parameters = parameters;
        }

        public ParametersCollection(IEnumerable<ParameterDefinitionWithValue> parameters)
        {
            Parameters = parameters.ToList();
        }

        public ParametersCollection()
        {
            Parameters = new List<ParameterDefinitionWithValue>();
        }

        /// <summary>
        /// Returns parameter with value <see cref="ParameterDefinitionWithValue"/> with specific name
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <returns>Parameter with value <see cref="ParameterDefinitionWithValue"/></returns>
        public ParameterDefinitionWithValue GetParameter(string name)
        {
            return Parameters.SingleOrDefault(p => p.Name == name);
        }

        /// <summary>
        /// Returns parameter's value by specific name
        /// </summary>
        /// <typeparam name="T">Type of the parameter</typeparam>
        /// <param name="name">Name of the parameter</param>
        /// <returns>Value of the parameter</returns>
        public T GetParameter<T>(string name)
        {
            var parameterDefinitionWithValue = Parameters.SingleOrDefault(p => p.Name == name);
            if (parameterDefinitionWithValue != null)
            {
                if (parameterDefinitionWithValue.Type == typeof(UnknownParameterType))
                {
                    var serializedValue = (string)parameterDefinitionWithValue.Value;
                    if (serializedValue == null)
                        return default(T);
                    return ParametersSerializer.Deserialize<T>(serializedValue);
                }
                return (T) parameterDefinitionWithValue.Value;
            }
            return default(T);
        }

        /// <summary>
        /// Adds parameter to parameters collection
        /// </summary>
        /// <param name="parameter">Parameter with value <see cref="ParameterDefinitionWithValue"/></param>
        public void AddParameter(ParameterDefinitionWithValue parameter)
        {
            Parameters.RemoveAll(p => p.Name == parameter.Name);
            Parameters.Add(parameter);
        }

        /// <summary>
        /// Adds parameters to parameters collection
        /// </summary>
        /// <param name="parameters">Collection of parameters with value <see cref="ParameterDefinitionWithValue"/></param>
        public void AddParameters(IEnumerable<ParameterDefinitionWithValue> parameters)
        {
            Parameters.RemoveAll(ep => parameters.Count(p => p.Name == ep.Name) > 0);
            Parameters.AddRange(parameters);
        }

        /// <summary>
        /// Replace parameters collection by new value
        /// </summary>
        /// <param name="parameters">>Collection of parameters with value <see cref="ParameterDefinitionWithValue"/></param>
        public void ReplaceParameters(List<ParameterDefinitionWithValue> parameters)
        {
            Parameters.Clear();
            AddParameters(parameters);
        }

       
        public IEnumerator<ParameterDefinitionWithValue> GetEnumerator()
        {
            return Parameters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
