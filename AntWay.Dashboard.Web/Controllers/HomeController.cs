using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Antway.Core;
using AntWay.Persistence.Model;
using AntWay.Persistence.Provider;
using Newtonsoft.Json;
using Workflow.Designer.Web.ViewModels;

namespace Client.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GeProccessHistoryDataTable(JQueryDataTableParamModel param)
        {
            var processPersistence = new ProcessPersistence
            {
                IDALProcessPersistence = PersistenceObjectsFactory.GetIDALWFLocatorObject()
            };

            var filter = new ProcessHistoryFilter
            {
                PaginatioFromRecord = param.iDisplayStart + 1,
                PaginatioToRecord = param.iDisplayStart + param.iDisplayLength,
                FilterFields = param.sSearch!=null
                                ? JsonConvert
                                    .DeserializeObject<List<DataTableFilterFields>>(param.sSearch)
                                : new List<DataTableFilterFields>(),
            };
            var dataList = processPersistence.GeProccessHistoryDataTableView(filter);

            var data = dataList
                           .Select(
                                   c => new string[]
                                       {
                                           c.Aplicacion,
                                           c.Localizador,
                                           c.EstadoActual,
                                           c.Tags,
                                           c.UltimaActualizacion
                                       }
                                   )
                           .ToList();

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalDisplayRecords = processPersistence
                                       .GeProccessHistoryTotalRegistros(filter),
                aaData = data
            },
            JsonRequestBehavior.AllowGet);
        }


        public ActionResult GeProccessHistoryDetailDataTable(JQueryDataTableParamModel param)
        {
            var processPersistence = new ProcessPersistence
            {
                IDALProcessPersistence = PersistenceObjectsFactory.GetIDALWFLocatorObject()
            };

            var filter = new ProcessHistoryDetailFilter
            {
                PaginatioFromRecord = param.iDisplayStart + 1,
                PaginatioToRecord = param.iDisplayStart + param.iDisplayLength,
                ProcessId = System.Guid.NewGuid(),
            };
            var dataList = processPersistence.GeProccessHistoryDetailDataTableView(filter);

            var data = dataList
                           .Select(
                                   c => new string[]
                                       {
                                          c.Activity,
                                          c.State,
                                          c.TriggerName,
                                          c.TransitionTime,
                                       }
                                   )
                           .ToList();

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalDisplayRecords = processPersistence
                                       .GeProccessHistoryDetailDataTableView(filter),
                aaData = data
            },
            JsonRequestBehavior.AllowGet);
        }

    }

}