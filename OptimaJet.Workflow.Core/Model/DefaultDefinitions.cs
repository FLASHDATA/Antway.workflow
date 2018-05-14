using System;
using System.Collections.Generic;

namespace OptimaJet.Workflow.Core.Model
{
    public static class DefaultDefinitions
    {
        public static readonly ParameterDefinition ParameterProcessId = new ParameterDefinition() { Name = "ProcessId", Type = typeof(Guid), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterParentProcessId = new ParameterDefinition() { Name = "ParentProcessId", Type = typeof(Guid), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterRootProcessId = new ParameterDefinition() { Name = "RootProcessId", Type = typeof(Guid), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterPreviousState = new ParameterDefinition() { Name = "PreviousState", Type = typeof(string), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterCurrentState = new ParameterDefinition() { Name = "CurrentState", Type = typeof(string), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterPreviousStateForDirect = new ParameterDefinition() { Name = "PreviousStateForDirect", Type = typeof(string), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterPreviousStateForReverse = new ParameterDefinition() { Name = "PreviousStateForReverse", Type = typeof(string), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterPreviousActivity = new ParameterDefinition() { Name = "PreviousActivity", Type = typeof(string), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterCurrentActivity = new ParameterDefinition() { Name = "CurrentActivity", Type = typeof(string), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterPreviousActivityForDirect = new ParameterDefinition() { Name = "PreviousActivityForDirect", Type = typeof(string), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterPreviousActivityForReverse = new ParameterDefinition() { Name = "PreviousActivityForReverse", Type = typeof(string), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterCurrentCommand = new ParameterDefinition() { Name = "CurrentCommand", Type = typeof(string), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterExecutedTimer = new ParameterDefinition() { Name = "ExecutedTimer", Type = typeof(string), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterExecutedActivityState = new ParameterDefinition() { Name = "ExecutedActivityState", Type = typeof(string), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterExecutedActivity = new ParameterDefinition() { Name = "ExecutedActivity", Type = typeof(ActivityDefinition), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterExecutedTransition = new ParameterDefinition() { Name = "ExecutedTransition", Type = typeof(TransitionDefinition), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterIdentityId = new ParameterDefinition() { Name = "IdentityId", Type = typeof(string), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterImpersonatedIdentityId = new ParameterDefinition() { Name = "ImpersonatedIdentityId", Type = typeof(string), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterSchemeId = new ParameterDefinition() { Name = "SchemeId", Type = typeof(Guid), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterSchemeCode = new ParameterDefinition() { Name = "SchemeCode", Type = typeof(string), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterIsPreExecution = new ParameterDefinition() { Name = "IsPreExecution", Type = typeof(bool), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterStartTransitionalProcessActivity = new ParameterDefinition() { Name = "StartTransitionalProcessActivity", Type = typeof(string), Purpose = ParameterPurpose.System };
        
        public static readonly ParameterDefinition ParameterIdentityIds = new ParameterDefinition()
                                                                     {
                                                                         Name = "IdentityIds",
                                                                         Type = typeof (IEnumerable<string>),
                                                                         Purpose = ParameterPurpose.System
                                                                     };

        public static readonly ParameterDefinition ParameterIdentityIdsForCurrentActivity = new ParameterDefinition()
                                                                     {
                                                                         Name = "IdentityIdsForCurrentActivity",
                                                                         Type = typeof (IEnumerable<string>),
                                                                         Purpose = ParameterPurpose.System
                                                                     };
                
        public static CommandDefinition CommandAuto = new CommandDefinition() { Name = "Auto" };
        public static CommandDefinition CommandSetState = new CommandDefinition() { Name = "SetState" };
        public static CommandDefinition CommandTimer = new CommandDefinition() { Name = "Timer" };


        public static readonly IEnumerable<ParameterDefinition> DefaultParameters = new List<ParameterDefinition>()
                                                                                        {
                                                                                            ParameterProcessId,
                                                                                            ParameterPreviousState,
                                                                                            ParameterCurrentState,
                                                                                            ParameterPreviousStateForDirect,
                                                                                            ParameterPreviousStateForReverse,
                                                                                            ParameterPreviousActivity,
                                                                                            ParameterPreviousActivityForDirect,
                                                                                            ParameterPreviousActivityForReverse,
                                                                                            ParameterCurrentCommand,
                                                                                            ParameterIdentityId,
                                                                                            ParameterImpersonatedIdentityId,
                                                                                            ParameterExecutedActivityState,
                                                                                            ParameterExecutedActivity,
                                                                                            ParameterExecutedTransition,
                                                                                            ParameterCurrentActivity,
                                                                                            ParameterSchemeId,
                                                                                            ParameterIdentityIds,
                                                                                            ParameterSchemeCode,
                                                                                            ParameterIsPreExecution,
                                                                                            ParameterExecutedTimer,
                                                                                            ParameterIdentityIdsForCurrentActivity,
                                                                                            ParameterParentProcessId,
                                                                                            ParameterRootProcessId,
                                                                                            ParameterStartTransitionalProcessActivity
                                                                                        };
    }
}
