using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WorkFlow.Views;


namespace WorkFlow.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var wf = new Views.WorkFlow
            {
                Activities = new List<object>
                {
                    new Views.ActivityState
                    {
                        IdActivity ="1",
                        StateDescription = "Start",
                        IdActivityNext = "2"
                    },
                    new Views.ActivityState
                    {
                        IdActivity ="2",
                        StateDescription = "Recogida información",
                        IdActivityNext ="2.1"
                    },
                    new Views.ActivityConditions
                    {
                        IdActivity ="2.1",
                        //NextActivityConditionsFalse 
                    },
                    new Views.ActivityState
                    {
                        StateCode = "Start"
                    },
                }
            };

            System.Console.ReadLine();
        }
    }
}
