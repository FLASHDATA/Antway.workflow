using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntWay.Oracle.Provider.Data;
using AntWay.Persistence.Provider;
using AntWay.Persistence.Model;

namespace AntWay.Oracle.Provider
{
    public class WFSchemaEFDAL : IDALWFSchema
    {
        public List<WorkflowSchemaView> GetWorkflowSchemes()
        {
            using (var ctx = new Model1())
            {
                var result = ctx.WF_SCHEMES
                             .ToList()
                             .Select(s => new WorkflowSchemaView { DBSchemeName = s.DB_SCHEME_NAME })
                             .ToList();

                return result;
            }
        }



        public T Fetch<T>(object pk)
        {
            string id = Convert.ToString(pk ?? "");

            using (var ctx = new Model1())
            {
                var entity = ctx.WF_SCHEMES
                             .FirstOrDefault(q => q.DB_SCHEME_NAME.ToUpper() == id.ToUpper());

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


        private WorkflowSchemaView MapFromDalToView(WF_SCHEMES entity)
        {
            if (entity == null) return null;

            var view = new WorkflowSchemaView
            {
                DBSchemeName = entity.DB_SCHEME_NAME,
            };

            return view;
        }


        private WF_SCHEMES MapFromViewToDal<T>(T objectView)
        {
            var view = (WorkflowSchemaView)Convert.ChangeType(objectView, typeof(T));

            var entity = new WF_SCHEMES
            {
                DB_SCHEME_NAME = view.DBSchemeName,
            };

            return entity;
        }
    }
}
