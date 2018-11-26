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
                case "EVALUAR_RIESGO":
                    assemblies = new IMHab.PreventBlanqueo.Riesgo.AntWay.AntWayBinding.RiesgoAssemblies();
                    break;

                case "SEPBLAC":
                    assemblies = new IMHab.PreventBlanqueo.SEPBLAC.AntWay.AntWayBinding.SEPBLACAssemblies();
                    break;
            }

            return assemblies;
        }
    }
}