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
using AntWay.Core.WorkflowEngine;
using OptimaJet.Workflow.Core.Model;
using AntWay.Persistence.Provider.Model;

namespace AntWay.Core.RunTime
{
    public class AntWayRuntime
    {
        protected WorkflowRuntime WorkflowRuntime { get; set; }


        public AntWayRuntime(WorkflowRuntime workflowRuntime)
        {
            WorkflowRuntime = workflowRuntime;
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


        public List<AntWayCommand> GetAvailableCommands(Guid processId, string identityId = null)
        {
            var commands = WorkflowRuntime.GetAvailableCommands(processId, identityId ?? string.Empty);

            var result = commands
                            .Select(c => new AntWayCommand(c))
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

        public void ExecuteCommandNext(Guid processId)
        {
            WorkflowRuntimeExtensions.ExecutecommandNext(WorkflowRuntime, processId);
        }


        public bool ExecuteCommand(Guid processId, AntWayCommand command,
                                   string identifyId = null)
        {
            return WorkflowRuntimeExtensions.ExecuteCommand(WorkflowRuntime, 
                                                            command.WorkflowCommand,
                                                            identifyId);
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
                                                       List<AntWayCommandParameter> commandParameters,
                                                       string identityId = null)
        {
            var cmdCallWorkFlow = WorkflowRuntime
                             .GetAvailableCommands(processId, identityId ?? string.Empty)
                             .FirstOrDefault(c => c.CommandName.ToLower() == "next");

            var antWayCommand = new AntWayCommand(cmdCallWorkFlow);

            string fromActivity = WorkflowClient.AntWayRunTime
                                 .GetCurrentActivityName(processId);

            string schemeName = fromActivity;
            string fromState = WorkflowClient.AntWayRunTime
                                  .GetCurrentStateName(processId);

            WorkflowClient.AntWayRunTime.ExecuteCommand(processId, antWayCommand);

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


        internal AntWayResponseView CallWorkflow(string schemeName,
                                        Guid? fromProcessGuid,
                                        List<AntWayCommandParameter> commandParameters)
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

            AntWayCommand command = WorkflowClient.AntWayRunTime
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
                    Description = $"Parameters does not match {schemeName}. Check DescriptionDetail.",
                    DescriptionDetail = GetSchemeParameters(schemeName)
                                        .Select(cp => $"Parameter {cp.ParameterName} of type {cp.Type.Name} is required {cp.IsRequired.ToString()}")
                                        .ToList()
                };
            }

            foreach (var c in commandParameters)
            {
                command.SetParameter(c.ParameterName, c.Value);
            }

            WorkflowClient.AntWayRunTime.ExecuteCommand(guid, command);

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


        internal List<AntWayCommandParameter> GetSchemeParameters(string schemeName)
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

            AntWayCommand command = WorkflowClient.AntWayRunTime
                             .GetAvailableCommands(guid, string.Empty)
                             .FirstOrDefault();

            var result = command.WorkflowCommand
                            .Parameters
                            .Select(p => new AntWayCommandParameter(p.ParameterName, p.Value
                                                        , p.IsRequired, p.Type))
                            .ToList();

            return result;
        }

        public bool ParametersMatchTheCommand
                                    (List<AntWayCommandParameter> sendedParameters,
                                     AntWayCommand commandToExecute)
        {
            var requiredParameters = commandToExecute.WorkflowCommand
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
    }
}
