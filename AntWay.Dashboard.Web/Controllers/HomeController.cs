using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Antway.Core;
using Newtonsoft.Json;
using AntWay.Dashboard.Web.ViewModels;
using AntWay.Persistence.Provider.Model.DataTable;
using AntWay.Persistence.Provider.Model;

namespace Client.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var schemesPersistence = new SchemesPersistence
            {
                IDALSchemes = PersistenceObjectsFactory.GetIDALWFSchemaObject()
            };

            var vm = new IndexViewModel
            {
                NewSchemeViewModel = new NewSchemeViewModel()
                {
                    DBSchemes = schemesPersistence
                                .GetSchemes()
                                .Select(s => s.DBSchemeName)
                                .Distinct()
                                .ToList()
                }
            };

            return View(vm);
        }


        [HttpPost]
        public JsonResult NewScheme(NewSchemeViewModel vm)
        {
            var schemesPersistence = new SchemesPersistence
            {
                IDALSchemes = PersistenceObjectsFactory.GetIDALWFSchemaObject()
            };

            var schemeView = new WorkflowSchemeView
            {
                SchemeName = vm.NewSchemeName.Replace(" ","_").ToUpper(),
                DBSchemeName = vm.NewSchemeDataBase,
                Description = vm.NewSchemeName
            };

            var newScheme = schemesPersistence.InsertScheme(schemeView);

            return Json(new { success = true });
        }


        #region "Dashboard content"
        public ActionResult GetWorkFlowsDataTable(JQueryDataTableParamModel param)
        {
            var processPersistence = new ProcessPersistence
            {
                IDALProcessPersistence = PersistenceObjectsFactory.GetIDALProcessObject()
            };

            var filter = new DataTableFilters
            {
                PaginationFromRecord = param.iDisplayStart + 1,
                PaginationToRecord = param.iDisplayStart + param.iDisplayLength,

                OrderBySQLQueryColIndex = (param.iSortCol_0 + 1),
                OrderByDirection = param.sSortDir_0,
                FilterFields = param.sSearch != null
                                ? JsonConvert
                                    .DeserializeObject<List<DataTableFilterFields>>(param.sSearch)
                                : new List<DataTableFilterFields>(),
            };

            var dataFiltered = processPersistence
                               .GetWorkFlowsDataTableView(filter);

            var dataList = dataFiltered
                           .Where(w => w.NumFila >= filter.PaginationFromRecord
                                       && w.NumFila <= filter.PaginationToRecord)
                           .ToList();

            if (param.iDisplayStart == 0)
            {
                dataList.Add(
                    new WorkFlowDataTableView
                    {
                        Order = -1,
                        WorkFlow = "TOTAL",
                        TotalProcesos = dataList.Sum(d => d.TotalProcesos),
                        TotalProcesosEstadoEnProceso = dataList.Sum(d => d.TotalProcesosEstadoEnProceso),
                        TotalProcesosEstadoFinalizado = dataList.Sum(d => d.TotalProcesosEstadoFinalizado),
                        TotalProcesosEstadoError = dataList.Sum(d => d.TotalProcesosEstadoError),
                    }
                );

                dataList = dataList
                            .OrderBy(d => d.Order)
                            .ToList();
            }

            var data = dataList
                           .Select(
                                   c => new string[]
                                       {
                                           c.WorkFlow,
                                           c.TotalProcesos.ToString(),
                                           c.TotalProcesosEstadoEnProceso.ToString(),
                                           c.TotalProcesosEstadoFinalizado.ToString(),
                                           c.TotalProcesosEstadoError.ToString(),
                                       }
                                   )
                           .ToList();

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalDisplayRecords = dataFiltered.Count + 1,
                aaData = data
            },
            JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetProccessesHistoryDataTable(JQueryDataTableParamModel param)
        {
            var processPersistence = new ProcessPersistence
            {
                IDALProcessPersistence = PersistenceObjectsFactory.GetIDALProcessObject()
            };

            string[] dataTableColNames = 
            {
                "WorkFlow", "Localizador", "EstadoActual", "Tags", "UltimaActualizacion"
            };

            var filter = new DataTableFilters
            {
                PaginationFromRecord = param.iDisplayStart + 1,
                PaginationToRecord = param.iDisplayStart + param.iDisplayLength,

                OrderByColumnName = dataTableColNames[param.iSortCol_0],
                OrderByDirection = param.sSortDir_0,

                FilterFields = param.sSearch!=null
                                ? JsonConvert
                                    .DeserializeObject<List<DataTableFilterFields>>(param.sSearch)
                                : new List<DataTableFilterFields>(),
            };
            var dataList = processPersistence.GetProccessesHistoryDataTableView(filter);

            var data = dataList
                           .Select(
                                   c => new string[]
                                       {
                                           c.WorkFlow,
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
                                       .GetProccessesHistoryTotalRegistros(filter),
                aaData = data
            },
            JsonRequestBehavior.AllowGet);
        }


        public ActionResult GeProccessHistoryDetailDataTable(JQueryDataTableParamModel param)
        {
            var processPersistence = new ProcessPersistence
            {
                IDALProcessPersistence = PersistenceObjectsFactory.GetIDALProcessObject()
            };

            var filter = new ProcessHistoryDetailFilter
            {
                PaginatioFromRecord = param.iDisplayStart + 1,
                PaginatioToRecord = param.iDisplayStart + param.iDisplayLength,
                ProcessId = System.Guid.NewGuid(),
            };
            var dataList = processPersistence.GetProccessHistoryDetailDataTableView(filter);

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
                                       .GetProccessHistoryDetailDataTableView(filter),
                aaData = data
            },
            JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region "Schemes content"

        public ActionResult GetSchemesDataTable(JQueryDataTableParamModel param)
        {
            var schemesPersistence = new SchemesPersistence
            {
                IDALSchemes = PersistenceObjectsFactory.GetIDALWFSchemaObject()
            };

            var filter = new DataTableFilters
            {
                PaginationFromRecord = param.iDisplayStart + 1,
                PaginationToRecord = param.iDisplayStart + param.iDisplayLength,

                OrderBySQLQueryColIndex = (param.iSortCol_0 + 1),
                OrderByDirection = param.sSortDir_0,
                FilterFields = param.sSearch != null
                                ? JsonConvert
                                    .DeserializeObject<List<DataTableFilterFields>>(param.sSearch)
                                : new List<DataTableFilterFields>(),
            };

            var dataFiltered = schemesPersistence
                               .GetSchemesDataTableView(filter);

            var dataList = dataFiltered
                           .Where(w => w.NumFila >= filter.PaginationFromRecord
                                       && w.NumFila <= filter.PaginationToRecord)
                           .ToList();

            var data = dataList
                           .Select(
                                   c => new string[]
                                       {
                                          c.SchemeName,
                                          c.SchemeDBName,
                                          c.Descripcion,
                                          c.Servicio.ToString()
                                       }
                                   )
                           .ToList();

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalDisplayRecords = dataFiltered.Count,
                aaData = data
            },
            JsonRequestBehavior.AllowGet);
        }


        #endregion
    }

}