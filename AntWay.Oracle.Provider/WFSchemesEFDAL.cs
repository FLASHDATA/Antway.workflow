using System;
using System.Collections.Generic;
using System.Linq;
using AntWay.Oracle.Provider.Data;
using AntWay.Persistence.Provider.Model;
using AntWay.Persistence.Provider.Model.DataTable;
using Devart.Data.Oracle;

namespace AntWay.Oracle.Provider
{
    public class WFSchemesEFDAL : IDALWFSchemes
    {
        public List<SchemeDataTableView> GetSchemesDataTableView(DataTableFilters filter)
        {
            var result = new List<SchemeDataTableView>();

            using (var ctx = new Model1())
            {
                string sql = "SELECT SCHEME_CODE as SchemeCode" +
                             ", SCHEME_NAME as SchemeName" +
                             ", DB_SCHEME_NAME as SchemeDBName" +
                             ", WORKFLOW_SERVICE as WorkFlowService" +
                             ", ACTIVE as Active" +
                             ", ROWNUM AS NumFila" +
                             " FROM WF_SCHEMES";

                filter.FilteredStringFields
                .ForEach(f => sql += $" WHERE {f.Field} like '%' || :{f.Field} || '%'");

                sql += $" ORDER BY {filter.OrderBySQLQueryColIndex} {filter.OrderByDirection}";


                var parameters = new List<OracleParameter>();

                filter.FilteredStringFields
                .ForEach(f => parameters.Add(new OracleParameter(f.Field, f.Value)));

                result = ctx.Database.SqlQuery<SchemeDataTableView>(sql, parameters.ToArray())
                         .ToList();
            }

            return result;
        }


        public List<WorkflowSchemeView> GetWorkflowSchemes()
        {
            using (var ctx = new Model1())
            {
                var result = ctx.WF_SCHEMES
                             .ToList()
                             .Select(s => new WorkflowSchemeView
                                     {
                                         SchemeCode = s.SCHEME_CODE,
                                         DBSchemeName = s.DB_SCHEME_NAME,
                                         SchemeName = s.SCHEME_NAME,
                                         Description = s.DESCRIPTION,
                                         WorkflowService = (s.WORKFLOW_SERVICE == 1),
                                         Active = (s.WORKFLOW_SERVICE == 1),
                                     })
                             .ToList();

                return result;
            }
        }

        public List<WorkflowSchemeView> GetWorkflowSchemesServices()
        {
            var ws = GetWorkflowSchemes();

            var result = ws
                        .Where(s => s.WorkflowService)
                        .ToList();

            return result;
        }

        public T Fetch<T>(object pk)
        {
            string id = Convert.ToString(pk ?? "");

            using (var ctx = new Model1())
            {
                var entity = ctx.WF_SCHEMES
                             .FirstOrDefault(q => q.SCHEME_CODE.ToUpper() == id.ToUpper());

                var result = MapFromDalToView(entity);

                return (T)Convert.ChangeType(result, typeof(T));
            }
        }


        public T Insert<T>(T objectView)
        {
            var value = MapFromViewToDal(objectView);

            using (var ctx = new Model1())
            {
                ctx.WF_SCHEMES.Attach(value);
                ctx.Entry(value).State = System.Data.Entity.EntityState.Added;
                int i = ctx.SaveChanges();

                var result = MapFromDalToView(value);

                return (T)Convert.ChangeType(result, typeof(T));
            }
        }


        public T Update<T>(T objectView)
        {
            var value = MapFromViewToDal(objectView);

            using (var ctx = new Model1())
            {
                ctx.WF_SCHEMES.Attach(value);
                ctx.Entry(value).State = System.Data.Entity.EntityState.Modified;
                int i = ctx.SaveChanges();

                var result = MapFromDalToView(value);

                return (T)Convert.ChangeType(result, typeof(T));
            }
        }


        private WorkflowSchemeView MapFromDalToView(WF_SCHEMES entity)
        {
            if (entity == null) return null;

            var view = new WorkflowSchemeView
            {
                SchemeCode = entity.SCHEME_CODE,
                SchemeName = entity.SCHEME_NAME,
                DBSchemeName = entity.DB_SCHEME_NAME,
                Description = entity.DESCRIPTION,
                Active = (entity.ACTIVE == 1),
                WorkflowService = (entity.WORKFLOW_SERVICE == 1)
            };

            return view;
        }


        private WF_SCHEMES MapFromViewToDal<T>(T objectView)
        {
            var view = (WorkflowSchemeView)Convert.ChangeType(objectView, typeof(T));

            var entity = new WF_SCHEMES
            { 
                SCHEME_CODE = view.SchemeCode,
                SCHEME_NAME = view.SchemeName,
                DB_SCHEME_NAME = view.DBSchemeName,
                DESCRIPTION = view.Description,
                WORKFLOW_SERVICE = view.WorkflowService ? 1 : 0,
                ACTIVE = view.Active ? 1 : 0,
            };

            return entity;
        }

        public SchemeDataTableView Fetch(string schemeCode, string locatorValue)
        {
            throw new NotImplementedException();
        }
    }
}
