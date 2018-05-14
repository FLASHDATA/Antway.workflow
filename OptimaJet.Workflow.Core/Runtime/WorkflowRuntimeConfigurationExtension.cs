using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Bus;
using OptimaJet.Workflow.Core.Cache;
using OptimaJet.Workflow.Core.Generator;
using OptimaJet.Workflow.Core.Parser;
using OptimaJet.Workflow.Core.Persistence;

namespace OptimaJet.Workflow.Core.Runtime
{
   
    public static class WorkflowRuntimeConfigurationExtension
    {
        /// <summary>
        /// Сonfigures the runtime with specified builder <see cref="IWorkflowBuilder"/>
        /// </summary>
        /// <param name="runtime">The instance of the runtime</param>
        /// <param name="builder">The instance of the builder <see cref="IWorkflowBuilder"/></param>
        /// <returns>Configured instance of the workflow runtime</returns>
        public static WorkflowRuntime WithBuilder(this WorkflowRuntime runtime, IWorkflowBuilder builder)
        {
            runtime.Builder = builder;
            return runtime;
        }

        /// <summary>
        /// Сonfigures the runtime with default (scheme in xml) builder <see cref="IWorkflowBuilder"/>
        /// </summary>
        /// <param name="runtime">The instance of the runtime</param>
        /// <returns>Configured instance of the workflow runtime</returns>
        public static WorkflowRuntime WithDefaultBuilder<TSchemeMedium>(this WorkflowRuntime runtime) where TSchemeMedium : class
        {
            runtime.Builder = new WorkflowBuilder<TSchemeMedium>();
            return runtime;
        }


        /// <summary>
        /// Сonfigures the runtime with specified rule provider <see cref="IWorkflowRuleProvider"/>
        /// </summary>
        /// <param name="runtime">The instance of the runtime</param>
        /// <param name="ruleProvider">The instance of the rule provider <see cref="IWorkflowRuleProvider"/></param>
        /// <returns>Configured instance of the workflow runtime</returns>
        public static WorkflowRuntime WithRuleProvider(this WorkflowRuntime runtime, IWorkflowRuleProvider ruleProvider)
        {
            runtime.RuleProvider = ruleProvider;
            return runtime;
        }

        /// <summary>
        /// Сonfigures the runtime with specified action provider <see cref="IWorkflowActionProvider"/>
        /// </summary>
        /// <param name="runtime">The instance of the runtime</param>
        /// <param name="workflowActionProvider">The instance of the action provider <see cref="IWorkflowActionProvider"/></param>
        /// <returns>Configured instance of the workflow runtime</returns>
        public static WorkflowRuntime WithActionProvider(this WorkflowRuntime runtime, IWorkflowActionProvider workflowActionProvider)
        {
            runtime.ActionProvider = workflowActionProvider;

            if (runtime.Bus != null)
            {
                runtime.Bus.Initialize(runtime);
            }

            return runtime;
        }

        /// <summary>
        /// Сonfigures the runtime with specified persistence provider <see cref="IPersistenceProvider"/>
        /// </summary>
        /// <param name="runtime">The instance of the runtime</param>
        /// <param name="persistenceProvider">The instance of the persistence provider <see cref="IPersistenceProvider"/></param>
        /// <returns>Configured instance of the workflow runtime</returns>
        public static WorkflowRuntime WithPersistenceProvider(this WorkflowRuntime runtime, IPersistenceProvider persistenceProvider)
        {
            runtime.PersistenceProvider = persistenceProvider;
            persistenceProvider.Init(runtime);
            return runtime;
        }

        /// <summary>
        /// Сonfigures the runtime with specified autocomplete provider (only for the Designer) <see cref="IDesignerAutocompleteProvider"/>
        /// </summary>
        /// <param name="runtime">The instance of the runtime</param>
        /// <param name="autocompleteProvider">The instance of the autocomplete provider <see cref="IDesignerAutocompleteProvider"/></param>
        /// <returns>Configured instance of the workflow runtime</returns>
        public static WorkflowRuntime WithDesignerAutocompleteProvider(this WorkflowRuntime runtime, IDesignerAutocompleteProvider autocompleteProvider)
        {
            runtime.DesignerAutocompleteProvider = autocompleteProvider;

            return runtime;
        }

        /// <summary>
        /// Сonfigures the runtime with specified workflow bus <see cref="IWorkflowBus"/>
        /// </summary>
        /// <param name="runtime">The instance of the runtime</param>
        /// <param name="bus">The instance of the workflow bus <see cref="IWorkflowBus"/></param>
        /// <returns>Configured instance of the workflow runtime</returns>
        public static WorkflowRuntime WithBus(this WorkflowRuntime runtime, IWorkflowBus bus)
        {
            runtime.Bus = bus;

            if (runtime.ActionProvider != null)
            {
                runtime.Bus.Initialize(runtime);
            }

            bus.ExecutionComplete += runtime.BusExecutionComplete;
            return runtime;
        }

        /// <summary>
        /// Subscribe a event handler on OnNeedDeterminingParameters event to obtain parameters for creating a scheme of a process where a scheme was changed <see cref="NeedDeterminingParametersEventArgs"/>
        /// </summary>
        /// <param name="runtime">The instance of the runtime</param>
        /// <param name="determiningParametersGetter">Event handler <see cref="NeedDeterminingParametersEventArgs"/></param>
        /// <returns>Configured instance of the workflow runtime</returns>
        public static WorkflowRuntime AttachDeterminingParametersGetter(this WorkflowRuntime runtime, EventHandler<NeedDeterminingParametersEventArgs> determiningParametersGetter)
        {
            runtime.OnNeedDeterminingParameters += determiningParametersGetter;
            return runtime;
        }

        /// <summary>
        /// Set  <see cref="WorkflowRuntime.IsAutoUpdateSchemeBeforeGetAvailableCommands"/> to true
        /// </summary>
        /// <param name="runtime">The instance of the runtime</param>
        /// <returns>Configured instance of the workflow runtime</returns>
        public static WorkflowRuntime SwitchAutoUpdateSchemeBeforeGetAvailableCommandsOn(this WorkflowRuntime runtime)
        {
            runtime.IsAutoUpdateSchemeBeforeGetAvailableCommands = true;
            return runtime;
        }

        /// <summary>
        /// Set  <see cref="WorkflowRuntime.IsAutoUpdateSchemeBeforeGetAvailableCommands"/> to true and add event handler to OnNeedDeterminingParameters event <see cref="WorkflowRuntime.OnNeedDeterminingParameters"/>
        /// </summary>
        /// <param name="runtime">The instance of the runtime</param>
        ///   /// <param name="determiningParametersGetter">Event handler <see cref="NeedDeterminingParametersEventArgs"/></param>
        /// <returns>Configured instance of the workflow runtime</returns>
        public static WorkflowRuntime SwitchAutoUpdateSchemeBeforeGetAvailableCommandsOn(this WorkflowRuntime runtime, EventHandler<NeedDeterminingParametersEventArgs> determiningParametersGetter)
        {
            runtime.IsAutoUpdateSchemeBeforeGetAvailableCommands = true;
            runtime.OnNeedDeterminingParameters += determiningParametersGetter;
            return runtime;
        }

        /// <summary>
        /// Set  <see cref="WorkflowRuntime.IsAutoUpdateSchemeBeforeGetAvailableCommands"/> to false
        /// </summary>
        /// <param name="runtime">The instance of the runtime</param>
        /// <returns>Configured instance of the workflow runtime</returns>
        public static WorkflowRuntime SwitchAutoUpdateSchemeBeforeGetAvailableCommandsOff(this WorkflowRuntime runtime)
        {
            runtime.IsAutoUpdateSchemeBeforeGetAvailableCommands = false;
            return runtime;
        }

        /// <summary>
        /// Start all workflow runtime services аnd compile global code actions
        /// </summary>
        /// <param name="runtime">The instance of the runtime</param>
        /// <returns>Configured instance of the workflow runtime</returns>
        public static WorkflowRuntime Start(this WorkflowRuntime runtime)
        {
            if (!runtime.ValidateSettings())
                throw new InvalidOperationException();
            runtime.Start(false,out _);
            return runtime;
        }

        /// <summary>
        /// System use only. The signature of this method can be changed without any notification. Start all workflow runtime services аnd compile global code actions
        /// </summary>
        /// <param name="runtime">The instance of the runtime</param>
        /// <param name="ignoreNotCompilledGlobalActions">if true all compillation errors in the global codeactions will be ignored</param>
        /// <param name="compillerErrors">compiller errors</param>
        /// <returns>Configured instance of the workflow runtime</returns>
        public static WorkflowRuntime Start(this WorkflowRuntime runtime, bool ignoreNotCompilledGlobalActions,out Dictionary<string,string> compillerErrors)
        {
            if (!runtime.ValidateSettings())
                throw new InvalidOperationException();
            runtime.Start(ignoreNotCompilledGlobalActions,out compillerErrors);
            return runtime;
        }

        /// <summary>
        /// System use only. The signature of this method can be changed without any notification. Start all workflow runtime services except timers.
        /// </summary>
        /// <param name="runtime">The instance of the runtime</param>
        /// <param name="ignoreNotCompilledGlobalActions">if true all compillation errors in the global codeactions will be ignored</param>
        /// <param name="compillerErrors">compiller errors</param>
        /// <returns>Configured instance of the workflow runtime</returns>
        public static WorkflowRuntime ColdStart(this WorkflowRuntime runtime, bool ignoreNotCompilledGlobalActions,out Dictionary<string,string> compillerErrors)
        {
            if (!runtime.ValidateSettings())
                throw new InvalidOperationException();
            runtime.ColdStart(ignoreNotCompilledGlobalActions,out compillerErrors);
            return runtime;
        }


        /// <summary>
        /// Сonfigures the workflow builder with specified cache for parced processes <see cref="IParsedProcessCache"/>
        /// </summary>
        /// <param name="bulder">The instance of the workflow builder</param>
        /// <param name="cache">The instance of the cache for parced processes <see cref="IParsedProcessCache"/></param>
        /// <returns>Configured instance of the workflow builder</returns>
        public static IWorkflowBuilder WithCache(this IWorkflowBuilder bulder, IParsedProcessCache cache)
        {
            bulder.SetCache(cache);
            return bulder;
        }

        /// <summary>
        /// Сonfigures the workflow builder with default in memory cache for parced processes <see cref="IParsedProcessCache"/>
        /// </summary>
        /// <param name="bulder">The instance of the workflow builder</param>
        /// <returns>Configured instance of the workflow builder</returns>
        public static IWorkflowBuilder WithDefaultCache(this IWorkflowBuilder bulder)
        {
            bulder.SetCache(new DefaultParcedProcessCache());
            return bulder;
        }

        /// <summary>
        /// Сonfigures the workflow builder with specified workflow scheme generator<see cref="IWorkflowGenerator{TSchemeMedium}"/>
        /// </summary>
        /// <param name="bulder">The instance of the workflow builder</param>
        /// <param name="generator">The instance of the workflow scheme generator <see cref="IWorkflowGenerator{TSchemeMedium}"/></param>
        /// <returns>Configured instance of the workflow builder</returns>
        public static IWorkflowBuilder WithGenerator<TSchemeMedium>(this WorkflowBuilder<TSchemeMedium> bulder, IWorkflowGenerator<TSchemeMedium> generator) where TSchemeMedium : class
        {
            bulder.Generator = generator;
            return bulder;
        }

        /// <summary>
        /// Сonfigures the workflow builder with specified workflow scheme parser<see cref="IWorkflowParser{TSchemeMedium}"/>
        /// </summary>
        /// <param name="bulder">The instance of the workflow builder</param>
        /// <param name="parser">The instance of the workflow scheme parser <see cref="IWorkflowParser{TSchemeMedium}"/></param>
        /// <returns>Configured instance of the workflow builder</returns>
        public static IWorkflowBuilder WithParser<TSchemeMedium>(this WorkflowBuilder<TSchemeMedium> bulder, IWorkflowParser<TSchemeMedium> parser) where TSchemeMedium : class
        {
            bulder.Parser = parser;
            return bulder;
        }

        /// <summary>
        /// Сonfigures the workflow builder with specified scheme persistence provider<see cref="ISchemePersistenceProvider{TSchemeMedium}"/>
        /// </summary>
        /// <param name="bulder">The instance of the workflow builder</param>
        /// <param name="schemePersistenceProvider">The instance of the scheme persistence provider <see cref="ISchemePersistenceProvider{TSchemeMedium}"/></param>
        /// <returns>Configured instance of the workflow builder</returns>
        public static IWorkflowBuilder WithShemePersistenceProvider<TSchemeMedium>(this WorkflowBuilder<TSchemeMedium> bulder, ISchemePersistenceProvider<TSchemeMedium> schemePersistenceProvider) where TSchemeMedium : class
        {
            bulder.SchemePersistenceProvider = schemePersistenceProvider;
            return bulder;
        }

        /// <summary>
        /// Сonfigures the runtime with specified timer manager <see cref="ITimerManager"/>
        /// </summary>
        /// <param name="runtime">The instance of the runtime</param>
        /// <param name="timerManager">The instance of the timer manager <see cref="ITimerManager"/></param>
        /// <returns>Configured instance of the workflow runtime</returns>
        public static WorkflowRuntime WithTimerManager(this WorkflowRuntime runtime, ITimerManager timerManager)
        {
            runtime.TimerManager = timerManager;
            runtime.TimerManager.Init(runtime);
            return runtime;
        }

        /// <summary>
        /// Enable code action compillation in runtime
        /// </summary>
        /// <param name="runtime">The instance of the runtime</param>
        /// <returns>Configured instance of the workflow runtime</returns>
        public static WorkflowRuntime EnableCodeActions(this WorkflowRuntime runtime)
        {
            WorkflowRuntime.CodeActionsCompillationEnable = true;
            return runtime;
        }

        /// <summary>
        /// Switch on compillation debug mode for code actions
        /// </summary>
        /// <param name="runtime">The instance of the runtime</param>
        /// <returns>Configured instance of the workflow runtime</returns>
        public static WorkflowRuntime CodeActionsDebugOn(this WorkflowRuntime runtime)
        {
            WorkflowRuntime.CodeActionsDebugMode = true;
            return runtime;
        }

        /// <summary>
        /// Disable code action compillation in runtime
        /// </summary>
        /// <param name="runtime">The instance of the runtime</param>
        /// <returns>Configured instance of the workflow runtime</returns>
        public static WorkflowRuntime DisableCodeActions(this WorkflowRuntime runtime)
        {
            WorkflowRuntime.CodeActionsCompillationEnable = false;
            return runtime;
        }

        /// <summary>
        /// Register reference on assembly for compilation of code actions
        /// </summary>
        /// <param name="runtime">The instance of the runtime</param>
        /// <param name="assembly">Assembly for register</param>
        /// <param name="ignoreForDesigner">If true then types from the assembly will not be registered in the designer</param>
        /// <param name="designerTypeFilter">Function which filters a types for designer</param>
        /// <returns>Configured instance of the workflow runtime</returns>
        public static WorkflowRuntime RegisterAssemblyForCodeActions(this WorkflowRuntime runtime, Assembly assembly, bool ignoreForDesigner = false,
            Func<Type, bool> designerTypeFilter = null)
        {
            WorkflowRuntime.CodeActionsRegisterAssembly(assembly);
            if (!ignoreForDesigner)
                Workflow.Designer.RegisterTypesFromAssembly(assembly, designerTypeFilter);
            return runtime;
        }

        /// <summary>
        /// Changes a JsonSerializerSettings which are using to serialise parameters in runtime
        /// </summary>
        /// <param name="runtime">The instance of the runtime</param>
        /// <param name="settings">JsonSerializerSettings object</param>
        /// <returns>Configured instance of the workflow runtime</returns>
        [Obsolete("Use ParametersSerializer.Settings to setup your custom Json serializer settings")]
        public static WorkflowRuntime SetParameterSerializerSettings(this WorkflowRuntime runtime, JsonSerializerSettings settings)
        {
            ParametersSerializer.Settings = settings;
            return runtime;
        }

        /// <summary>
        /// Provides access to Workflow Engine .NET Persistence Process Parameters serialization mechanism
        /// </summary>
        /// <param name="runtime">The instance of the runtime</param>
        /// <param name="serializedValue">Serialized value</param>
        /// <param name="parameterType">Parameter type</param>
        /// <returns>Deserialized object</returns>
        [Obsolete("Use ParametersSerializer.Deserialize to deserialize a parameter")]
        public static object DeserializeParameter(this WorkflowRuntime runtime, string serializedValue, Type parameterType)
        {
            return ParametersSerializer.Deserialize(serializedValue,parameterType);
        }

        /// <summary>
        /// Provides access to Workflow Engine .NET Persistence Process Parameters serialization mechanism
        /// </summary>
        /// <param name="runtime">The instance of the runtime</param>
        /// <param name="value">Parameter value</param>
        /// <param name="parameterType">Parameter type</param>
        /// <returns>Deserialized object</returns>
        [Obsolete("Use ParametersSerializer.Serialize to serialize a parameter")]
        public static string SerializeParameter(this WorkflowRuntime runtime, object value, Type parameterType)
        {
            return ParametersSerializer.Serialize(value,parameterType);
        }

        /// <summary>
        /// Sets the order of the Action, Condition, or Rule search by name
        /// </summary>
        public static WorkflowRuntime SetExecutionSearchOrder(this WorkflowRuntime runtime, ExecutionSearchOrder order)
        {
            runtime.ExecutionSearchOrder = order;
            return runtime;
        }

        [Obsolete(
            "If you are sure that your schemes do not mention nonexistent Actions, Conditions or Rules, do not use this setting. This setting is only for resolving possible compatibility issues and can be removed.")]
        public static WorkflowRuntime SetIgnoreMissingExecutionItems(this WorkflowRuntime runtime, bool value)
        {
            runtime.IgnoreMissingExecutionItems = value;
            return runtime;
        }
    }
}
