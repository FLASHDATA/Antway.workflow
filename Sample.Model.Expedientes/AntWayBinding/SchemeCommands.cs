using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntWay.Core.Model;

namespace Sample.Model.Expedientes.AntWayBinding
{
    public class SchemeCommandNames
    {
        public static SchemeCommandNames Single
                                       => new SchemeCommandNames();

        public string Siguiente => DefaultSchemeCommandNames.Single.Siguiente;
        public string Anterior => DefaultSchemeCommandNames.Single.Anterior;
        public string Firmar => "Firmar";
    }
}
