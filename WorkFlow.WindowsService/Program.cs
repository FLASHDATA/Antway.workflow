using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Runtime;
using WorkFlowEngine;

namespace WorkFlow.Director.WindowsService
{
    class Program
    {
        #region Nested classes to support running as service
        public const string ServiceName = "MyService";

        public class Service : ServiceBase
        {
            public Service()
            {
                ServiceName = Program.ServiceName;
            }

            protected override void OnStart(string[] args)
            {
                Program.Start(args);
            }

            protected override void OnStop()
            {
                Program.Stop();
            }
        }
        #endregion

        static void Main(string[] args)
        {
            if (!Environment.UserInteractive)
                // running as service
                using (var service = new Service())
                    ServiceBase.Run(service);
            else
            {
                // running as console app
                Start(args);

                Console.WriteLine("Press any key to stop...");
                Console.ReadKey(true);

                Stop();
            }
        }

        //private static string GetIdCurrentActivity(WorkFlowState wfState)
        //{
        //    //TODO: Call DAL
        //    //Get from DB the current activity
        //    return "START";
        //}

        private static void Start(string[] args)
        {
            var wfRT = WorkFlowEngine.WorkflowServer.Runtime;
            wfRT.Start();

            Console.WriteLine("Pulse enter para cerrar el servicio.");
            Console.ReadLine();
        }

        private static void Stop()
        {
        }

    }
}
