using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkFlow.Views
{
    public class ActivityConditions : Activity
    {
        public List<Parameter> ParamtersIN { get; set; }
        public List<Condition> Conditions { get; set; }

        public Activity NextActivityConditionsTrue { get; set; }
        public Activity NextActivityConditionsFalse { get; set; }
    }

}
