using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntWay.Persistence.Provider.Model.DataTable
{
    public class ProcessHistoryDataTableView
    {
        public string WorkFlow { get; set; }
        public string Localizador { get; set; }
        public string EstadoActual { get; set; }
        public string Tags { get; set; }
        public string UltimaActualizacion { get; set; }
    }

}
