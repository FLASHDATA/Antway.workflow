using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntWay.Persistence.Model
{
    public class ProcessHistoryDataTableView
    {
        public string ID { get; set; }
        public string Application { get; set; }
        public string Localizador { get; set; }
        public string EstadoActual { get; set; }
        public string Tags { get; set; }
        public string UltimaActualizacion { get; set; }
    }

    public class ProcessHistoryFilter
    {
        public string DatabaseSchema { get; set; }
        public string Application { get; set; }
    }
}
