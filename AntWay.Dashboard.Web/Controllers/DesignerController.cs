using System.Collections.Generic;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml.Linq;
using AntWay.Dashboard.Web.Models;
using AntWay.Persistence.Provider.Model;
using Antway.Core.Persistence;
using OptimaJet.Workflow.Core.Runtime;
using AntWay.Core.Providers;
using AntWay.Core.Mapping;
using AntWay.Core.Runtime;
using Sample.Model.Expedientes;
using AntWay.Dashboard.Web.Factories;
using Temp.Factory;

namespace Client.Web.Controllers
{
    public class DesignerController : Controller
    {
        private static List<KeyValuePair<string, string>>
            DatabaseScheme = new List<KeyValuePair<string, string>>();


        public ActionResult Index(string id=null)
        {
            var schemesPersistence = new SchemesPersistence
            {
                IDALSchemes = PersistenceObjectsFactory.GetIDALWFSchemaObject()
            };

            var scheme = schemesPersistence.GetScheme(id);
            if (scheme == null)
            {
                 throw new NotImplementedException("Esquema inexistente");
            }

            IAssemblies assemblies = AssemblyFactory.GetAssemblyObject(id);
            if (assemblies != null)
            {
                WorkflowClient.WithAssemblies(assemblies);
            }

            ICommandsMapping commandsMapping = CommandFactory.GetCommandMapping(id);
            WorkflowClient.WithCommands(commandsMapping);


            WorkflowClient.DataBaseScheme = scheme?.DBSchemeName
                                               ?? ConfigurationManager.AppSettings["WFSchema"]
                                                  .ToString();

            DatabaseScheme.RemoveAll(s => s.Key == scheme.DBSchemeName);
            DatabaseScheme.Add(new KeyValuePair<string, string>
                                (scheme.SchemeCode, scheme.DBSchemeName));

            var vm = new DesignerViewModel { SchemeName = id??"SimpleWF" };
            return View(vm);
        }


        public ActionResult API()
        {
            Stream filestream = null;
            if (Request.Files.Count > 0)
                filestream = Request.Files[0].InputStream;

            var pars = new NameValueCollection();
            pars.Add(Request.Params);
            
            if(Request.HttpMethod.Equals("POST", StringComparison.InvariantCultureIgnoreCase))
            {
                var parsKeys = pars.AllKeys;
                foreach (var key in Request.Form.AllKeys)
                {
                    if (!parsKeys.Contains(key))
                    {
                        pars.Add(Request.Form);
                    }
                }
            }

            if (Request.Params.Keys[0]=="schemecode")
            {
                var schemeCodeFromParameters = Request.Params[0];
                var dbs = DatabaseScheme.FirstOrDefault(s => s.Key == schemeCodeFromParameters);
                WorkflowClient.DataBaseScheme = dbs.Value;
            }

            var res = WorkflowClient.AntWayRunTime.DesignerAPI(pars, filestream);
            var operation = pars["operation"].ToLower();
            if (operation == "downloadscheme")
                return File(Encoding.UTF8.GetBytes(res), "text/xml", "scheme.xml");
	        else if (operation == "downloadschemebpmn")
                return File(UTF8Encoding.UTF8.GetBytes(res), "text/xml", "scheme.bpmn");

            return Content(res);
        }

    }
}


 