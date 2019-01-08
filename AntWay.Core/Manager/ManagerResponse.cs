using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntWay.Core.Manager
{
    public class ManagerResponse
    {
        public ManagerResponse()
        {
            AvailableCommands = new List<string>();
        }

        public Guid ProcessId { get; set; }
        public string ActivityId { get; set; }
        public string ActivityName { get; set; }
        public string StateName { get; set; }
        public List<string> AvailableCommands { get; set; }

        public bool Success { get; set; }
        public bool TimeExpired { get; set; }

        public List<string> ValidationMessages { get; set; }
    }
}
