using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using AntWay.Dashboard.Web.ViewModels;
using AntWay.Persistence.Provider.Model.DataTable;
using AntWay.Persistence.Provider.Model;
using Antway.Core.Persistence;
using AntWay.Core.Runtime;
using AntWay.Core.Manager;
using IMHab.PreventBlanqueo.SEPBLAC.AntWay.AntWayBinding;
using Temp.Factory;

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
        public JsonResult SchemeNew(NewSchemeViewModel vm)
        {
            var schemesPersistence = new SchemesPersistence
            {
                IDALSchemes = PersistenceObjectsFactory.GetIDALWFSchemaObject()
            };

            var schemeView = new WorkflowSchemeView
            {
                SchemeCode = vm.NewSchemeCode.Replace(" ", "_").ToUpper(),
                SchemeName = vm.NewSchemeName,
                DBSchemeName = vm.NewSchemeDataBase,
                Active = true,
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
                        TotalProcesosEstadoEjecutando = dataList.Sum(d => d.TotalProcesosEstadoEjecutando),
                        TotalProcesosEstadoDormido = dataList.Sum(d => d.TotalProcesosEstadoDormido),
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
                                           c.TotalProcesosEstadoEjecutando.ToString(),
                                           c.TotalProcesosEstadoDormido.ToString(),
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
                "WorkFlow", "Localizador", "Status", "Tags", "UltimaActualizacion"
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
                                           c.StatusDescripcion,
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
                                          c.SchemeCode,
                                          c.SchemeName,
                                          c.SchemeDBName,
                                          //c.WorkFlowService.ToString(),
                                          c.Active.ToString(),
                                          c.SchemeCode, //edit
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

        [HttpPost]
        public ActionResult GetErrorFromLocator(string scheme, string locator)
        {
            var vm = new ActivityManagerViewModel
            {
                TagDescription = "Error no especificado"
            };

            IAssemblies assemblies = AssemblyFactory.GetAssemblyObject(scheme);
            IActivityManager activityManager = new ActivityManagerVoid();

            var managerResponse = WorkflowClient
                                 .StartWF(scheme,
                                          null,
                                          locator, 
                                          assemblies,
                                          activityManager,
                                          false);
            if (!managerResponse.Success)
            {
                var errors = WorkflowClient.AntWayRunTime
                             .GetPersistedErrorMessages(managerResponse);
                vm.TagDescription = String.Join("<br/>", errors);

                var processInstance = WorkflowClient.AntWayRunTime
                                      .GetProcessInstance(managerResponse.ProcessId);
                vm.StateList = WorkflowClient.AntWayRunTime.GetStates(processInstance);
            }
 
            return View("ActivityManager", vm);
        }

        [HttpPost]
        public JsonResult SetState(ActivityStatePostViewModel vm)
        {
            IAssemblies assemblies = AssemblyFactory.GetAssemblyObject(vm.scheme);
            IActivityManager activityManager = new ActivityManagerVoid();

            var managerResponse = WorkflowClient
                                .StartWF(vm.scheme,
                                         null,
                                         vm.locator,
                                         assemblies,
                                         activityManager,
                                         false);


            WorkflowClient.AntWayRunTime
                .SetState(managerResponse.ProcessId, vm.stateToSet, true);

            return Json(new { success = true });
        }


        [HttpPost]
        public ActionResult SchemeGet(string scheme)
        {
            var schemesPersistence = new SchemesPersistence
            {
                IDALSchemes = PersistenceObjectsFactory.GetIDALWFSchemaObject()
            };

            var viewScheme = schemesPersistence.GetScheme(scheme);

            var vm = new EditSchemeViewModel
            {
                SchemeCode = viewScheme.SchemeCode,
                SchemeName = viewScheme.SchemeName,
                SchemeDataBase = viewScheme.DBSchemeName,
                Description = viewScheme.Description,
                Active = viewScheme.Active,
                WorkflowService = viewScheme.WorkflowService,
            };

            return View("SchemeEdit", vm);
        }

        [HttpPost]
        public JsonResult SchemeEdit(EditSchemeViewModel scheme)
        {
            var schemesPersistence = new SchemesPersistence
            {
                IDALSchemes = PersistenceObjectsFactory.GetIDALWFSchemaObject()
            };


            var wfSchemeView = new WorkflowSchemeView
            {
                SchemeCode = scheme.SchemeCode,
                SchemeName = scheme.SchemeName,
                DBSchemeName = scheme.SchemeDataBase,
                Description = scheme.Description,
                Active = scheme.Active,
                WorkflowService = scheme.WorkflowService,
            };

            schemesPersistence.UpdateScheme(wfSchemeView);

            return Json(new { success = true });
        }

        #endregion
    }

}