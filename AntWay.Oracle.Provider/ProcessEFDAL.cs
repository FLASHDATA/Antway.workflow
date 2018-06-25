using System;
using System.Collections.Generic;
using System.Linq;
using AntWay.Oracle.Provider.Data;
using AntWay.Persistence.Provider;
using AntWay.Persistence.Model;
using Devart.Data.Oracle;


namespace AntWay.Oracle.Provider
{
    public class ProcessEFDAL : IDALProcessPersistence
    {

        public int GeProccessHistoryTotalRegistros(ProcessHistoryFilter filter)
        {
            int result = 0;
            using (var ctx = new Model1())
            {
                string sql = "SELECT distinct(Localizador)" +
                             " FROM VW_PROCESS_HISTORY" +
                             " WHERE 1=1" +
                             " AND UltimaActualizacion>=:DateFrom" +
                             " AND UltimaActualizacion<=:DateTo";  

                filter.FilteredStringFields
                    .ForEach(f => sql += $" AND {f.Field} like '%' || :{f.Field} || '%'");

                var parameters = new List<OracleParameter>
                {
                    new OracleParameter("DateFrom", filter.FilterFrom.Value),
                    new OracleParameter("DateTo", filter.FilterTo.Value),
                };

                filter.FilteredStringFields
                    .ForEach(f => parameters.Add(new OracleParameter(f.Field, f.Value)));


                var processesList = ctx.Database.SqlQuery<string>
                                            (sql, parameters.ToArray())
                                            .ToList();

                result = processesList.Count();
            }

            return result;
        }

        public List<ProcessHistoryDataTableView>
                        GeProccessHistoryDataTableView(ProcessHistoryFilter filter)
        {
            var result = new List<ProcessHistoryDataTableView>();
            using (var ctx = new Model1())
            {
                string sql = "SELECT Aplicacion, Localizador" +
                             ", EstadoActual, Tags, UltimaActualizacion" +
                             " FROM VW_PROCESS_HISTORY" +
                             " WHERE NUM_FILA >=:FromRecord AND NUM_FILA <=:ToRecord" +
                             " AND UltimaActualizacion>=:DateFrom " +
                             " AND UltimaActualizacion<=:DateTo ";

                filter.FilteredStringFields
                    .ForEach(f => sql += $" AND {f.Field} like '%' || :{f.Field} || '%'");


                var parameters = new List<OracleParameter>
                                    {
                                        new OracleParameter("DateFrom", filter.FilterFrom.Value),
                                        new OracleParameter("DateTo", filter.FilterTo.Value),
                                        new OracleParameter("FromRecord", filter.PaginatioFromRecord),
                                        new OracleParameter("ToRecord", filter.PaginatioToRecord),
                                    };

                filter.FilteredStringFields
                    .ForEach(f => parameters.Add(new OracleParameter(f.Field, f.Value)));

                result = ctx.Database.SqlQuery<ProcessHistoryDataTableView>
                                (sql, parameters.ToArray())
                                .ToList();
            }


            return result;
        }


        public int GeProccessHistoryDetailTotalRegistros(ProcessHistoryDetailFilter filter)
        {
            int result = 0;
            using (var ctx = new Model1())
            {
                string sql = "SELECT distinct(Id)" +
                             " FROM VW_PROCESS_DETAIL" +
                             " WHERE ProcessId = :ProcessId";

                var parameters = new List<OracleParameter>
                {
                    new OracleParameter("ProcessId",filter.ProcessId)
                };

                var processesList = ctx.Database.SqlQuery<string>
                                            (sql, parameters.ToArray())
                                            .ToList();

                result = processesList.Count();
            }

            return result;
        }


        public List<ProcessHistoryDetailDataTableView> 
                    GetProcessHistorryDetailTableView(ProcessHistoryDetailFilter filter)
        {
            var result = new List<ProcessHistoryDetailDataTableView>();
            using (var ctx = new Model1())
            {
                string sql = "SELECT FromActivityName, ToActivityName," +
                             " FromStateName, ToStateName, TriggerName, Transitiontime" +
                             " FROM VW_PROCESS_DETAIL" +
                             " WHERE ProcessId = :ProcessId" +
                             " AND NUM_FILA >=:FromRecord AND NUM_FILA <=:ToRecord";

                var parameters = new List<OracleParameter>
                {
                    new OracleParameter("ProcessId", filter.ProcessId),
                    new OracleParameter("FromRecord", filter.PaginatioFromRecord),
                    new OracleParameter("ToRecord", filter.PaginatioToRecord),
                };

                result = ctx.Database.SqlQuery<ProcessHistoryDetailDataTableView>
                              (sql, parameters.ToArray())
                              .ToList();
            }

            return result;
        }

        public ProcessPersistenceView GetLocatorFromGuid(Guid guid)
        {
            using (var ctx = new Model1())
            {
                var entity = ctx.WF_LOCATOR
                             .FirstOrDefault(q => q.ID_WFPROCESSINSTANCE == guid);

                var result = MapFromDalToView(entity);
                return result;
            }
        }

        public T Fetch<T>(object pk)
        {
            string id = Convert.ToString(pk ?? "");
            
            using (var ctx = new Model1())
            {
                var entity = ctx.WF_LOCATOR
                             .FirstOrDefault(q => q.LOCATOR_VALUE.ToUpper() == id.ToUpper());

                var result = MapFromDalToView(entity);

                return (T)Convert.ChangeType(result, typeof(T));
            }
        }

        public T Insert<T>(T objectView)
        {
            var value = MapFromViewToDal(objectView);

            using (var ctx = new Model1())
            {
                ctx.WF_LOCATOR.Attach(value);
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
                ctx.WF_LOCATOR.Attach(value);
                ctx.Entry(value).State = System.Data.Entity.EntityState.Modified;
                int i = ctx.SaveChanges();

                var result = MapFromDalToView(value);

                return (T)Convert.ChangeType(result, typeof(T));
            }
        }


        private ProcessPersistenceView MapFromDalToView(WF_LOCATOR entity)
        {
            if (entity == null) return null;

            var view = new ProcessPersistenceView
            {
                WFProcessGuid = entity.ID_WFPROCESSINSTANCE,
                LocatorFieldName = entity.LOCATOR_FIELD_NAME,
                LocatorValue = entity.LOCATOR_VALUE,
                Application = entity.APPLICATION,
                Scheme = entity.SCHEME,
            };

            return view;
        }


        private WF_LOCATOR MapFromViewToDal<T>(T objectView)
        {
            var view = (ProcessPersistenceView)Convert.ChangeType(objectView, typeof(T));

            var entity = new WF_LOCATOR
            {
                ID_WFPROCESSINSTANCE = view.WFProcessGuid,
                APPLICATION = view.Application,
                SCHEME = view.Scheme,
                LOCATOR_FIELD_NAME = view.LocatorFieldName,
                LOCATOR_VALUE = view.LocatorValue,
            };

            return entity;
        }

    }
}
