using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AntWay.Core.Manager;
using AntWay.Core.Mapping;
using AntWay.Core.Runtime;

namespace AntWay.Core.Activity
{
    /// <summary>
    /// TODO: Conveniente repartir métodos de esta clase en otros
    /// </summary>
    public static class AntWayActivityActivator
    {
        public static IAntWayRuntimeActivity GetAntWayRuntimeActivity(string actityId,
                                                List<Type> types,
                                                IActivityManager activityManager = null)
        {
            try
            {
                IAntWayRuntimeActivity obj = null;
                foreach (var t in types)
                {
                    var idFromActivityClass = t.GetAttributeValue((ActivityAttribute a) => a.Id);

                    bool createInstance = false;
                    if (actityId == idFromActivityClass)
                    {
                        createInstance = true;
                        if (activityManager != null &&
                            t.GetAttributeValue((ActivityAttribute a) => a.RunOnlyIfIsInManager))
                        {
                            var activities = activityManager.GetActivitiesManager();
                            var activityFromType = activities.FirstOrDefault(a => a.ClassActivityType == t);
                            createInstance = (activityFromType != null);
                        }

                        if (createInstance)
                        {
                            obj = Activator.CreateInstance(t) as IAntWayRuntimeActivity;
                            break;
                        }
                    }
                }

                return obj;
            }
            catch (Exception ex)
            {
                log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
                log4net.Config.XmlConfigurator.Configure();

                log.Error(ex.Message);
                if (ex is System.Reflection.ReflectionTypeLoadException)
                {
                    var typeLoadException = ex as ReflectionTypeLoadException;
                    var loaderExceptions = typeLoadException.LoaderExceptions;
                    string error = "";
                    foreach (var exc in loaderExceptions)
                    {
                        error += exc + ",";
                    }
                    log.Error(error);
                }

                return null;
            }
        }

        /// <summary>
        /// Deprecated
        /// </summary>
        /// <param name="actityId"></param>
        /// <param name="types"></param>
        /// <param name="attributeFilter"></param>
        /// <returns></returns>
        public static IAntWayRuntimeActivity GetAntWayRuntimeActivity(string actityId,
                                                  List<Type> types,
                                                  ActivityAttribute attributeFilter = null
                                             )
        {
            string idFromActivityClass = null;
            string attributeNameFromClass = null;
            try
            {
                IAntWayRuntimeActivity obj = null;
                foreach (var t in types)
                {
                    idFromActivityClass = t.GetAttributeValue((ActivityAttribute a) => a.Id);

                    if (attributeFilter != null)
                    {
                        attributeNameFromClass = t.GetAttributeValue((ActivityAttribute a) => a.Name);
                    }

                    if (actityId == idFromActivityClass &&
                         attributeFilter?.Name == attributeNameFromClass)
                    {
                        obj = Activator.CreateInstance(t) as IAntWayRuntimeActivity;
                        break;
                    }
                }

                return obj;
            }
            catch (Exception ex)
            {
                log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
                log4net.Config.XmlConfigurator.Configure();

                log.Error(ex.Message);
                if (ex is System.Reflection.ReflectionTypeLoadException)
                {
                    var typeLoadException = ex as ReflectionTypeLoadException;
                    var loaderExceptions = typeLoadException.LoaderExceptions;
                    string error = "";
                    foreach (var exc in loaderExceptions)
                    {
                        error += exc + ",";
                    }
                    log.Error(error);
                }

                return null;
            }
        }

        public static T RunMethod<T>(string methodName, Guid processId,
                                     IAntWayRuntimeActivity activityInstance)
        {
            MethodInfo methodInfo = activityInstance.GetType().GetMethod(methodName);
            ParameterInfo[] parameters = methodInfo.GetParameters();
            object[] parametersArray = new object[] { new object[] { processId} };
            T result = (T)methodInfo.Invoke(activityInstance, parametersArray);

            return result;
        }

        internal static object RunMethod(string methodName, Guid processId,
                                         IAntWayRuntimeActivity activityInstance)
        {
            MethodInfo methodInfo = activityInstance.GetType().GetMethod(methodName);
            ParameterInfo[] parameters = methodInfo.GetParameters();
            object[] parametersArray = new object[] { new object[] { processId } };
            object result = methodInfo.Invoke(activityInstance, parametersArray);

            return result;
        }

        public static List<string> ObjectsDifference(object oOldRecord, object oNewRecord,
                                                    List<string> differences = null)
        {
            var result = differences ?? new List<string>();

            var oType = oOldRecord.GetType();

            if (!oType.IsClass)
            {
                if (!object.Equals(oOldRecord, oNewRecord))
                {
                    // Handle the display values when the underlying value is null
                    var sOldValue = oOldRecord == null ? "null" : oOldRecord.ToString();
                    var sNewValue = oNewRecord == null ? "null" : oNewRecord.ToString();

                    result.Add(oType.Name + " was: " + sOldValue + "; is: " + sNewValue);
                    return result;
                }
            }

            foreach (var oProperty in oType.GetProperties())
            {
                var oOldValue = oProperty.GetValue(oOldRecord, null);
                var oNewValue = oProperty.GetValue(oNewRecord, null);
                
                if (oOldValue != null)
                {
                    Type myType = oOldValue.GetType();
                    if (myType.Name.ToLower() != "string" && myType.IsClass)
                    {
                        result = ObjectsDifference(oOldValue, oNewValue, result);
                        break;
                    }
                }
                
                if (!object.Equals(oOldValue, oNewValue))
                {
                    // Handle the display values when the underlying value is null
                    var sOldValue = oOldValue == null ? "null" : oOldValue.ToString();
                    var sNewValue = oNewValue == null ? "null" : oNewValue.ToString();

                    result.Add(oProperty.Name + " was: " + sOldValue + "; is: " + sNewValue);
                }
            }

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
