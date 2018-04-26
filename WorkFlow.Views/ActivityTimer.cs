using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkFlow.Views
{
    public class ActivityTimer : Activity
    {
        public DateTime DateTimeToCheck { get; set; }
        public long ExpirestAtInSeconds { get; set; }
        public int? RepeatLoopTimes { get; set; }
        public long RepeatLoopIntervalInSecods { get; set; }
    }
}
