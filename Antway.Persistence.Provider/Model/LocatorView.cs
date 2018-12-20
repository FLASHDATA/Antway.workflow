using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntWay.Persistence.Provider.Model
{
    public class LocatorView
    {
        public Guid WFProcessGuid { get; set; }
        public string SchemeCode { get; set; }
        public string LocatorFieldName { get; set; }
        public string LocatorValue { get; set; }
    }
}
