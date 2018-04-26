using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkFlow.Views
{
    public class ActivityProcess : Activity
    {
        public string ProcessId { get; set; }
        public string ProcessName { get; set; }

        public string ServiceURL { get; set; }
    }

}
