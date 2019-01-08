using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Antway.Core.Persistence;
using AntWay.Core.Manager;
using AntWay.Core.Mapping;
using AntWay.Core.Model;
using AntWay.Core.Runtime;
using AntWay.Persistence.Provider.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;
using static AntWay.Core.Manager.Checksum;

namespace AntWay.Core.Activity
{
    public abstract class AntWayActivityRuntimeBase
    {
        public const string InvokedMethodInputBindingName = "InvokedMethodInputBinding";
        public const string InvokedMethodOutputBindingName = "InvokedMethodOutputBinding";

        public string ActivityId { get; set; }
        public string ActivityName { get; set; }

        protected ActivityExecution ActivityExecution;

        public async Task<ActivityExecution> RunAsync(ProcessInstance pi,
                                                              WorkflowRuntime runtime,
                                                              object[] parameters = null)
        {
            var result = await Task.Run(() => Run(pi, runtime, parameters));
            return result;
        }

        public ActivityExecution Run(ProcessInstance pi,
                                             WorkflowRuntime runtime,
                                             object[] parameters = null)
        {
            ActivityExecution = new ActivityExecution
            {
                ActivityId = ActivityId,
                ActivityName = ActivityName,
                ProcessId = pi.ProcessId,
            };

            ActivityExecution.ExecutionSuccess = true;

            //STUFF TO OVERRIDE
            string locator = GetLocalizadorFromProcessId(pi.ProcessId);
            try
            {
                RunImplementation(locator, pi.ProcessId);
            }
            catch(Exception ex)
            {
                ActivityExecution.ExecutionSuccess = false;
            }
            //

            PersistActivityExecution(ActivityExecution, pi, runtime);

            return ActivityExecution;
        }

        public virtual void RunImplementation(string locator, Guid processId)
        {
            return;
        }


        public object InvokedMethodInputBinding(object[] parameters)
        {
            return InputBinding(GetLocalizadorFromProcessId((Guid)parameters[0]), (Guid)parameters[0]);
        }

        public object InvokedMethodOutputBinding(object[] parameters)
        {
            return OutputBinding(GetLocalizadorFromProcessId((Guid)parameters[0]), (Guid)parameters[0]);
        }

        public virtual object InputBinding(string locator, Guid processId)
        {
            return null;
        }


        public virtual object OutputBinding(string locator, Guid processId)
        {
            return null;
        }


        public string GetLocalizadorFromProcessId(Guid processId)
        {
            var locatorPersistence = new LocatorPersistence
            { IDALocators = PersistenceObjectsFactory.GetIDALLocatorsObject() };
            var wfInstance = locatorPersistence.GetWorkflowLocatorFromGuid(processId);

            return wfInstance.LocatorValue;
        }

        protected virtual void PersistActivityExecution
                 (ActivityExecution activityExecution, ProcessInstance pi, WorkflowRuntime runtime)
        {
            activityExecution.EndTime = DateTime.Now;

            activityExecution.ParametersInput = activityExecution.ParametersInput;
            activityExecution.ParametersOutput = activityExecution.ParametersOutput;

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
                var paramsInput = activityExecution.ParametersInput;
                activityExecution.InputChecksum = GetChecksum(paramsInput, ChecksumType.Input);
            }

            if (activityExecution.ParametersOutput != null)
            {
                var paramsOutput = activityExecution.ParametersOutput;
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

    public static class AntWayActivityExtensions
    {
        public static TO GetActivityParametersOutput<TO>
                        (this AntWayActivityRuntimeBase activityClass,
                         ProcessInstance processInstance)
        {
            string idFromActivityClass = activityClass.GetType()
                                          .GetAttributeValue((ActivityAttribute a) => a.Id);

            var activityExecutionPersisted = WorkflowClient.AntWayRunTime
                                             .GetLastActivityExecution(processInstance,
                                                                       idFromActivityClass);

            TO persistedResult = JsonConvert.DeserializeObject<TO>
                                  (activityExecutionPersisted.ParametersOutput.ToString());

            return persistedResult;
        }

        public static TI GetActivityParametersInput<TI>
                (this AntWayActivityRuntimeBase activityClass,
                 ProcessInstance processInstance)
        {
            string idFromActivityClass = activityClass.GetType()
                                          .GetAttributeValue((ActivityAttribute a) => a.Id);

            var activityExecutionPersisted = WorkflowClient.AntWayRunTime
                                             .GetLastActivityExecution(processInstance,
                                                                       idFromActivityClass);

            TI persistedResult = JsonConvert.DeserializeObject<TI>
                                  (activityExecutionPersisted.ParametersInput.ToString());

            return persistedResult;
        }



        public static List<string> DifferenceBetweenPersistedAndBindedObject<TA, TM>
                (this TA activity, TM activityModel,
                 Guid processId,
                 string jsonPersisted, ChecksumType checksumType)
            where TA : IAntWayRuntimeActivity
            where TM: IActivityModel
        {
            List<string> result = new List<string>();

            try
            {
                string locator = activity.GetLocalizadorFromProcessId(processId);

                var bindedObject = (checksumType == ChecksumType.Input)
                                        ? activity.InputBinding(locator, processId)
                                        : activity.OutputBinding(locator, processId);

                object persistedObject = activityModel.DeserializePersistedObject(jsonPersisted);

                var differences = AntWayActivityActivator
                                  .ObjectsDifference(persistedObject, bindedObject);

                if (differences.Any())
                {
                    result.Add($"Checksum Error at {activity.ActivityId}");
                    result.AddRange(differences);
                }
            }
            catch (Exception ex)
            {
                result.Add($"Antway checksum error in DifferenceBetweenPersistedAndBindedObject" +
                           $"{activity.ActivityId}: {ex.Message}");
            }

            return result;
        }
    }
}

