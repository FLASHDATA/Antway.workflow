using System.Collections.Generic;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml.Linq;
using AntWay.Core.WorkflowEngine;
using Workflow.Scheme.Designer.Web.Models;
using AntWay.Persistence.Provider;
using Antway.Core;

namespace WF.Sample.Controllers
{
    public class DesignerController : Controller
    {
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

            WorkflowClient.DataBaseScheme = scheme?.DBSchemeName
                                               ?? ConfigurationManager.AppSettings["WFSchema"].ToString();

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


 