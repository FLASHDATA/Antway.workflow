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
using Sample.Model.Expedientes.AntWayBinding;

namespace Sample.Model.Expedientes
{
    [Activity(Id = "02c97ede-8f9e-40ca-7865-f4aa680fb3bb")]
    public class ActivityEnviarASignar: AntWayActivityRuntimeBase, IAntWayRuntimeActivity
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

                //result.ParametersIn = new ActivityEnviarASignarParametersOutput
                //{
                //    PARAMETER_SIGNATURA = ActivityEnviarASignarParametersOutput.PARAMETER_SIGNATURA_ENVIAT_A_SIGNAR
                //};

                //result.ParametersOut = new ActivityEnviarASignarParametersOutput
                //{
                //    PARAMETER_SIGNATURA = ActivityEnviarASignarParametersOutput.PARAMETER_SIGNATURA_ENVIAT_A_SIGNAR
                //};

            result.ExecutionSuccess = true;

            PersistActivityExecution<ActivityEnviarASignar>(result, pi, runtime);

            return result;
        }

        public object BindingMethod_ParamaterSignatura(object[] parameters)
        {
            string processId = parameters[0].ToString();
            var result = (ActivityEnviarASignarParametersOutput) parameters[1];

            //Recogemos Parameter PARAMETER_SIGNATURA_SIGNAT de Base de datos
            //Para simplificar simulado en una lista en memoria
            result.PARAMETER_SIGNATURA = GetParameterSignaturaSignat(processId);

            return result;
        }


        //SIMULACIÓN BINDING CON BD
        //Lista en memoria (en lugar de acceder a un DAL)
        private static List<KeyValuePair<string, string>> ParameterSignaturaFromDB 
                            = new List<KeyValuePair<string, string>>();

        //Get de la lista
        public static string GetParameterSignaturaSignat(string idActivity)
        {
            var entity = ParameterSignaturaFromDB.FirstOrDefault(p => p.Key == idActivity);

            return entity.Value ?? ActivityEnviarASignarParametersOutput.PARAMETER_SIGNATURA_ENVIAT_A_SIGNAR;
        }

        //Set en la lista
        public static void SetParameterSignaturaSignat(string idActivity)
        {
            ParameterSignaturaFromDB
                   .RemoveAll(p => p.Key == idActivity);

                ParameterSignaturaFromDB
                    .Add(new KeyValuePair<string, string>
                         (idActivity, ActivityEnviarASignarParametersOutput.PARAMETER_SIGNATURA_SIGNAT)
                        );
        }

    }


    public class ActivityEnviarASignarParametersInput
    {
        public const string PARAMETER_SIGNATURA_ENVIAT_A_SIGNAR = 
                                ExpedientesWorkflowParameterNames.ESTAT_SIGNATURA_ENVIAT_A_SIGNAR;

        [ParameterValues(Values = new string[]
                            { PARAMETER_SIGNATURA_ENVIAT_A_SIGNAR })]
        public string PARAMETER_SIGNATURA { get; set; }
    }

    public class ActivityEnviarASignarParametersOutput
    {
        public const string PARAMETER_SIGNATURA_ENVIAT_A_SIGNAR =
                               ExpedientesWorkflowParameterNames.ESTAT_SIGNATURA_ENVIAT_A_SIGNAR;
        public const string PARAMETER_SIGNATURA_SIGNAT =
                               ExpedientesWorkflowParameterNames.ESTAT_SIGNATURA_SIGNAT;

        [ParameterBinding(IOBindMethod = nameof(ActivityEnviarASignar.BindingMethod_ParamaterSignatura))]
        public string PARAMETER_SIGNATURA { get; set; }
    }
}
