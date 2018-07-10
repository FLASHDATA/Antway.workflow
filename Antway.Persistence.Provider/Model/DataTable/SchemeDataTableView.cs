using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AntWay.Persistence.Provider.Model.DataTable
{
    public class SchemeDataTableView
    {
        public string SchemeName { get; set; }
        public string SchemeDBName { get; set; }
        public string Descripcion { get; set; }

        public bool Servicio { get; set; }

        public int NumFila { get; set; }
    }
}