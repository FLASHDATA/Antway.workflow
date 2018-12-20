using AntWay.Core.Manager;
using AntWay.Core.Mapping;
using AntWay.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntWay.Core.Model
{
    public class StartWorkflow
    {
        public AntWayRuntime AntwayRuntime { get; set; }
        public string SchemeCode { get; set; }
        public string LocalizadorFieldName { get; set; }
        public string Localizador { get; set; }
        public IAssemblies Assemblies { get; set; }
        public IActivityManager ActivityManager { get; set; }
        public ActivitiesMapping ActivitiesMapping { get; set; }
        public bool ForceNewProcess { get; set; }
        public string Actor { get; set; }
    }
}
