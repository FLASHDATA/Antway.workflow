using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow;
using System.Collections.Specialized;
using System.IO;
using AntWay.Core.Model;
using Antway.Core;
using OptimaJet.Workflow.Core.Model;
using AntWay.Persistence.Provider.Model;
using Antway.Core.Persistence;
using Newtonsoft.Json;
using AntWay.Core.Scheme;
using System.Linq.Expressions;
using AntWay.Core.Mapping;

namespace AntWay.Core.Runtime
{
    public class AntWayRuntime
    {
        public static AntWayRuntime Single => new AntWayRuntime();
        protected WorkflowRuntime WorkflowRuntime { get; set; }
        internal static WorkflowSchemeView Scheme { get; set; }

        public AntWayRuntime() { }

        internal AntWayRuntime(WorkflowRuntime workflowRuntime)
        {
            WorkflowRuntime = workflowRuntime;
        }

        internal AntWayRuntime(WorkflowRuntime workflowRuntime,
                             WorkflowSchemeView scheme)
        {
            WorkflowRuntime = workflowRuntime;
            Scheme = scheme;
        }

        public Guid CreateInstance(string schemeCode)
        {
            var processId = Guid.NewGuid();
            WorkflowRuntime.CreateInstance(schemeCode, processId);

            return processId;
        }


        public Guid CreateInstanceAndPersist(ProcessPersistenceView wfScheme)
        {
            var processId = Guid.NewGuid();

            WorkflowRuntime.CreateInstance(wfScheme.SchemeCode, processId);

            var locatorPersistence = new LocatorPersistence
            {
                IDALocators = PersistenceObjectsFactory.GetIDALLocatorsObject()
            };
            wfScheme.WFProcessGuid = processId;
            wfScheme.SchemeCode = wfScheme.SchemeCode;

            locatorPersistence.AddWorkflowLocator(wfScheme);

            return processId;
        }


        public string DesignerAPI(NameValueCollection pars, Stream filestream = null)
        {
            string result = WorkflowRuntime.DesignerAPI(pars, filestream, true);
            return result;
        }

        public void SetState(Guid processId,
                             string stateFrom, string activityFrom,
                             string stateTo, string activityTo)
        {
            var pi = GetProcessInstance(processId);
            SetState(pi.ProcessInstance, stateFrom, activityFrom, stateTo, activityTo);
        }


        public void SetState(ProcessInstance processInstance, 
                             string stateFrom, string activityFrom,
                             string stateTo, string activityTo)
        {
            ActivityDefinition adFrom = new ActivityDefinition
            {
                State = stateFrom,
                Name = activityFrom
            };

            ActivityDefinition adTo = new ActivityDefinition
            {
                State = stateTo,
                Name = activityTo
            };

            WorkflowRuntime.PersistenceProvider
            .UpdatePersistenceState(processInstance,
                                    TransitionDefinition.Create(adFrom, adTo));
        }

        public void SavePersistenceParameter(ProcessInstance processInstance)
        {
            WorkflowRuntime.PersistenceProvider.SavePersistenceParameters(processInstance);
        }

        public List<WorkflowCommand> GetAvailableCommands(Guid processId, string identityId = null)
        {
            var commands = WorkflowRuntime.GetAvailableCommands(processId, identityId ?? string.Empty);

            var result = commands
                         .ToList();

            return result;
        }

        public string GetCurrentStateName(Guid processId)
        {
            return WorkflowRuntime.GetCurrentStateName(processId);
        }

        public string GetCurrentActivityName(Guid processId)
        {
            return WorkflowRuntime.GetCurrentActivityName(processId);
        }

        public Task<bool> ExecuteTriggeredConditionalTransitions(ProcessInstance processInstance,
                                                                 List<TransitionDefinition> transtions)
        {
            var conditionalTransitions = transtions.Where(t => t.IsConditionTransition).ToList();
            if (!conditionalTransitions.Any()) return new Task<bool>(() => false);
            return WorkflowRuntime.ExecuteTriggeredTransitions(processInstance, conditionalTransitions);
        }

        public bool ExecuteCommandSiguente(Guid processId, string identifyId = null)
        {
            WorkflowCommand command = WorkflowClient.AntWayRunTime
                                       .GetAvailableCommands(processId, identifyId ?? string.Empty)
                                       .FirstOrDefault(c => c.CommandName.Trim().ToLower() == DefaultSchemeCommandNames.Single.Siguiente);

            if (command == null) return false;

            return ExecuteCommand(processId, command.CommandName, identifyId);
        }

        public bool ExecuteCommand(Guid processId, string commandName,
                                    string identifyId = null)
        {
            var command = WorkflowClient.AntWayRunTime
                          .GetAvailableCommands(processId, identifyId)
                          .Where(c => c.CommandName.Trim().ToLower() == commandName.ToLower())
                          .FirstOrDefault();

            return ExecuteCommand(command, identifyId);
        }

        public bool ExecuteCommand(WorkflowCommand command,
                                   string identifyId = null)
        {
            var cmdExecResult = WorkflowRuntime.ExecuteCommand(command,
                                        identifyId ?? string.Empty,
                                        string.Empty);

            return cmdExecResult.WasExecuted;
        }

        public bool WorkflowCommandAvailable(Guid processId, string identityId = null)
        {
            var pi = WorkflowClient.AntWayRunTime.GetProcessInstance(processId);
            bool isSCheme = pi.ProcessInstance.CurrentActivity.IsScheme;

            if (!isSCheme) return false;

            bool commandNextAvailable =  WorkflowRuntime
                                        .GetAvailableCommands(processId, identityId ?? string.Empty)
                                        .Any(c => c.CommandName.ToLower() == "next");

            return commandNextAvailable;
        }

        public AntWayResponseView CallWorkflowCommand(Guid processId,
                                                       List<CommandParameter> commandParameters,
                                                       string identityId = null)
        {
            var cmdCallWorkFlow = WorkflowRuntime
                             .GetAvailableCommands(processId, identityId ?? string.Empty)
                             .FirstOrDefault(c => c.CommandName.ToLower() == "next");

            string fromActivity = WorkflowClient.AntWayRunTime
                                 .GetCurrentActivityName(processId);

            string schemeName = fromActivity;
            string fromState = WorkflowClient.AntWayRunTime
                                  .GetCurrentStateName(processId);

            WorkflowClient.AntWayRunTime.ExecuteCommand(cmdCallWorkFlow);

            string toActivity = WorkflowClient.AntWayRunTime
                               .GetCurrentActivityName(processId);

            var result = CallWorkflow(schemeName, processId, commandParameters);

            if (result.Success)
            {
                WorkflowClient.AntWayRunTime
                              .SetState(processId,
                                          fromState, schemeName,
                                          result.Value.ToString(),
                                          toActivity);
            }

            return result;
        }

        public List<ActivityDefinition> GetActivities(ProcessInstance processInstance)
        {
            var result = new List<ActivityDefinition>();

            result.AddRange(
                    processInstance
                    .ProcessScheme
                    .Activities
                    .Where(a => a.Id != null)
                    .ToList()
            );

            return result;
        }

        public T GetActivityExecutionObject<T>(ProcessInstance processInstance, string activityId)
        {
            var jsonString = GetActivityExecutionJsonObject(processInstance, activityId);

            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        public string GetActivityExecutionJsonObject(ProcessInstance processInstance, string activityId)
        {
            var jsonString = processInstance.GetParameter(activityId)?.Value.ToString();

            return jsonString;
        }
        
        internal AntWayResponseView CallWorkflow(string schemeName,
                                        Guid? fromProcessGuid,
                                        List<CommandParameter> commandParameters)
        {
            var schemesPersistence = new SchemesPersistence
            {
                IDALSchemes = PersistenceObjectsFactory.GetIDALWFSchemaObject()
            };

            var scheme = schemesPersistence.GetScheme(schemeName);
            if (scheme == null)
            {
                return new AntWayResponseView
                {
                    CodeResponse = AntWayResponseView.CODE_RESPONSE_ERROR,
                    Description = $"Scheme {schemeName} does not exist"
                };
            }

            if (!scheme.WorkflowService)
            {
                return new AntWayResponseView
                {
                    CodeResponse = AntWayResponseView.CODE_RESPONSE_ERROR,
                    Description = $"Scheme {schemeName} is not a service"
                };
            }

            var fromProcessInstance = fromProcessGuid != null
                                        ? WorkflowClient.AntWayRunTime
                                          .GetProcessInstance(fromProcessGuid.Value)
                                        : null;

            string databaseSchemeFromProcessInstance = WorkflowClient.DataBaseScheme;
            WorkflowClient.DataBaseScheme = scheme.DBSchemeName;

            var guid = WorkflowClient.AntWayRunTime.CreateInstance(schemeName);

            WorkflowCommand command = WorkflowClient.AntWayRunTime
                             .GetAvailableCommands(guid, string.Empty)
                             .FirstOrDefault();

            if (command == null)
            {
                WorkflowClient.DataBaseScheme = databaseSchemeFromProcessInstance;
                return new AntWayResponseView
                {
                    CodeResponse = AntWayResponseView.CODE_RESPONSE_ERROR,
                    Description = $"Not command found in scheme {schemeName}"
                };
            }

            //Comprobar si cumple con los parámetros requeridos
            bool parametersMatch = ParametersMatchTheCommand(commandParameters, command);

            //Si no los cumple...
            if (!parametersMatch)
            {
                WorkflowClient.DataBaseScheme = databaseSchemeFromProcessInstance;

                return new AntWayResponseView
                {
                    CodeResponse = AntWayResponseView.CODE_RESPONSE_ERROR,
                    Description = $"Parameters do not match {schemeName}. Check DescriptionDetail.",
                    DescriptionDetail = GetCommandParameters(schemeName)
                                        .Select(cp => $"Parameter {cp.ParameterName} of type {cp.Type.Name} is required {cp.IsRequired.ToString()}")
                                        .ToList()
                };
            }

            foreach (var c in commandParameters)
            {
                command.SetParameter(c.ParameterName, c.Value);
            }

            WorkflowClient.AntWayRunTime.ExecuteCommand(command);

            var pi = WorkflowClient.AntWayRunTime
                        .GetProcessInstance(guid);

            pi.ProcessInstance
               .SetParameter("CalledFromScheme", fromProcessInstance.ProcessInstance.SchemeCode);
            pi.ProcessInstance
               .SetParameter("CalledFromProcessId", fromProcessInstance.ProcessInstance.ProcessId);
            WorkflowRuntime.PersistenceProvider.SavePersistenceParameters(pi.ProcessInstance);

            WorkflowClient.DataBaseScheme = databaseSchemeFromProcessInstance;

            return new AntWayResponseView
            {
                CodeResponse = AntWayResponseView.CODE_RESPONSE_OK,
                Value = pi.ProcessInstance.CurrentState
            };
        }


        internal List<CommandParameter> GetCommandParameters(string schemeName)
        {
            var schemesPersistence = new SchemesPersistence
            {
                IDALSchemes = PersistenceObjectsFactory.GetIDALWFSchemaObject()
            };

            var scheme = schemesPersistence.GetScheme(schemeName);
            if (scheme == null)
            {
                throw new NotImplementedException("Esquema inexistente");
            }

            if (!scheme.WorkflowService)
            {
                throw new NotImplementedException("Esquema no marcado como servicio");
            }

            WorkflowClient.DataBaseScheme = scheme.DBSchemeName;
            var guid = WorkflowClient.AntWayRunTime.CreateInstance(schemeName);

            WorkflowCommand command = WorkflowClient.AntWayRunTime
                             .GetAvailableCommands(guid, string.Empty)
                             .FirstOrDefault();

            var result = command
                            .Parameters
                            .Select(p => AntWayCommandParameter.Single
                                        .NewCommandParameter(p.ParameterName, p.Value
                                                            , p.IsRequired))
                            .ToList();

            return result;
        }

        public bool ParametersMatchTheCommand
                                    (List<CommandParameter> sendedParameters,
                                     WorkflowCommand commandToExecute)
        {
            var requiredParameters = commandToExecute
                                    .Parameters
                                    .Where(c => c.IsRequired)
                                    .ToList();

            foreach (var rp in requiredParameters)
            {
                bool match = sendedParameters
                             .Any(sp => sp.ParameterName == rp.ParameterName);

                if (!match) return false;
            }

            return true;
        }


        public AntWayProcessInstance GetProcessInstance(Guid processId)
        {
            var processInstance = WorkflowRuntime.GetProcessInstanceAndFillProcessParameters(processId);
            return new AntWayProcessInstance(processInstance);
        }


        #region "EvaluateExpression Functions for Conditions in designer"
        public bool EvaluateExpression<TA, TPO>(ProcessInstance processInstance,
                                 Expression<Func<TPO, bool>> condition) where TA : IAntWayRuntimeActivity
        {
            Type type = typeof(TA);
            string activityId = GetActivityIdFromClassType(type);
            bool result = EvaluateExpression<TA, TPO>(processInstance, activityId, condition);

            return result;
        }


        public bool EvaluateExpression<TA, TPO>(ProcessInstance processInstance, string activityId,
                                          Expression<Func<TPO, bool>> condition) where TA : IAntWayRuntimeActivity
        {
            List<TPO> l = new List<TPO>();
            List<ActivityExecution> paramValues = AntWayRuntime.Single
                                                    .GetActivityExecutionObject<List<ActivityExecution>>
                                                            (processInstance, activityId);
            TPO parametersOut = JsonConvert.DeserializeObject<TPO>
                                    (paramValues.LastOrDefault().ParametersOut.ToString());

            IAntWayRuntimeActivity activityInstance = WorkflowClient.AntWayRunTime
                                                       .GetActivityExecutionObject<List<TA>>
                                                            (processInstance, activityId)
                                                       .LastOrDefault();

            if (activityInstance == null) return false;

            activityInstance.ParametersBind = MappingReflection
                                              .GetParametersBind(processInstance.ProcessId.ToString(),
                                                                 activityInstance, parametersOut);
            l.Add((TPO)activityInstance.ParametersBind);

            var f = from g in l.
                    Where(condition.Compile())
                    select g;
            f.ToList();

            bool result = f.Any();

            return result;
        }
        #endregion

        protected string GetActivityIdFromClassType(Type type)
        {
            Type myType1 = Type.GetType($"{type.FullName}, {type.Assembly.ManifestModule.Name.Replace(".dll", "")}");
            if (myType1 == null) return null;

            var result = myType1.GetAttributeValue((ActivityAttribute a) => a.Id);

            return result;
        }
    }
}
