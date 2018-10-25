using AntWay.Core.Activity;
using AntWay.Core.Mapping;
using AntWay.Core.Model;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;
using System;


namespace Sample.Model.Expedientes.Activities
{
    [Activity(Id = "bf174ffb-463b-72a4-0c9b-4e8a4ae499e7")]
    public class ActivityExpedienteNew : AntWayActivityRuntimeBase, IAntWayRuntimeActivity
    {
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

            result.ParametersOutput = new ActivityExpedienteNewModel
            {
                Id = System.Guid.NewGuid().ToString(),
                //FechaCreacionExpediente = DateTime.Now,
                FechaCreacionExpediente = new DateTime(2018, 10, 1)
            };

            result.ExecutionSuccess = true;

            PersistActivityExecution<ActivityExpedienteNewModel>(result, pi, runtime);

            return result;
        }

        public object BindingMethod_FechaCreacionExpediente(object[] parameters)
        {
            string processId = parameters[0].ToString();

            var resultFromPersistence = DateTime.Now.AddYears(-1);
            //var resultFromPersistence = new DateTime(2018, 10, 1);
            if (parameters.Length == 1) return resultFromPersistence;

            var result = (ActivityExpedienteNewModel)parameters[1];
            result.FechaCreacionExpediente = resultFromPersistence;

            return result;
        }

        public object BindingMethod_Canceled(object[] parameters)
        {
            string processId = parameters[0].ToString();

            bool resultFromPersistence = false;
            if (parameters.Length == 1) return resultFromPersistence;

            var result = (ActivityExpedienteNewModel)parameters[1];
            result.Canceled = resultFromPersistence;

            return result;
        }
    }


    public class ActivityExpedienteNewModel
    {
        //[ParameterBinding(OutputChecksum = true)]
        public string Id { get; set; }

        [ParameterBinding(OutputBindMethod = nameof(ActivityExpedienteNew.BindingMethod_Canceled),
                          OutputChecksum = true)]
        public bool Canceled { get; set; }

        [ParameterBinding(OutputBindMethod = nameof(ActivityExpedienteNew.BindingMethod_FechaCreacionExpediente),
                          OutputChecksum = true)]
        public DateTime FechaCreacionExpediente { get; set; }

        public string Descripcion { get; set; }
    }
}
