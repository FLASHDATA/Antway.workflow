using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntWay.Views
{
    public class AntWayView
    {
        public AntWayView()
        {
            AvailableCommands = new List<string>();
        }

        public List<string> AvailableCommands { get; set; }
        public string CurrentActivityName { get; set; }
        public string CurrentActivityState { get; set; }
    }

}
