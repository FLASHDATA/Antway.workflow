using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AntWay.BLL;
using AntWay.EFDAL;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;

namespace AntWay.Core
{
    public static class WorkflowRuntimeExtensions
    {
        public static string InsertarNotificacion(ProcessInstance processInstance,
                                                   string message,
                                                   string toUser, string toGroup)
        {
            string httpResponse = "200";
            try
            {
                string urlInsertarNotificacion = "http://localhost:9000/api/home/InsertarNotificacion";

                var schemePersistence = new SchemesPersistenceBLL
                                            { IDALLocator = new WFLocatorEFDAL(), IDALSchema = new WFSchemaEFDAL() };
                var wfScheme = schemePersistence.GetWorkflowSchemes(processInstance.ProcessId);

                string urlService = $"{urlInsertarNotificacion}"+
                                    $"?fromScheme={processInstance?.SchemeCode ?? ""}"+
                                    $"&locator={wfScheme?.LocatorValue??""}" +
                                    $"&toUser={toUser??""}&toGroup={toGroup??""}" +
                                    $"&message={message}";
                
                var response = new HttpClient().GetAsync(urlService).Result;
                httpResponse = Convert.ToInt16(response.StatusCode).ToString();
            }
            catch (Exception ex)
            {
                httpResponse = "404";
            }

            return httpResponse;
        }


        public static bool ExecutecommandNext(WorkflowRuntime worklowRuntime, 
                                       Guid wfProcessGuid,
                                       string identifyId = null)
        {
            return Executecommand(worklowRuntime, wfProcessGuid, "next", identifyId);
        }


        public static bool Executecommand(WorkflowRuntime worklowRuntime,
                                          Guid wfProcessGuid, string commandName,
                                          string identifyId = null)
        {
            WorkflowCommand command = worklowRuntime
                                      .GetAvailableCommands(wfProcessGuid, identifyId ?? string.Empty)
                                      .FirstOrDefault(c => c.CommandName.Trim().ToLower() == commandName.ToLower());

            if (command == null) return false;

            var cmdExecResult = worklowRuntime.ExecuteCommand(command, identifyId ?? string.Empty, string.Empty);

            return cmdExecResult.WasExecuted;
        }
    }
}
