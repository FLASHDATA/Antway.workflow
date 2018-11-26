using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntWay.Persistence.Provider.Model.DataTable
{
    public class ProcessHistoryDetailDataTableView
    {
        public string FromActivityName { private get; set; }
        public string ToActivityName { private get; set; }
        public string FromStateName { private get; set; }
        public string ToStateName { private get; set; }
        public string TriggerName { get; set; }
        public string TransitionTime { get; set; }

        public string Activity => $"{FromActivityName}/{ToActivityName}";
        public string State => $"{FromStateName}/{ToStateName}";
    }

    public class ProcessHistoryDetailFilter
    {
        public Guid ProcessId { get; set; }
        public int PaginatioFromRecord { get; set; }
        public int PaginatioToRecord { get; set; }
    }

}

