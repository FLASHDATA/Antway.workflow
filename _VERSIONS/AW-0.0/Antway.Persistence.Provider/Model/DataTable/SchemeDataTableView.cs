using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AntWay.Persistence.Provider.Model.DataTable
{
    public class SchemeDataTableView
    {
        public string SchemeCode { get; set; }
        public string SchemeName { get; set; }
        public string SchemeDBName { get; set; }

        public bool WorkFlowService { get; set; }
        public bool Active { get; set; }

        public int NumFila { get; set; }
    }
}