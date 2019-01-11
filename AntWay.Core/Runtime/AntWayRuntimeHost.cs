using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntWay.Core.Runtime
{
    /// <summary>
    /// TODO: Review
    /// </summary>
    public static class AntWayRuntimeHost
    { 
        public static List<KeyValuePair<string, string>> ParametersTemp
                                                  = new List<KeyValuePair<string, string>>();

        public static void AddParameter(string processId, object parameter)
        {
            string serializedObj = JsonConvert.SerializeObject(parameter);
            ParametersTemp.Add(new KeyValuePair<string, string>(processId, serializedObj));
        }

        public static T FindParameter<T>(string id)
        {
            var pkvp = ParametersTemp.FirstOrDefault(p => p.Key == id);

            var serializedObj = pkvp.Key != null ? pkvp.Value : null;

            if (serializedObj == null) return default(T);

            return JsonConvert.DeserializeObject<T>(serializedObj);
        }
    }
}
