using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntWay.Data;
using AntWay.Views;

namespace AntWay.EFDAL
{
    public class WFLocatorEFDAL : IDALLocator
    {
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


        private WorkflowLocatorView MapFromDalToView(WF_LOCATOR entity)
        {
            if (entity == null) return null;

            var view = new WorkflowLocatorView
            {
                WFProcessGuid = entity.ID_WFPROCESSINSTANCE,
                LocatorValue = entity.LOCATOR_VALUE,
            };

            return view;
        }


        private WF_LOCATOR MapFromViewToDal<T>(T objectView)
        {
            var view = (WorkflowLocatorView)Convert.ChangeType(objectView, typeof(T));

            var entity = new WF_LOCATOR
            {
                ID_WFPROCESSINSTANCE = view.WFProcessGuid,
                LOCATOR_VALUE = view.LocatorValue,
            };

            return entity;
        }
    }
}
