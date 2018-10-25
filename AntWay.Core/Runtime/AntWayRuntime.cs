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
using OptimaJet.Workflow.Core.Model;
using AntWay.Persistence.Provider.Model;
using Antway.Core.Persistence;
using Newtonsoft.Json;
using System.Linq.Expressions;
using AntWay.Core.Mapping;
using AntWay.Core.Activity;
using System.Text.RegularExpressions;
using AntWay.Core.Manager;
using static AntWay.Core.Manager.Checksum;

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


        public void SkipToState(Guid processId, string stateName)
        {
            WorkflowRuntime.SetState(processId, string.Empty, string.Empty,
                                     stateName,
                                     new Dictionary<string, object>());
        }

        //TODO: Review
        //Utilizado en el contexto de llamadas entre esquemas de workflows
        public void SetState(Guid processId,
                             string stateFrom, string activityFrom,
                             string stateTo, string activityTo)
        {
            var pi = GetProcessInstance(processId);
            SetState(pi, stateFrom, activityFrom, stateTo, activityTo);
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

        public void SetErrorState(ProcessInstance processInstance, string activityDescription)
        {
            WorkflowRuntime.SetErrorState(processInstance, activityDescription);
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
            bool isSCheme = pi.CurrentActivity.IsScheme;

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

        public List<ProcessHistoryItem> GetProcessHistory(ProcessInstance processInstance)
        {
            var result = WorkflowRuntime.PersistenceProvider
                        .GetProcessHistory(processInstance.ProcessId);

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

            pi.SetParameter("CalledFromScheme", fromProcessInstance.SchemeCode);
            pi.SetParameter("CalledFromProcessId", fromProcessInstance.ProcessId);
            WorkflowRuntime.PersistenceProvider.SavePersistenceParameters(pi);

            WorkflowClient.DataBaseScheme = databaseSchemeFromProcessInstance;

            return new AntWayResponseView
            {
                CodeResponse = AntWayResponseView.CODE_RESPONSE_OK,
                Value = pi.CurrentState
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


        public ProcessInstance GetProcessInstance(Guid processId)
        {
            var processInstance = WorkflowRuntime.GetProcessInstanceAndFillProcessParameters(processId);
            return processInstance;
        }


        public ManagerResponse ValidateModel(ProcessInstance processInstance)
        {
            IAntWayRuntimeActivity activityInstance = null;
            object activityModelInstance = null;

            //GET CHECKSUM VALUES FROM BINDINGMETHODS
            List<ActivityManager> activityManagerList = WorkflowClient.IActivityManager.GetActivitiesManager();
            foreach(ActivityManager am in activityManagerList)
            {
                activityInstance = Activator.CreateInstance(am.ClassActivityType)
                                             as IAntWayRuntimeActivity;

                activityModelInstance = Activator.CreateInstance(am.ClassActivityModelType);

                var idFromActivityClass = activityInstance.GetType().GetAttributeValue((ActivityAttribute a) => a.Id);
                activityInstance.ActivityId = idFromActivityClass;

                var activityExecutionPersisted = GetLastActivityExecution(processInstance, activityInstance.ActivityId);

                var errorResponse = new ManagerResponse
                                    {
                                        Success = false,
                                        ActivityId = activityExecutionPersisted.ActivityId,
                                        ActivityName = activityExecutionPersisted.ActivityName,
                                    };

                if (activityExecutionPersisted?.InputChecksum != null)
                {
                    string checksumInputPersisted = activityExecutionPersisted.InputChecksum;
                    string checksumInputBinded = GetChecksum(processInstance,
                                                             activityInstance,
                                                             activityModelInstance,
                                                             ChecksumType.Input);
                    if (checksumInputBinded != checksumInputPersisted) return errorResponse;
                }

                if (activityExecutionPersisted?.OutputChecksum != null)
                {
                    string checksumOutputPersisted = activityExecutionPersisted.OutputChecksum;
                    string checksumOutputBinded = GetChecksum(processInstance,
                                                              activityInstance,
                                                              activityModelInstance,
                                                              ChecksumType.Output);
                    if (checksumOutputBinded != checksumOutputPersisted) return errorResponse;
                }
            }

            return new ManagerResponse { Success = true  };
        }

        protected string GetChecksum(ProcessInstance processInstance,
                                     IAntWayRuntimeActivity activityInstance,
                                     object activityModelInstance,
                                     ChecksumType checksumType)
        {
            string cadTochecksumFromBindingMethods = "";
            var methods = MappingReflection
                          .GetActivityMethodAttributes(activityModelInstance, checksumType);

            foreach (string method in methods)
            {
                object methodResult = AntWayActivityActivator.RunMethod
                                    (method, processInstance.ProcessId.ToString(), activityInstance);

                cadTochecksumFromBindingMethods += methodResult.ToString();
            }
            string result = Checksum.Single.CalculateChecksum(cadTochecksumFromBindingMethods);

            return result;
        }

        public ActivityExecution GetLastActivityExecution(ProcessInstance processInstance, string activityId)
        {
            var result = WorkflowClient.AntWayRunTime
                        .GetActivityExecutionObject<List<ActivityExecution>>
                                (processInstance, activityId)
                        .OrderByDescending(ae => ae.EndTime)
                        .FirstOrDefault();

            return result;
        }

       
        #region "EvaluateExpression Functions for Conditions in designer"
        public bool EvaluateExpression<TA, TPO>(ProcessInstance processInstance,
                                 Expression<Func<TPO, bool>> condition,
                                 ChecksumType checksumType) where TA : IAntWayRuntimeActivity
        {
            Type type = typeof(TA);
            string activityId = GetActivityIdFromClassType(type);
            bool result = EvaluateExpression<TA, TPO>(processInstance, activityId, condition, checksumType);

            return result;
        }


        public bool EvaluateExpression<TA, TPO>(ProcessInstance processInstance, string activityId,
                                          Expression<Func<TPO, bool>> condition,
                                          ChecksumType checksumType) where TA : IAntWayRuntimeActivity
        {
            List<TPO> l = new List<TPO>();
            List<ActivityExecution> paramValues = AntWayRuntime.Single
                                                    .GetActivityExecutionObject<List<ActivityExecution>>
                                                            (processInstance, activityId);
            TPO parametersOut = JsonConvert.DeserializeObject<TPO>
                                    (paramValues.LastOrDefault().ParametersOutput.ToString());

            IAntWayRuntimeActivity activityInstance = WorkflowClient.AntWayRunTime
                                                       .GetActivityExecutionObject<List<TA>>
                                                            (processInstance, activityId)
                                                       .LastOrDefault();

            if (activityInstance == null) return false;

            TPO parametersBind = MappingReflection
                                .GetParametersBind(processInstance.ProcessId.ToString(),
                                                                 activityInstance, parametersOut,
                                                                  checksumType);
            l.Add(parametersBind);

            var f = from g in l.
                    Where(condition.Compile())
                    select g;
            f.ToList();

            bool result = f.Any();

            return result;
        }
        #endregion


        #region "Antway Timers"

        public string GetTransitionExpiredState(ProcessInstance processInstance)
        {
            TransitionDefinition td = GetTransitionExpired(processInstance);
            if (td == null) return null;

            var tExpiredTo = processInstance
                               .ProcessScheme
                               .Transitions
                               .FirstOrDefault(t => t.Trigger.Type == TriggerType.TimerExpired &&
                                                t.From.Name == td.From.Name);

            if (tExpiredTo?.To == null) return null;

            return tExpiredTo.To.State;
        }

        public TransitionDefinition GetTransitionExpired(ProcessInstance processInstance)
        {
            if (processInstance.CurrentActivity.IsFinal) return null;

            var timerIntervals = GetTimerIntervalTransitions(processInstance);

            foreach (var timerInterval in timerIntervals)
            {
                bool expired = CheckTransitionExpired(processInstance, timerInterval);
                if (expired) return timerInterval;
            }

            return null;
        }

        protected List<TransitionDefinition> GetTimerIntervalTransitions(ProcessInstance processInstance)
        {
            var result = processInstance
                        .ProcessScheme
                        .Transitions
                        .Where(t => t.Trigger.Type == TriggerType.Timer &&
                                    t.Classifier == TransitionClassifier.TimeInterval)
                        .ToList();

            return result;
        }

        protected bool CheckTransitionExpired(ProcessInstance processInstance,
                                            TransitionDefinition timeIntervalDefinition)
        {
            var processHistory = GetProcessHistory(processInstance);

            var activityFromExecuted = processHistory
                                       .LastOrDefault(ph => ph.FromActivityName == timeIntervalDefinition.From.Name);
            if (activityFromExecuted == null) return false;

            DateTime? transitionExpires = GetNextExecutionDateTime(activityFromExecuted.TransitionTime,
                                                                    timeIntervalDefinition.Trigger.Timer);
            if (transitionExpires == null) return false;

            var activityToExecuted = processHistory
                                 .LastOrDefault(ph => ph.ToActivityName == timeIntervalDefinition.To.Name);

            return activityToExecuted != null
                        ? (activityToExecuted.TransitionTime > transitionExpires)
                        : (DateTime.Now > transitionExpires);
        }

        private DateTime? GetNextExecutionDateTime(DateTime dateTimeFrom,
                                                  TimerDefinition timerDefinition)
        {
            var timerValueParameterName = GetTimerValueParameterName(timerDefinition);

            switch (timerDefinition.Type)
            {
                case TimerType.Date:
                    var date1 = DateTime.Parse(timerDefinition.Value, WorkflowRuntime.SchemeParsingCulture);
                    return date1.Date;

                case TimerType.DateAndTime:
                    var date2 = DateTime.Parse(timerDefinition.Value, WorkflowRuntime.SchemeParsingCulture);
                    return date2;

                case TimerType.Interval:
                    var interval = GetInterval(timerDefinition.Value);
                    if (interval <= 0)
                        return null;
                    return dateTimeFrom.AddMilliseconds(interval);

                case TimerType.Time:
                    var now = dateTimeFrom;
                    var date3 = DateTime.Parse(timerDefinition.Value, WorkflowRuntime.SchemeParsingCulture);
                    date3 = now.Date + date3.TimeOfDay;
                    if (date3.TimeOfDay < now.TimeOfDay)
                        date3 = date3.AddDays(1);
                    return date3;
            }

            return null;
        }

        private string GetTimerValueParameterName(TimerDefinition timerDefinition)
        {
            return GetTimerValueParameterName(timerDefinition.Name);
        }

        private string GetTimerValueParameterName(string timerName)
        {
            return string.Format("TimerValue_{0}", timerName);
        }

        private double GetInterval(string value)
        {
            int res;

            if (int.TryParse(value, out res))
                return res;

            var daysRegex = @"\d*\s*((days)|(day)|(d))(\W|\d|$)";
            var hoursRegex = @"\d*\s*((hours)|(hour)|(h))(\W|\d|$)";
            var minutesRegex = @"\d*\s*((minutes)|(minute)|(m))(\W|\d|$)";
            var secondsRegex = @"\d*\s*((seconds)|(second)|(s))(\W|\d|$)";
            var millisecondsRegex = @"\d*\s*((milliseconds)|(millisecond)|(ms))(\W|\d|$)";

            return new TimeSpan(GetTotal(value, daysRegex), GetTotal(value, hoursRegex), GetTotal(value, minutesRegex), GetTotal(value, secondsRegex),
                GetTotal(value, millisecondsRegex)).TotalMilliseconds;
        }

        private static int GetTotal(string s, string regex)
        {
            int total = 0;
            foreach (var match in Regex.Matches(s, regex, RegexOptions.IgnoreCase))
            {
                int parsed;
                var value = Regex.Match(match.ToString(), @"\d*").Value;
                int.TryParse(value, out parsed);
                total += parsed;
            }

            return total;
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
