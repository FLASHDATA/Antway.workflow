﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AntWay.Core.Manager;
using AntWay.Core.Mapping;
using AntWay.Core.Model;
using Newtonsoft.Json;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;
using static AntWay.Core.Manager.Checksum;

namespace AntWay.Core.Activity
{
    public abstract class AntWayActivityRuntimeBase
    {
        public string ActivityId { get; set; }
        public string ActivityName { get; set; }

        public virtual List<string> DifferenceBetweenPersistedAndBindedObject(Guid processId,
                                                                    string jsonPersisted,
                                                                    ChecksumType checksumType)
        {
            return new List<string> { "Diferencia sin especificar" };
        }

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

        protected virtual void PersistActivityExecution<TActivityModel>
                 (ActivityExecution activityExecution, ProcessInstance pi, WorkflowRuntime runtime)
        {
            activityExecution.EndTime = DateTime.Now;
            var parameterToStore = new List<ActivityExecution>() { activityExecution };

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

            if (activityExecution.ParametersInput != null)
            {
                TActivityModel paramsInput = (TActivityModel)activityExecution.ParametersInput;
                activityExecution.InputChecksum = GetChecksum(paramsInput, ChecksumType.Input);
            }

            if (activityExecution.ParametersOutput != null)
            {
                TActivityModel paramsOutput = (TActivityModel)activityExecution.ParametersOutput;
                activityExecution.OutputChecksum = GetChecksum(paramsOutput, ChecksumType.Output);
            }

            //Guardar en BD.
            pi.SetParameter(activityExecution.ActivityId,
                            parameterToStore,
                            ParameterPurpose.Persistence);


            runtime.PersistenceProvider.SavePersistenceParameters(pi);
        }

        protected string GetChecksum<TActivityModel>(TActivityModel activityModel,
                                                     ChecksumType checksumType)
        {
            if (activityModel == null) return null;

            string cadToChecksum = JsonConvert.SerializeObject(activityModel);

            string result = Checksum.Single.CalculateChecksum(cadToChecksum);
            return result;
        }

        public string GetChecksumFromBindingMethods<TActivityModel>(TActivityModel activityModel)
        {
            if (activityModel == null) return null;
            string cadToChecksum = "";

            string result = Checksum.Single.CalculateChecksum(cadToChecksum);
            return result;
        }

    }
}

