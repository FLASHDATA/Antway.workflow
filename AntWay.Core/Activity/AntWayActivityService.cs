using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AntWay.Core.Mapping;
using AntWay.Core.Model;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;

namespace AntWay.Core.Activity
{
    [Activity(Id = "ServiceCall")]
    public class AntWayActivityService : AntWayActivityRuntimeBase, IAntWayRuntimeActivity
    {
        public object ParametersBind { get; set; }
        
        public override ActivityExecution Run(ProcessInstance pi,
                                              WorkflowRuntime runtime,
                                              object[] parameters = null)
        {
            var result = new ActivityExecution
            {
                ActivityId = ActivityId,
                ActivityName = ActivityName,
                ProcessId = pi.ProcessId,
            };

            var url = parameters[0].ToString();

            try
            {
                var response = new HttpClient().GetAsync(url).Result;

                result.ParametersOut = new AntWayActivityServiceParametersOutput
                {
                    PARAMETER_HTTP_RESPONSE = "200"
                };
                result.ExecutionSuccess = true;
            }
            catch (Exception ex)
            {
                result.ParametersOut = new AntWayActivityServiceParametersOutput
                {
                    PARAMETER_HTTP_RESPONSE = "403"
                };
                result.ExecutionSuccess = false; 
            }

            PersistActivityExecution(result, pi, runtime);

            return result;
        }
    }

    public class AntWayActivityServiceParametersInput
    {
        public string URL { get; set; }
    }

    public class AntWayActivityServiceParametersOutput
    {
        //[ParameterValues(Values = new string[] { 200, 403 })]
        public string PARAMETER_HTTP_RESPONSE { get; set; }
    }
}
