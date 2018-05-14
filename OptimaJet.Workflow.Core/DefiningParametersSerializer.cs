using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OptimaJet.Workflow.Core
{
    public static class DefiningParametersSerializer
    {
        public static string Serialize(IDictionary<string, object> parameters)
        {
            var json = new StringBuilder("{");

            bool isFirst = true;

            foreach (var parameter in parameters.OrderBy(p => p.Key))
            {
                if (string.IsNullOrEmpty(parameter.Key))
                    continue;

                if (!isFirst)
                    json.Append(",");

                json.AppendFormat("{0}:[", parameter.Key);

                var isSubFirst = true;

                if (parameter.Value is IEnumerable)
                {
                    var enumerableValue = (parameter.Value as IEnumerable);

                    var valuesToString = new List<string>();

                    foreach (var val in enumerableValue)
                    {
                        valuesToString.Add(val.ToString());
                    }

                    foreach (var parameterValue in valuesToString.OrderBy(p => p))
                    {
                        if (!isSubFirst)
                            json.Append(",");
                        json.AppendFormat("\"{0}\"", parameterValue);
                        isSubFirst = false;
                    }
                }
                else
                {
                    json.AppendFormat("\"{0}\"", parameter.Value);
                }

                json.Append("]");

                isFirst = false;

            }

            json.Append("}");

            return json.ToString();
        }
    }
}
