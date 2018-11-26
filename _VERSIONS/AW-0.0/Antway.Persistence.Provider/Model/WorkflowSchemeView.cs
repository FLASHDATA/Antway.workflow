using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntWay.Persistence.Provider.Model
{
    public class WorkflowSchemeView
    {
        public string SchemeCode { get; set; }
        public string DBSchemeName { get; set; }
        public string SchemeName { get; set; }
        public string Description { get; set; }
        public bool WorkflowService { get; set; }
        public bool Active { get; set; }
    }
}
