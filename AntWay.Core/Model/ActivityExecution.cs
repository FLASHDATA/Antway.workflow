using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AntWay.Core.Model
{
    public class ActivityExecution
    {
        public Guid ProcessId { get; set; }
        public string ActivityId { get; set; }
        public string ActivityName { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public object ParametersIn { get; set; }
        public object ParametersOut { get; set; }

        public bool ExecutionSuccess { get; set; }
        public string ExecutionMessage { get; set; }

        public ActivityExecution()
        {
            StartTime = DateTime.Now;
        }
    }
}
