using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntWay.Oracle.Provider.Data;
using AntWay.Persistence.Provider;
using AntWay.Persistence.Model;
using Devart.Data.Oracle;

namespace AntWay.Oracle.Provider
{
    public class WFLocatorEFDAL : IDALProcessPersistence
    {

        public List<ProcessHistoryDataTableView> GeProccessHistoryDataTableView(ProcessHistoryFilter filter)
        {
            var result = new List<ProcessHistoryDataTableView>();
            using (var ctx = new Model1())
            {
                string sql = "SELECT LOC.APPLICATION AS Aplicacion, LOC.ID_WFPROCESSINSTANCE as ID, LOC.LOCATOR_VALUE AS Localizador " +
                             " , PH.STATENAME as EstadoActual, PH.TAGS AS Tags " +
                             " , PH.LASTTRANSITION AS UltimaActualizacion " +
                             " FROM WF_LOCATOR LOC " +
                             " INNER JOIN( " +
                             "      SELECT pi.ID, pi.STATENAME, pth.Tags, pth.LastTransition " +
                             $"     FROM {filter.DatabaseSchema}.WORKFLOWPROCESSINSTANCE pi " +
                             "      LEFT JOIN ( " +
                             "        select PROCESSID " +
                             "          , listagg(TOSTATENAME,', ') within group(order by TRANSITIONTIME) Tags " +
                             "          , MAX(TransitionTime) AS LastTransition " +
                             "        from WFSCHEMA1.WORKFLOWPROCESSTRANSITIONH " +
                             "        WHERE TRANSITIONCLASSIFIER = 'SaveState' " +
                             "        GROUP BY PROCESSID " +
                             "       )  pth " +
                             "      ON pi.ID = pth.PROCESSID " +
                             " ) PH " +
                             " ON LOC.ID_WFPROCESSINSTANCE = PH.Id " +
                             " WHERE APPLICATION = :Aplicacion ";


                var parameters = new object[]
                                    {
                                        new OracleParameter("Aplicacion", filter.Application),
                                    };

                result = ctx.Database.SqlQuery<ProcessHistoryDataTableView>(sql, parameters).ToList();
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
