using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntWay.Persistence.Model
{
    public class WorkflowLocatorView
    {
        public Guid WFProcessGuid { get; set; }

        public string LocatorFieldName { get; set; }
        public string LocatorValue { get; set; }

        public string AlternLocatorFieldName1 { get; set; }
        public string AlternLocatorFieldValue { get; set; }
    }
}
