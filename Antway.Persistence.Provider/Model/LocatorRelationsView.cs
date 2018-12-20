using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntWay.Persistence.Provider.Model
{
    public class LocatorRelationsView
    {
        public Guid WFProcessGuid { get; set; }
        public string Entity { get; set; }
        public string EntityValue { get; set; }
    }
}
