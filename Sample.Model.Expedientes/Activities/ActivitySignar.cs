using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntWay.Core.Activity;
using AntWay.Core.Mapping;
using AntWay.Core.Model;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;

namespace Sample.Model.Expedientes
{
    [Activity(Id = "1defd107-436b-a7d1-dda4-26732d901ff0")]
    public class ActivitySignar : AntWayActivityRuntimeBase, IAntWayRuntimeActivity
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

            //result.ParametersIn = new ActivitySignarParametersInput
            //{
            //    PARAMETER_SIGNATURA = ActivitySignarParametersInput.PARAMETER_SIGNATURA_ENVIAT_A_SIGNAR
            //};

            //result.ParametersOut = new ActivitySignarParametersOutput
            //{
            //    PARAMETER_SIGNATURA = ActivitySignarParametersOutput.PARAMETER_SIGNATURA_SIGNAT
            //};

            result.ExecutionSuccess = true;

            PersistActivityExecution<ActivitySignar>(result, pi, runtime);

            return result;
        }
    }


    //public class ActivitySignarParametersInput
    //{
    //    public const string PARAMETER_SIGNATURA_ENVIAT_A_SIGNAR =
    //                            ExpedientesWorkflowParameterNames.ESTAT_SIGNATURA_ENVIAT_A_SIGNAR;

    //    //[ParameterValues(Values = new string[]
    //    //                    { PARAMETER_SIGNATURA_ENVIAT_A_SIGNAR })]
    //    public string PARAMETER_SIGNATURA { get; set; }
    //}

    //public class ActivitySignarParametersOutput
    //{
    //    public const string PARAMETER_SIGNATURA_SIGNAT =
    //                            ExpedientesWorkflowParameterNames.ESTAT_SIGNATURA_SIGNAT;

    //    //[ParameterValues(Values = new string[] 
    //    //                    { PARAMETER_SIGNATURA_SIGNAT })]
    //    public string PARAMETER_SIGNATURA { get; set; }
    //}
}
