using System;
using OptimaJet.Workflow.Core.Runtime;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using WorkFlowEngine;
using AntWay.Core;
using System.Globalization;
using AntWay.BLL;
using AntWay.EFDAL;
using System.Configuration;

namespace Client.ConsoleApp
{
    class Program
    {
        static Guid Raw16ToGuid(string text)
        {
            byte[] ret = new byte[text.Length / 2];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = Convert.ToByte(text.Substring(i * 2, 2), 16);
            }

            return new Guid(ret);
        }


        static string schemeCode = "SimpleWF";
        static Guid? processId = null;
        static void Main(string[] args)
        {
            Workflow.SingleDataBaseScheme = ConfigurationManager.AppSettings["WFSchema"].ToString();

            //Workflow.ActionProvider = Workflow.ActionProvider ?? new ActionProvider();
            //Workflow.SingleDataBaseScheme = "XX";

            //Ejemplo1();
            //Ejemplo2();

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


        private static void EjemploLlamadaAlProcesoX()
        {
            Console.WriteLine("Ejemplo lamada al ProcesoX");
            CreateInstance();
            var command = Workflow.SingleRuntime
                            .GetAvailableCommands(processId.Value, string.Empty)
                            .Where(c => c.CommandName.Trim().ToLower() == "next")
                            .FirstOrDefault();

            Workflow.SingleRuntime.ExecuteCommand(command, string.Empty, string.Empty);
            Console.WriteLine("ProcessId = '{0}'. CurrentState: {1}, CurrentActivity: {2}",
                        processId,
                        Workflow.SingleRuntime.GetCurrentStateName(processId.Value),
                        Workflow.SingleRuntime.GetCurrentActivityName(processId.Value));
            Console.WriteLine("Espera 5 segundos y saltará la action que llama a 'Example.OWIN.Service.HomeController.Get'");

            System.Threading.Thread.Sleep(5000);
            Console.WriteLine("Pulsa enter y consulta estado respuesta");
            Console.ReadLine();

            var processInstance = Workflow.SingleRuntime
                                 .GetProcessInstanceAndFillProcessParameters(processId.Value);
            
            var statusProcesoX = processInstance.GetParameter("httpResponse_ProcesoX")?.Value;
            Console.WriteLine($"{statusProcesoX}");
        }


        private static void EjemploMailing()
        {
            Console.WriteLine("EjemploMailing");
            CreateInstance();
            var command = Workflow.SingleRuntime
                            .GetAvailableCommands(processId.Value, string.Empty)
                            .Where(c => c.CommandName.Trim().ToLower() == "next")
                            .FirstOrDefault();

            Workflow.SingleRuntime.ExecuteCommand(command, string.Empty, string.Empty);
            Console.WriteLine("ProcessId = '{0}'. CurrentState: {1}, CurrentActivity: {2}",
                        processId,
                        Workflow.SingleRuntime.GetCurrentStateName(processId.Value),
                        Workflow.SingleRuntime.GetCurrentActivityName(processId.Value));
            Console.WriteLine("Espera 5 segundos y saltará la action 'MyAction'");
            Console.WriteLine("Espera 12 segundos y saltará la action 'MyAction2'");

            //System.Threading.Thread.Sleep(5200);

            Console.WriteLine("ProcessId = '{0}'. CurrentState: {1}, CurrentActivity: {2}",
                       processId,
                       Workflow.SingleRuntime.GetCurrentStateName(processId.Value),
                       Workflow.SingleRuntime.GetCurrentActivityName(processId.Value));

            ExecuteCommand();
        }

        private static void GetProcessDefinitionSample()
        {
            var pd = WorkFlowEngine.Workflow.SingleRuntime.GetProcessDefinition(schemeCode);

            var schemeParameters = pd.Parameters
                                   .Where(p => p.Purpose != OptimaJet.Workflow.Core.Model.ParameterPurpose.System)
                                   .ToList();

            var schemeCommands = pd.Commands;
            //TODO: Implementar una clase por cada comando
            //Ejemplo:
            //public class cmdGO : ICmd
            //{
            //    public string Name { get; set; }
            //    public List<Parameter> Parameters { }
            //}
        }

        private static void WorkflowSample()
        {
            Console.WriteLine("Operation:");
            Console.WriteLine("0 - CreateInstance");
            Console.WriteLine("1 - GetAvailableCommands");
            Console.WriteLine("2 - ExecuteCommand");
            Console.WriteLine("3 - GetAvailableState");
            Console.WriteLine("4 - SetState");
            Console.WriteLine("5 - DeleteProcess");
            Console.WriteLine("9 - Exit");
            Console.WriteLine("The process isn't created.");
            CreateInstance();

            do
            {
                if (processId.HasValue)
                {
                    Console.WriteLine("ProcessId = '{0}'. CurrentState: {1}, CurrentActivity: {2}",
                        processId,
                        Workflow.SingleRuntime.GetCurrentStateName(processId.Value),
                        Workflow.SingleRuntime.GetCurrentActivityName(processId.Value));
                }

                Console.Write("Enter code of operation:");
                char operation = Console.ReadLine().FirstOrDefault();

                switch (operation)
                {
                    case '0':
                        CreateInstance();
                        break;
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

        private static void CreateInstance()
        {
            processId = Guid.NewGuid();
            try
            {
                Workflow.SingleRuntime.CreateInstance(schemeCode, processId.Value);
                Console.WriteLine("CreateInstance - OK.", processId);
            }
            catch (Exception ex)
            {
                Console.WriteLine("CreateInstance - Exception: {0}", ex.Message);
                processId = null;
            }
        }

        private static void GetAvailableCommands()
        {
            if (processId == null)
            {
                Console.WriteLine("The process isn't created. Please, create process instance.");
                return;
            }

            //var processGuid = Raw16ToGuid("66160E76DBE42D4CAFA288CD1DF1E15D");
            //var commands1 = WorkflowInit.Runtime.GetAvailableCommands(processGuid, string.Empty);
            //var activityname = WorkflowInit.Runtime.GetCurrentActivityName(processId.Value);


            var commands = Workflow.SingleRuntime.GetAvailableCommands(processId.Value, string.Empty);

            Console.WriteLine("Available commands:");
            if (commands.Count() == 0)
            {
                Console.WriteLine("Not found!");
            }
            else
            {
                foreach (var command in commands)
                {
                    Console.WriteLine("- {0} (LocalizedName:{1}, Classifier:{2})", command.CommandName, command.LocalizedName, command.Classifier);
                } 
            }
        }

        private static void ExecuteCommand()
        {
            if (processId == null)
            {
                Console.WriteLine("The process isn't created. Please, create process instance.");
                return;
            }
            WorkflowCommand command = null;
            do
            {
                GetAvailableCommands();
                Console.Write("Enter command:");
                var commandName = Console.ReadLine().ToLower().Trim();
                if (commandName == string.Empty)
                    return;
                command = Workflow.SingleRuntime.GetAvailableCommands(processId.Value, string.Empty)
                    .Where(c => c.CommandName.Trim().ToLower() == commandName).FirstOrDefault();
                if (command == null)
                    Console.WriteLine("The command isn't found.");
            } while (command == null);
            

            
            foreach(var cp in command.Parameters??new List<CommandParameter>())
            {
                Console.Write($"{cp.ParameterName}:");
                var stringValue = Console.ReadLine();

                switch (cp.Type.Name.ToLower())
                {
                    case "decimal":
                        command.SetParameter(cp.ParameterName, Convert.ToDecimal(stringValue));
                        break;

                    case "boolean":
                        command.SetParameter(cp.ParameterName, stringValue == "1");
                        break;

                    case "string":
                        command.SetParameter(cp.ParameterName, stringValue);
                        break;

                    default:
                        command.SetParameter(cp.ParameterName, stringValue);
                        break;
                }
            }
 
            Workflow.SingleRuntime.ExecuteCommand(command, string.Empty, string.Empty);

            Console.WriteLine("ExecuteCommand - OK.", processId);
        }

        private static void GetAvailableState()
        {
            if (processId == null)
            {
                Console.WriteLine("The process isn't created. Please, create process instance.");
                return;
            }
            var states = Workflow.SingleRuntime.GetAvailableStateToSet(processId.Value, Thread.CurrentThread.CurrentCulture);
            Console.WriteLine("Available state to set:");
            if (states.Count() == 0)
            {
                Console.WriteLine("Not found!");
            }
            else
            {
                foreach (var state in states)
                {
                    Console.WriteLine("- {0}", state.Name);
                }
            }
        }

        private static void SetState()
        {
            if (processId == null)
            {
                Console.WriteLine("The process isn't created. Please, create process instance.");
                return;
            }
            string stateName = string.Empty;
            WorkflowState state;
            do
            {
                GetAvailableState();
                Console.Write("Enter state:");
                stateName = Console.ReadLine().ToLower().Trim();
                if (stateName == string.Empty)
                    return;
                state = Workflow.SingleRuntime.GetAvailableStateToSet(processId.Value, Thread.CurrentThread.CurrentCulture)
                    .Where(c => c.Name.Trim().ToLower() == stateName).FirstOrDefault();
                if (state == null)
                    Console.WriteLine("The state isn't found.");
                else
                    break;
            } while (true);
            if (state != null)
            {
                Workflow.SingleRuntime.SetState(processId.Value, string.Empty, string.Empty, state.Name, new Dictionary<string, object>());
                Console.WriteLine("SetState - OK.", processId);
            }
        }

        private static void DeleteProcess()
        {
            if (processId == null)
            {
                Console.WriteLine("The process isn't created. Please, create process instance.");
                return;
            }
            Workflow.SingleRuntime.PersistenceProvider.DeleteProcess(processId.Value);
            Console.WriteLine("DeleteProcess - OK.", processId);
            processId = null;
        }
    }



}
