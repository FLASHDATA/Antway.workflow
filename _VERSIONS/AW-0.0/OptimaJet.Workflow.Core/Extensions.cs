using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace OptimaJet.Workflow.Core
{
    public static class Extensions
    {
        public static XElement SingleOrDefault(this XElement element, string name)
        {
            return element.Name == name ? element : element.Elements(name).SingleOrDefault();
        }

        public static Type ToNullableType(this Type type)
        {
            var newType = Nullable.GetUnderlyingType(type) ?? type;
            return newType.GetTypeInfo().IsValueType ? typeof(Nullable<>).MakeGenericType(newType) : newType;
        }

        public static bool IsNullable(this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        public static Type GetUnderlyingType(this Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        public static object GetDefaultValue(this Type type)
        {
            if (type.GetTypeInfo().IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        private static List<Type> _numerics = new List<Type>()
        {
            typeof(Byte),
            typeof(SByte),
            typeof(Int16),
            typeof(UInt16),
            typeof(Int32),
            typeof(UInt32),
            typeof(Int64),
            typeof(UInt64),
            typeof(Single),
            typeof(Double),
            typeof(Decimal)
        };

        public static bool IsNumeric(this Type type)
        {
            return _numerics.Contains(type);
        }

        public static string ToLowerCaseString(this bool value)
        {
            return value.ToString().ToLower();
        }

        public static bool ExtendedEquals(this object value, object valueToCompare)
        {
            if (valueToCompare == null)
            {
                return value == null;
            }

            if (value.GetType() == valueToCompare.GetType() && value is IEnumerable)
            {
                var valueArray = (value as IEnumerable).Cast<object>().ToArray();
                var valueToCompareArray = (valueToCompare as IEnumerable).Cast<object>().ToArray();

                if (valueArray.Length != valueToCompareArray.Length)
                    return false;

                for (int i = 0; i < valueArray.Length; i++)
                {
                    if (valueArray[i] == null && valueToCompareArray[i] != null)
                        return false;
                    if (valueArray[i] != null && valueToCompareArray[i] == null)
                        return false;
                    if (valueArray[i] != null && valueToCompareArray[i] != null)
                        if (!valueArray[i].Equals(valueToCompareArray[i]))
                            return false;
                }

                return true;
            }

            return value.Equals(valueToCompare);
        }

        public static string ToFormattedString<T>(this List<T> value, bool quotate)
        {
            return value.ToFormattedString(",", quotate);
        }

        public static string ToFormattedString<T>(this List<T> value, string delimeter, bool quotate)
        {
            var builder = new StringBuilder();
            bool isFirst = true;
            foreach (var item in value)
            {
                if (!isFirst)
                    builder.Append(delimeter);
                isFirst = false;
                if (quotate)
                    builder.AppendFormat(CultureInfo.InvariantCulture, "'{0}'", item);
                else
                    builder.AppendFormat(CultureInfo.InvariantCulture, "{0}", item);


            }

            return builder.ToString();
        }

        public static bool TryGetValueIgnoreCase<TV>(this IDictionary<string, TV> dictionary, string key, out TV value)
        {
            value = default(TV);
            //was StringComparison.InvariantCultureIgnoreCase
            var trueKeys = dictionary.Keys.Where(k => k.Equals(key, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!trueKeys.Any())
            {
                return false;
            }
            if (trueKeys.Count() > 1)
            {
                return false;
            }

            var trueKey = trueKeys.Single();

            return dictionary.TryGetValue(trueKey, out value);

        }

        public static dynamic ToDynamic(this Dictionary<string, object> dictionary)
        {
            var expando = new ExpandoObject();
            var expandoCollection = (ICollection<KeyValuePair<string, object>>) expando;

            foreach (var kvp in dictionary)
            {
                expandoCollection.Add(kvp);
            }

            return expando;
        }

        public static string ToValidCSharpIdentifierName(this string originalString)
        {
            var pattern = @"[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Nl}\p{Mn}\p{Mc}\p{Cf}\p{Pc}\p{Lm}]";

            var result = Regex.Replace(originalString, pattern, "_");

            if (!char.IsLetter(result, 0))
            {
                result = result.Insert(0, "_");
            }

            return result.Replace(" ", "_");
        }
        
        public static T? FirstOrNull<T>(this IEnumerable<T> items, Func<T, bool> predicate) where T : struct {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            foreach (var item in items) {
                if (predicate(item)) return item;
            }
            return null;
        }
    }
}
