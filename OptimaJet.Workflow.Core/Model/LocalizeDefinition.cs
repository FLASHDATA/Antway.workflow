using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OptimaJet.Workflow.Core.Model
{
    /// <summary>
    /// Type of localized resource
    /// </summary>
    public enum LocalizeType
    {
        /// <summary>
        /// Command <see cref="CommandDefinition.Name"/>
        /// </summary>
        Command,
        /// <summary>
        /// Command <see cref="ActivityDefinition.State"/>
        /// </summary>
        State,
        /// <summary>
        /// Command <see cref="ParameterDefinitionReference.Name"/>
        /// </summary>
        Parameter
    }

    /// <summary>
    /// Represent a localization record in a process scheme
    /// </summary>
    public sealed class LocalizeDefinition
    {
        /// <summary>
        /// Type of localized resource <see cref="LocalizeType"/>
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public LocalizeType Type { get; set; }

        /// <summary>
        /// If true specifies that the record will be used by default in case of absence of a record with specific culture
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// System name
        /// </summary>
        public string ObjectName { get; set; }

        /// <summary>
        /// Culture name
        /// </summary>
        public string Culture { get; set; }

        /// <summary>
        /// Localized name
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Create LocalizeDefinition object
        /// </summary>
        /// <param name="objectName">System name</param>
        /// <param name="type">Type of localized resource <see cref="LocalizeType"/></param>
        /// <param name="culture">Culture name</param>
        /// <param name="value">Localized name</param>
        /// <param name="isDefault">If true specifies that the record will be used by default in case of absence of a record with specific culture</param>
        /// <returns>LocalizeDefinition object</returns>
        public static LocalizeDefinition Create(string objectName, string type, string culture, string value, string isDefault)
        {
            LocalizeType parsedType;
            Enum.TryParse(type, true, out parsedType);

            return new LocalizeDefinition
                       {
                           Culture = culture,
                           IsDefault = !string.IsNullOrEmpty(isDefault) && bool.Parse(isDefault),
                           ObjectName = objectName,
                           Type = parsedType,
                           Value = value
                       };

        }

        /// <summary>
        /// Create LocalizeDefinition object
        /// </summary>
        /// <param name="objectName">System name</param>
        /// <param name="type">Type of localized resource <see cref="LocalizeType"/></param>
        /// <param name="culture">Culture name</param>
        /// <param name="value">Localized name</param>
        /// <param name="isDefault">If true specifies that the record will be used by default in case of absence of a record with specific culture</param>
        /// <returns>LocalizeDefinition object</returns>
        public static LocalizeDefinition Create(string objectName, LocalizeType type, string culture, string value, bool isDefault)
        {
            return new LocalizeDefinition
            {
                Culture = culture,
                IsDefault = isDefault,
                ObjectName = objectName,
                Type = type,
                Value = value
            };

        }

        public LocalizeDefinition Clone()
        {
            return MemberwiseClone() as LocalizeDefinition;
        }
    }
}
