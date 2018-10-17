using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AntWay.Core.Activity;


namespace AntWay.Core.Mapping
{
    public static class MappingReflection
    {
        public static TPO GetParametersBind<TPO>(string processId,
                                                 IAntWayRuntimeActivity activityInstance,
                                                 TPO parametersOutput)
        {
            List<string> methods = GetActivityMethodAttributes(parametersOutput);

            foreach (string method in methods)
            {
                activityInstance.ParametersBind = AntWayActivityActivator.RunMethod
                                                    (method, processId, activityInstance, parametersOutput);
            }

            return (TPO) activityInstance.ParametersBind;
        }


        public static List<string>
                     GetActivityMethodAttributes(object obj)
        {
            var result = new List<string>();

            var properties = obj.GetType()
                            .GetProperties()
                            .Where(p => p.CustomAttributes.Count() > 0);

            foreach (var p in properties)
            {
                var values = GetParameterMethod(p);
                result.Add(values);
            }

            return result;
        }

        private static string GetParameterMethod(PropertyInfo prop)
        {
            var attr = (BindingMethodAttribute)
                        prop
                        .GetCustomAttributes(false)
                        .Where(ca => ca.GetType() == typeof(BindingMethodAttribute))
                        .FirstOrDefault();

            var result = attr.Name;
            return result;
        }



        public static List<KeyValuePair<string, List<string>>>
                      GetSchemeValuesParameters(object obj)
        {
            var result = new List<KeyValuePair<string, List<string>>>();

            var properties = obj.GetType()
                                    .GetProperties()
                                    .Where(p => p.CustomAttributes.Count() > 0);

            foreach (var p in properties)
            {
                var values = GetParameterValues(p);
                result.Add(new KeyValuePair<string, List<string>>(p.Name, values));
            }

            return result;
        }

        
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
