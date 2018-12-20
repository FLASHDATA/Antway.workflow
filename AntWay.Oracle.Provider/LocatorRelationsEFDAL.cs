using System;
using System.Collections.Generic;
using System.Linq;
using AntWay.Oracle.Provider.Data;
using AntWay.Persistence.Provider;
using AntWay.Persistence.Provider.Model;


namespace AntWay.Oracle.Provider
{
    public class LocatorRelationsEFDAL : IDAL
    {
        public T Fetch<T>(object pk)
        {
            throw new NotImplementedException();
        }

        public T Insert<T>(T objectView)
        {
            var value = MapFromViewToDal(objectView);

            using (var ctx = new Model1())
            {
                ctx.LOCATORS_RELATIONS.Attach(value);
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
                ctx.LOCATORS_RELATIONS.Attach(value);
                ctx.Entry(value).State = System.Data.Entity.EntityState.Modified;
                int i = ctx.SaveChanges();

                var result = MapFromDalToView(value);

                return (T)Convert.ChangeType(result, typeof(T));
            }
        }


        private LocatorRelationsView MapFromDalToView(LOCATORS_RELATIONS entity)
        {
            if (entity == null) return null;

            var view = new LocatorRelationsView
            {
                WFProcessGuid = entity.ID_WFPROCESSINSTANCE.Value,
                Entity = entity.ENTITY,
                EntityValue = entity.ENTITY_VALUE,
            };

            return view;
        }


        private LOCATORS_RELATIONS MapFromViewToDal<T>(T objectView)
        {
            var view = (LocatorRelationsView)Convert.ChangeType(objectView, typeof(T));

            var entity = new LOCATORS_RELATIONS
            {
                ID_WFPROCESSINSTANCE = view.WFProcessGuid,
                ENTITY = view.Entity,
                ENTITY_VALUE = view.EntityValue,
            };

            return entity;
        }

    }
}
