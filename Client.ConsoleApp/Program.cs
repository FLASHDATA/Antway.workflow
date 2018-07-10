using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Configuration;
using AntWay.Core.WorkflowEngine;
using AntWay.Core.Model;
using AntWay.Persistence.Provider.Model;

namespace Client.ConsoleApp
{
    class Program
    {
        static Guid ProcessId;
        static void Main(string[] args)
        {
            EjemploCallWorkFlow();

            //Ejemplo1();

            //PersistenceProviderSample();
            //GetProcessDefinitionSample();
            //WorkflowSample();

            Console.WriteLine("Pulsa Enter para ejecutar un ejemplo");
            Console.ReadLine();
            while (true)
            {
                EjemploLlamadaAlProcesoX();
                Console.WriteLine("Pulsa Enter para seguir");
                Console.ReadLine();
            }

            Console.WriteLine("Pulsa enter para cerrar");
            Console.ReadLine();
        }

        private static void EjemploCallWorkFlow()
        {
            var processPersistenceViewNew = new ProcessPersistenceView
            {
                WFProcessGuid = ProcessId,
                LocatorFieldName = "Expedients.NUM_EXPEDIENT",
                LocatorValue = System.Guid.NewGuid().ToString(),
                SchemeCode = "SEPBLAC",
                SchemeDatabase = "WFSCHEMA1",
            };

            ProcessId = WorkflowClient.GetAntWayRunTime("SEPBLAC")
                        .CreateInstanceAndPersist(processPersistenceViewNew);

            bool wfCmdAvailable = WorkflowClient.AntWayRunTime.WorkflowCommandAvailable(ProcessId);

            if (!wfCmdAvailable)
            {
                Console.WriteLine("No hay ningun comando workflow habilitado");
                return;
            }

            List<AntWayCommandParameter> awCmdParameters = new List<AntWayCommandParameter>
            {
                new AntWayCommandParameter("EsPRG", false),
                new AntWayCommandParameter("IndiciosActuaCuentaAjena", false),
                new AntWayCommandParameter("EsPJAsociacionSinAnimoLucro", false),
                new AntWayCommandParameter("EsPFMenorDeEdad", true),
                new AntWayCommandParameter("EsPFDiscapacitado", false),
                new AntWayCommandParameter("ExisteRepresentante", true),
                new AntWayCommandParameter("EsRepPRG", false),
                new AntWayCommandParameter("RepIndiciosActuaCuentaAjena", false),
                new AntWayCommandParameter("PaisEsParaisoFiscal", false),
                new AntWayCommandParameter("RepPaisEsParaisoFiscal", false),
                new AntWayCommandParameter("EsPJInversionInmuebles", false),
                new AntWayCommandParameter("EsPFSinActividadEconomica", false),
                new AntWayCommandParameter("EsPFActividadCuentaPropia", false),
                new AntWayCommandParameter("EsRepActividadCuentaPropia", true),
                new AntWayCommandParameter("EsRepSinActividadEconomica", false),
            };

            var result = WorkflowClient.AntWayRunTime
                         .CallWorkflowCommand(ProcessId, awCmdParameters);
        }


        private static void EjemploLlamadaAlProcesoX()
        {
            Console.WriteLine("Ejemplo lamada al ProcesoX");
            //ProcessId = CreateInstance("FOO");

            WorkflowClient.AntWayRunTime.ExecuteCommandNext(ProcessId);

            Console.WriteLine("ProcessId = '{0}'. CurrentState: {1}, CurrentActivity: {2}",
                        ProcessId,
                        WorkflowClient.AntWayRunTime.GetCurrentStateName(ProcessId),
                        WorkflowClient.AntWayRunTime.GetCurrentActivityName(ProcessId));
            Console.WriteLine("Espera 5 segundos y saltará la action que llama a 'Example.OWIN.Service.HomeController.Get'");

            System.Threading.Thread.Sleep(5000);
            Console.WriteLine("Pulsa enter y consulta estado respuesta");
            Console.ReadLine();

            var processInstance = WorkflowClient.AntWayRunTime
                                 .GetProcessInstance(ProcessId);
            
            var statusProcesoX = processInstance.GetParameter("httpResponse_ProcesoX")?.Value;
            Console.WriteLine($"{statusProcesoX}");
        }


        private static void EjemploMailing()
        {
            Console.WriteLine("EjemploMailing");
            //CreateInstance("FOO");
            WorkflowClient.AntWayRunTime.ExecuteCommandNext(ProcessId);

            Console.WriteLine("ProcessId = '{0}'. CurrentState: {1}, CurrentActivity: {2}",
                        ProcessId,
                        WorkflowClient.AntWayRunTime.GetCurrentStateName(ProcessId),
                        WorkflowClient.AntWayRunTime.GetCurrentActivityName(ProcessId));
            Console.WriteLine("Espera 5 segundos y saltará la action 'MyAction'");
            Console.WriteLine("Espera 12 segundos y saltará la action 'MyAction2'");

            //System.Threading.Thread.Sleep(5200);

            Console.WriteLine("ProcessId = '{0}'. CurrentState: {1}, CurrentActivity: {2}",
                       ProcessId,
                       WorkflowClient.AntWayRunTime.GetCurrentStateName(ProcessId),
                       WorkflowClient.AntWayRunTime.GetCurrentActivityName(ProcessId));

            ExecuteCommand();
        }

        private static void WorkflowSample()
        {
            Console.WriteLine("Operation:");
            Console.WriteLine("1 - GetAvailableCommands");
            Console.WriteLine("2 - ExecuteCommand");
            Console.WriteLine("3 - GetAvailableState");
            Console.WriteLine("4 - SetState");
            Console.WriteLine("5 - DeleteProcess");
            Console.WriteLine("9 - Exit");
            Console.WriteLine("The process isn't created.");
            //CreateInstance("FOO");

            do
            {
                Console.Write("Enter code of operation:");
                char operation = Console.ReadLine().FirstOrDefault();

                switch (operation)
                {
                    case '1':
                        GetAvailableCommands();
                        break;
                    case '2':
                        ExecuteCommand();
                        break;
                    case '3':
                        GetAvailableState();
                        break;
                    case '4':
                        SetState();
                        break;
                    case '5':
                        DeleteProcess();
                        break;
                    case '9':
                        return;
                    default:
                        Console.WriteLine("Unknown code. Please, repeat.");
                        break;
                }
                Console.WriteLine();
            } while (true);
        }

        private static void GetAvailableCommands()
        {
            if (ProcessId == null)
            {
                Console.WriteLine("The process isn't created. Please, create process instance.");
                return;
            }

            var commands = WorkflowClient.AntWayRunTime.GetAvailableCommands(ProcessId, string.Empty);

            Console.WriteLine("Available commands:");
            if (commands.Count() == 0)
            {
                Console.WriteLine("Not found!");
            }
            else
            {
                foreach (var command in commands)
                {
                    Console.WriteLine("- {0}", command.CommandName);
                } 
            }
        }

        private static void ExecuteCommand()
        {
            if (ProcessId == null)
            {
                Console.WriteLine("The process isn't created. Please, create process instance.");
                return;
            }
            AntWayCommand command = null;
            do
            {
                GetAvailableCommands();
                Console.Write("Enter command:");
                var commandName = Console.ReadLine().ToLower().Trim();
                if (commandName == string.Empty)
                    return;
                command = WorkflowClient.AntWayRunTime.GetAvailableCommands(ProcessId, string.Empty)
                    .Where(c => c.CommandName.Trim().ToLower() == commandName).FirstOrDefault();
                if (command == null)
                    Console.WriteLine("The command isn't found.");
            } while (command == null);



            //foreach(var cp in command.Parameters??new List<CommandParameter>())
            //{
            //    Console.Write($"{cp.ParameterName}:");
            //    var stringValue = Console.ReadLine();

            //    switch (cp.Type.Name.ToLower())
            //    {
            //        case "decimal":
            //            command.SetParameter(cp.ParameterName, Convert.ToDecimal(stringValue));
            //            break;

            //        case "boolean":
            //            command.SetParameter(cp.ParameterName, stringValue == "1");
            //            break;

            //        case "string":
            //            command.SetParameter(cp.ParameterName, stringValue);
            //            break;

            //        default:
            //            command.SetParameter(cp.ParameterName, stringValue);
            //            break;
            //    }
            //}

            var result = WorkflowClient
                            .AntWayRunTime
                            .ExecuteCommand(ProcessId, command);

                        Console.WriteLine("ExecuteCommand - OK.", ProcessId);
        }

        private static void GetAvailableState()
        {
            //if (processId == null)
            //{
            //    Console.WriteLine("The process isn't created. Please, create process instance.");
            //    return;
            //}
            //var states = WorkflowClient.Runtime.GetAvailableStateToSet(processId.Value, Thread.CurrentThread.CurrentCulture);
            //Console.WriteLine("Available state to set:");
            //if (states.Count() == 0)
            //{
            //    Console.WriteLine("Not found!");
            //}
            //else
            //{
            //    foreach (var state in states)
            //    {
            //        Console.WriteLine("- {0}", state.Name);
            //    }
            //}
        }

        private static void SetState()
        {
            //if (processId == null)
            //{
            //    Console.WriteLine("The process isn't created. Please, create process instance.");
            //    return;
            //}
            //string stateName = string.Empty;
            //WorkflowState state;
            //do
            //{
            //    GetAvailableState();
            //    Console.Write("Enter state:");
            //    stateName = Console.ReadLine().ToLower().Trim();
            //    if (stateName == string.Empty)
            //        return;
            //    state = WorkflowClient.Runtime.GetAvailableStateToSet(processId.Value, Thread.CurrentThread.CurrentCulture)
            //        .Where(c => c.Name.Trim().ToLower() == stateName).FirstOrDefault();
            //    if (state == null)
            //        Console.WriteLine("The state isn't found.");
            //    else
            //        break;
            //} while (true);
            //if (state != null)
            //{
            //    WorkflowClient.Runtime.SetState(processId.Value, string.Empty, string.Empty, state.Name, new Dictionary<string, object>());
            //    Console.WriteLine("SetState - OK.", processId);
            //}
        }

        private static void DeleteProcess()
        {
            //if (processId == null)
            //{
            //    Console.WriteLine("The process isn't created. Please, create process instance.");
            //    return;
            //}
            //WorkflowClient.Runtime.PersistenceProvider.DeleteProcess(processId.Value);
            //Console.WriteLine("DeleteProcess - OK.", processId);
            //processId = null;
        }
    }



}
