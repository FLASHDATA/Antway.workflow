using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using OptimaJet.Workflow.Core.CodeActions;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.License;
using OptimaJet.Workflow.Core.Runtime;

namespace OptimaJet.Workflow.Core.Model
{
 
    /// <summary>
    /// Represent a instance of a specific process 
    /// </summary>
    public class ProcessInstance
    {
        private ProcessInstance()
        { }

        #region System Process Parameters

        /// <summary>
        /// Returns Id of the process
        /// </summary>
        public Guid ProcessId
        {
            get { return GetParameter<Guid>(DefaultDefinitions.ParameterProcessId.Name); }
            set { AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterProcessId, value)); }
        }

        /// <summary>
        /// Returns Id of the parent process if this process is subprocess (parallel branch)
        /// </summary>
        public Guid? ParentProcessId
        {
            get { return GetParameter<Guid?>(DefaultDefinitions.ParameterParentProcessId.Name); }
            set { AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterParentProcessId, value)); }
        }

        /// <summary>
        /// Returns Id of the root process in the subprocesses hierarchy if this process is subprocess (parallel branch)
        /// </summary>
        public Guid RootProcessId
        {
            get { return GetParameter<Guid>(DefaultDefinitions.ParameterRootProcessId.Name); }
            set { AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterRootProcessId, value)); }
        }

        /// <summary>
        /// Returns Id of the scheme of the process
        /// </summary>
        public Guid SchemeId
        {
            get { return GetParameter<Guid>(DefaultDefinitions.ParameterSchemeId.Name); }
            set { AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterSchemeId, value)); }
        }

        /// <summary>
        /// Returns the name of the state  which was initial for last executed direct transition <see cref="TransitionDefinition.From"/>
        /// </summary>
        public string PreviousState
        {
            get { return GetParameter<string>(DefaultDefinitions.ParameterPreviousState.Name); }
            set { AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterPreviousState, value)); }
        }

        /// <summary>
        /// Returns the name of the currently executing timer, filled if the transition process was initiated by a timer 
        /// </summary>
        public string ExecutedTimer
        {
            get { return GetParameter<string>(DefaultDefinitions.ParameterExecutedTimer.Name); }
            set { AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterExecutedTimer, value)); }
        }

        /// <summary>
        /// Returns the name of the state  which was initial for last executed transition  marked as direct <see cref="TransitionDefinition.From"/>
        /// </summary>
        public string PreviousStateForDirect
        {
            get { return GetParameter<string>(DefaultDefinitions.ParameterPreviousStateForDirect.Name); }
            set
            {
                AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterPreviousStateForDirect, value));
            }
        }

        /// <summary>
        /// Returns the name of the state  which was initial for last executed transition marked as reverse <see cref="TransitionDefinition.From"/>
        /// </summary>
        public string PreviousStateForReverse
        {
            get { return GetParameter<string>(DefaultDefinitions.ParameterPreviousStateForReverse.Name); }
            set
            {
                AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterPreviousStateForReverse, value));
            }
        }

        /// <summary>
        /// Returns the name of the activity  which was initial for last executed transition <see cref="TransitionDefinition.From"/>
        /// </summary>
        public string PreviousActivityName
        {
            get { return GetParameter<string>(DefaultDefinitions.ParameterPreviousActivity.Name); }
            set
            {
                AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterPreviousActivity, value));
            }
        }

        /// <summary>
        /// Returns the name of the activity  which was initial for last executed transition marked as direct <see cref="TransitionDefinition.From"/>
        /// </summary>
        public string PreviousActivityForDirectName
        {
            get { return GetParameter<string>(DefaultDefinitions.ParameterPreviousActivityForDirect.Name); }
            set
            {
                AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterPreviousActivityForDirect, value));
            }
        }

        /// <summary>
        /// Returns the name of the activity  which was initial for last executed transition marked as reverse <see cref="TransitionDefinition.From"/>
        /// </summary>
        public string PreviousActivityForReverseName
        {
            get { return GetParameter<string>(DefaultDefinitions.ParameterPreviousActivityForReverse.Name); }
            set
            {
                AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterPreviousActivityForReverse, value));
            }
        }

        /// <summary>
        /// Returns the name of the currently executing command, filled if the transition process was initiated by a command 
        /// </summary>
        public string CurrentCommand
        {
            get { return GetParameter<string>(DefaultDefinitions.ParameterCurrentCommand.Name); }
            set
            {
                AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterCurrentCommand, value));
            }
        }


        /// <summary>
        /// Returns the user id which execute a command or set a state
        /// </summary>
        public string IdentityId
        {
            get { return GetParameter<string>(DefaultDefinitions.ParameterIdentityId.Name); }
            set { AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterIdentityId, value)); }
        }

        /// <summary>
        /// Returns the user id for whom executes a command or sets a state
        /// </summary>
        public string ImpersonatedIdentityId
        {
            get { return GetParameter<string>(DefaultDefinitions.ParameterImpersonatedIdentityId.Name); }
            set
            {
                AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterImpersonatedIdentityId, value));
            }
        }

        /// <summary>
        ///  Returns the name of the state  which is final for currently executing transition <see cref="TransitionDefinition.To"/>
        /// </summary>
        public string ExecutedActivityState
        {
            get { return GetParameter<string>(DefaultDefinitions.ParameterExecutedActivityState.Name); }
            set
            {
                AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterExecutedActivityState, value));
            }
        }

        /// <summary>
        ///  Returns the activity which is final for currently executing transition <see cref="TransitionDefinition.To"/>
        /// </summary>
        public ActivityDefinition ExecutedActivity
        {
            get { return GetParameter<ActivityDefinition>(DefaultDefinitions.ParameterExecutedActivity.Name); }
            set
            {
                AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterExecutedActivity, value));
            }
        }

        /// <summary>
        ///  Returns the currently executing transition <see cref="TransitionDefinition.To"/>
        /// </summary>
        public TransitionDefinition ExecutedTransition
        {
            get { return GetParameter<TransitionDefinition>(DefaultDefinitions.ParameterExecutedTransition.Name); }
            set
            {
                AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterExecutedTransition, value));
            }
        }

        /// <summary>
        /// Returns the name of the current activity. Activity which was final for last executed transition <see cref="TransitionDefinition.From"/>
        /// </summary>
        public string CurrentActivityName
        {
            get { return GetParameter<string>(DefaultDefinitions.ParameterCurrentActivity.Name); }
            set { AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterCurrentActivity, value)); }
        }

        /// <summary>
        /// Returns the list of user ids which have the ability to execute a transition which leads to set executed activity. 
        /// (which have the ability to execute current transition)
        /// It availiable only on pre-execution mode
        /// </summary>
        public List<string> IdentityIds
        {
            get { return GetParameter<List<string>>(DefaultDefinitions.ParameterIdentityIds.Name); }
            set { AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterIdentityIds, value)); }
        }

        /// <summary>
        /// Returns the list of user ids which have the ability to execute a transition which leads to set executed activity. 
        /// (which have the ability to execute current transition)
        /// The main difference between this property and IdentityIds, is that it takes into account only the last transition that leads into current activity
        /// It availiable only on pre-execution mode
        /// </summary>
        public List<string> IdentityIdsForCurrentActivity
        {
            get { return GetParameter<List<string>>(DefaultDefinitions.ParameterIdentityIdsForCurrentActivity.Name); }
            set { AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterIdentityIdsForCurrentActivity, value)); }
        }
        

        /// <summary>
        /// Returns the code of the scheme of the process
        /// </summary>
        public string SchemeCode
        {
            get { return GetParameter<string>(DefaultDefinitions.ParameterSchemeCode.Name); }
            set { AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterSchemeCode, value)); }
        }

        /// <summary>
        /// Returns the name of the current state. State which was final for last executed transition <see cref="TransitionDefinition.From"/>
        /// </summary>
        public string CurrentState
        {
            get { return GetParameter<string>(DefaultDefinitions.ParameterCurrentState.Name); }
            set { AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterCurrentState, value)); }
        }

        /// <summary>
        /// Returns the name of the activity from which the transitional process was started.
        /// </summary>
        public string StartTransitionalProcessActivity
        {
            get
            {
                var res = GetParameter<string>(DefaultDefinitions.ParameterStartTransitionalProcessActivity.Name);
                if (string.IsNullOrEmpty(res))
                    return CurrentActivityName;
                return res;
            }
            set { AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterStartTransitionalProcessActivity, value)); }
        }


        /// <summary>
        /// Returns the name of the current activity. Activity which was final for last executed transition <see cref="TransitionDefinition.From"/>
        /// </summary>
        public ActivityDefinition CurrentActivity
        {
            get { return ProcessScheme.FindActivity(CurrentActivityName); }
        }

        /// <summary>
        /// Returns true within pre-execution mode 
        /// </summary>
        public bool IsPreExecution
        {
            get { return GetParameter<bool>(DefaultDefinitions.ParameterIsPreExecution.Name); }
            set
            {
                AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterIsPreExecution, value));
            }
        }


        #endregion

        /// <summary>
        /// Returns parsed scheme of the process <see cref="ProcessDefinition"/>
        /// </summary>
        public ProcessDefinition ProcessScheme { get; set; }

        /// <summary>
        /// Sign that the scheme of the process is obsolete
        /// </summary>
        public bool IsSchemeObsolete { get; internal set; }

        /// <summary>
        /// Returns true if the pcocess is subprocess
        /// </summary>
        public bool IsSubprocess
        {
            get { return ParentProcessId.HasValue; }
        }

        /// <summary>
        /// Sign that parameters for creating scheme of the process was changed
        /// </summary>
        public bool IsDeterminingParametersChanged { get; internal set; }
       
        /// <summary>
        /// Returns the list of process parameters <see cref="ParameterDefinitionWithValue"/>
        /// </summary>
        public ParametersCollection ProcessParameters = new ParametersCollection();

        /// <summary>
        /// Returns the list of process parameters <see cref="ParameterDefinitionWithValue"/> from a subprocess which was merged with current process. 
        /// </summary>
        public ParametersCollection MergedSubprocessParameters = null;

        /// <summary>
        /// Create ProcessInstance object
        /// </summary>
        /// <param name="schemeId">Id of the scheme of the process</param>
        /// <param name="processId">Id of the process</param>
        /// <param name="processScheme">Parsed scheme of the process</param>
        /// <param name="isSchemeObsolete">Sign that the scheme of the process is obsolete</param>
        /// <param name="isDeterminingParametersChanged">Sign that parameters for creating scheme of the process was changed</param>
        /// <returns>ProcessInstance object</returns>
        public static ProcessInstance Create(Guid schemeId, Guid processId, ProcessDefinition processScheme,
            bool isSchemeObsolete, bool isDeterminingParametersChanged)
        {
            int maxNumberOfActivities = Licensing.GetLicenseRestrictions<WorkflowEngineNetRestrictions>().MaxNumberOfActivities;

            if (maxNumberOfActivities > 0)
            {
                if (processScheme.Activities.Count > maxNumberOfActivities)
                    throw new LicenseException(
                        string.Format(
                            "Maximum number of activities for your license {0} is less than number of activities in your scheme {1}.",
                            maxNumberOfActivities, processScheme.Activities.Count));
            }

            int maxNumberOfTransitions = Licensing.GetLicenseRestrictions<WorkflowEngineNetRestrictions>().MaxNumberOfTransitions;

            if (maxNumberOfTransitions > 0)
            {
                if (processScheme.Transitions.Count > maxNumberOfTransitions)
                    throw new LicenseException(
                        string.Format(
                            "Maximum number of transitions for your license {0} is less than number of transitions in your scheme {1}.",
                            maxNumberOfTransitions, processScheme.Transitions.Count));
            }

            int maxNumberOfCommands = Licensing.GetLicenseRestrictions<WorkflowEngineNetRestrictions>().MaxNumberOfCommands;

            if (maxNumberOfCommands > 0)
            {
                if (processScheme.Commands.Count > maxNumberOfCommands)
                    throw new LicenseException(
                        string.Format(
                            "Maximum number of commands for your license {0} is less than number of commands in your scheme {1}.",
                            maxNumberOfCommands, processScheme.Commands.Count));
            }

            return new ProcessInstance()
            {
                SchemeId = schemeId,
                ProcessId = processId,
                ProcessScheme = processScheme,
                IsSchemeObsolete = isSchemeObsolete,
                IsDeterminingParametersChanged = isDeterminingParametersChanged
            };
        }

        /// <summary>
        /// Adds parameter to process parameters collection
        /// </summary>
        /// <param name="parameter">Parameter with value <see cref="ParameterDefinitionWithValue"/></param>
        public void AddParameter(ParameterDefinitionWithValue parameter)
        {
            ProcessParameters.AddParameter(parameter);
        }

        /// <summary>
        /// Adds parameters to process parameters collection
        /// </summary>
        /// <param name="parameters">Collection of parameters with value <see cref="ParameterDefinitionWithValue"/></param>
        public void AddParameters(IEnumerable<ParameterDefinitionWithValue> parameters)
        {
            ProcessParameters.AddParameters(parameters);
        }

        /// <summary>
        /// Checks that the parameter whether exists or not
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <returns>true if exists</returns>
        public bool IsParameterExisting(string name)
        {
            return ProcessParameters.Any(p => p.Name.Equals(name) && p.Value != null);
        }

        /// <summary>
        /// Returns parameter with value <see cref="ParameterDefinitionWithValue"/> with specific name
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <returns>Parameter with value <see cref="ParameterDefinitionWithValue"/></returns>
        public ParameterDefinitionWithValue GetParameter(string name)
        {
            return ProcessParameters.GetParameter(name);
        }

        /// <summary>
        /// Returns parameter's value by specific name
        /// </summary>
        /// <typeparam name="T">Type of the parameter</typeparam>
        /// <param name="name">Name of the parameter</param>
        /// <returns>Value of the parameter</returns>
        public T GetParameter<T>(string name)
        {
            return ProcessParameters.GetParameter<T>(name);
        }

        /// <summary>
        /// Set value of the parameter with specific name. If the definition of the parameter with specific name is absent in the scheme of the process <see cref="ProcessDefinition.Parameters"/>,
        /// there will be creted Temporary parameter  <see cref="ParameterDefinition.Purpose"/> with specified name
        /// </summary>
        /// <typeparam name="T">Type of the parameter</typeparam>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        public void SetParameter<T>(string name, T value, ParameterPurpose purposeIfMissing = ParameterPurpose.Temporary)
        {

            var parameterDefinitionWithValue = ProcessParameters.GetParameter(name);
            if (parameterDefinitionWithValue == null)
            {
                var parameterDefinition =
                    ProcessScheme.Parameters.FirstOrDefault(
                        p => p.Name.Equals(name, StringComparison.Ordinal)) ??
                    new ParameterDefinition()
                    {
                        Name = name,
                        Type = typeof (T),
                        Purpose = purposeIfMissing
                    };
                ProcessParameters.AddParameter(ParameterDefinition.Create(parameterDefinition, value));
            }
            else
            {
                if (parameterDefinitionWithValue.Type == typeof(UnknownParameterType))
                    parameterDefinitionWithValue.Value = value == null ? null : ParametersSerializer.Serialize(value, typeof(T));
                else
                    parameterDefinitionWithValue.Value = value;
            }
        }

        /// <summary>
        /// Remove parameter from process parameters
        /// </summary>
        /// <param name="name">Parameter name</param>
        public void RemoveParameter(string name)
        {
            var parameterDefinitionWithValue = ProcessParameters.GetParameter(name);
            if (parameterDefinitionWithValue != null)
            {
                parameterDefinitionWithValue.Value = null;
            }
        }

        /// <summary>
        /// Replace process parameters collection by new value
        /// </summary>
        /// <param name="parameters">>Collection of parameters with value <see cref="ParameterDefinitionWithValue"/></param>
        public void SetProcessParameters(List<ParameterDefinitionWithValue> parameters)
        {
            ProcessParameters.ReplaceParameters(parameters);
        }

        #region CodeActions Invoke

        /// <summary>
        /// Check condition from global or local code actions with specific name
        /// </summary>
        /// <param name="name">Name of the condition to check</param>
        /// <param name="runtime">The instance of <see cref="WorkflowRuntime"/></param>
        /// <param name="parameter">Additional action parameter</param>
        /// <returns>Condition result</returns>
        public bool ExecuteConditionFromCodeActions(string name, WorkflowRuntime runtime, string parameter)
        {
            var codeActionsInvoker = CodeActionUtils.GetCodeActionsInvokerForCondition(name, runtime, ProcessScheme, out _);

            if (codeActionsInvoker != null)
            {
                return codeActionsInvoker.InvokeCondition(name, this, runtime, parameter);
            }
           
            throw new NotImplementedException($"Condition {name} not implemented");
        }

        /// <summary>
        /// Execute action from global or local code actions with specific name
        /// </summary>
        /// <param name="name">Name of the action to execute</param>
        /// <param name="runtime">The instance of <see cref="WorkflowRuntime"/></param>
        /// <param name="parameter">Additional action parameter</param>
        public void ExecuteCodeAction(string name, WorkflowRuntime runtime, string parameter)
        {
            var codeActionsInvoker = CodeActionUtils.GetCodeActionsInvokerForAction(name, runtime, ProcessScheme, out _);
            if (codeActionsInvoker != null)
            {
                codeActionsInvoker.InvokeAction(name, this, runtime, parameter);
                return;
            }

            throw new NotImplementedException($"Action {name} not implemented");
        }


        #endregion

        #region Localized

        /// <summary>
        /// Returns localized state name in specific culture
        /// </summary>
        /// <param name="stateName">System state name</param>
        /// <param name="culture">Culture</param>
        /// <returns>Localized state name</returns>
        public string GetLocalizedStateName(string stateName, CultureInfo culture)
        {
            return ProcessScheme.GetLocalizedStateName(stateName, culture);
        }

        /// <summary>
        /// Returns localized command name in specific culture
        /// </summary>
        /// <param name="commandName">System command name</param>
        /// <param name="culture">Culture</param>
        /// <returns>Localized command name</returns>
        public string GetLocalizedCommandName(string commandName, CultureInfo culture)
        {
            return ProcessScheme.GetLocalizedCommandName(commandName, culture);
        }

        /// <summary>
        /// Returns localized parameter name in specific culture
        /// </summary>
        /// <param name="parameterName">System parameter name</param>
        /// <param name="culture">Culture</param>
        /// <returns>Localized parameter name</returns>
        public string GetLocalizedParameterName(string parameterName, CultureInfo culture)
        {
            return ProcessScheme.GetLocalizedParameterName(parameterName, culture);
        }

        #endregion

        #region Debug info

        /// <summary>
        /// Returns process parameters formatted to string
        /// </summary>
        /// <param name="purpose">Parameter purpose to filter parameters <see cref="ParameterPurpose"/></param>
        /// <returns>Formatted string with parameters values</returns>
        public string ProcessParametersToString(ParameterPurpose purpose)
        {
            var res = new StringBuilder();
            foreach (var item in ProcessParameters.Where(p => p.Purpose == purpose).OrderBy(p => p.Name))
            {
                res.AppendLine(string.Format("{0}='{1}'", item.Name, item.Value));
            }
            return res.ToString();
        }

        #endregion

        internal void SetStartTransitionalProcessActivity()
        {
            StartTransitionalProcessActivity = CurrentActivityName;
        }

        public void InitPersistenceParametersFromScheme()
        {
            foreach (var parameter in ProcessScheme.Parameters.Where(p => p.Purpose == ParameterPurpose.Persistence && p.InitialValue != null))
            {
                try
                {
                    var value = ParametersSerializer.Deserialize(parameter.InitialValue, parameter.Type);
                    SetParameter(parameter.Name, value);
                }
                catch (Exception)
                {
                    if (parameter.Type == typeof(string))
                    {
                        SetParameter(parameter.Name, parameter.InitialValue);
                    }
                    else
                    {
                        throw new InvalidJsonParameterValueException("Initial value of {0} parameter must be a valid JSON or type of {0} parameter must be String", parameter.Name);
                    }
                }
            }
        }
    }
}
