using System;
using System.Collections.Generic;
using System.Linq;
using AntWay.Oracle.Provider.Data;
using AntWay.Persistence.Provider;
using AntWay.Persistence.Provider.Model.DataTable;
using Devart.Data.Oracle;

namespace AntWay.Oracle.Provider
{
    public class ProcessEFDAL : IDALProcessPersistence
    {
        public List<WorkFlowDataTableView> GetWorkFlowsDataTableView(DataTableFilters filter)
        {
            var result = new List<WorkFlowDataTableView>();

            using (var ctx = new Model1())
            {
                List<string> schemesDB = GetDBSchemes();

                //SELECT DISTINCT DB_SCHEME_NAME
                //FROM WF_SCHEMES
                string sql = "SELECT WorkFlow" +
                             ", TotalProcesos" +
                             ", TotalProcesosEstadoEjecutando" +
                             ", TotalProcesosEstadoDormido" +
                             ", TotalProcesosEstadoFinalizado" +
                             ", TotalProcesosEstadoError" +
                             ", ROWNUM AS NumFila" +
                             " FROM (";

                int idx = 0;
                foreach (string schemeDBName in schemesDB)
                {
                    if (idx > 0)
                    {
                        sql += " UNION ";
                    }

                    sql += " SELECT wps.SCHEMECODE AS WorkFlow, " +
                            " SUM(1) TotalProcesos, " +
                            " SUM(CASE WHEN wfps.STATUS <= 1 then 1 ELSE 0 end) TotalProcesosEstadoEjecutando," +
                            " SUM(CASE WHEN wfps.STATUS = 2 then 1 ELSE 0 end) TotalProcesosEstadoDormido," +
                            " SUM(CASE WHEN wfps.STATUS = 3 then 1 ELSE 0 end) TotalProcesosEstadoFinalizado," +
                            " SUM(CASE WHEN wfps.STATUS = 4 OR wfps.STATUS = 5 then 1 ELSE 0 end) TotalProcesosEstadoError" +
                            $" FROM {schemeDBName}.WORKFLOWPROCESSINSTANCES wfps" +
                             " LEFT JOIN " +
                             " (" +
                             " SELECT PROCESSID, MAX(TRANSITIONTIME) AS LAST_TRANSITION " +
                            $" FROM {schemeDBName}.WORKFLOWPROCESSTRANSITIONH " +
                             " GROUP BY PROCESSID " +
                             " ) PH " +
                             " ON wfps.ID = PH.PROCESSID " +
                            $" INNER JOIN {schemeDBName}.WORKFLOWPROCESSINSTANCE wpi ON wfps.ID = wpi.ID" +
                            $" INNER JOIN {schemeDBName}.WORKFLOWPROCESSSCHEME wps ON wpi.SCHEMEID = wps.ID" +
                             " WHERE (LAST_TRANSITION IS NULL OR" +
                             " (LAST_TRANSITION>=:DateFrom AND LAST_TRANSITION<=:DateTo)" +
                             " )";

                    filter.FilteredStringFields
                        .ForEach(f => sql += $" AND {f.Field} like '%' || :{f.Field} || '%'");

                    sql += " GROUP BY wps.SCHEMECODE";

                    idx++;
                }

                sql += $" ORDER BY {filter.OrderBySQLQueryColIndex} {filter.OrderByDirection}";

                sql += ") qry";

                var parameters = new List<OracleParameter>
                {
                    new OracleParameter("DateFrom", filter.FilterFrom.Value),
                    new OracleParameter("DateTo", filter.FilterTo.Value),
                };

                filter.FilteredStringFields
                .ForEach(f => parameters.Add(new OracleParameter(f.Field, f.Value)));

                result = ctx.Database.SqlQuery<WorkFlowDataTableView>(sql, parameters.ToArray())
                         .ToList();
            }

            return result;
        }


        // Processes History
        protected string SQLProcessesHistory
        {
            get
            {
                List<string> schemesDB = GetDBSchemes();

                string sql = "SELECT ROWNUM AS NUM_FILA" +
                            " , QRY.WorkFlow" +
                            " ,QRY.LOCALIZADOR, QRY.ESTADOACTUAL, QRY.TAGS, QRY.ULTIMAACTUALIZACION" +
                            " FROM" +
                            " (" +
                            "  SELECT LOC.SCHEME_CODE AS WorkFlow" +
                            "  , LOC.LOCATOR_VALUE AS Localizador" +
                            "  , PH.STATENAME as EstadoActual, PH.TAGS AS Tags" +
                            "  , PH.LASTTRANSITION AS UltimaActualizacion" +
                            " FROM LOCATORS LOC" +
                            " INNER JOIN(";

                string sqlSubquery = "";
                foreach (string schemeDBName in schemesDB)
                {
                    if (sqlSubquery.Length > 1)
                    {
                        sqlSubquery += " UNION ";
                    }
                    sqlSubquery += 
                              " SELECT pi.ID, pi.STATENAME, pth.Tags, pth.LastTransition" +
                              $" FROM {schemeDBName}.WORKFLOWPROCESSINSTANCE pi" +
                              " LEFT JOIN(" +
                              "           select PROCESSID" +
                              "            , listagg(TOSTATENAME, ', ') within group(order by TRANSITIONTIME) Tags" +
                              "            , MAX(TransitionTime) AS LastTransition" +
                              $"           from {schemeDBName}.WORKFLOWPROCESSTRANSITIONH" +
                              "           GROUP BY PROCESSID" +
                              " )  pth" +
                              " ON pi.ID = pth.PROCESSID";
                }

                sql +=   sqlSubquery +
                       " ) PH" +
                       " ON LOC.ID_WFPROCESSINSTANCE = PH.Id" +
                       " ) QRY" +
                       " WHERE UltimaActualizacion>=:DateFrom" +
                       " AND UltimaActualizacion<=:DateTo";

                return sql;
            }
        }

        public int GetProccessesHistoryTotalRegistros(DataTableFilters filter)
        {
            int result = 0;
            using (var ctx = new Model1())
            {
                string sql = "SELECT 1" +
                              " FROM (" +
                                 SQLProcessesHistory +
                              ") sq" +
                              " WHERE 1 = 1 ";

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
                        GetProccessesHistoryDataTableView(DataTableFilters filter)
        {
            var result = new List<ProcessHistoryDataTableView>();
            using (var ctx = new Model1())
            {
                string sql = " SELECT NUM_FILA, sq.WorkFlow, sq.LOCALIZADOR " +
                             " , sq.ESTADOACTUAL, sq.TAGS, sq.ULTIMAACTUALIZACION " +
                             " FROM( " +
                                     SQLProcessesHistory +
                             " ) sq" +
                             " WHERE NUM_FILA >=:FromRecord AND NUM_FILA <=:ToRecord";
                             
                filter.FilteredStringFields
                    .ForEach(f => sql += $" AND {f.Field} like '%' || :{f.Field} || '%'");

                sql += $" ORDER BY {filter.OrderByColumnName} {filter.OrderByDirection}";


                var parameters = new List<OracleParameter>
                                    {
                                        new OracleParameter("DateFrom", filter.FilterFrom.Value),
                                        new OracleParameter("DateTo", filter.FilterTo.Value),
                                        new OracleParameter("FromRecord", filter.PaginationFromRecord),
                                        new OracleParameter("ToRecord", filter.PaginationToRecord),
                                    };

                filter.FilteredStringFields
                    .ForEach(f => parameters.Add(new OracleParameter(f.Field, f.Value)));

                result = ctx.Database.SqlQuery<ProcessHistoryDataTableView>
                                (sql, parameters.ToArray())
                                .ToList();
            }


            return result;
        }

        
        //Process History Detail
        public int GetProccessHistoryDetailTotalRegistros(ProcessHistoryDetailFilter filter)
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
                    GetProcessHistoryDetailTableView(ProcessHistoryDetailFilter filter)
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

        public List<string> GetDBSchemes()
        {
            List<string> schemesDB;
            using (var ctx = new Model1())
            {
                schemesDB = ctx.WF_SCHEMES
                          .Select(s => s.DB_SCHEME_NAME)
                          .Distinct()
                          .ToList();
            }

            return schemesDB;
        }

        public List<string> GetSchemes()
        {
            List<string> schemes;
            using (var ctx = new Model1())
            {
                schemes = ctx.WF_SCHEMES
                          .Select(s => s.SCHEME_NAME)
                          .ToList();
            }

            return schemes;
        }
    }
}
