using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Bus;
using OptimaJet.Workflow.Core.CodeActions;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.License;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Subprocess;

namespace OptimaJet.Workflow.Core.Runtime
{
    /// <summary>
    /// Provides main API to operations with workflow processes
    /// </summary>
    public sealed partial class WorkflowRuntime
    {
        private class PreExecutionContext
        {
            public TransitionDefinition TransitionForActors { get; set; }

            public bool HaveTransitionForActors => TransitionForActors != null;
        }

        /// <summary>
        /// Creates the WFE runtime object
        /// </summary>
        /// <param name="runtimeId">Id of the runtime (reserved for further extension)</param>
        public WorkflowRuntime(Guid runtimeId)
        {
            ExecutionSearchOrder = ExecutionSearchOrder.LocalGlobalProvider;
            SchemeParsingCulture = CultureInfo.InvariantCulture;
            Id = runtimeId;

            int maxNumberOfThreads 
                = Licensing.GetLicenseRestrictions<WorkflowEngineNetRestrictions>().MaxNumberOfThreads;
            if (maxNumberOfThreads > 0)
                LicenseSemaphore = new SemaphoreSlim(maxNumberOfThreads);
            RuleProvider = new EmptyWorkflowRuleProvider();
            ActionProvider = new EmptyWorkflowActionProvider();
            Licensing.TheadsCountChanged += LicensingOnTheadsCountChanged;
        }
      

        /// <summary>
        /// Creates the WFE runtime object
        /// </summary>
        public WorkflowRuntime():this(Guid.Empty)
        {
        }

        #region Runtime configuration

        #region Runtime management

        /// <summary>
        /// Runtime identifier
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Allow automatic schema of a process update before getting the commands list if its allowed in current activity
        /// </summary>
        public bool IsAutoUpdateSchemeBeforeGetAvailableCommands { get; set; }

        /// <summary>
        /// Raises when runtime need to obtain parameters for creating the scheme of the process
        /// </summary>
        public event EventHandler<NeedDeterminingParametersEventArgs> OnNeedDeterminingParameters;

        /// <summary>
        /// Raises when the scheme of the process was changed
        /// </summary>
        public event EventHandler<SchemaWasChangedEventArgs> OnSchemaWasChanged;

        /// <summary>
        /// Raises when workflow error occurred
        /// </summary>
        public event EventHandler<WorkflowErrorEventArgs> OnWorkflowError;

        /// <summary>
        /// Raises when runtime can not find starting transition of a subprocess in a new root schem
        /// </summary>
        public event EventHandler<StartingTransitionNotFoundEventArgs> OnStartingTransitionNotFound;

        /// <summary>
        /// Raises when current activity of a process was changed
        /// </summary>
        public event EventHandler<ProcessActivityChangedEventArgs> ProcessActivityChanged;

        /// <summary>
        /// Raises before execution of choosen activity
        /// </summary>
        public event EventHandler<BeforeActivityExecutionEventArgs> BeforeActivityExecution
        {
            add { Bus.BeforeExecution += value; }
            remove { Bus.BeforeExecution -= value; }
        }

        /// <summary>
        /// Raises when the timer value must be obtained 
        /// </summary>
        public event EventHandler<NeedTimerValueEventArgs> NeedTimerValue
        {
            add
            {
                if (TimerManager == null)
                    throw new NullReferenceException("TimerManager was not specified for the workflow runtime.");
                TimerManager.NeedTimerValue += value;
            }
            remove
            {
                if (TimerManager == null)
                    throw new NullReferenceException("TimerManager was not specified for the workflow runtime.");
                TimerManager.NeedTimerValue -= value;

            }
        }

        internal IWorkflowBus Bus;

        private IWorkflowRuleProvider _ruleProvider;

        /// <summary>
        /// Instance of the autocomplete provider (only for the Designer) <see cref="IDesignerAutocompleteProvider"/>
        /// </summary>
        public IDesignerAutocompleteProvider DesignerAutocompleteProvider { get; set; }
        

        /// <summary>
        /// Instance of the Workflow rule provider <see cref="IWorkflowRuleProvider"/>
        /// </summary>
        public IWorkflowRuleProvider RuleProvider
        {
            get
            {
                if (_ruleProvider == null)
                    throw new InvalidOperationException();
                return _ruleProvider;
            }
            internal set { _ruleProvider = value; }
        }

        private IWorkflowBuilder _builder;

        /// <summary>
        /// Instance of the Workflow builder <see cref="IWorkflowBuilder"/>
        /// </summary>
        public IWorkflowBuilder Builder
        {
            get
            {
                if (_builder == null)
                    throw new InvalidOperationException();
                return _builder;
            }
            internal set { _builder = value; }
        }

        /// <summary>
        /// Instance of the Timer manager <see cref="ITimerManager"/>
        /// </summary>
        public ITimerManager TimerManager { get; internal set; }

        private IPersistenceProvider _persistenceProvider;

        /// <summary>
        /// Instance of the Persistence provider <see cref="IPersistenceProvider"/>
        /// </summary>
        public IPersistenceProvider PersistenceProvider
        {
            get
            {
                if (_persistenceProvider == null)
                    throw new InvalidOperationException();
                return _persistenceProvider;
            }
            internal set { _persistenceProvider = value; }
        }

        /// <summary>
        /// Instance of the Action provider <see cref="IWorkflowActionProvider"/>
        /// </summary>
        public IWorkflowActionProvider ActionProvider { get; internal set; }

        /// <summary>
        /// Raises when the status of the procees <see cref="ProcessStatus"/> was changed
        /// </summary>
        public event EventHandler<ProcessStatusChangedEventArgs> ProcessStatusChanged;

        internal bool ValidateSettings()
        {
            return Bus != null && Builder != null && PersistenceProvider != null;
        }

        /// <summary>
        /// Culture for parsing some text parameters from scheme. Default is InvariantCulture
        /// </summary>
        public CultureInfo SchemeParsingCulture { get; set; }

        /// <summary>
        /// Get date and time which used by runtime
        /// </summary>
        public DateTime RuntimeDateTimeNow
        {
            get
            {
                if (UseUtcDateTimeAsRuntimeTime)
                {
                    return DateTime.UtcNow;
                }

                return DateTime.Now;
            }
        }

        /// <summary>
        /// Runtime use Utc date and time if true and Local date and time if false
        /// </summary>
        public bool UseUtcDateTimeAsRuntimeTime { get; set; }

        /// <summary>
        /// Runtime in cold start state if true
        /// </summary>
        public  bool IsCold { get; private set; }

        /// <summary>
        /// Start all workflow runtime services аnd compile global code actions
        /// </summary>
        internal void Start(bool ignoreNotCompilled, out Dictionary<string,string> compillerErrors)
        {
            
            PersistenceProvider.ResetWorkflowRunning();

            var globalActions =
                PersistenceProvider.LoadGlobalParameters<CodeActionDefinition>(
                    CodeActionsGlobalParameterName);

            GlobalActionsInvoker = CodeActionsCompiller.CompileGlobalCodeActions(globalActions, ignoreNotCompilled, out compillerErrors);

            if (TimerManager != null)
            {
                PersistenceProvider.ClearTimersIgnore();
                TimerManager.Start();
            }
            Bus.Start();

            IsCold = false;

        }

        /// <summary>
        /// Start all workflow runtime services except timers
        /// </summary>
        internal void ColdStart(bool ignoreNotCompilled, out Dictionary<string,string> compillerErrors)
        {
            IsCold = true;

            PersistenceProvider.ResetWorkflowRunning();

            var globalActions =
                PersistenceProvider.LoadGlobalParameters<CodeActionDefinition>(
                    CodeActionsGlobalParameterName);

            GlobalActionsInvoker = CodeActionsCompiller.CompileGlobalCodeActions(globalActions,ignoreNotCompilled, out compillerErrors);

            Bus.Start();
        }

        #endregion

        #region Code Actions
        
        /// <summary>
        /// The order of the Action, Condition, or Rule search by name
        /// </summary>
        public ExecutionSearchOrder ExecutionSearchOrder { get; internal set; }
        
        [Obsolete("If you are sure that your schemes do not mention nonexistent Actions, Conditions or Rules, do not use this setting. This setting is only for resolving possible compatibility issues and can be removed.")]
        public bool IgnoreMissingExecutionItems { get; internal set; }

        internal static string CodeActionsGlobalParameterName => "CodeAction";

        /// <summary>
        /// Default value is true. Enable compillation for Code Actions <see cref="CodeActionDefinition"/> 
        /// </summary>
        public static bool CodeActionsCompillationEnable
        {
            set { CodeActionsCompiller.CompillationEnable = value; }
            get { return CodeActionsCompiller.CompillationEnable; }
        }

        /// <summary>
        /// Default value is false. Enable debug in code action. You can put a brekpoint  by <see cref="Debugger.Break"/> or /*break*/ in a code action's code
        /// </summary>
        public static bool CodeActionsDebugMode
        {
            set { CodeActionsCompiller.DebugMode = value; }
            get { return CodeActionsCompiller.DebugMode; }
        }

        /// <summary>
        /// Register assembly in <see cref="CodeActionsCompiller"/> as referenced assembly
        /// </summary>
        public static void CodeActionsRegisterAssembly(Assembly assembly)
        {
            CodeActionsCompiller.RegisterAssembly(assembly);
        }

        /// <summary>
        /// Invoker which calls the global Code Actions 
        /// </summary>
        public CodeActionsInvoker GlobalActionsInvoker { get; private set; }

        #endregion

        #region Licensing

        private static object _licensingLock = new object();
        /// <summary>
        /// Register the license to remove license restrictions
        /// </summary>
        /// <param name="licenseText">License text</param>
        public static void RegisterLicense(string licenseText)
        {
            Licensing.RegisteWorkflowLicense(licenseText);
        }

        internal SemaphoreSlim LicenseSemaphore;

        private void LicensingOnTheadsCountChanged(object sender, TheadsCountChangedEventArgs args)
        {
            lock (_licensingLock)
            {
                if (args.NewValue < 0)
                {
                    var oldLicSemaphore = LicenseSemaphore;
                    LicenseSemaphore = null;
                    if (oldLicSemaphore != null)
                    {
                        var releaseCount = args.OldValue - oldLicSemaphore.CurrentCount;
                        if (releaseCount > 0)
                            oldLicSemaphore.Release(releaseCount);
                    }

                }
                else
                {
                    var oldLicSemaphore = LicenseSemaphore;
                    LicenseSemaphore = new SemaphoreSlim(args.NewValue);
                    if (oldLicSemaphore != null)
                    {
                        var releaseCount = args.OldValue - oldLicSemaphore.CurrentCount;
                        if (releaseCount > 0)
                            oldLicSemaphore.Release(releaseCount);
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Workflow Public API Sync

        /// <summary>
        /// Check existence of the specified process 
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <returns>True if process with specified identifier is exists</returns>
        public bool IsProcessExists(Guid processId)
        {
            return PersistenceProvider.IsProcessExists(processId);
        }

        /// <summary>
        /// Create instance of process.
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="processId">Process id</param>
        public void CreateInstance(string schemeCode, Guid processId)
        {
            CreateInstance(schemeCode, processId, new Dictionary<string, object>());
        }

        /// <summary>
        /// Create instance of process.
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="processId">Process id</param>
        /// <param name="identityId">The user id which execute initial command if command is available</param>
        /// <param name="impersonatedIdentityId">The user id for whom executes initial command if command is available</param>
        public void CreateInstance(string schemeCode, Guid processId, string identityId, string impersonatedIdentityId)
        {
            CreateInstance(schemeCode, processId, identityId, impersonatedIdentityId,
                new Dictionary<string, object>());
        }

        /// <summary>
        /// Create instance of process.
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="processId">Process id</param>
        /// <param name="schemeCreationParameters">Parameters for creating the scheme of the process (defining parameters)</param>
        public void CreateInstance(string schemeCode, Guid processId, IDictionary<string, object> schemeCreationParameters)
        {
            CreateInstance(schemeCode, processId, null, null, schemeCreationParameters);
        }

        /// <summary>
        /// Create instance of the process.
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="processId">Process id</param>
        /// <param name="identityId">The user id which execute initial command if command is available</param>
        /// <param name="impersonatedIdentityId">The user id for whom executes initial command if command is available</param>
        /// <param name="schemeCreationParameters">The parameters for creating scheme of process (defining parameters)</param>
        public void CreateInstance(string schemeCode, Guid processId, string identityId, string impersonatedIdentityId, 
            IDictionary<string, object> schemeCreationParameters)
        {
            CreateInstance(new CreateInstanceParams(schemeCode, processId)
            {
                IdentityId = identityId,
                ImpersonatedIdentityId = impersonatedIdentityId,
                SchemeCreationParameters = schemeCreationParameters
            });
        }


        /// <summary>
        /// Create instance of the process.
        /// </summary>
        /// <param name="createInstanceParams">Parameters for creaition of an instance of a process</param>
        public void CreateInstance(CreateInstanceParams createInstanceParams)
        {
            CreateInstanceAsync(createInstanceParams).Wait();
        }

        /// <summary>
        /// Delete instance of the process and all child subprocesses.
        /// </summary>
        /// <param name="processId">Process id</param>
        public void DeleteInstance(Guid processId)
        {
            var processInstance = Builder.GetProcessInstance(processId);
            PersistenceProvider.FillSystemProcessParameters(processInstance);
            var tree = GetProcessInstancesTree(processInstance);
            DropProcesses(new List<Guid> {processId}, tree);
            if (processInstance.IsSubprocess)
            {
                SaveProcessInstancesTree(tree);
            }
            else
            {
                DeleteProcessInstancesTree(tree);
            }
        }

        private async Task InitCreatedProcess(string identityId, string impersonatedIdentityId, ProcessInstance processInstance,
            IDictionary<string, object> initialProcessParameters, CancellationToken token, ProcessInstancesTree processInstancesTree = null)
        {
            PersistenceProvider.InitializeProcess(processInstance);

            //Initial parameters from scheme
            processInstance.InitPersistenceParametersFromScheme();

            //Initial parameters from dictionary
            if (initialProcessParameters != null && initialProcessParameters.Any())
            {
                foreach (var parameter in initialProcessParameters)
                {
                    processInstance.SetParameter(parameter.Key, parameter.Value);
                }
            }

            //Request timer values
            TimerManager?.RequestTimerValue(processInstance, processInstance.ProcessScheme.InitialActivity);

            PersistenceProvider.SavePersistenceParameters(processInstance);

            await SetProcessNewStatus(processInstance, ProcessStatus.Initialized).ConfigureAwait(false);

            await ExecuteCreatedProcess(processInstance, identityId, impersonatedIdentityId, token, processInstancesTree).ConfigureAwait(false);
        }

        internal async Task ExecuteCreatedProcess(ProcessInstance processInstance, string identityId, string impersonatedIdentityId, CancellationToken token, ProcessInstancesTree processInstancesTree = null)
        {
            var haveImplementation = processInstance.ProcessScheme.InitialActivity.HaveImplementation;

            var autoTransitions =
                processInstance.ProcessScheme.GetAutoTransitionForActivity(processInstance.ProcessScheme.InitialActivity)
                    .ToList();


            var forkAutoTransitions =
                processInstance.ProcessScheme.GetAutoTransitionForActivity(processInstance.ProcessScheme.InitialActivity, ForkTransitionSearchType.Fork).ToList();


            processInstance.CurrentActivityName = processInstance.ProcessScheme.InitialActivity.Name;
            processInstance.CurrentState = processInstance.ProcessScheme.InitialActivity.State;
            processInstance.IdentityId = identityId;
            processInstance.ImpersonatedIdentityId = impersonatedIdentityId;

            //Additional autotransitions from undefined timers
            if (TimerManager != null)
            {
                var transitionsForImmediateExecution = TimerManager.GetTransitionsForImmediateExecution(processInstance, processInstance.ProcessScheme.InitialActivity).ToList();
                autoTransitions.AddRange(transitionsForImmediateExecution.Where(t => !t.IsFork));
                forkAutoTransitions.AddRange(transitionsForImmediateExecution.Where(t => t.IsFork));
            }


            var haveAutoTransitions = autoTransitions.Any();
            var haveForkAutoTransitions = forkAutoTransitions.Any();

            if (haveImplementation || haveAutoTransitions || haveForkAutoTransitions)
            {
                await  SetProcessNewStatus(processInstance, ProcessStatus.Running).ConfigureAwait(false);

                try
                {
                    if (haveImplementation)
                    {
                        await ExecuteRootActivity(processInstance, token).ConfigureAwait(false);
                        //Requred to save parameters 
                        PersistenceProvider.SavePersistenceParameters(processInstance);
                    }

                    //Subprocess creation
                    if (haveForkAutoTransitions)
                    {
                        await ChooseTransitionAndCreateSubprocess(forkAutoTransitions, processInstance, token, processInstancesTree).ConfigureAwait(false);
                    }

                    if (haveAutoTransitions)
                    {
                        processInstance.SetStartTransitionalProcessActivity();

                        ProcessActivityChanged?.Invoke(this, new ProcessActivityChangedEventArgs(processInstance, false));

                        var newExecutionParameters = new List<ExecutionRequestParameters>();
                        newExecutionParameters.AddRange(autoTransitions.Select(at => ExecutionRequestParameters.Create(processInstance, at)));
                        await Bus.QueueExecution(newExecutionParameters, token).ConfigureAwait(false);
                    }
                    else
                    {
                        await SetProcessNewStatus(processInstance, ProcessStatus.Idled).ConfigureAwait(false);

                        ProcessActivityChanged?.Invoke(this, new ProcessActivityChangedEventArgs(processInstance, true));

                        TimerManager?.ClearAndRegisterTimers(processInstance);
                    }
                }
                catch (Exception ex)
                {
                    await SetProcessNewStatus(processInstance, ProcessStatus.Idled).ConfigureAwait(false);
                    throw new Exception($"Create instace of process Id={processInstance.ProcessId}", ex);
                }
            }
            else
            {
                await SetProcessNewStatus(processInstance, ProcessStatus.Idled).ConfigureAwait(false);

                ProcessActivityChanged?.Invoke(this, new ProcessActivityChangedEventArgs(processInstance, true));

                TimerManager?.ClearAndRegisterTimers(processInstance);
            }
        }


        private async Task CreateSubprocesses(ProcessInstance processInstance, List<TransitionDefinition> transitions, CancellationToken token, ProcessInstancesTree processInstancesTree = null)
        {
            if (processInstancesTree == null)
            {
                processInstancesTree = GetProcessInstancesTree(processInstance);
            }

            foreach (var transition in transitions)
            {
                await CreateSubprocess(processInstance, transition, token, processInstancesTree).ConfigureAwait(false);
            }
        }

        private async Task CreateSubprocess(ProcessInstance parentProcessInstance,
            TransitionDefinition startTransition, CancellationToken token, ProcessInstancesTree processInstancesTree)
        {
            var parentId = parentProcessInstance.ProcessId;
            //Generating subprocess id
            var newSubprocessId = Guid.NewGuid();
            //Updating the subprocess tree
            var parent = processInstancesTree.GetNodeById(parentId);

            if (parent.HaveChildWithName(startTransition.Name))
                return;

            var instancesTree = new ProcessInstancesTree(newSubprocessId, startTransition.Name);
            parent.AddChild(instancesTree);
            //Saving tree to persistence store
            SaveProcessInstancesTree(parent);

            try
            {
                var processInstance = Builder.CreateNewSubprocess(newSubprocessId, parentProcessInstance, startTransition);
                //not required to pass initial parameters because they were copied in previous call
                await InitCreatedProcess(processInstance.IdentityId, processInstance.ImpersonatedIdentityId, processInstance, new Dictionary<string, object>(), token,
                        instancesTree)
                    .ConfigureAwait(false);
            }
            catch (Exception)
            {
                parent.RemoveChild(instancesTree);
                throw;
            }

        }

        /// <summary>
        /// Returns process instance tree (root process and subprocesses)
        /// </summary>
        /// <param name="rootProcessId">Root process id</param>
        /// <returns>ProcessInstancesTree object</returns>
        public ProcessInstancesTree GetProcessInstancesTree(Guid rootProcessId)
        {
            var subprocessTree = PersistenceProvider.LoadGlobalParameter<List<SerializableSubprocessTree>>(
               "ProcessTree", rootProcessId.ToString("N"));

            return subprocessTree == null
                ? new ProcessInstancesTree(rootProcessId)
                : ProcessInstancesTree.RestoreFromSerializableObject(subprocessTree, this);
        }

        /// <summary>
        /// Returns process instance tree (root process and subprocesses)
        /// </summary>
        /// <param name="processInstance">ProcessInstance for which tree is builds</param>
        /// <returns>ProcessInstancesTree object</returns>
        public ProcessInstancesTree GetProcessInstancesTree(ProcessInstance processInstance)
        {
            var rootProcessId = processInstance.RootProcessId;

            if (!processInstance.IsSubprocess && !processInstance.ProcessScheme.ContainsSubprocesses)
               return new ProcessInstancesTree(rootProcessId);

            return GetProcessInstancesTree(rootProcessId);
        }

        private void SaveProcessInstancesTree(ProcessInstancesTree tree)
        {
            var valueForSave = tree.GetSerializableObject();

            PersistenceProvider.SaveGlobalParameter("ProcessTree", tree.Root.Id.ToString("N"), valueForSave);
        }

        private void DeleteProcessInstancesTree(ProcessInstancesTree tree)
        {
            PersistenceProvider.DeleteGlobalParameters("ProcessTree", tree.Root.Id.ToString("N"));
        }


        /// <summary>
        /// Set flag IsObsolete for all schemas of process with specific code and parameters
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="parameters">The parameters for creating scheme of process</param>
        public void SetSchemeIsObsolete(string schemeCode, Dictionary<string, object> parameters)
        {
            _builder.SetSchemeIsObsolete(schemeCode, parameters);
        }

        /// <summary>
        /// Set flag IsObsolete for all schemas of process with specific code
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        public void SetSchemeIsObsolete(string schemeCode)
        {
            _builder.SetSchemeIsObsolete(schemeCode);
        }

        /// <summary>
        /// Sets new value of named timer. Use this method outside of your process. 
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="timerName">Timer name in Scheme</param>
        /// <param name="newValue">New value of the timer</param>
        public void SetTimerValue(Guid processId, string timerName, DateTime newValue)
        {
            SetTimerValueAsync(processId, timerName, newValue).Wait(); //.RunSynchronously();
        }

        /// <summary>
        /// Sets new value of named timer.  Use this method inside of your process (in actions). 
        /// </summary>
        /// <param name="processInstance">Process instance</param>
        /// <param name="timerName">Timer name in Scheme</param>
        /// <param name="newValue">New value of the timer</param>
        public void SetTimerValue(ProcessInstance processInstance, string timerName, DateTime newValue)
        {
            TimerManager?.SetTimerValue(processInstance, timerName, newValue);
        }

        /// <summary>
        /// Resets value of named timer.  Use this method outside of your process. 
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="timerName">Timer name in Scheme</param>
        public void ResetTimerValue(Guid processId, string timerName)
        {
            ResetTimerValueAsync(processId, timerName).Wait(); //.RunSynchronously();
        }

        /// <summary>
        /// Resets value of named timer. Use this method inside of your process (in actions). 
        /// </summary>
        /// <param name="processInstance">Process instance</param>
        /// <param name="timerName">Timer name in Scheme</param>
        public void ResetTimerValue(ProcessInstance processInstance, string timerName)
        {
            TimerManager?.ResetTimerValue(processInstance, timerName);
        }

        /// <summary>
        /// Updating scheme of specific process 
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="ignoreAutoSchemeUpdate">If true the attribute of Activity - IsAutoScheme update will be ignored.</param>
        public void UpdateSchemeIfObsolete(Guid processId, bool ignoreAutoSchemeUpdate = false)
        {
            UpdateSchemeIfObsolete(processId, new Dictionary<string, object>(), ignoreAutoSchemeUpdate);
        }

        /// <summary>
        /// Updating scheme of specific process 
        /// </summary>
        /// <param name="processId">The process id</param>
        /// <param name="parameters">Parameters for creating scheme of process</param>
        /// <param name="ignoreAutoSchemeUpdate">If true the attribute of Activity - IsAutoScheme update will be ignored.</param>
        public void UpdateSchemeIfObsolete(Guid processId, IDictionary<string, object> parameters, bool ignoreAutoSchemeUpdate = false)
        {
            var processInstance = Builder.GetProcessInstance(processId);
            PersistenceProvider.FillProcessParameters(processInstance);
            UpdateScheme(processInstance, parameters, ignoreAutoSchemeUpdate);
        }

        private ProcessInstance UpdateScheme(ProcessInstance processInstance, IDictionary<string, object> parameters = null, bool ignoreAutoSchemeUpdate = false)
        {
            if ((!processInstance.CurrentActivity.IsAutoSchemeUpdate && !ignoreAutoSchemeUpdate) ||
                (!processInstance.IsSchemeObsolete && !processInstance.IsDeterminingParametersChanged))
            {
                return processInstance;
            }

            var changes = new List<SchemaWasChangedEventArgs>();

            var wasDropped = false;

            SetProcessNewStatus(processInstance, ProcessStatus.Running).Wait();//.RunSynchronously();
            try
            {
                var tree = GetProcessInstancesTree(processInstance);
                var currentNode = tree.GetNodeById(processInstance.ProcessId);
                
                //we need to get new root process scheme
                if (parameters == null)
                {
                    var args = new NeedDeterminingParametersEventArgs
                    {
                        ProcessId =
                            processInstance.RootProcessId
                    };

                    OnNeedDeterminingParameters?.Invoke(this, args);

                    if (args.DeterminingParameters == null)
                        args.DeterminingParameters = new Dictionary<string, object>();

                    parameters = args.DeterminingParameters;
                }

                var rootSchemeCode = string.IsNullOrEmpty(processInstance.ProcessScheme.RootSchemeCode)
                    ? processInstance.ProcessScheme.Name
                    : processInstance.ProcessScheme.RootSchemeCode;
                var newRootScheme = Builder.CreateNewProcessScheme(rootSchemeCode,
                    parameters);

                ProcessDefinition updatedProcessScheme;
                ProcessDefinition oldRootScheme;

                var needToSaveProcessInstanceTree = false;

                if (!processInstance.IsSubprocess)
                {
                    oldRootScheme = processInstance.ProcessScheme;
                    updatedProcessScheme = newRootScheme;
                }
                else
                {
                    var startingTransitionName = currentNode.Name;
                    TransitionDefinition startingTransition = null;
                    try
                    {
                        startingTransition = newRootScheme.FindTransition(startingTransitionName);
                    }
                    catch (TransitionNotFoundException)
                    {
                    }

                    var rootProcess = Builder.GetProcessInstance(tree.Root.Id);
               

                    oldRootScheme = rootProcess.ProcessScheme;
                    if (startingTransition == null)
                    {
                        if (OnStartingTransitionNotFound != null)
                        {
                            var args = new StartingTransitionNotFoundEventArgs(processInstance.ProcessId, tree.Root.Id,
                                oldRootScheme, newRootScheme, startingTransitionName);
                            OnStartingTransitionNotFound?.Invoke(this, args);

                            if (args.Decision ==
                                StartingTransitionNotFoundEventArgs.SubprocessUpdateDecision.DropProcess)
                            {
                                DropProcesses(new List<Guid> {processInstance.ProcessId}, tree);
                                SaveProcessInstancesTree(tree);
                                wasDropped = true;
                                return null;
                            }
                            else if (args.Decision == StartingTransitionNotFoundEventArgs.SubprocessUpdateDecision.Ignore)
                            {
                                return processInstance;
                            }
                            else if (args.Decision ==
                                     StartingTransitionNotFoundEventArgs.SubprocessUpdateDecision
                                         .StartWithNewTransition)
                            {
                                startingTransition = newRootScheme.FindTransition(args.NewTransitionName);
                                //change transition name in process tree
                                currentNode.Name = startingTransition.Name;
                                needToSaveProcessInstanceTree = true;
                            }
                        }
                        else
                        {
                            //Process is dropping by default
                            DropProcesses(new List<Guid> {processInstance.ProcessId}, tree);
                            SaveProcessInstancesTree(tree);
                            wasDropped = true;
                            return null;
                        }
                    }

                    updatedProcessScheme = Builder.CreateNewSubprocessScheme(newRootScheme, startingTransition);
                }

                BindProcessToNewScheme(processInstance, updatedProcessScheme, changes);

                if (needToSaveProcessInstanceTree)
                {
                    SaveProcessInstancesTree(currentNode);
                }

                UpdateChildrenSchemes(currentNode, oldRootScheme, newRootScheme, changes, ignoreAutoSchemeUpdate);
            }
            finally
            {
                if (!wasDropped)
                    SetIdledStatusAfterSchemaUpdated(processInstance);
            }

            if (OnSchemaWasChanged != null)
            {
                foreach (var change in changes)
                {
                    OnSchemaWasChanged?.Invoke(this, change);
                }
            }

            PersistenceProvider.FillProcessParameters(processInstance);
            return processInstance;
        }

        private void UpdateChildrenSchemes(ProcessInstancesTree currentNode, ProcessDefinition oldRootScheme, ProcessDefinition newRootScheme,
            List<SchemaWasChangedEventArgs> changes, bool ignoreAutoSchemeUpdate)
        {
            if (!currentNode.Children.Any())
                return;

            foreach (var child in currentNode.Children)
            {
                var childInstance = Builder.GetProcessInstance(child.Id);
                var wasDropped = false;
                SetProcessNewStatus(childInstance, ProcessStatus.Running).Wait();//.RunSynchronously();
                try
                {
                    PersistenceProvider.FillSystemProcessParameters(childInstance);
                    if (childInstance.CurrentActivity.IsAutoSchemeUpdate || ignoreAutoSchemeUpdate)
                    {
                        TransitionDefinition childStatrtingTransition = null;
                        try
                        {
                            childStatrtingTransition = newRootScheme.FindTransition(child.Name);
                        }
                        catch (TransitionNotFoundException)
                        {
                        }

                        if (childStatrtingTransition != null)
                        {
                            var newChidScheme = Builder.CreateNewSubprocessScheme(newRootScheme,
                                childStatrtingTransition);
                            BindProcessToNewScheme(childInstance, newChidScheme, changes);
                        }
                        else if (OnStartingTransitionNotFound != null)
                        {
                            var args = new StartingTransitionNotFoundEventArgs(childInstance.ProcessId,
                                childInstance.RootProcessId,
                                oldRootScheme, newRootScheme, child.Name);
                            OnStartingTransitionNotFound(this, args);

                            if (args.Decision ==
                                StartingTransitionNotFoundEventArgs.SubprocessUpdateDecision.DropProcess)
                            {
                                DropProcesses(new List<Guid> {childInstance.ProcessId}, currentNode);
                                SaveProcessInstancesTree(currentNode);
                                wasDropped = true;
                                continue;
                            }
                            else if (args.Decision == StartingTransitionNotFoundEventArgs.SubprocessUpdateDecision.Ignore)
                            {
                                continue;
                            }
                            else if (args.Decision ==
                                     StartingTransitionNotFoundEventArgs.SubprocessUpdateDecision
                                         .StartWithNewTransition)
                            {
                                childStatrtingTransition = newRootScheme.FindTransition(args.NewTransitionName);
                                var newChidScheme = Builder.CreateNewSubprocessScheme(newRootScheme, childStatrtingTransition);
                                BindProcessToNewScheme(childInstance, newChidScheme, changes);
                                
                                //change transition name in process tree
                                child.Name = childStatrtingTransition.Name;
                                SaveProcessInstancesTree(child);
                            }
                        }
                        else
                        {
                            //Process is dropping by default
                            DropProcesses(new List<Guid> {childInstance.ProcessId}, currentNode);
                            SaveProcessInstancesTree(currentNode);
                            wasDropped = true;
                            continue;
                        }
                    }

                    UpdateChildrenSchemes(child, oldRootScheme, newRootScheme, changes, ignoreAutoSchemeUpdate);
                }
                finally
                {
                    if (!wasDropped)
                        SetIdledStatusAfterSchemaUpdated(childInstance);
                }
            }
        }

        private void SetIdledStatusAfterSchemaUpdated(ProcessInstance processInstance)
        {
            ActivityDefinition current = null;
            try
            {
                current = processInstance.CurrentActivity;
            }
            catch (ActivityNotFoundException)
            {
            }
            if (current != null)
                SetIdledOrFinalizedStatus(processInstance, processInstance.CurrentActivity).Wait(); //.RunSynchronously();
            else
                SetProcessNewStatus(processInstance, ProcessStatus.Idled).Wait(); //.RunSynchronously();
        }

        private void BindProcessToNewScheme(ProcessInstance processInstance, ProcessDefinition updatedProcessScheme,
            List<SchemaWasChangedEventArgs> changes)
        {
            var schemaWasObsolete = processInstance.IsSchemeObsolete;
            var determiningParametersWasChanged = processInstance.IsDeterminingParametersChanged;

            processInstance.SchemeId = updatedProcessScheme.Id;
            processInstance.IsSchemeObsolete = false;
            processInstance.IsDeterminingParametersChanged = false;
            processInstance.ProcessScheme = updatedProcessScheme;
            PersistenceProvider.BindProcessToNewScheme(processInstance, true);

            var changedArgs = new SchemaWasChangedEventArgs
            {
                DeterminingParametersWasChanged = determiningParametersWasChanged,
                ProcessId = processInstance.ProcessId,
                SchemeId = processInstance.SchemeId,
                SchemaWasObsolete = schemaWasObsolete
            };

            changes.Add(changedArgs);
        }


        /// <summary>
        /// Pre-execution from initial activity of the process
        /// </summary>
        /// <param name="processId">The process id</param>
        /// <param name="ignoreCurrentStateCheck">If false and Current State Name and State Name of Current Activity is different (in case of scheme upgrade) do not run pre-execution</param>
        public void PreExecuteFromInitialActivity(Guid processId, bool ignoreCurrentStateCheck = false)
        {
            PreExecuteFromInitialActivityAsync(processId, ignoreCurrentStateCheck).Wait(); //.RunSynchronously();
        }

        /// <summary>
        /// Pre-execution from current activity of the process
        /// </summary>
        /// <param name="processId">The process id</param>
        /// <param name="ignoreCurrentStateCheck">If false and Current State Name and State Name of Current Activity is different (in case of scheme upgrade) do not run pre-execution</param>
        public void PreExecuteFromCurrentActivity(Guid processId, bool ignoreCurrentStateCheck = false)
        {
            PreExecuteFromCurrentActivityAsync(processId, ignoreCurrentStateCheck).Wait(); //.RunSynchronously();
        }

        /// <summary>
        /// Pre-execution from specified activity of the process
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="fromActivityName">Activity name which begins pre-execution</param>
        /// <param name="ignoreCurrentStateCheck">If false and Current State Name and State Name of Current Activity is different (in case of scheme upgrade) do not run pre-execution</param>
        public void PreExecute(Guid processId, string fromActivityName, bool ignoreCurrentStateCheck = false)
        {
            PreExecuteAsync(processId, fromActivityName, ignoreCurrentStateCheck).Wait(); //.RunSynchronously();
        }

        private async Task PreExecute(Guid processId, string fromActivityName, bool ignoreCurrentStateCheck,
            ProcessInstance processInstance, CancellationToken token)
        {
            PersistenceProvider.FillPersistedProcessParameters(processInstance);

            var activity = processInstance.ProcessScheme.FindActivity(processInstance.CurrentActivityName);
            var currentActivity = processInstance.ProcessScheme.FindActivity(fromActivityName);
            if (!ignoreCurrentStateCheck && activity.State != currentActivity.State)
                return;

            var executor = new ActivityExecutor(this, true);


            processInstance.AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterProcessId, processId));
            processInstance.AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterSchemeId,
                processInstance.SchemeId));
            processInstance.AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterIsPreExecution, true));

            var preExecutionContext = new PreExecutionContext();

            do
            {
                if (!string.IsNullOrEmpty(currentActivity.State))
                    processInstance.AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterCurrentState,
                        currentActivity.State));

                var transitions =
                    processInstance.ProcessScheme.GetPossibleTransitionsForActivity(currentActivity)
                        .Where(t => t.Classifier == TransitionClassifier.Direct && t.Trigger.Type != TriggerType.Timer);

                currentActivity = null;

                var transitionDefinitions = transitions as IList<TransitionDefinition> ?? transitions.ToList();
                var autotransitions = transitionDefinitions.Where(t => t.Trigger.Type == TriggerType.Auto).ToList();
                var commandTransitions = transitionDefinitions.Where(t => t.Trigger.Type == TriggerType.Command).ToList();
                var newExecutionParameters = FillExecutionRequestParameters(processInstance, autotransitions, preExecutionContext, commandTransitions);
                if (newExecutionParameters.Count > 0)
                {
                    var response = await executor.Execute(newExecutionParameters, token).ConfigureAwait(false);
                    if (!PreExecuteProcessResponse(response))
                    {
                        currentActivity = processInstance.ProcessScheme.FindTransition(response.ExecutedTransitionName).To;
                    }
                }

                if (currentActivity == null)
                {
                    if (commandTransitions.Count(t => t.IsAlwaysTransition && !t.Conditions.First().ResultOnPreExecution.HasValue) < 2)
                    {
                        newExecutionParameters = FillExecutionRequestParameters(processInstance,
                            commandTransitions, preExecutionContext);

                        if (newExecutionParameters.Count > 0)
                        {
                            var response = await executor.Execute(newExecutionParameters, token).ConfigureAwait(false);

                            if (!PreExecuteProcessResponse(response))
                            {
                                currentActivity =
                                    processInstance.ProcessScheme.FindTransition(response.ExecutedTransitionName).To;
                            }
                        }
                    }
                }
            } while (currentActivity != null && !currentActivity.IsFinal);
        }

        private bool PreExecuteProcessResponse(ExecutionResponseParameters response)
        {
            if (response.IsEmplty)
                return true;

            if (response.IsError)
            {
                var executionResponseParametersError = response as ExecutionResponseParametersError;
                var exception = executionResponseParametersError?.Exception ?? new Exception($"Error while pre executing process ProcessId={response.ProcessId}");
                throw exception;
            }

            return false;
        }

        /// <summary>
        /// Return the list of commands which is availiable from initial activity for specified user
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="identityId">User id for whom formed initial commands list</param>
        /// <returns>List of <see cref="WorkflowCommand"/> commands</returns>
        public IEnumerable<WorkflowCommand> GetInitialCommands(string schemeCode, string identityId)
        {
            return GetInitialCommands(schemeCode, new List<string>() {identityId});
        }

        /// <summary>
        /// Return the list of commands which is availiable from initial activity for specified users
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="identityIds">List of User ids for which formed initial commands list</param>
        /// <param name="processParameters">Parameters for creating scheme of process</param>
        /// <param name="commandNameFilter">Selects only the specified command if not null </param>
        /// <returns>List of <see cref="WorkflowCommand"/> commands</returns>
        public IEnumerable<WorkflowCommand> GetInitialCommands(string schemeCode, IEnumerable<string> identityIds,
            IDictionary<string, object> processParameters = null, string commandNameFilter = null)
        {
            var processDefinition = processParameters != null
                ? Builder.GetProcessScheme(schemeCode, processParameters)
                : Builder.GetProcessScheme(schemeCode);


            var initialActivity = processDefinition.InitialActivity;

            List<TransitionDefinition> commandTransitions;
            if (string.IsNullOrEmpty(commandNameFilter))
                commandTransitions = processDefinition.GetCommandTransitions(initialActivity).ToList();
            else
            {
                commandTransitions =
                    processDefinition.GetCommandTransitions(initialActivity)
                        .Where(c => c.Trigger.Command.Name == commandNameFilter)
                        .ToList();
            }

            var commands = new List<WorkflowCommand>();

            foreach (var transitionDefinition in commandTransitions.Where(c => c.IsAlwaysTransition))
            {
                var command = WorkflowCommand.Create(Guid.NewGuid(), transitionDefinition, processDefinition, ParametersSerializer.Deserialize);
                if (!commands.Any(c=>c.CommandName.Equals(command.CommandName)))
                    commands.Add(command);
            }
            return commands;
        }

        /// <summary>
        /// Return the list of commands which is availiable from current activity for specified user
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="identityId">User id for whom formed initial commands list</param>
        /// <returns>List of <see cref="WorkflowCommand"/> commands</returns>
        public IEnumerable<WorkflowCommand> GetAvailableCommands(Guid processId, string identityId)
        {
            return GetAvailableCommands(processId, new List<string>() {identityId});
        }

        /// <summary>
        /// Return the list of commands which is availiable from current activity for specified user
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="identityIds">List of User ids for which formed initial commands list</param>
        /// <param name="commandNameFilter">Selects only the specified command if not null</param>
        /// <param name="mainIdentityId">User id for priority check of rules</param>
        /// <returns>List of <see cref="WorkflowCommand"/> commands</returns>
        public IEnumerable<WorkflowCommand> GetAvailableCommands(Guid processId, IEnumerable<string> identityIds, string commandNameFilter = null, string mainIdentityId = null)
        {
            LicenseSemaphore?.Wait();
            try
            {
                var processInstance = GetProcessInstance(processId);


                if (IsAutoUpdateSchemeBeforeGetAvailableCommands)
                {
                    processInstance = UpdateScheme(processInstance);

                    if (processInstance == null)
                    {
                        return new List<WorkflowCommand>();
                    }
                }

                //Getting commands from all child subprocesses
                var processTree = GetProcessInstancesTree(processInstance);
                var childrenIds = processTree.GetNodeById(processInstance.ProcessId).GetAllChildrenIds();

                var identities = identityIds as IList<string> ?? identityIds.ToList();

                var commands = GetCommands(processInstance, identities, commandNameFilter, mainIdentityId, processTree);

                foreach (var childId in childrenIds)
                {
                    var childProcessInstance = GetProcessInstance(childId);
                    commands.AddRange(GetCommands(childProcessInstance, identities, commandNameFilter, mainIdentityId, processTree));
                }

                if (processInstance.IsSubprocess)
                {
                    commands.ForEach(c => c.IsForSubprocess = true);
                }
                else
                {
                    commands.Where(c => !c.ProcessId.Equals(processId)).ToList().ForEach(c => c.IsForSubprocess = true);
                }

                var filteredCommands = new List<WorkflowCommand>();
                foreach (var workflowCommand in commands)
                {
                    if (
                        !filteredCommands.Any(
                            c =>
                                c.CommandName.Equals(workflowCommand.CommandName) &&
                                c.ProcessId == workflowCommand.ProcessId))
                    {
                        filteredCommands.Add(workflowCommand);
                    }
                }
                return filteredCommands;

            }
            finally
            {
                LicenseSemaphore?.Release();
            }
        }

        private ProcessInstance GetProcessInstance(Guid processId)
        {
            var processInstance = Builder.GetProcessInstance(processId);
            PersistenceProvider.FillProcessParameters(processInstance);
            return processInstance;
        }

        private List<WorkflowCommand> GetCommands(ProcessInstance processInstance, IEnumerable<string> identityIds,
            string commandNameFilter, string mainIdentityId, ProcessInstancesTree processInstancesTree)
        {
            var identityIdsList = !string.IsNullOrWhiteSpace(mainIdentityId)
                ? identityIds.Except(new List<string> {mainIdentityId}).ToList()
                : identityIds.ToList();

            var processId = processInstance.ProcessId;
            var currentActivity = processInstance.ProcessScheme.FindActivity(processInstance.CurrentActivityName);

            List<TransitionDefinition> commandTransitions;
            if (string.IsNullOrEmpty(commandNameFilter))
                commandTransitions = processInstance.ProcessScheme.GetCommandTransitions(currentActivity, ForkTransitionSearchType.Both).ToList();
            else
            {
                commandTransitions =
                    processInstance.ProcessScheme.GetCommandTransitions(currentActivity, ForkTransitionSearchType.Both)
                        .Where(c => c.Trigger.Command.Name == commandNameFilter)
                        .ToList();
            }


            var commands = new List<WorkflowCommand>();

            foreach (
                var transitionDefinition in
                    commandTransitions.OrderBy(
                        t =>
                            t.Classifier == TransitionClassifier.Direct
                                ? 0
                                : t.Classifier == TransitionClassifier.Reverse ? 1 : 2)
                        .ThenBy(t => t.Trigger.Command.Name))
            {
                if (transitionDefinition.ForkType == TransitionForkType.ForkStart)
                {
                    var currenNode = processInstancesTree.GetNodeById(processId);
                    if (currenNode.HaveChildWithName(transitionDefinition.Name))
                        continue;
                }

                List<string> availiableIds = null;
                if (!string.IsNullOrWhiteSpace(mainIdentityId) &&
                    ValidateActor(processInstance, mainIdentityId, transitionDefinition))
                    availiableIds = new List<string> {mainIdentityId};

                if (availiableIds == null)
                    availiableIds =
                        identityIdsList.Where(
                            id => ValidateActor(processInstance, id, transitionDefinition))
                            .ToList();

                if (availiableIds.Any())
                {
                    var command = WorkflowCommand.Create(processId, transitionDefinition, processInstance.ProcessScheme, ParametersSerializer.Deserialize);
                    foreach (var availiableId in availiableIds)
                    {
                        command.AddIdentity(availiableId);
                    }

                    commands.Add(command);
                }
            }


            return commands;
        }

        /// <summary>
        /// Execute specified command for specified users 
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <param name="identityId">The user id which execute command</param>
        /// <param name="impersonatedIdentityId">The user id for whom executes command (impersonation)</param>
        /// <returns>Result of the execution</returns>
        public CommandExeutionResult ExecuteCommand(WorkflowCommand command, string identityId, string impersonatedIdentityId)
        {
           return ExecuteCommandAsync(command, identityId, impersonatedIdentityId).Result;
        }

        private async Task ChooseTransitionAndCreateSubprocess(List<TransitionDefinition> forkTransitions, ProcessInstance processInstance, CancellationToken token, ProcessInstancesTree tree = null)
        {
            var forkAlwaysTransitions = forkTransitions.Where(t => t.IsAlwaysTransition).ToList();
            var forkConditionalTransitions = forkTransitions.Where(t => !t.IsAlwaysTransition).ToList();

            if (tree == null && (forkAlwaysTransitions.Any() || forkConditionalTransitions.Any()))
            {
                tree = GetProcessInstancesTree(processInstance);
            }

            if (forkAlwaysTransitions.Any())
            {
                await CreateSubprocesses(processInstance, forkAlwaysTransitions, token, tree).ConfigureAwait(false);
            }

            if (forkConditionalTransitions.Any())
            {
                var executor = new ActivityExecutor(this, false);
                var newExecutionParameters = new List<ExecutionRequestParameters>();
                newExecutionParameters.AddRange(forkConditionalTransitions.Select(at => ExecutionRequestParameters.Create(processInstance, at)));
                //using Activity executor to choose executing transition
                newExecutionParameters.ForEach(p => p.Methods = new ActionDefinitionReference[0]);

                var res = await executor.Execute(newExecutionParameters, token).ConfigureAwait(false);

                if (!PreExecuteProcessResponse(res))
                {
                    var transition = processInstance.ProcessScheme.FindTransition(res.ExecutedTransitionName);

                    await CreateSubprocesses(processInstance, new List<TransitionDefinition> {transition}, token, tree).ConfigureAwait(false);
                }
            }
        }


        /// <summary>
        /// Execute specified command for specified users 
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="identityId">The user id which execute command</param>
        /// <param name="impersonatedIdentityId">The user id for whom executes command (impersonation)</param>
        /// <param name="command">Command to execute</param>
        /// <returns>Result of the execution</returns>
        [Obsolete(@"Since version 1.5 argument processId not used to determine the process, the process is determined by the command.ProcessId property. 
Please use the following method: public void ExecuteCommand(WorkflowCommand command, string identityId, string impersonatedIdentityId)")]
        public CommandExeutionResult ExecuteCommand(Guid processId, string identityId, string impersonatedIdentityId, WorkflowCommand command)
        {
            return ExecuteCommandAsync(processId, identityId, impersonatedIdentityId, command).Result; //.RunSynchronously();
        }

        /// <summary>
        /// Return the initial state for process scheme
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="processParameters">Parameters for creating scheme of process</param>
        /// <returns><see cref="WorkflowState"/> object</returns>
        public WorkflowState GetInitialState(string schemeCode, IDictionary<string, object> processParameters = null)
        {
            var processDefinition = processParameters != null
                ? Builder.GetProcessScheme(schemeCode, processParameters)
                : Builder.GetProcessScheme(schemeCode);

            var initialActivity = processDefinition.InitialActivity;

            return new WorkflowState()
            {
                Name = initialActivity.State,
                SchemeCode = schemeCode,
                VisibleName = processDefinition.GetLocalizedStateName(initialActivity.State, CultureInfo.CurrentCulture)
            };
        }

        /// <summary>
        /// Return the current state of specified process
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <returns><see cref="WorkflowState"/> object</returns>
        public WorkflowState GetCurrentState(Guid processId)
        {
            var processInstance = Builder.GetProcessInstance(processId);
            PersistenceProvider.FillSystemProcessParameters(processInstance);
            var stateName = processInstance.GetParameter(DefaultDefinitions.ParameterCurrentState.Name).Value;
            var schemeCode = processInstance.ProcessScheme.Name;
            return stateName == null
                ? null
                : new WorkflowState()
                {
                    Name = stateName.ToString(),
                    SchemeCode = schemeCode,
                    VisibleName =
                        processInstance.ProcessScheme.GetLocalizedStateName(stateName.ToString(),
                            CultureInfo.CurrentCulture)
                };
        }

        /// <summary>
        /// Return the current state name of specified process
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <returns>Name of current state</returns>
        public string GetCurrentStateName(Guid processId)
        {
            var processInstance = Builder.GetProcessInstance(processId);
            PersistenceProvider.FillSystemProcessParameters(processInstance);
            var stateName = processInstance.GetParameter(DefaultDefinitions.ParameterCurrentState.Name).Value;
            return stateName == null
                ? null
                : stateName.ToString();
        }

        /// <summary>
        /// Return the current activity name of specified process
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <returns>Current activity name</returns>
        public string GetCurrentActivityName(Guid processId)
        {
            var processInstance = Builder.GetProcessInstance(processId);

            PersistenceProvider.FillSystemProcessParameters(processInstance);

            return processInstance.GetParameter(DefaultDefinitions.ParameterCurrentActivity.Name).Value.ToString();
        }

        /// <summary>
        /// Get the list of all states which available for set of specified process localized in current culture
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <returns>List of <see cref="WorkflowState"/> objects</returns>
        public IEnumerable<WorkflowState> GetAvailableStateToSet(Guid processId)
        {
            return GetAvailableStateToSet(processId, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Get the list of all states which available for set of specified process localized in specified culture
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="culture">Culture to localize state names</param>
        /// <returns>List of <see cref="WorkflowState"/> objects</returns>
        public IEnumerable<WorkflowState> GetAvailableStateToSet(Guid processId, CultureInfo culture)
        {
            if (LicenseSemaphore != null)
                LicenseSemaphore.Wait();
            try
            {
                var processInstance = Builder.GetProcessInstance(processId);
                
                var activities = processInstance.ProcessScheme.Activities.Where(a => a.IsForSetState && a.IsState && a.NestingLevel == processInstance.ProcessScheme.InitialActivity.NestingLevel);
                return
                    activities.Select(
                        activity =>
                            new WorkflowState
                            {
                                Name = activity.State,
                                VisibleName = processInstance.GetLocalizedStateName(activity.State, culture),
                                SchemeCode = processInstance.ProcessScheme.Name
                            }).ToList();
            }
            finally
            {
                if (LicenseSemaphore != null)
                    LicenseSemaphore.Release();
            }
        }

        /// <summary>
        /// Get the list of all states which available for set of specified scheme in current culture
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="parameters">The parameters for creating scheme of process</param>
        /// <returns >List of <see cref="WorkflowState"/> objects</returns>
        public IEnumerable<WorkflowState> GetAvailableStateToSet(string schemeCode,
            IDictionary<string, object> parameters = null)
        {
            return GetAvailableStateToSet(schemeCode, CultureInfo.CurrentCulture, parameters);
        }

        /// <summary>
        /// Get the list of all states which available for set of specified scheme in specified culture
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="culture">Culture to localize state names</param>
        /// <param name="parameters">The parameters for creating scheme of process</param>
        /// <returns>List of <see cref="WorkflowState"/> objects</returns>
        public IEnumerable<WorkflowState> GetAvailableStateToSet(string schemeCode, CultureInfo culture,
            IDictionary<string, object> parameters = null)
        {
            if (LicenseSemaphore != null)
                LicenseSemaphore.Wait();

            try
            {
                var processScheme = parameters == null
                    ? Builder.GetProcessScheme(schemeCode)
                    : Builder.GetProcessScheme(schemeCode, parameters);

                var activities = processScheme.Activities.Where(a => a.IsForSetState && a.IsState);
                return
                    activities.Select(
                        activity =>
                            new WorkflowState
                            {
                                Name = activity.State,
                                VisibleName = processScheme.GetLocalizedStateName(activity.State, culture),
                                SchemeCode = processScheme.Name
                            })
                        .ToList();
            }
            finally
            {
                if (LicenseSemaphore != null)
                    LicenseSemaphore.Release();
            }
        }

        /// <summary>
        /// Set specified state for specified process 
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="identityId">The user id which set the state</param>
        /// <param name="impersonatedIdentityId">The user id for whom sets the state (impersonation)</param>
        /// <param name="stateName">State name to set</param>
        /// <param name="parameters">Dictionary of ProcessInstance parameters which transferred to executed actions</param>
        /// <param name="preventExecution">Actions due to transition process do not executed if true</param>
        public void SetState(Guid processId, string identityId, string impersonatedIdentityId, string stateName,
            IDictionary<string, object> parameters = null, bool preventExecution = false)
        {
            var processInstance = Builder.GetProcessInstance(processId);
            var activityToSet =
                processInstance.ProcessScheme.Activities.FirstOrDefault(
                    a => a.IsState && 
                    //a.IsForSetState &&
                    a.State == stateName);

            if (activityToSet == null)
                throw new ActivityNotFoundException();

            if (!preventExecution)
            {
                if (parameters == null)
                {
                    parameters = new Dictionary<string, object>();
                }

                SetActivityWithExecution(identityId, impersonatedIdentityId, parameters, activityToSet, processInstance);
            }
            else
                SetActivityWithoutExecution(activityToSet, processInstance);
        }

        /// <summary>
        /// Set specified activity as current without execution of the implementation of the activity
        /// </summary>
        /// <param name="activityToSet">Activity to set</param>
        /// <param name="processInstance">Process instance for set activity as current</param>
        /// <param name="doNotSetRunningStatus">The status of the process - <see cref="ProcessStatus.Running"/> will not be set if true</param>
        public void SetActivityWithoutExecution(ActivityDefinition activityToSet, ProcessInstance processInstance, bool doNotSetRunningStatus = false)
        {
            SetActivityWithoutExecutionAsync(activityToSet, processInstance, doNotSetRunningStatus).Wait(); //.RunSynchronously();
        }

        /// <summary>
        /// Set specified activity as current without execution of the implementation of the activity (Async version)
        /// </summary>
        /// <param name="activityToSet">Activity to set</param>
        /// <param name="processInstance">Process instance for set activity as current</param>
        /// <param name="doNotSetRunningStatus">The status of the process - <see cref="ProcessStatus.Running"/> will not be set if true</param>
        public async Task SetActivityWithoutExecutionAsync(ActivityDefinition activityToSet, ProcessInstance processInstance, bool doNotSetRunningStatus = false)
        {
            if (!doNotSetRunningStatus)
                await SetProcessNewStatus(processInstance, ProcessStatus.Running).ConfigureAwait(false);

            try
            {
                PersistenceProvider.FillSystemProcessParameters(processInstance);
                var from = processInstance.CurrentActivity;
                var to = activityToSet;
                PersistenceProvider.UpdatePersistenceState(processInstance, TransitionDefinition.Create(@from, to));
            }
            catch (Exception ex)
            {
                throw new Exception($"Workflow Id={processInstance.ProcessId}", ex);
            }
            finally
            {
                if (!doNotSetRunningStatus)
                   await SetProcessNewStatus(processInstance, ProcessStatus.Idled).ConfigureAwait(false);
            }
        }


        /// <summary>
        /// Set specified activity as current and executing the implementation of the activity
        /// </summary>
        /// <param name="identityId">The user id which set the activity</param>
        /// <param name="impersonatedIdentityId">The user id for whom sets the activity (impersonation)</param>
        /// <param name="parameters">Dictionary of ProcessInstance parameters which transferred to executed actions</param>
        /// <param name="activityToSet">Activity to set</param>
        /// <param name="processInstance">Process instance for set activity as current</param>
        /// <param name="doNotSetRunningStatus">The status of the process - <see cref="ProcessStatus.Running"/> will not be set if true</param>
        public void SetActivityWithExecution(string identityId, string impersonatedIdentityId,
            IDictionary<string, object> parameters, ActivityDefinition activityToSet, ProcessInstance processInstance, bool doNotSetRunningStatus = false)
        {
            SetActivityWithExecutionAsync(identityId, impersonatedIdentityId, parameters, activityToSet, processInstance, doNotSetRunningStatus).Wait(); //.RunSynchronously();
        }

        /// <summary>
        /// Set specified activity as current and executing the implementation of the activity (Async version)
        /// </summary>
        /// <param name="identityId">The user id which set the activity</param>
        /// <param name="impersonatedIdentityId">The user id for whom sets the activity (impersonation)</param>
        /// <param name="parameters">Dictionary of ProcessInstance parameters which transferred to executed actions</param>
        /// <param name="activityToSet">Activity to set</param>
        /// <param name="processInstance">Process instance for set activity as current</param>
        /// <param name="doNotSetRunningStatus">The status of the process - <see cref="ProcessStatus.Running"/> will not be set if true</param>
        /// <param name="token">Cancellation token</param>
        public async Task SetActivityWithExecutionAsync(string identityId, string impersonatedIdentityId,
            IDictionary<string, object> parameters, ActivityDefinition activityToSet, ProcessInstance processInstance, bool doNotSetRunningStatus = false, CancellationToken token = default(CancellationToken))
        {
            if (!doNotSetRunningStatus)
                await SetProcessNewStatus(processInstance, ProcessStatus.Running).ConfigureAwait(false);

            try
            {
                PersistenceProvider.FillSystemProcessParameters(processInstance);
                PersistenceProvider.FillPersistedProcessParameters(processInstance);

                foreach (var commandParameter in parameters)
                {
                    processInstance.SetParameter(commandParameter.Key, commandParameter.Value);
                }

                if (string.IsNullOrEmpty(processInstance.CurrentCommand))
                    processInstance.CurrentCommand = DefaultDefinitions.CommandSetState.Name;

                processInstance.IdentityId = identityId;
                processInstance.ImpersonatedIdentityId = impersonatedIdentityId;
                
            }
            catch (Exception)
            {
                if (!doNotSetRunningStatus)
                    await SetProcessNewStatus(processInstance, ProcessStatus.Idled).ConfigureAwait(false);
                throw;
            }

            try
            {
                processInstance.SetStartTransitionalProcessActivity();
                await Bus.QueueExecution(ExecutionRequestParameters.Create(processInstance, activityToSet, new List<ConditionDefinition> {ConditionDefinition.Always}), token)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (!doNotSetRunningStatus)
                    await SetProcessNewStatus(processInstance, ProcessStatus.Idled).ConfigureAwait(false);
                throw new Exception($"Workflow Id={processInstance.ProcessId}", ex);
            }
        }

        /// <summary>
        /// Get the list of user ids which can execute any command for specified process activity
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="beginningWithRoot">If true the list of actors will be obtained from the root process, even if you passed Id of the subprocess</param>
        /// <param name="activityName">Activity name in which transitions are checked. Current activity if null.</param>
        /// <returns>List of user ids</returns>
        public IEnumerable<string> GetAllActorsForAllCommandTransitions(Guid processId, bool beginningWithRoot = false, string activityName = null)
        {
            return GetAllActorsForCommandTransitions(processId, null, beginningWithRoot, activityName);
        }

        /// <summary>
        /// Get the list of user ids which can execute any command bound whith direct transitions (by transition classifier) for specified process activity
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="beginningWithRoot">If true the list of actors will be obtained from the root process, even if you passed Id of the subprocess</param>
        /// <param name="activityName">Activity name in which transitions are checked. Current activity if null.</param>
        /// <returns>List of user ids</returns>
        public IEnumerable<string> GetAllActorsForDirectCommandTransitions(Guid processId, bool beginningWithRoot = false, string activityName = null)
        {
            return GetAllActorsForCommandTransitions(processId,
                new List<TransitionClassifier> {TransitionClassifier.Direct}, beginningWithRoot, activityName);
        }

        /// <summary>
        /// Get the list of user ids which can execute any command bound whith direct or undefined transitions (by transition classifier) for specified process activity
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="beginningWithRoot">If true the list of actors will be obtained from the root process, even if you passed Id of the subprocess</param> 
        /// <param name="activityName">Activity name in which transitions are checked. Current activity if null.</param>
        /// <returns>List of user ids</returns>
        public IEnumerable<string> GetAllActorsForDirectAndUndefinedCommandTransitions(Guid processId, bool beginningWithRoot = false,
            string activityName = null)
        {
            return GetAllActorsForCommandTransitions(processId,
                new List<TransitionClassifier> {TransitionClassifier.Direct, TransitionClassifier.NotSpecified},
                beginningWithRoot, activityName);
        }

        /// <summary>
        /// Get the list of user ids which can execute any command bound whith reverse transitions (by transition classifier) for specified process activity
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="beginningWithRoot">If true the list of actors will be obtained from the root process, even if you passed Id of the subprocess</param>
        /// <param name="activityName">Activity name in which transitions are checked. Current activity if null.</param>
        /// <returns>List of user ids</returns>
        public IEnumerable<string> GetAllActorsForReverseCommandTransitions(Guid processId, bool beginningWithRoot = false, string activityName = null)
        {
            return GetAllActorsForCommandTransitions(processId,
                new List<TransitionClassifier> {TransitionClassifier.Reverse}, beginningWithRoot, activityName);
        }

        /// <summary>
        /// Get the list of user ids which can execute any command bound whith a transitions selected by filter (by transition classifier) for specified process activity
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="classifiers">Filter for transitions by transition classifier</param>
        /// <param name="beginningWithRoot">If true the list of actors will be obtained from the root process, even if you passed Id of the subprocess</param>
        /// <param name="activityName">Activity name in which transitions are checked. Current activity if null.</param>
        /// <returns>List of user ids</returns>
        public IEnumerable<string> GetAllActorsForCommandTransitions(Guid processId,  List<TransitionClassifier> classifiers, bool beginningWithRoot = false, string activityName = null)
        {
            var processInstance = Builder.GetProcessInstance(processId);
            PersistenceProvider.FillSystemProcessParameters(processInstance);

            return GetAllActorsForCommandTransitionsInternal(processInstance, classifiers, beginningWithRoot, activityName);
        }

        private IEnumerable<string> GetAllActorsForCommandTransitionsInternal(ProcessInstance processInstance, List<TransitionClassifier> classifiers, bool beginningWithRoot, string activityName)
        {
            var result = new List<string>();

            var tree = GetProcessInstancesTree(processInstance);

            var childrenIds = beginningWithRoot ? tree.Root.GetAllChildrenIds() : tree.GetNodeById(processInstance.ProcessId).GetAllChildrenIds();
            if (beginningWithRoot)
                childrenIds.Add(processInstance.RootProcessId);
            var transitionsToExclude = tree.Root.GetAllChildrenNames();

            var activity = string.IsNullOrEmpty(activityName)
                ? processInstance.ProcessScheme.FindActivity(processInstance.CurrentActivityName)
                : processInstance.ProcessScheme.FindActivity(activityName);

            if (activity != null &&
                activity.NestingLevel == processInstance.ProcessScheme.InitialActivity.NestingLevel)
            {
                result.AddRange(GetAllActorsForCommandTransitions(processInstance, activity, classifiers,
                    transitionsToExclude));
            }

            childrenIds = childrenIds.Distinct().ToList();
            childrenIds.Remove(processInstance.ProcessId);


            foreach (var childId in childrenIds)
            {
                result.AddRange(GetAllActorsForCommandTransitions(activityName, classifiers, childId, transitionsToExclude));
            }

            return result.Distinct();
        }

        /// <summary>
        /// Get the list of user ids which can execute any command bound whith a transitions selected by filter (by transition classifier)
        /// for executed activity (inside of a transitional process) or for current activity (when a process is idled).
        /// Using of this method is preferable for notification of users which can execute next commands.
        /// </summary>
        /// <param name="processInstance">Process instance</param>
        /// <param name="classifiers">Filter for transitions by transition classifier</param>
        /// <returns>List of user ids</returns>
        public IEnumerable<string> GetAllActorsForCommandTransitions(ProcessInstance processInstance, List<TransitionClassifier> classifiers = null)
        {
            return GetAllActorsForCommandTransitionsInternal(processInstance, classifiers, false,
                processInstance.ExecutedActivity != null ? processInstance.ExecutedActivity.Name : processInstance.CurrentActivityName);

        }

       
        /// <summary>
        /// Get the list of user ids which can execute any command bound whith a transitions selected by filter. This method is prefearable to use when you are using subprocesses
        /// </summary>
        /// <param name="filter">Sets the current process and the method of searching in the Process Instances tree</param>
        /// <param name="classifiers">Filter for transitions by transition classifier</param>
        /// <returns>List of user ids</returns>
        public IEnumerable<string> GetAllActorsForCommandTransitions(TreeSearchFilter filter, List<TransitionClassifier> classifiers = null)
        {
            var processInstance = filter.ProcessInstance;

            if (processInstance == null)
            {
                processInstance = Builder.GetProcessInstance(filter.ProcessId);
                PersistenceProvider.FillSystemProcessParameters(processInstance);
            }

            var result = new List<string>();

            var tree = GetProcessInstancesTree(processInstance);

            var transitionsToExclude = tree.Root.GetAllChildrenNames(); //Exclude all subpocess transitions

            var processInstancesCache = new Dictionary<Guid, ProcessInstance>() {{processInstance.ProcessId, processInstance}};

            var startPoint = tree.GetNodeById(processInstance.ProcessId);

            if (filter.StartFrom == TreeSearchStart.FromParent && processInstance.ParentProcessId.HasValue)
            {
                startPoint = tree.GetNodeById(processInstance.ParentProcessId.Value);
            }
            else if (filter.StartFrom == TreeSearchStart.FromRoot)
            {
                startPoint = tree.Root;
            }

            var searchProcessIds = new List<Guid>();

            if (filter.Include == TreeSearchInclude.AllUpIncludeStartPoint || filter.Include == TreeSearchInclude.AllUpExcludeWithoutStartPoint)
            {
                var current = startPoint;
                while (!current.IsRoot)
                {
                    current = current.Parent;
                    searchProcessIds.Add(current.Id);
                }
            }
            else if (filter.Include == TreeSearchInclude.AllDownIncludeStartPoint || filter.Include == TreeSearchInclude.AllDownExcludeStartPoint)
            {
                searchProcessIds = startPoint.GetAllChildrenIds();
            }

            if (filter.Include == TreeSearchInclude.OnlyStartPoint || filter.Include == TreeSearchInclude.AllUpIncludeStartPoint ||
                filter.Include == TreeSearchInclude.AllDownIncludeStartPoint)
            {
                result.AddRange(GetAllActorsInternal(startPoint.Id, processInstancesCache, null, classifiers, transitionsToExclude));
            }

            foreach (var anotherId in searchProcessIds)
            {
                result.AddRange(GetAllActorsInternal(anotherId, processInstancesCache, null, classifiers, transitionsToExclude));
            }

            return result.Distinct();
        }

        private List<string> GetAllActorsInternal(Guid processId, Dictionary<Guid, ProcessInstance> cache, string specificActivityName,
            List<TransitionClassifier> classifiers, List<string> transitionsToExclude)
        {
            ProcessInstance processInstance;
            if (!cache.ContainsKey(processId))
            {
                processInstance = Builder.GetProcessInstance(processId);
                PersistenceProvider.FillSystemProcessParameters(processInstance);
                cache.Add(processId,processInstance);
            }
            else
            {
                processInstance = cache[processId];
            }

            var activityToSearch = processInstance.ExecutedActivity ?? processInstance.CurrentActivity;

            if (!string.IsNullOrEmpty(specificActivityName))
            {
                ActivityDefinition newActivityToSearch = null;

                try
                {
                    newActivityToSearch = processInstance.ProcessScheme.FindActivity(specificActivityName);
                }
                catch (ActivityNotFoundException)
                {}

                if (newActivityToSearch != null && newActivityToSearch.NestingLevel == processInstance.ProcessScheme.InitialActivity.NestingLevel)
                {
                    activityToSearch = newActivityToSearch;
                }
            }

            return GetAllActorsForCommandTransitions(processInstance, activityToSearch, classifiers, transitionsToExclude);

        }

        private List<string> GetAllActorsForCommandTransitions(string activityName, List<TransitionClassifier> classifiers, Guid childId,
             List<string> transitionsToExclude)
        {
            var childProcess = Builder.GetProcessInstance(childId);
            PersistenceProvider.FillSystemProcessParameters(childProcess);

            var childProcessActivity = string.IsNullOrEmpty(activityName)
                ? childProcess.ProcessScheme.FindActivity(childProcess.CurrentActivityName)
                : childProcess.ProcessScheme.FindActivity(activityName);

            if (childProcessActivity != null &&
                childProcessActivity.NestingLevel == childProcess.ProcessScheme.InitialActivity.NestingLevel)
            {
               return  GetAllActorsForCommandTransitions(childProcess, childProcessActivity, classifiers, transitionsToExclude);
            }

            return new List<string>(0);
        }

        private List<string> GetAllActorsForCommandTransitions(ProcessInstance processInstance, ActivityDefinition selectedActivity, List<TransitionClassifier> classifiers,
            List<string> transitionsToExclude)
        {
            var result = new List<string>();
            PersistenceProvider.FillPersistedProcessParameters(processInstance);

            var commandTransitions =
                processInstance.ProcessScheme.GetCommandTransitions(selectedActivity)
                    .Where(
                        ct =>
                            (classifiers == null || classifiers.Contains(ct.Classifier)) &&
                            !transitionsToExclude.Contains(ct.Name))
                    .ToList();

            foreach (var ct in commandTransitions)
            {
                result.AddRange(GetActors(processInstance, ct));
            }
            return result;
        }

        /// <summary>
        /// Get procees definition (parsed scheme) for specified process
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <returns><see cref="ProcessDefinition"/> object</returns>
        public ProcessDefinition GetProcessScheme(Guid processId)
        {
            return Builder.GetProcessInstance(processId).ProcessScheme;
        }

        /// <summary>
        /// Get localized state name for specified scheme in current culture
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="stateName">State name to localize</param>
        /// <param name="parameters">The parameters for creating scheme of process</param>
        /// <returns>Localized state name</returns>
        public string GetLocalizedStateNameBySchemeCode(string schemeCode, string stateName,
            IDictionary<string, object> parameters = null)
        {
            var processDefinition = parameters != null
                ? Builder.GetProcessScheme(schemeCode, parameters)
                : Builder.GetProcessScheme(schemeCode);
            return processDefinition.GetLocalizedStateName(stateName, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Get status of specified process
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <returns><see cref="ProcessStatus"/> object</returns>
        public ProcessStatus GetProcessStatus(Guid processId)
        {
            return PersistenceProvider.GetInstanceStatus(processId);
        }
        
        /// <summary>
        /// Get localized state name for specified scheme in current culture
        /// </summary>
        /// <param name="schemeId">Id of the scheme</param>
        /// <param name="stateName">State name to localize</param>
        /// <returns>Localized state name</returns>
        public string GetLocalizedStateNameBySchemeId(Guid schemeId, string stateName)
        {
            var processscheme = Builder.GetProcessScheme(schemeId);
            return processscheme.GetLocalizedStateName(stateName, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Get localized state name for specified process in current culture
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="stateName">State name to localize</param>
        /// <returns>Localized state name</returns>
        public string GetLocalizedStateName(Guid processId, string stateName)
        {
            var processInstance = Builder.GetProcessInstance(processId);
            return processInstance.GetLocalizedStateName(stateName, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Get localized command name for specified process in current culture
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="commandName">Command name to localize</param>
        /// <returns>Localized command name</returns>
        public string GetLocalizedCommandName(Guid processId, string commandName)
        {
            var processInstance = Builder.GetProcessInstance(processId);
            return processInstance.GetLocalizedCommandName(commandName, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Get localized command name for specified scheme in current culture
        /// </summary>
        /// <param name="schemeId">Id of the scheme</param>
        /// <param name="commandName">Command name to localize</param>
        /// <returns>Localized command name</returns>
        public string GetLocalizedCommandNameBySchemeId(Guid schemeId, string commandName)
        {
            var processscheme = Builder.GetProcessScheme(schemeId);
            return processscheme.GetLocalizedCommandName(commandName, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Get process instance with all parameters for specified process id
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <returns><see cref="ProcessInstance"/> object</returns>
        public ProcessInstance GetProcessInstanceAndFillProcessParameters(Guid processId)
        {
            var pi = Builder.GetProcessInstance(processId);
            PersistenceProvider.FillProcessParameters(pi);
            return pi;
        }

        /// <summary>
        /// Returns the history of process
        /// </summary>
        /// <param name="processId">Id of the process</param>
        /// <returns></returns>
        public List<ProcessHistoryItem> GetProcessHistory(Guid processId)
        {
            return PersistenceProvider.GetProcessHistory(processId);
        }

        #endregion

        #region Workflow Public API Async

        /// <summary>
        /// Check existence of the specified process 
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <returns>True if process with specified identifier is exists</returns>
        public async Task<bool> IsProcessExistsAsync(Guid processId)
        {
            return await Task.Run(() => IsProcessExists(processId));
        }

        /// <summary>
        /// Create instance of process (async version)
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="processId">Process id</param>
        /// <param name="token">Cancellation token</param>
        public Task CreateInstanceAsync(string schemeCode, Guid processId, CancellationToken token = default(CancellationToken))
        {
            return CreateInstanceAsync(new CreateInstanceParams(schemeCode, processId), token);
        }

        /// <summary>
        /// Create instance of process (async version)
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="processId">Process id</param>
        /// <param name="identityId">The user id which execute initial command if command is available</param>
        /// <param name="impersonatedIdentityId">The user id for whom executes initial command if command is available</param>
        /// <param name="token">Cancellation token</param>
        public Task CreateInstanceAsync(string schemeCode, Guid processId, string identityId,
            string impersonatedIdentityId, CancellationToken token = default(CancellationToken))
        {
            return CreateInstanceAsync(new CreateInstanceParams(schemeCode, processId)
            {
                IdentityId = identityId,
                ImpersonatedIdentityId = impersonatedIdentityId,
            }, token);
        }


        /// <summary>
        /// Create instance of process (async version)
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="processId">Process id</param>
        /// <param name="schemeCreationParameters">The parameters for creating scheme of process</param>
        /// <param name="token">Cancellation token</param>
        public Task CreateInstanceAsync(string schemeCode, Guid processId, IDictionary<string, object> schemeCreationParameters, CancellationToken token = default(CancellationToken))
        {
            return CreateInstanceAsync(new CreateInstanceParams(schemeCode, processId)
            {
                SchemeCreationParameters = schemeCreationParameters
            }, token);
        }

        /// <summary>
        /// Create instance of process (async version)
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="processId">Process id</param>
        /// <param name="identityId">The user id which execute initial command if command is available</param>
        /// <param name="impersonatedIdentityId">The user id for whom executes initial command if command is available</param>
        /// <param name="schemeCreationParameters">The parameters for creating scheme of process</param>
        /// <param name="token">Cancellation token</param>
        public Task CreateInstanceAsync(string schemeCode, Guid processId, string identityId,
            string impersonatedIdentityId, IDictionary<string, object> schemeCreationParameters, CancellationToken token = default(CancellationToken))
        {
            return CreateInstanceAsync(new CreateInstanceParams(schemeCode, processId)
            {
                IdentityId = identityId,
                ImpersonatedIdentityId = impersonatedIdentityId,
                SchemeCreationParameters = schemeCreationParameters
            }, token);
        }

        /// <summary>
        /// Create instance of the process (async version)
        /// </summary>
        /// <param name="createInstanceParams">Parameters for creaition of an instance of a process</param>
        /// <param name="token">Cancellation token</param>
        public Task CreateInstanceAsync(CreateInstanceParams createInstanceParams, CancellationToken token = default(CancellationToken))
        {
            var processInstance = Builder.CreateNewProcess(createInstanceParams.ProcessId, createInstanceParams.SchemeCode,
                createInstanceParams.SchemeCreationParameters ?? new Dictionary<string, object>());

            return InitCreatedProcess(createInstanceParams.IdentityId, createInstanceParams.ImpersonatedIdentityId, processInstance, createInstanceParams.InitialProcessParameters,
                token);
        }

        /// <summary>
        /// Delete instance of the process and all child subprocesses.  (async version)
        /// </summary>
        /// <param name="processId">Process id</param>
        public async Task DeleteInstanceAsync(Guid processId)
        {
            await Task.Run(() => DeleteInstance(processId));
        }

        /// <summary>
        /// Set flag IsObsolete for all schemas of process with specific code and parameters (async version)
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="parameters">The parameters for creating scheme of process</param>
        public async Task SetSchemeIsObsoleteAsync(string schemeCode, Dictionary<string, object> parameters)
        {
            await Task.Run(() => SetSchemeIsObsolete(schemeCode, parameters));
        }

        /// <summary>
        /// Set flag IsObsolete for all schemas of process with specific code (async version)
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        public async Task SetSchemeIsObsoleteAsync(string schemeCode)
        {
            await Task.Run(() => SetSchemeIsObsolete(schemeCode));
        }

        /// <summary>
        /// Sets new value of named timer (async version)
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="timerName">Timer name in Scheme</param>
        /// <param name="newValue">New value of the timer</param>
        public Task SetTimerValueAsync(Guid processId, string timerName, DateTime newValue)
        {
            return TimerManager != null ? TimerManager.SetTimerValue(processId, timerName, newValue) : Task.FromResult(false);
        }

        /// <summary>
        /// Resets value of named timer (async version)
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="timerName">Timer name in Scheme</param>
        public Task ResetTimerValueAsync(Guid processId, string timerName)
        {
            return TimerManager != null ? TimerManager.ResetTimerValue(processId, timerName) : Task.FromResult(false);
        }
        
        /// <summary>
        /// Updating scheme of specific process 
        /// </summary>
        /// <param name="processId">Process id</param>
        public async Task UpdateSchemeIfObsoleteAsync(Guid processId)
        {
            await Task.Run(() => UpdateSchemeIfObsolete(processId));
        }

        /// <summary>
        /// Updating scheme of specific process (async version)
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="parameters">Parameters for creating scheme of process</param>
        public async Task UpdateSchemeIfObsoleteAsync(Guid processId, IDictionary<string, object> parameters)
        {
            await Task.Run(() => UpdateSchemeIfObsolete(processId, parameters));
        }

        /// <summary>
        /// Pre-execution from initial activity of the process (async version)
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="ignoreCurrentStateCheck">If false and Current State Name and State Name of Current Activity is different (in case of scheme upgrade) do not run pre-execution</param>
        /// <param name="token">Cancellation token</param>
        public Task PreExecuteFromInitialActivityAsync(Guid processId, bool ignoreCurrentStateCheck = false, CancellationToken token = default (CancellationToken))
        {
            var processInstance = Builder.GetProcessInstance(processId);
            PersistenceProvider.FillSystemProcessParameters(processInstance);
            return PreExecute(processId, processInstance.ProcessScheme.InitialActivity.Name, ignoreCurrentStateCheck, processInstance,token);
        }

        /// <summary>
        /// Pre-execution from current activity of the process (async version)
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="ignoreCurrentStateCheck">If false and Current State Name and State Name of Current Activity is different (in case of scheme upgrade) do not run pre-execution</param>
        /// <param name="token">Cancellation token</param> 
        public Task PreExecuteFromCurrentActivityAsync(Guid processId, bool ignoreCurrentStateCheck = false, CancellationToken token = default(CancellationToken))
        {
            var processInstance = Builder.GetProcessInstance(processId);
            PersistenceProvider.FillSystemProcessParameters(processInstance);
            return PreExecute(processId, processInstance.CurrentActivityName, ignoreCurrentStateCheck, processInstance, token);
        }

        /// <summary>
        /// Pre-execution from specified activity of the process (async version)
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="fromActivityName">Activity name which begins pre-execution</param>
        /// <param name="ignoreCurrentStateCheck">If false and Current State Name and State Name of Current Activity is different (in case of scheme upgrade) do not run pre-execution</param>
        /// <param name="token">Cancellation token</param>
        public Task PreExecuteAsync(Guid processId, string fromActivityName, bool ignoreCurrentStateCheck = false, CancellationToken token = default(CancellationToken))
        {
            var processInstance = Builder.GetProcessInstance(processId);
            PersistenceProvider.FillSystemProcessParameters(processInstance);
            return PreExecute(processId, fromActivityName, ignoreCurrentStateCheck, processInstance, token);
        }

        /// <summary>
        /// Return the list of commands which is availiable from initial activity for specified user (async version)
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="identityId">User id for whom formed initial commands list</param>
        /// <returns>List of <see cref="WorkflowCommand"/> commands</returns>
        public async Task<IEnumerable<WorkflowCommand>> GetInitialCommandsAsync(string schemeCode, string identityId)
        {
            return await Task.Run(() => GetInitialCommands(schemeCode, identityId));
        }

        /// <summary>
        /// Return the list of commands which is availiable from initial activity for specified users (async version)
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="identityIds">List of User ids for which formed initial commands list</param>
        /// <param name="processParameters">Parameters for creating scheme of process</param>
        /// <param name="commandNameFilter">Selects only the specified command if not null </param>
        /// <returns>List of <see cref="WorkflowCommand"/> commands</returns>
        public async Task<IEnumerable<WorkflowCommand>> GetInitialCommandsAsync(string schemeCode,
            IEnumerable<string> identityIds,
            IDictionary<string, object> processParameters = null, string commandNameFilter = null)
        {
            return
                await Task.Run(() => GetInitialCommands(schemeCode, identityIds, processParameters, commandNameFilter));
        }

        /// <summary>
        /// Return the list of commands which is availiable from current activity for specified user (async version)
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="identityId">User id for whom formed initial commands list</param>
        /// <returns>List of <see cref="WorkflowCommand"/> commands</returns>
        public async Task<IEnumerable<WorkflowCommand>> GetAvailableCommandsAsync(Guid processId, string identityId)
        {
            return await Task.Run(() => GetAvailableCommands(processId, identityId));
        }

        /// <summary>
        /// Return the list of commands which is availiable from current activity for specified user (async version)
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="identityIds">List of User ids for which formed initial commands list</param>
        /// <param name="commandNameFilter">Selects only the specified command if not null</param>
        /// <param name="mainIdentityId">User id for priority check of rules</param>
        /// <returns>List of <see cref="WorkflowCommand"/> commands</returns>
        public async Task<IEnumerable<WorkflowCommand>> GetAvailableCommandsAsync(Guid processId,
            IEnumerable<string> identityIds, string commandNameFilter = null, string mainIdentityId = null)
        {
            return await Task.Run(() => GetAvailableCommands(processId, identityIds, commandNameFilter, mainIdentityId));
        }

        /// <summary>
        /// Execute specified command for specified users (async version)
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <param name="identityId">The user id which execute command</param>
        /// <param name="impersonatedIdentityId">The user id for whom executes command (impersonation)</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Result of the execution</returns>
        public async Task<CommandExeutionResult> ExecuteCommandAsync(WorkflowCommand command, string identityId, string impersonatedIdentityId, CancellationToken token = default (CancellationToken))
        {
            string commandValidationError;

            if (!command.Validate(out commandValidationError))
            {
                throw new InvalidCommandException("Command is invalid. {0}.", commandValidationError);
            }

            var processInstance = Builder.GetProcessInstance(command.ProcessId);

            await SetProcessNewStatus(processInstance, ProcessStatus.Running).ConfigureAwait(false);

            IEnumerable<TransitionDefinition> transitions;

            try
            {
                PersistenceProvider.FillSystemProcessParameters(processInstance);


                if (processInstance.CurrentActivityName != command.ValidForActivityName)
                {
                    throw new InvalidCommandException(
                        $"Impossible to execute command {command.CommandName} valid for activity {command.ValidForActivityName}. Current activity {processInstance.CurrentActivityName}.");
                }

                transitions = processInstance.ProcessScheme.GetCommandTransitionForActivity(processInstance.CurrentActivity,command.CommandName, ForkTransitionSearchType.Both).ToList();

                if (!transitions.Any())
                {
                    throw new InvalidOperationException();
                }
            }
            catch (Exception)
            {
                await SetProcessNewStatus(processInstance, ProcessStatus.Idled).ConfigureAwait(false);
                throw;
            }

            try
            {
                //WFE-60
                PersistenceProvider.FillPersistedProcessParameters(processInstance);

                var commandDefinition = processInstance.ProcessScheme.FindCommand(command.CommandName);

                foreach (var commandParameter in command.Parameters)
                {
                    var parameter = commandParameter;
                    var parameterDefinition = commandDefinition.InputParameters.Where(p => p.Name.Equals(parameter.ParameterName, StringComparison.Ordinal))
                        .Select(p => p.Parameter).FirstOrDefault();

                    var parameterName = parameterDefinition != null ? parameterDefinition.Name : commandParameter.ParameterName;
                    processInstance.SetParameter(parameterName, commandParameter.Value);
                }

                processInstance.AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterCurrentCommand,
                    command.CommandName));

                processInstance.AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterIdentityId,
                    identityId));

                processInstance.AddParameter(
                    ParameterDefinition.Create(DefaultDefinitions.ParameterImpersonatedIdentityId,
                        impersonatedIdentityId));

                //WFE-60
               // PersistenceProvider.SavePersistenceParameters(processInstance);
               // PersistenceProvider.FillPersistedProcessParameters(processInstance);
            }
            catch (Exception)
            {
                await SetProcessNewStatus(processInstance, ProcessStatus.Idled).ConfigureAwait(false);
                throw;
            }

            var wasExecuted = await ExecuteTriggeredTransitions(processInstance, transitions.ToList(), TriggerType.Command, token).ConfigureAwait(false);

            return new CommandExeutionResult()
            {
                WasExecuted = wasExecuted,
                ProcessInstance = processInstance
            };
        }

        /// <summary>
        /// Execute specified command for specified users (async version)
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="identityId">The user id which execute command</param>
        /// <param name="impersonatedIdentityId">The user id for whom executes command (impersonation)</param>
        /// <param name="command">Command to execute</param>
        /// <returns>Result of the execution</returns>
        [Obsolete(@"Since version 1.5 argument processId not used to determine the process, the process is determined by the command.ProcessId property.
Please use the following method: public void ExecuteCommandAsync(WorkflowCommand command, string identityId, string impersonatedIdentityId)")]
        public Task<CommandExeutionResult> ExecuteCommandAsync(Guid processId, string identityId, string impersonatedIdentityId, WorkflowCommand command)
        {
            return ExecuteCommandAsync(command, identityId, impersonatedIdentityId);
        }

        /// <summary>
        /// Return the initial state for process scheme (async version)
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="processParameters">Parameters for creating scheme of process</param>
        /// <returns><see cref="WorkflowState"/> object</returns>
        public async Task<WorkflowState> GetInitialStateAsync(string schemeCode,
            IDictionary<string, object> processParameters = null)
        {
            return await Task.Run(() => GetInitialState(schemeCode, processParameters));
        }

        /// <summary>
        /// Return the current state of specified process (async version)
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <returns><see cref="WorkflowState"/> object</returns>
        public async Task<WorkflowState> GetCurrentStateAsync(Guid processId)
        {
            return await Task.Run(() => GetCurrentState(processId));
        }

        /// <summary>
        /// Return the current state name of specified process (async version)
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <returns>Name of current state</returns>
        public async Task<string> GetCurrentStateNameAsync(Guid processId)
        {
            return await Task.Run(() => GetCurrentStateName(processId));
        }

        /// <summary>
        /// Return the current activity name of specified process (async version)
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <returns>Current activity name</returns>
        public async Task<string> GetCurrentActivityNameAsync(Guid processId)
        {
            return await Task.Run(() => GetCurrentActivityName(processId));
        }

        /// <summary>
        /// Get the list of all states which available for set of specified process localized in current culture (async version)
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <returns>List of <see cref="WorkflowState"/> objects</returns>
        public async Task<IEnumerable<WorkflowState>> GetAvailableStateToSetAsync(Guid processId)
        {
            return await Task.Run(() => GetAvailableStateToSet(processId));
        }

        /// <summary>
        /// Get the list of all states which available for set of specified process localized in specified culture (async version)
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="culture">Culture to localize state names</param>
        /// <returns>List of <see cref="WorkflowState"/> objects</returns>
        public async Task<IEnumerable<WorkflowState>> GetAvailableStateToSetAsync(Guid processId, CultureInfo culture)
        {
            return await Task.Run(() => GetAvailableStateToSet(processId, culture));
        }

        /// <summary>
        /// Get the list of all states which available for set of specified scheme in current culture (async version)
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="parameters">The parameters for creating scheme of process</param>
        /// <returns >List of <see cref="WorkflowState"/> objects</returns>
        public async Task<IEnumerable<WorkflowState>> GetAvailableStateToSetAsync(string schemeCode,
            IDictionary<string, object> parameters = null)
        {
            return await Task.Run(() => GetAvailableStateToSet(schemeCode, parameters));
        }

        /// <summary>
        /// Get the list of all states which available for set of specified scheme in specified culture (async version)
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="culture">Culture to localize state names</param>
        /// <param name="parameters">The parameters for creating scheme of process</param>
        /// <returns>List of <see cref="WorkflowState"/> objects</returns>
        public async Task<IEnumerable<WorkflowState>> GetAvailableStateToSetAsync(string schemeCode, CultureInfo culture,
            IDictionary<string, object> parameters = null)
        {
            return await Task.Run(() => GetAvailableStateToSet(schemeCode, culture, parameters));
        }

        /// <summary>
        /// Set specified state for specified process (async version)
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="identityId">The user id which set the state</param>
        /// <param name="impersonatedIdentityId">The user id for whom sets the state (impersonation)</param>
        /// <param name="stateName">State name to set</param>
        /// <param name="parameters">Dictionary of ProcessInstance parameters which transferred to executed actions</param>
        /// <param name="preventExecution">Actions due to transition process do not executed if true</param>
        public async Task SetStateAsync(Guid processId, string identityId, string impersonatedIdentityId,
            string stateName, IDictionary<string, object> parameters = null, bool preventExecution = false)
        {
            await Task.Run(() => SetState(processId, identityId, impersonatedIdentityId, stateName, parameters, preventExecution));
        }

        /// <summary>
        /// Get the list of user ids which can execute any command for specified process activity (async version)
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="beginningWithRoot">If true the list of actors will be obtained from the root process, even if you passed Id of the subprocess</param>
        /// <param name="activityName">Activity name in which transitions are checked. Current activity if null.</param>
        /// <returns>List of user ids</returns>
        public async Task<IEnumerable<string>> GetAllActorsForAllCommandTransitionsAsync(Guid processId, bool beginningWithRoot = false,
            string activityName = null)
        {
            return await Task.Run(() => GetAllActorsForAllCommandTransitions(processId, beginningWithRoot, activityName));
        }

        /// <summary>
        /// Get the list of user ids which can execute any command bound whith direct transitions (by transition classifier) for specified process activity (async version)
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="beginningWithRoot">If true the list of actors will be obtained from the root process, even if you passed Id of the subprocess</param>
        /// <param name="activityName">Activity name in which transitions are checked. Current activity if null.</param>
        /// <returns>List of user ids</returns>
        public async Task<IEnumerable<string>> GetAllActorsForDirectCommandTransitionsAsync(Guid processId, bool beginningWithRoot = false,
            string activityName = null)
        {
            return await Task.Run(() => GetAllActorsForDirectCommandTransitions(processId, beginningWithRoot, activityName));
        }

        /// <summary>
        /// Get the list of user ids which can execute any command bound whith direct or undefined transitions (by transition classifier) for specified process activity (async version)
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="beginningWithRoot">If true the list of actors will be obtained from the root process, even if you passed Id of the subprocess</param>
        /// <param name="activityName">Activity name in which transitions are checked. Current activity if null.</param>
        /// <returns>List of user ids</returns>
        public async Task<IEnumerable<string>> GetAllActorsForDirectAndUndefinedCommandTransitionsAsync(Guid processId, bool beginningWithRoot = false,
            string activityName = null)
        {
            return await Task.Run(() => GetAllActorsForDirectAndUndefinedCommandTransitions(processId, beginningWithRoot, activityName));
        }

        /// <summary>
        /// Get the list of user ids which can execute any command bound whith reverse transitions (by transition classifier) for specified process activity (async version)
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="beginningWithRoot">If true the list of actors will be obtained from the root process, even if you passed Id of the subprocess</param>
        /// <param name="activityName">Activity name in which transitions are checked. Current activity if null.</param>
        /// <returns>List of user ids</returns>
        public async Task<IEnumerable<string>> GetAllActorsForReverseCommandTransitionsAsync(Guid processId, bool beginningWithRoot = false,
            string activityName = null)
        {
            return await Task.Run(() => GetAllActorsForReverseCommandTransitions(processId, beginningWithRoot, activityName));
        }

        /// <summary>
        /// Get the list of user ids which can execute any command bound whith a transitions selected by filter (by transition classifier)
        /// for executed activity (inside of a transitional process) or for current activity (when a process is idled).
        /// Using of this method is preferable for notification of users which can execute next commands. 
        /// (async version)
        /// </summary>
        /// <param name="processInstance">Process instance</param>
        /// <param name="classifiers">Filter for transitions by transition classifier</param>
        /// <returns>List of user ids</returns>
        public async Task<IEnumerable<string>> GetAllActorsForCommandTransitionsAsync(ProcessInstance processInstance, List<TransitionClassifier> classifiers = null)
        {
            return await Task.Run(() => GetAllActorsForCommandTransitions(processInstance, classifiers));
        }

        /// <summary>
        /// Get the list of user ids which can execute any command bound whith a transitions selected by filter. This method is prefearable to use when you are using subprocesses
        /// (async version)
        /// </summary>
        /// <param name="filter">Sets the current process and the method of searching in the Process Instances tree</param>
        /// <param name="classifiers">Filter for transitions by transition classifier</param>
        /// <returns>List of user ids</returns>
        public async Task<IEnumerable<string>> GetAllActorsForCommandTransitionsAsync(TreeSearchFilter filter, List<TransitionClassifier> classifiers = null)
        {
            return await Task.Run(() => GetAllActorsForCommandTransitions(filter, classifiers));
        }

        /// <summary>
        /// Get procees definition (parsed scheme) for specified process (async version)
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <returns><see cref="ProcessDefinition"/> object</returns>
        public async Task<ProcessDefinition> GetProcessSchemeAsync(Guid processId)
        {
            return await Task.Run(() => GetProcessScheme(processId));
        }

        /// <summary>
        /// Get localized state name for specified scheme in current culture (async version)
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="stateName">State name to localize</param>
        /// <param name="parameters">The parameters for creating </param>
        /// <returns>Localized state name</returns>
        public async Task<string> GetLocalizedStateNameBySchemeCodeAsync(string schemeCode, string stateName,
            IDictionary<string, object> parameters = null)
        {
            return await Task.Run(() => GetLocalizedStateNameBySchemeCode(schemeCode, stateName, parameters));
        }

        /// <summary>
        /// Get status of specified process (async version)
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <returns><see cref="ProcessStatus"/> object</returns>
        public async Task<ProcessStatus> GetProcessStatusAsync(Guid processId)
        {
            return await Task.Run(() => GetProcessStatus(processId));
        }

        /// <summary>
        /// Get localized state name for specified scheme in current culture (async version)
        /// </summary>
        /// <param name="schemeId">Id of the scheme</param>
        /// <param name="stateName">State name to localize</param>
        /// <returns>Localized state name</returns>
        public async Task<string> GetLocalizedStateNameBySchemeIdAsync(Guid schemeId, string stateName)
        {
            return await Task.Run(() => GetLocalizedStateNameBySchemeId(schemeId, stateName));
        }

        /// <summary>
        /// Get localized state name for specified process in current culture (async version)
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="stateName">State name to localize</param>
        /// <returns>Localized state name</returns>
        public async Task<string> GetLocalizedStateNameAsync(Guid processId, string stateName)
        {
            return await Task.Run(() => GetLocalizedStateName(processId, stateName));
        }

        /// <summary>
        /// Get localized command name for specified process in current culture (async version)
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <param name="commandName">Command name to localize</param>
        /// <returns>Localized command name</returns>
        public async Task<string> GetLocalizedCommandNameAsync(Guid processId, string commandName)
        {
            return await Task.Run(() => GetLocalizedCommandName(processId, commandName));
        }

        /// <summary>
        /// Get localized command name for specified scheme in current culture (async version)
        /// </summary>
        /// <param name="schemeId">Id of the scheme</param>
        /// <param name="commandName">Command name to localize</param>
        /// <returns>Localized command name</returns>
        public async Task<string> GetLocalizedCommandNameBySchemeIdAsync(Guid schemeId, string commandName)
        {
            return await Task.Run(() => GetLocalizedCommandNameBySchemeId(schemeId, commandName));
        }

        /// <summary>
        /// Get process instance with all parameters for specified process id (async version)
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <returns><see cref="ProcessInstance"/> object</returns>
        public async  Task<ProcessInstance> GetProcessInstanceAndFillProcessParametersAsync(Guid processId)
        {
            return await Task.Run(() => GetProcessInstanceAndFillProcessParameters(processId));
        }

        /// <summary>
        /// Returns the history of process (async version)
        /// </summary>
        /// <param name="processId">Id of the process</param>
        /// <returns></returns>
        public async Task<List<ProcessHistoryItem>> GetProcessHistoryAsync(Guid processId)
        {
            return await Task.Run(() => PersistenceProvider.GetProcessHistory(processId));
        }

        #endregion

        #region Private functions

        internal async Task ExecuteTimer(Guid processId, string timerName)
        {
            var processInstance = Builder.GetProcessInstance(processId);
            PersistenceProvider.FillProcessParameters(processInstance);

            var currentTimerTransitions = processInstance.ProcessScheme.GetTimerTransitionForActivity(processInstance.CurrentActivity, ForkTransitionSearchType.Both).
                Where(p => p.Trigger.Timer.Name == timerName).ToList();

            if (currentTimerTransitions.Any())
            {
                await SetProcessNewStatus(processInstance, ProcessStatus.Running).ConfigureAwait(false);

                processInstance.ExecutedTimer = timerName;
                processInstance.CurrentCommand = DefaultDefinitions.CommandTimer.Name;
                processInstance.IdentityId = null;
                processInstance.ImpersonatedIdentityId = null;

                await ExecuteTriggeredTransitions(processInstance, currentTimerTransitions, TriggerType.Timer, CancellationToken.None);
            }
        }

        private async Task<bool> ExecuteTriggeredTransitions(ProcessInstance processInstance, List<TransitionDefinition> transitions, TriggerType type, CancellationToken token)
        {
            //fork command transitions
            var forkCommandTransitions = transitions.Where(t => t.IsFork).ToList();
            //common commands transitions
            var commonCommandTransitions = transitions.Where(t => !t.IsFork).ToList();

            if (forkCommandTransitions.Any())
            {
                try
                {
                    await ChooseTransitionAndCreateSubprocess(forkCommandTransitions, processInstance, token).ConfigureAwait(false);

                    if (!commonCommandTransitions.Any())
                    {
                        await SetProcessNewStatus(processInstance, ProcessStatus.Idled).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    await SetProcessNewStatus(processInstance, ProcessStatus.Idled).ConfigureAwait(false);
                    throw new Exception($"Error Execute {type:G} Workflow Id={processInstance.ProcessId}", ex);
                }
            }

            if (commonCommandTransitions.Any())
            {
                try
                {
                    processInstance.SetStartTransitionalProcessActivity();
                    var newExecutionParameters = new List<ExecutionRequestParameters>();
                    newExecutionParameters.AddRange(commonCommandTransitions.Select(at => ExecutionRequestParameters.Create(processInstance, at)));
                    return await Bus.QueueExecution(newExecutionParameters, token).ConfigureAwait(false); 
                }
                catch (Exception ex)
                {
                    await SetProcessNewStatus(processInstance, ProcessStatus.Idled).ConfigureAwait(false);
                    throw new Exception($"Error Execute {type:G} Workflow Id={processInstance.ProcessId}", ex);
                }
            }

            return false;
        }

        internal async Task SetProcessNewStatus(ProcessInstance processInstance, ProcessStatus newStatus, bool supressEvent = false)
        {
            var oldStatus = PersistenceProvider.GetInstanceStatus(processInstance.ProcessId);
            if (newStatus == ProcessStatus.Finalized)
            {
                PersistenceProvider.SetWorkflowFinalized(processInstance);
                //Outer func
                if (processInstance.IsSubprocess)
                {
                    await MergeSubprocess(processInstance).ConfigureAwait(false);
                }
            }
            else if (newStatus == ProcessStatus.Idled)
                PersistenceProvider.SetWorkflowIdled(processInstance);
            else if (newStatus == ProcessStatus.Initialized)
                PersistenceProvider.SetWorkflowIniialized(processInstance);
            else if (newStatus == ProcessStatus.Running)
                PersistenceProvider.SetWorkflowRunning(processInstance);
            else if (newStatus == ProcessStatus.Terminated)
#pragma warning disable 612
                PersistenceProvider.SetWorkflowTerminated(processInstance, ErrorLevel.Critical, "Terminated");
#pragma warning restore 612
            else
            {
                return;
            }

            if (ProcessStatusChanged != null && oldStatus.Id != newStatus.Id && !supressEvent)
                ProcessStatusChanged(this,
                    new ProcessStatusChangedEventArgs(processInstance.ProcessId, processInstance.IsSubprocess, oldStatus, newStatus)
                    {
                        ProcessInstance = processInstance,
                        SchemeCode = processInstance.ProcessScheme.Name
                    });
        }

        private async Task MergeSubprocess(ProcessInstance processInstance)
        {
            // ReSharper disable once PossibleInvalidOperationException
            var parentProcessInstance = Builder.GetProcessInstance(processInstance.ParentProcessId.Value);
            
            try
            {
                //Searching exit transition from the subprocess in parent's process scheme
                var from = parentProcessInstance.ProcessScheme.FindActivity(processInstance.PreviousActivityName);
                var to = parentProcessInstance.ProcessScheme.FindActivity(processInstance.CurrentActivityName);
                var finalTransition = parentProcessInstance.ProcessScheme.FindTransitions(@from, to)
                    .FirstOrDefault(t => t.IsFork);

                if (finalTransition == null)
                {
                    DropProcessAndItsSubprocesses(processInstance);
                    return;
                }

                if (finalTransition.MergeViaSetState)
                {
                    parentProcessInstance.MergedSubprocessParameters = new ParametersCollection(processInstance.ProcessParameters);
                    var parameters = new Dictionary<string, object>();
                    foreach (var parameter in processInstance.ProcessParameters.Where(p => p.Purpose != ParameterPurpose.System))
                    {
                        if (!parentProcessInstance.ProcessParameters.Any(p => p.Name.Equals(parameter.Name, StringComparison.Ordinal)))
                        {
                            parameters.Add(parameter.Name, parameter.Value);
                        }
                        else if (parentProcessInstance.GetParameter(parameter.Name).Value == null)
                        {
                            parameters.Add(parameter.Name, parameter.Value);
                        }
                    }
                   
                    parameters.Add(DefaultDefinitions.ParameterCurrentCommand.Name, processInstance.CurrentCommand);
                    SetActivityWithExecution(processInstance.IdentityId, processInstance.ImpersonatedIdentityId,
                        parameters, to, parentProcessInstance);
                    //delete merged process and its subprocesses
                    DropProcessAndItsSubprocesses(processInstance);
                }
                else
                {
                    await SetProcessNewStatus(parentProcessInstance, ProcessStatus.Running).ConfigureAwait(false);

                    try
                    {
                        PersistenceProvider.FillProcessParameters(parentProcessInstance);
                        parentProcessInstance.MergedSubprocessParameters = new ParametersCollection(processInstance.ProcessParameters);
                        foreach (var parameter in processInstance.ProcessParameters)
                        {
                            var newParameter = parameter.Clone();
                            if (!parentProcessInstance.ProcessParameters.Any(p => p.Name.Equals(parameter.Name, StringComparison.Ordinal)))
                            {
                                parentProcessInstance.AddParameter(newParameter);
                            }
                            else
                            {
                                if (parentProcessInstance.GetParameter(parameter.Name).Value == null)
                                {
                                    parentProcessInstance.AddParameter(newParameter);
                                }
                            }
                        }

                        PersistenceProvider.SavePersistenceParameters(parentProcessInstance);
                        var autoTransitions = parentProcessInstance.ProcessScheme.GetAutoTransitionForActivity(to);
                        var transitionDefinitions = autoTransitions as IList<TransitionDefinition> ?? autoTransitions.ToList();
                        if (transitionDefinitions.Any())
                        {
                            var newExecutionParameters = new List<ExecutionRequestParameters>();
                            newExecutionParameters.AddRange(transitionDefinitions.Select(at => ExecutionRequestParameters.Create(parentProcessInstance, at)));
                            await Bus.QueueExecution(newExecutionParameters, CancellationToken.None).ConfigureAwait(false);
                        }
                        else
                        {
                            await SetProcessNewStatus(parentProcessInstance, ProcessStatus.Idled).ConfigureAwait(false);
                        }

                        //delete merged process and its subprocesses
                        DropProcessAndItsSubprocesses(processInstance);
                    }
                    catch (Exception)
                    {
                        await SetProcessNewStatus(parentProcessInstance, ProcessStatus.Idled).ConfigureAwait(false);
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                await SetProcessNewStatus(processInstance, ProcessStatus.Idled).ConfigureAwait(false);
                throw new Exception($"Error Merging Workflow Id={parentProcessInstance.ProcessId}", ex);
            }
        }

        private void DropProcessAndItsSubprocesses(ProcessInstance processInstance)
        {
            var processInstanceTree = GetProcessInstancesTree(processInstance);
            if (processInstanceTree.GetNodeById(processInstance.ProcessId) == null)
                return;

            DropProcesses(new List<Guid> {processInstance.ProcessId}, processInstanceTree);
            SaveProcessInstancesTree(processInstanceTree);
        }

        private Task SetIdledOrFinalizedStatus(ProcessInstance processInstance, ActivityDefinition newCurrentActivity)
        {
            return SetProcessNewStatus(processInstance,newCurrentActivity.IsFinal ? ProcessStatus.Finalized : ProcessStatus.Idled);
        }

        private async Task ExecuteRootActivity(ProcessInstance processInstance, CancellationToken token)
        {
            PersistenceProvider.FillProcessParameters(processInstance);
            
            try
            {
                var executionRequestParameters = ExecutionRequestParameters.Create(processInstance,
                    processInstance.ProcessScheme.InitialActivity, new List<ConditionDefinition>
                    {
                        ConditionDefinition.Always
                    });
                await Bus.QueueExecution(executionRequestParameters, token, true).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await SetProcessNewStatus(processInstance, ProcessStatus.Idled).ConfigureAwait(false);
                throw new Exception($"Error Execute Root Workflow Id={processInstance.ProcessId}", ex);
            }
        }

        private List<ExecutionRequestParameters> FillExecutionRequestParameters(ProcessInstance processInstance, List<TransitionDefinition> transitions,
            PreExecutionContext preExecutionContext,
            List<TransitionDefinition> transitionsForAdditional = null)
        {
            var newExecutionParameters = new List<ExecutionRequestParameters>();

            if (transitions == null || transitions.Count == 0)
                return newExecutionParameters;

            List<string> identityIds = new List<string>();
            List<string> currentActivityIdentityIds = new List<string>();

            foreach (var transition in transitions)
            {
                bool isNewTransition = false;
                var parametersLocal = ExecutionRequestParameters.Create(processInstance, transition, true);

                if (transition.Trigger.Type != TriggerType.Auto || transition.Restrictions.Any())
                {
                    preExecutionContext.TransitionForActors = transition;
                    isNewTransition = true;
                }


                if (transition.To.HavePreExecutionImplementation && preExecutionContext.HaveTransitionForActors)
                {
                    var actors = GetActors(processInstance, preExecutionContext.TransitionForActors);
                    var collection = actors as IList<string> ?? actors.ToList();
                    identityIds.AddRange(collection);

                    if (isNewTransition)
                        currentActivityIdentityIds.AddRange(collection);
                }

                if (transition.Trigger.Type == TriggerType.Command)
                {
                    processInstance.AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterCurrentCommand,
                        transition.Trigger.Command.Name));
                }

                newExecutionParameters.Add(parametersLocal);
            }

            #region transitionsForAdditional
            if (transitionsForAdditional != null)
            {
                foreach (var transition in transitionsForAdditional)
                {
                    if (!transition.Restrictions.Any())
                        continue;

                    var actors = GetActors(processInstance, transition);
                    currentActivityIdentityIds.AddRange(actors);
                }
            }
            #endregion

            processInstance.AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterIdentityIds, identityIds));
            processInstance.AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterIdentityIdsForCurrentActivity, currentActivityIdentityIds));

            return newExecutionParameters;
        }

        internal async Task BusExecutionComplete(object sender, ExecutionResponseEventArgs e)
        {
            var executionResponseParameters = e.Parameters;
            var processInstance = e.Parameters.ProcessInstance;
            PersistenceProvider.FillSystemProcessParameters(processInstance);

            if (executionResponseParameters.IsEmplty)
            {
                //means that the process is stopped and transitional process completed
                await SetProcessNewStatus(processInstance, ProcessStatus.Idled).ConfigureAwait(false);

                //because transition process is finished 
                //need to check that all subprocesses can exists
                CheckSubprocesses(processInstance);

                //need to send that the transitional process was finished
                if (ProcessActivityChanged != null)
                {
                    PersistenceProvider.FillPersistedProcessParameters(processInstance);
                    ProcessActivityChanged(this, new ProcessActivityChangedEventArgs(processInstance, true));
                }

                TimerManager?.ClearAndRegisterTimers(processInstance);

                return;
            }


            if (executionResponseParameters.IsError)
            {
                var executionErrorParameters = executionResponseParameters as ExecutionResponseParametersError;
                var exception = executionErrorParameters?.Exception ?? new Exception($"Error Execution Complete Workflow Id={processInstance.ProcessId}");

                PersistenceProvider.FillPersistedProcessParameters(processInstance);

                var defaultStatus = ProcessStatus.Terminated;

                var allCommandOrTimerTransitions =
                    processInstance.ProcessScheme.GetCommandTransitions(processInstance.CurrentActivity)
                        .Union(
                            processInstance.ProcessScheme.GetTimerTransitionForActivity(processInstance.CurrentActivity));

                if (allCommandOrTimerTransitions.Any())
                    defaultStatus = ProcessStatus.Error;

                if (OnWorkflowError != null)
                {
                    TransitionDefinition executedTransition = null;
                    if (!string.IsNullOrEmpty(executionErrorParameters?.ExecutedTransitionName))
                    {
                        executedTransition = processInstance.ProcessScheme.FindTransition(executionErrorParameters.ExecutedTransitionName);
                    }

                    var workflowErrorEventArgs = new WorkflowErrorEventArgs(processInstance, defaultStatus, executedTransition, exception);

                    OnWorkflowError(this, workflowErrorEventArgs);

                    defaultStatus = workflowErrorEventArgs.ProcessStatus;
                }

                await SetProcessNewStatus(processInstance, defaultStatus).ConfigureAwait(false);

                if (defaultStatus == ProcessStatus.Error || defaultStatus == ProcessStatus.Terminated)
                    throw exception;

                return;
            }


            try
            {
                ActivityDefinition newCurrentActivity;
                if (string.IsNullOrEmpty(executionResponseParameters.ExecutedTransitionName))
                {
                    ActivityDefinition from;

                    try
                    {
                        @from = processInstance.CurrentActivity;
                    }
                    catch (ActivityNotFoundException)
                    {
                        @from = ActivityDefinition.Create(processInstance.CurrentActivityName,
                            processInstance.GetParameter(DefaultDefinitions.ParameterCurrentState.Name)
                                .Value.ToString(), false, false, false, false);
                    }

                    var to =
                        processInstance.ProcessScheme.FindActivity(executionResponseParameters.ExecutedActivityName);
                    newCurrentActivity = to;
                    PersistenceProvider.UpdatePersistenceState(processInstance,
                        TransitionDefinition.Create(@from, to));
                }
                else
                {
                    var executedTransition =
                        processInstance.ProcessScheme.FindTransition(executionResponseParameters.ExecutedTransitionName);
                    newCurrentActivity = executedTransition.To;
                    PersistenceProvider.UpdatePersistenceState(processInstance, executedTransition);
                }

                TimerManager?.RequestTimerValue(processInstance, newCurrentActivity);

                PersistenceProvider.SavePersistenceParameters(processInstance);

                var forkAutoTransitions = processInstance.ProcessScheme.GetAutoTransitionForActivity(newCurrentActivity, ForkTransitionSearchType.Fork).ToList();

                var autoTransitions = processInstance.ProcessScheme.GetAutoTransitionForActivity(newCurrentActivity).ToList();

                if (TimerManager != null)
                {
                    var transitionsForImmediateExecution = TimerManager.GetTransitionsForImmediateExecution(processInstance, newCurrentActivity).ToList();
                    autoTransitions.AddRange(transitionsForImmediateExecution.Where(t => !t.IsFork));
                    forkAutoTransitions.AddRange(transitionsForImmediateExecution.Where(t => t.IsFork));
                }

                //subprocess creation
                if (forkAutoTransitions.Any())
                {
                    await ChooseTransitionAndCreateSubprocess(forkAutoTransitions, processInstance,e.Token).ConfigureAwait(false);
                }

                PersistenceProvider.FillProcessParameters(processInstance); //TODO redundant?

                if (!autoTransitions.Any())
                {
                    processInstance.CurrentActivityName = newCurrentActivity.Name;
                    processInstance.CurrentState = processInstance.ExecutedActivityState;

                    await SetIdledOrFinalizedStatus(processInstance, newCurrentActivity).ConfigureAwait(false);

                    ProcessActivityChanged?.Invoke(this, new ProcessActivityChangedEventArgs(processInstance, true));

                    //because transition process is finished 
                    //need to check that all subprocesses can exists
                    CheckSubprocesses(processInstance);

                    TimerManager?.ClearAndRegisterTimers(processInstance);

                    return;
                }

                ProcessActivityChanged?.Invoke(this, new ProcessActivityChangedEventArgs(processInstance, false));

                var newExecutionParameters = new List<ExecutionRequestParameters>();
                newExecutionParameters.AddRange(autoTransitions.Select(at => ExecutionRequestParameters.Create(processInstance, at)));
                await Bus.QueueExecution(newExecutionParameters,e.Token).ConfigureAwait(false);
            }
            catch (ActivityNotFoundException)
            {
                await SetProcessNewStatus(processInstance, ProcessStatus.Terminated).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await SetProcessNewStatus(processInstance, ProcessStatus.Idled).ConfigureAwait(false);
                throw new Exception($"Error Execution Complete Workflow Id={processInstance.ProcessId}", ex);
            }
        }

        private void CheckSubprocesses(ProcessInstance processInstance)
        {
            var processTree = GetProcessInstancesTree(processInstance);

            var node = processTree.GetNodeById(processInstance.ProcessId);
            if (node == null)
                return;

            var children = node.Children.ToList();
            var instancesToDrop = new List<ProcessInstance>();

            if (children.Any())
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var child in children)
                {
                    var instance = Builder.GetProcessInstance(child.Id);
                    if (instance.ProcessScheme.AllowedActivities != null &&
                        !instance.ProcessScheme.AllowedActivities.Contains(processInstance.CurrentActivityName))
                    {
                        instancesToDrop.Add(instance);
                    }
                }
            }

            if (instancesToDrop.Any())
            {
                DropProcesses(instancesToDrop.Select(i => i.ProcessId).ToList(), processTree);
                SaveProcessInstancesTree(processTree);
            }
        }

        private void DropProcesses(List<Guid> ids, ProcessInstancesTree processInstancesTree)
        {
            if (!ids.Any())
                return;

            var idsToDelete = new List<Guid>();
            foreach (var id in ids)
            {
                idsToDelete.Add(id);
                var nodeById = processInstancesTree.GetNodeById(id);
                var childrenIds = nodeById.GetAllChildrenIds();
                if (childrenIds.Any())
                    idsToDelete.AddRange(childrenIds);
                nodeById.Remove();
            }

            var todelete = idsToDelete.Distinct().ToArray();

            PersistenceProvider.DeleteProcess(todelete);
       }

        private IEnumerable<string> GetActors(ProcessInstance processInstance, TransitionDefinition transition)
        {
            if (!transition.Restrictions.Any())
                return new List<string>();

            List<string> result = null;

            foreach (var restrictionDefinition in transition.Restrictions.Where(r => r.Type == RestrictionType.Allow))
            {
                var allowed = new List<string>();
                allowed.AddRange(GetIdentities(processInstance, restrictionDefinition.Actor.Rule,
                    restrictionDefinition.Actor.Value));

                result = result == null
                    ? allowed
                    : transition.AllowConcatenationType == ConcatenationType.And
                        ? result.Intersect(allowed).ToList()
                        : result.Concat(allowed).Distinct().ToList();
            }

            if (result == null)
                return new List<string>();
            if (!result.Any())
                return result;

            if (transition.RestrictConcatenationType == ConcatenationType.Or)
            {
                foreach (
                    var restrictionDefinition in transition.Restrictions.Where(r => r.Type == RestrictionType.Restrict))
                {
                    var restricted = new List<string>();
                    restricted.AddRange(GetIdentities(processInstance, restrictionDefinition.Actor.Rule,
                        restrictionDefinition.Actor.Value));

                    result.RemoveAll(p => restricted.Contains(p));
                    if (!result.Any())
                        return result;
                }
            }
            else
            {
                var restricted = new List<string>();
                foreach (
                    var restrictionDefinition in transition.Restrictions.Where(r => r.Type == RestrictionType.Restrict))
                {
                    restricted.AddRange(GetIdentities(processInstance, restrictionDefinition.Actor.Rule,
                        restrictionDefinition.Actor.Value));
                }

                result.RemoveAll(p => restricted.Contains(p));
            }

            return result;
        }

        private IEnumerable<string> GetIdentities(ProcessInstance processInstance, string ruleName, string parameter)
        {
            if (ExecutionSearchOrder.IsProviderFirst())
            {
                if (RuleProvider.GetRules().Contains(ruleName))
                {
                    return RuleProvider.GetIdentities(processInstance, this, ruleName, parameter);
                }

                var codeActionsInvoker = CodeActionUtils.GetCodeActionsInvokerForRuleGet(ruleName, this, processInstance.ProcessScheme, out _);

                if (codeActionsInvoker == null)
                {
#pragma warning disable 618
                    if (!IgnoreMissingExecutionItems)
                        throw new NotImplementedException($"Rule with name {ruleName} is not implemented");
                    return new List<string>();
#pragma warning restore 618
                }

                return codeActionsInvoker.InvokeRuleGet(ruleName, processInstance, this, parameter);
            }

            if (ExecutionSearchOrder.IsProviderLast())
            {
                var codeActionsInvoker = CodeActionUtils.GetCodeActionsInvokerForRuleGet(ruleName, this, processInstance.ProcessScheme, out _);

                if (codeActionsInvoker != null)
                {
                    return codeActionsInvoker.InvokeRuleGet(ruleName, processInstance, this, parameter);
                }

                if (RuleProvider.GetRules().Contains(ruleName))
                {
                    return RuleProvider.GetIdentities(processInstance, this, ruleName, parameter);
                }

#pragma warning disable 618
                if (!IgnoreMissingExecutionItems)
                    throw new NotImplementedException($"Rule with name {ruleName} is not implemented");
                return new List<string>();
#pragma warning restore 618
            }
            else
            {
                var codeActionsInvoker = CodeActionUtils.GetCodeActionsInvokerForRuleGet(ruleName, this, processInstance.ProcessScheme, out bool isGlobal);

                var isLocalPrevaleGlobal = ExecutionSearchOrder.IsLocalPrevaleGlobal();

                if (codeActionsInvoker != null && ((isLocalPrevaleGlobal && !isGlobal) || (!isLocalPrevaleGlobal && isGlobal)))
                {
                    return codeActionsInvoker.InvokeRuleGet(ruleName, processInstance, this, parameter);
                }

                if (RuleProvider.GetRules().Contains(ruleName))
                {
                    return RuleProvider.GetIdentities(processInstance, this, ruleName, parameter);
                }

                if (codeActionsInvoker != null)
                {
                    return codeActionsInvoker.InvokeRuleGet(ruleName, processInstance, this, parameter);
                }

#pragma warning disable 618
                if (!IgnoreMissingExecutionItems)
                    throw new NotImplementedException($"Rule with name {ruleName} is not implemented");
                return new List<string>();
#pragma warning restore 618
            }
        }

        private bool ValidateActor(ProcessInstance processInstance, string identityId,
            TransitionDefinition transition)
        {
            if (!transition.Restrictions.Any())
                return true;
            var onceAllowed = false;
            var restrictedCount = 0;
            foreach (var restrictionDefinition in transition.Restrictions)
            {
                var result = CheckIdentity(processInstance, identityId, restrictionDefinition.Actor.Rule,
                    restrictionDefinition.Actor.Value);


                if (result && restrictionDefinition.Type == RestrictionType.Restrict)
                {
                    restrictedCount++;
                    if (transition.RestrictConcatenationType == ConcatenationType.Or)
                        return false;
                }

                if (restrictionDefinition.Type == RestrictionType.Allow)
                {
                    if (!result && transition.AllowConcatenationType == ConcatenationType.And)
                    {
                        return false;
                    }

                    if (result)
                        onceAllowed = true;
                }
            }

            var count = transition.Restrictions.Count(r => r.Type == RestrictionType.Restrict);
            if (transition.RestrictConcatenationType == ConcatenationType.And && count > 0 &&
                restrictedCount == count)
                return false;

            return onceAllowed;
        }

        public bool CheckIdentity(ProcessInstance processInstance, string identityId, string ruleName, string parameter)
        {
            if (ExecutionSearchOrder.IsProviderFirst())
            {
                if (RuleProvider.GetRules().Contains(ruleName))
                {
                    return RuleProvider.Check(processInstance, this, identityId: identityId, ruleName: ruleName, parameter: parameter);
                }

                var codeActionsInvoker = CodeActionUtils.GetCodeActionsInvokerForRuleCheck(ruleName, this, processInstance.ProcessScheme, out _);

                if (codeActionsInvoker == null)
                {
#pragma warning disable 618
                    if (!IgnoreMissingExecutionItems)
                        throw new NotImplementedException($"Rule with name {ruleName} is not implemented");
                    return false;
#pragma warning restore 618   
                }
                 

                return codeActionsInvoker.InvokeRuleCheck(ruleName, processInstance, this, identityId: identityId, parameter: parameter);
            }

            if (ExecutionSearchOrder.IsProviderLast())
            {
                var codeActionsInvoker = CodeActionUtils.GetCodeActionsInvokerForRuleCheck(ruleName, this, processInstance.ProcessScheme, out _);

                if (codeActionsInvoker != null)
                {
                    return codeActionsInvoker.InvokeRuleCheck(ruleName, processInstance, this, identityId: identityId, parameter: parameter);
                }

                if (RuleProvider.GetRules().Contains(ruleName))
                {
                    return RuleProvider.Check(processInstance, this, identityId: identityId, ruleName: ruleName, parameter: parameter);
                }

#pragma warning disable 618
                if (!IgnoreMissingExecutionItems)
                    throw new NotImplementedException($"Rule with name {ruleName} is not implemented");
                return false;
#pragma warning restore 618  
            }
            else
            {
                var codeActionsInvoker = CodeActionUtils.GetCodeActionsInvokerForRuleCheck(ruleName, this, processInstance.ProcessScheme, out bool isGlobal);

                var isLocalPrevaleGlobal = ExecutionSearchOrder.IsLocalPrevaleGlobal();

                if (codeActionsInvoker != null && ((isLocalPrevaleGlobal && !isGlobal) || (!isLocalPrevaleGlobal && isGlobal)))
                {
                    return codeActionsInvoker.InvokeRuleCheck(ruleName, processInstance, this, identityId: identityId, parameter: parameter);
                }

                if (RuleProvider.GetRules().Contains(ruleName))
                {
                    return RuleProvider.Check(processInstance, this, identityId: identityId, ruleName: ruleName, parameter: parameter);
                }

                if (codeActionsInvoker != null)
                {
                    return codeActionsInvoker.InvokeRuleCheck(ruleName, processInstance, this, identityId: identityId, parameter: parameter);
                }

#pragma warning disable 618
                if (!IgnoreMissingExecutionItems)
                    throw new NotImplementedException($"Rule with name {ruleName} is not implemented");
                return false;
#pragma warning restore 618  
            }
        }

        public bool CheckIdentityByActor(ProcessInstance processInstance, string identityId, string actorName)
        {
            var actor = processInstance.ProcessScheme.Actors.FirstOrDefault(c => c.Name == actorName);
            if (actor == null)
                return false;

            return CheckIdentity(processInstance, identityId, actor.Name, actor.Value);
        }

        #endregion

        #region Internal API for events

        internal bool IsProcessStatusChangedSubscribed => ProcessStatusChanged != null;

        internal bool IsProcessActivityChangedSubscribed => ProcessActivityChanged != null;

        internal void InvokeProcessStatusChanged(ProcessStatusChangedEventArgs args)
        {
            ProcessStatusChanged?.Invoke(this, args);
        }

        internal void InvokeProcessActivityChanged(ProcessActivityChangedEventArgs args)
        {
            ProcessActivityChanged?.Invoke(this, args);
        }

        #endregion
    }
}