using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AntWay.Core.Runtime;

namespace Model.Factory
{
    public static class AssemblyFactory
    {
        public static IAssemblies GetAssemblyObject(string schemeCode)
        {
            IAssemblies assemblies = null;

            switch (schemeCode)
            {
                case "EVALUAR_RIESGO":
                    assemblies = new SEPBLAC.Scheme.Riesgo.AntWay.AntWayBinding.RiesgoAssemblies();
                    break;

                case "SEPBLAC":
                    assemblies = new SEPBLAC.Scheme.SEPBLAC.AntWay.AntWayBinding.SEPBLACAssemblies();
                    break;
            }

            return assemblies;
        }
    }
}