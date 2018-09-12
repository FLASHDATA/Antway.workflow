using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntWay.Persistence.Provider.Model
{
    public class ProcessPersistenceView
    {
        public Guid WFProcessGuid { get; set; }
        
        public string SchemeCode { get; set; }

        public string LocatorFieldName { get; set; }
        public string LocatorValue { get; set; }

        public string AlternLocatorFieldName1 { get; set; }
        public string AlternLocatorFieldValue { get; set; }
    }
}
