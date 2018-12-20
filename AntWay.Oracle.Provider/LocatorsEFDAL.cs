using System;
using System.Collections.Generic;
using System.Linq;
using AntWay.Oracle.Provider.Data;
using AntWay.Persistence.Provider.Model;
using Devart.Data.Oracle;


namespace AntWay.Oracle.Provider
{
    public class LocatorsEFDAL : IDALLocators
    {

        public List<LocatorView> GetLocatorsFromScheme(string scheme)
        {
            var result = new List<LocatorView>();

            using (var ctx = new Model1())
            {
                result = ctx.LOCATORS
                            .Where(l => l.SCHEME_CODE == scheme)
                            .Select(l => new LocatorView
                            {
                                LocatorValue = l.LOCATOR_VALUE,
                                WFProcessGuid = l.ID_WFPROCESSINSTANCE                                
                            })
                            .ToList();
            }

            return result;
        }

        public LocatorView GetLocatorFromGuid(Guid guid)
        {
            using (var ctx = new Model1())
            {
                var entity = ctx.LOCATORS
                             .FirstOrDefault(q => q.ID_WFPROCESSINSTANCE == guid);

                var result = MapFromDalToView(entity);
                return result;
            }
        }

        public LocatorView Fetch(string schemeCode, string locatorValue)
        {
            using (var ctx = new Model1())
            {
                var entity = ctx.LOCATORS
                             .FirstOrDefault(q => q.LOCATOR_VALUE.ToUpper() == locatorValue &&
                                             q.SCHEME_CODE.ToUpper() ==  schemeCode);

                var result = MapFromDalToView(entity);

                return result;
            }
        }

        public T Fetch<T>(object pk)
        {
            throw new NotImplementedException();
        }

        public T Insert<T>(T objectView)
        {
            var value = MapFromViewToDal(objectView);

            using (var ctx = new Model1())
            {
                ctx.LOCATORS.Attach(value);
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
                ctx.LOCATORS.Attach(value);
                ctx.Entry(value).State = System.Data.Entity.EntityState.Modified;
                int i = ctx.SaveChanges();

                var result = MapFromDalToView(value);

                return (T)Convert.ChangeType(result, typeof(T));
            }
        }


        private LocatorView MapFromDalToView(LOCATORS entity)
        {
            if (entity == null) return null;

            var view = new LocatorView
            {
                WFProcessGuid = entity.ID_WFPROCESSINSTANCE,
                LocatorFieldName = entity.LOCATOR_FIELD_NAME,
                LocatorValue = entity.LOCATOR_VALUE,
                SchemeCode = entity.SCHEME_CODE,
            };

            return view;
        }


        private LOCATORS MapFromViewToDal<T>(T objectView)
        {
            var view = (LocatorView)Convert.ChangeType(objectView, typeof(T));

            var entity = new LOCATORS
            {
                ID_WFPROCESSINSTANCE = view.WFProcessGuid,
                SCHEME_CODE = view.SchemeCode,
                LOCATOR_FIELD_NAME = view.LocatorFieldName,
                LOCATOR_VALUE = view.LocatorValue,
            };

            return entity;
        }

    }
}
