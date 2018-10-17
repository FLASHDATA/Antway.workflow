using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AntWay.Core.Mapping;

namespace AntWay.Core.Activity
{
    public static class AntWayActivityActivator
    {
        public static IAntWayRuntimeActivity GetAntWayObjectFromActivity(string actityId)
        {
            string idFromActivityClass = null;

            var type = typeof(IAntWayRuntimeActivity);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(s => s.GetTypes())
                        .Where(p => type.IsAssignableFrom(p))
                        .ToList();

            IAntWayRuntimeActivity obj = null;
            foreach (var t in types)
            {
                Type myType1 = Type.GetType($"{t.FullName}, {t.Assembly.ManifestModule.Name.Replace(".dll", "")}");
                if (myType1 == null) continue;

                idFromActivityClass = myType1.GetAttributeValue((ActivityAttribute a) => a.Id);

                if (actityId == idFromActivityClass)
                {
                    obj = Activator.CreateInstance(myType1) as IAntWayRuntimeActivity;
                    break;
                }
            }

            return obj;
        }

        public static T RunMethod<T>(string methodName, string processId,
                                     IAntWayRuntimeActivity activityInstance,
                                     T parametersBind)
        {
            MethodInfo methodInfo = activityInstance.GetType().GetMethod(methodName);
            ParameterInfo[] parameters = methodInfo.GetParameters();
            object[] parametersArray = new object[] { new object[] { processId, parametersBind } };
            T result = (T)methodInfo.Invoke(activityInstance, parametersArray);

            return result;
        }
    }


    public static class AttributeExtensions
    {
        public static TValue GetAttributeValue<TAttribute, TValue>(
            this Type type,
            Func<TAttribute, TValue> valueSelector)
            where TAttribute : Attribute
        {
            var att = type.GetCustomAttributes(typeof(TAttribute), true)
                        .FirstOrDefault() as TAttribute;
            if (att != null)
            {
                return valueSelector(att);
            }

            return default(TValue);
        }
    }
}
