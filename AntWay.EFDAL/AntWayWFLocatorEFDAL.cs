using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntWay.Data;
using AntWay.Views;

namespace AntWay.EFDAL
{
    public class AntWayWFLocatorEFDAL : IDALAntWay
    {
        public T Fetch<T>(object pk)
        {
            string id = Convert.ToString(pk ?? "");

            using (var ctx = new Model1())
            {
                var entity = ctx.AW_WF_LOCATOR
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
                ctx.AW_WF_LOCATOR.Attach(value);
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
                ctx.AW_WF_LOCATOR.Attach(value);
                ctx.Entry(value).State = System.Data.Entity.EntityState.Modified;
                int i = ctx.SaveChanges();

                var result = MapFromDalToView(value);

                return (T)Convert.ChangeType(result, typeof(T));
            }
        }


        private AntWayWorkflowLocatorView MapFromDalToView(AW_WF_LOCATOR entity)
        {
            if (entity == null) return null;

            var view = new AntWayWorkflowLocatorView
            {
                WFProcessGuid = entity.ID_WFPROCESSINSTANCE,
                LocatorValue = entity.LOCATOR_VALUE,
            };

            return view;
        }


        private AW_WF_LOCATOR MapFromViewToDal<T>(T objectView)
        {
            var view = (AntWayWorkflowLocatorView)Convert.ChangeType(objectView, typeof(T));

            var entity = new AW_WF_LOCATOR
            {
                ID_WFPROCESSINSTANCE = view.WFProcessGuid,
                LOCATOR_VALUE = view.LocatorValue,
            };

            return entity;
        }
    }
}
