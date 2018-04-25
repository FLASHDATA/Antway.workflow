using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Interfaces;

namespace WorkFlow.EFDAL
{
    public class SociosEFDAL //: IDAL
    {
        //public bool Delete<T>(T objectView)
        //{
        //    throw new NotImplementedException();
        //}

        //public T Fetch<T>(object pk)
        //{
        //    string idDocumento = Convert.ToString(pk);

        //    using (var ctx = new Model1())
        //    {
        //        var entity = ctx.SOCIS
        //                     .FirstOrDefault(q => q.ID_DOCUMENT_PERSONES == idDocumento);

        //        var result = MapFromDalToView(entity);

        //        return (T)Convert.ChangeType(result, typeof(T));
        //    }
        //}

        //public T Insert<T>(T objectView)
        //{
        //    var value = MapFromViewToDal(objectView);

        //    using (var ctx = new Model1())
        //    {
        //        ctx.SOCIS.Attach(value);
        //        ctx.Entry(value).State = System.Data.Entity.EntityState.Added;
        //        int i = ctx.SaveChanges();

        //        var result = MapFromDalToView(value);

        //        return (T)Convert.ChangeType(result, typeof(T));
        //    }
        //}

        //public T Update<T>(T objectView)
        //{
        //    var value = MapFromViewToDal(objectView);

        //    using (var ctx = new Model1())
        //    {
        //        ctx.SOCIS.Attach(value);
        //        ctx.Entry(value).State = System.Data.Entity.EntityState.Modified;
        //        int i = ctx.SaveChanges();

        //        var result = MapFromDalToView(value);

        //        return (T)Convert.ChangeType(result, typeof(T));
        //    }
        //}



        //private SocioView MapFromDalToView(SOCI entity)
        //{
        //    if (entity == null) return null;

        //    var view = new SocioView
        //    {
        //        IdDocumento = entity.ID_DOCUMENT_PERSONES,
        //        ActaNotarial = entity.ACTA_NOTARIAL,
        //        Administradora = (entity.ADMINISTRADORA >0),
        //        Cargo = entity.CARREG,
        //        Participacion = entity.PARTICIPACIO,
        //        TipoControl = entity.TIPO_CONTROL,
        //        //FechaAlta = entity.
        //    };


        //    return view;
        //}


        //private SOCI MapFromViewToDal<T>(T objectView)
        //{
        //    var view = (SocioView) Convert.ChangeType(objectView, typeof(T));

        //    var entity = new SOCI
        //    {
        //        ID_DOCUMENT_PERSONES = view.IdDocumento,
        //        ACTA_NOTARIAL = view.ActaNotarial,
        //        ADMINISTRADORA = view.Administradora ? 1 : 0,
        //        PARTICIPACIO = view.Participacion,
        //        CARREG = view.Cargo,
        //        TIPO_CONTROL = view.TipoControl,
        //        ID_DOCUMENT_PERSONA_JURIDICA = view.IdDocumentoPersonaJuridica,
        //        //DATA_ALTA = view.FechaAlta,
        //    };

        //    return entity;
        //}
    }
}
