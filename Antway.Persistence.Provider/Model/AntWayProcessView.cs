using System;
using System.Collections.Generic;


namespace AntWay.Persistence.Model
{
    public class AntWayProcessView
    {
        public AntWayProcessView()
        {
            AvailableCommands = new List<string>();
        }

        public List<string> AvailableCommands { get; set; }
        public string CurrentActivityName { get; set; }
        public string CurrentActivityState { get; set; }
    }

}
