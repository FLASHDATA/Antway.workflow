using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Fault
{
    public class WrongSubprocessMarkupException : Exception
    {
        public WrongSubprocessMarkupException(ActivityDefinition activity, string errormessage,
            params object[] errormessageParameters)
            : base(string.Format("Wrong subprocesses markup. Error in activity {0}. {1}", activity.Name,
                string.Format(errormessage, errormessageParameters)))
        {

        }

        public WrongSubprocessMarkupException(TransitionDefinition transition, string errormessage,
           params object[] errormessageParameters)
            : base(string.Format("Wrong subprocesses markup. Error in transition {0}. {1}", transition.Name,
                string.Format(errormessage, errormessageParameters)))
        {

        }
    }
}
