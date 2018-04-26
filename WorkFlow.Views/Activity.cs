using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkFlow.Views
{
    public enum ActivityType
    {
        State,
        Input,
        Condition,
        Timer,
        Process
    }

    public abstract class Activity
    {
        public string IdActivity { get; set; }
        public string Description { get; set; }
        public ActivityType ActivityType { get; set; }
        //public Activity NextActivity { get; set; }

        public string IdActivityNext { get; set; }
    }

    public class Condition
    {
        public string Expression { get; set; }
        public bool Result { get; set; }
    }

    public class Parameter
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public object Value { get; set; }
    }

}
