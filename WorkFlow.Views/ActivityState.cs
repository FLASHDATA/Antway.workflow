using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkFlow.Views
{
    public class ActivityState : Activity
    {
        public string StateCode { get; set; }
        public string StateDescription { get; set; }
    }
}
