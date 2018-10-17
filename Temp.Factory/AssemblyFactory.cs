using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AntWay.Core.Runtime;

namespace Temp.Factory
{
    public static class AssemblyFactory
    {
        public static IAssemblies GetAssemblyObject(string schemeCode)
        {
            IAssemblies assemblies = null;

            switch (schemeCode)
            {
                case "EXPEDIENTES":
                    assemblies = new Sample.Model.Expedientes.ExpedientesAssemblies();
                    break;
            }

            return assemblies;
        }
    }
}