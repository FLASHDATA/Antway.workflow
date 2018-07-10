using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AntWay.Persistence.Provider.Model.DataTable
{
    public class WorkFlowDataTableView
    {
        public string WorkFlow { get; set; }
        public int TotalProcesos { get; set; }
        public int TotalProcesosEstadoFinalizado { get; set; }
        public int TotalProcesosEstadoEnProceso  { get; set; }
        public int TotalProcesosEstadoError { get; set; }

        public int Order { get; set; }
        public int NumFila { get; set; }
    }
}