using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntWay.Core.Model
{
    public class ActivityModelBase<T> : IActivityModel
    {
        public virtual object DeserializePersistedObject(string jsonPersisted)
        {
            JObject jsonObject = (JObject) JsonConvert
                                            .DeserializeObject(jsonPersisted);

            T persistedObject = JsonConvert.DeserializeObject<T>(jsonObject.ToString());

            return persistedObject;
        }
    }
}
