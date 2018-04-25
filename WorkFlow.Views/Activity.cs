using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkFlow.Views
{
    public class Activity
    {
        public string IdActivity { get; set; }
        //public DateTime? Caducidad { get; set; }

        public List<Parameter> ParametrosIN { get; set; }
        public List<Condition> Condiciones { get; set; }

        public Process ProcesoIN { get; set; }
        public Process ProcesoOUT { get; set; }

        public Activity NextActivity { get; set; }
    }

    public class Parameter
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public object Value { get; set; }
    }

    public class Condition
    {
        public string Expression { get; set; }
        public bool Result { get; set; }
    }

    public class Process
    {
        public string IdProcess { get; set; }

    }
}
