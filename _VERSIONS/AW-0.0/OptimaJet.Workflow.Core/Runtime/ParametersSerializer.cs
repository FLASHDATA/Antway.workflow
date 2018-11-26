using System;
using Newtonsoft.Json;

namespace OptimaJet.Workflow.Core.Runtime
{
    public static class ParametersSerializer
    {
        /// <summary>
        /// Serializer setting for JSON serializer
        /// </summary>
        public static JsonSerializerSettings Settings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        /// <summary>
        /// Provides access to Workflow Engine .NET Persistence Process Parameters serialization mechanism
        /// </summary>
        /// <param name="serializedValue">Serialized value</param>
        /// <param name="parameterType">Parameter type</param>
        /// <returns>Deserialized object</returns>
        public static object Deserialize(string serializedValue, Type parameterType)
        {
            return JsonConvert.DeserializeObject(serializedValue, parameterType, Settings);
        }

        /// <summary>
        /// Provides access to Workflow Engine .NET Persistence Process Parameters serialization mechanism
        /// </summary>
        /// <param name="serializedValue">Serialized value</param>
        /// <returns>Deserialized object</returns>
        public static T Deserialize<T>(string serializedValue)
        {
            return JsonConvert.DeserializeObject<T>(serializedValue, Settings);
        }

        /// <summary>
        /// Provides access to Workflow Engine .NET Persistence Process Parameters deserialization mechanism
        /// </summary>
        /// <param name="value">Parameter value</param>
        /// <param name="parameterType">Parameter type</param>
        /// <returns>Serialized string</returns>
        public static string Serialize(object value, Type parameterType)
        {
            if (value == null)
                return null;

            return JsonConvert.SerializeObject(value, parameterType, Settings);
        }

        /// <summary>
        /// Provides access to Workflow Engine .NET Persistence Process Parameters deserialization mechanism
        /// </summary>
        /// <param name="value">Parameter value</param>
        /// <returns>Serialized string</returns>
        public static string Serialize(object value)
        {
            if (value == null)
                return null;

            return JsonConvert.SerializeObject(value, Settings);
        }


    }
}