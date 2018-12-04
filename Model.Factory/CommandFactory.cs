using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AntWay.Core.Mapping;
using AntWay.Core.Model;
using OptimaJet.Workflow.Core.Runtime;
using RiesgoAntWay = IMHab.PreventBlanqueo.Riesgo.AntWay;
using SEPBLACAntWay = IMHab.PreventBlanqueo.SEPBLAC.AntWay;

namespace Model.Factory
{
    /// <summary>
    /// TODO: Modificar a inyección de dependencias
    /// </summary>
    public static class CommandFactory
    {
        public static ICommandsMapping GetCommandMapping(string schemeCode)
        {
            ICommandsMapping commandsMapping = null;

            switch (schemeCode)
            {
                case "EVALUAR_RIESGO":
                    commandsMapping = new CommandsMapping(RiesgoAntWay.AntWayBinding.SchemeCommandNames.Single);
                    break;

                case "SEPBLAC":
                    commandsMapping = new CommandsMapping(SEPBLACAntWay.AntWayBinding.SchemeCommandNames.Single);
                    break;

                default:
                    commandsMapping = new CommandsMapping(DefaultSchemeCommandNames.Single);
                    break;
            }

            return commandsMapping;
        }
    }
}