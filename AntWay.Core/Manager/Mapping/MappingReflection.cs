using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AntWay.Core.Activity;
using static AntWay.Core.Manager.Checksum;

namespace AntWay.Core.Mapping
{
    internal static class MappingReflection
    {
        internal static TAM GetParametersBind<TAM>(string processId,
                                                 IAntWayRuntimeActivity activityInstance,
                                                 TAM parameters,
                                                 ChecksumType type)
        {
            object result = null;
            string method = GetActivityMethodAttributes(parameters, type)
                                   .FirstOrDefault();

            if (method == null)
            {
                return (TAM) result;
            }

            result = AntWayActivityActivator.RunMethod(method, processId, activityInstance, parameters);
            
            return (TAM) result;
        }


        internal static List<string>
                     GetActivityMethodAttributes(object obj, ChecksumType type)
        {
            var result = new List<string>();

            var properties = obj.GetType()
                            .GetProperties()
                            .Where(p => p.CustomAttributes.Count() > 0);

            foreach (var p in properties)
            {
                var values = GetParameterMethod(p, type);
                if (values != null)
                {
                    result.Add(values);
                }
            }

            return result;
        }

        private static string GetParameterMethod(PropertyInfo prop, ChecksumType type)
        {
            var attr = (ParameterBindingAttribute)
                        prop
                        .GetCustomAttributes(false)
                        .Where(ca => ca.GetType() == typeof(ParameterBindingAttribute))
                        .FirstOrDefault();

            var result = type == ChecksumType.Input ? attr.InputBindMethod : attr.OutputBindMethod;
            return result;
        }



        //public static List<KeyValuePair<string, List<string>>>
        //              GetSchemeValuesParameters(object obj)
        //{
        //    var result = new List<KeyValuePair<string, List<string>>>();

        //    var properties = obj.GetType()
        //                            .GetProperties()
        //                            .Where(p => p.CustomAttributes.Count() > 0);

        //    foreach (var p in properties)
        //    {
        //        var values = GetParameterValues(p);
        //        result.Add(new KeyValuePair<string, List<string>>(p.Name, values));
        //    }

        //    return result;
        //}

        
        private static List<string> GetParameterValues(PropertyInfo prop)
        {
            var attr = (ParameterValuesAttribute)
                        prop
                        .GetCustomAttributes(false)
                        .Where(ca => ca.GetType() == typeof(ParameterValuesAttribute))
                        .FirstOrDefault();

            var result = attr.Values.ToList();
            return result;
        }
    }
}
