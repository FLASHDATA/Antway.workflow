using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;

namespace AntWay.Core.Model
{
    public abstract class AntWayActivityRuntimeBase
    {
        public string ActivityId { get; set; }
        public string ActivityName { get; set; }
        

        public virtual async Task<ActivityExecution> RunAsync(ProcessInstance pi,
                                                              WorkflowRuntime runtime,
                                                              object[] parameters = null)
        {
            var result = await Task.Run(() => Run(pi, runtime, parameters));
            return result;
        }

        public virtual ActivityExecution Run(ProcessInstance pi,
                                             WorkflowRuntime runtime,
                                             object[] parameters = null)
        {
            throw new NotImplementedException();
        }

        protected virtual void PersistActivityExecution(ActivityExecution activityExecution,
                                                     ProcessInstance pi,
                                                     WorkflowRuntime runtime)
        {
            activityExecution.EndTime = DateTime.Now;
            var parameterToStore = new List<ActivityExecution> { activityExecution };

            //Recogemos de BD objeto actual, para acumularlo.
            var jsonString = pi.ProcessParameters
                            .FirstOrDefault(p => p.Purpose == ParameterPurpose.Persistence &&
                                            p.Name == activityExecution.ActivityId)
                            ?.Value
                            .ToString();
            if (jsonString != null)
            {
                var parameterHistory = JsonConvert.DeserializeObject<List<ActivityExecution>>(jsonString);
                if (parameterHistory.Any())
                {
                    parameterToStore.AddRange(parameterHistory);
                }
            }

            //Guardar en BD.
            pi.SetParameter(activityExecution.ActivityId,
                            parameterToStore,
                            ParameterPurpose.Persistence);

            pi.SetParameter($"{activityExecution.ActivityId}/{AntWayProcessParameters.ACTIVITY_EXECUTION_SUCCEED}",
                            activityExecution.ExecutionSuccess,
                            ParameterPurpose.Persistence);

            runtime.PersistenceProvider.SavePersistenceParameters(pi);
        }

        public virtual ActivityExecution GetLastActivityExecution(string activityId,
                                                                  ProcessInstance pi)
        {
            var jsonString = pi.ProcessParameters
                           .FirstOrDefault(p => p.Purpose == ParameterPurpose.Persistence &&
                                           p.Name == activityId)
                           ?.Value
                           .ToString();

            if (jsonString == null) { return null;  }

            List<ActivityExecution> activityExecutions = JsonConvert
                                                         .DeserializeObject<List<ActivityExecution>>(jsonString);

            var lastExecuted = activityExecutions
                               .OrderByDescending(ae => ae.EndTime)
                               .FirstOrDefault();

            return lastExecuted;
        }
    }
}

