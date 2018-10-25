using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AntWay.Core.Model
{
    public class ActivityExecution
    {
        public Guid ProcessId { get; set; }
        public string ActivityId { get; set; }
        public string ActivityName { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public object ParametersInput { get; set; }
        public object ParametersOutput { get; set; }

        public string InputChecksum { get; set; }
        public string OutputChecksum { get; set; }

        public bool ExecutionSuccess { get; set; }
        public string ExecutionMessage { get; set; }

        public ActivityExecution()
        {
            StartTime = DateTime.Now;
        }
    }
}
