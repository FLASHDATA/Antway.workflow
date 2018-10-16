using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using OptimaJet.Workflow.Core.Runtime;
using AntWay.Core.WorkflowEngine;
using AntWay.Persistence.Provider.Model;
using Antway.Core;
using Antway.Core.Persistence;

namespace AntWay.Director.Service
{
    class Program
    {
        private static List<WorkflowServer> WorkflowRuntimesList;
        //private static System.Timers.Timer TimerInitWorkflows;

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
                Console.WriteLine("Antway.Director.Service" + Environment.NewLine);

                // running as console app
                Start(args);

                Console.WriteLine("Pulse enter para cerrar el servicio.");
                Console.ReadLine();

                Stop();
            }
        }

        private static void Start(string[] args)
        {
            //InitRuntimes();
            Task.Run(async () => await InitRuntimes());
        }



        private static Task InitRuntimes()
        {
            //TimerInitWorkflowsInit();
            while (true)
            {
                //Init All
                //WorkflowServer.WithActionProvider(new NotificacionesActionProvider());
                var schemesPersistence = new SchemesPersistence
                {
                    IDALSchemes = PersistenceObjectsFactory.GetIDALWFSchemaObject(),
                };
                var schemes = schemesPersistence.GetSchemes();

                WorkflowRuntimesList = schemes
                                       .Select(s => s.DBSchemeName)
                                       .Distinct()
                                       .Select(s => new WorkflowServer(s))
                                       .ToList();

                foreach (var wfs in WorkflowRuntimesList)
                {
                    wfs.Start();
                }
                ////

                //Wait before restart runtime.
                //Sleeps for minutes in App.Config, RuntimeRefreshTimerInMinutes
                var elapsedTime = Convert.ToInt16(ConfigurationManager.AppSettings["RuntimeRefreshTimerInMinutes"])
                                    * 60 * 1000;
                System.Threading.Thread.Sleep(elapsedTime);
            }

            //return Task.FromResult<object>(null);
        }

        #region "Timers"

        //private static void TimerInitWorkflowsInit()
        //{
        //    var elapsedTime = 600;
        //    TimerInitWorkflows = new System.Timers.Timer(elapsedTime * 1000)
        //    {
        //        AutoReset = true
        //    };  // in milliseconds
        //    TimerInitWorkflows.Elapsed += new System.Timers.ElapsedEventHandler(TimerInitWorkflows_Elapsed);
        //    TimerInitWorkflows.Start();

        //    TimerInitWorkflows_Elapsed(null, null);
        //}


        //protected static void TimerInitWorkflows_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    Workflow.WithActionProvider(new NotificacionesActionProvider());
        //    var schemesPersistence = new SchemesPersistence
        //    {
        //        IDALLocator = PersistenceObjectsFactory.GetIDALWFLocatorObject(),
        //        IDALSchema = PersistenceObjectsFactory.GetIDALWFSchemaObject(),
        //    };
        //    var schemes = schemesPersistence.GetSchemes();

        //    WorkflowRuntimesList = schemes
        //                          .Select(s => new Workflow(s.DBSchemeName))
        //                          .ToList();

        //    foreach (var wfs in WorkflowRuntimesList)
        //    {
        //        wfs.Start();
        //    }
        //}

        #endregion

        private static void Stop()
        {
        }

    }
}



     