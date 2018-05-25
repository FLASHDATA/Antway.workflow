﻿using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using AntWay.BLL;
using AntWay.EFDAL;
using WorkFlowEngine;

namespace AntWay.Director.Service
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

        private static void Start(string[] args)
        {
            var awBLL = new AntWayBLL { IDALSchema = new WFSchemaEFDAL(), IDALLocator = new WFLocatorEFDAL() };

            var schemes = awBLL.GetSchemes();

            var listWfs = schemes
                          .Select(s => new Workflow(s.DBSchemeName))
                          .ToList();

            foreach(var wfs in listWfs)
            {
                wfs.Start();
            }

            Console.WriteLine("Pulse enter para cerrar el servicio.");
            Console.ReadLine();
        }

        private static void Stop()
        {
        }

    }
}
