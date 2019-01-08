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
        public override void RunImplementation(string locator, Guid processId)
        {
            var url = locator;

            try
            {
                var response = new HttpClient().GetAsync(url).Result;

                ActivityExecution.ParametersOutput = new ActivityServiceModel()
                {
                    PARAMETER_HTTP_RESPONSE = "200"
                };
            }
            catch (Exception ex)
            {
                ActivityExecution.ParametersOutput = new ActivityServiceModel()
                {
                    PARAMETER_HTTP_RESPONSE = "403"
                };
                ActivityExecution.ExecutionSuccess = false;
            }
        }
    }

    public class ActivityServiceModel
    {
        public string URL { get; set; }

        public string PARAMETER_HTTP_RESPONSE { get; set; }
    }
}
