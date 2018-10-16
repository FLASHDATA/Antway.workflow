using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AntWay.Core.Mapping;
using AntWay.Core.Model;
using OptimaJet.Workflow.Core.Runtime;

namespace AntWay.Dashboard.Web.Factories
{
    public static class CommandFactory
    {
        public static ICommandsMapping GetCommandMapping(string schemeCode)
        {
            ICommandsMapping commandsMapping = null;

            switch (schemeCode)
            {
                case "EXPEDIENTES":
                    commandsMapping = new CommandsMapping(Sample.Model.Expedientes.SchemeCommandNames.Single);
                    break;

                default:
                    commandsMapping = new CommandsMapping(DefaultSchemeCommandNames.Single);
                    break;
            }

            return commandsMapping;
        }
    }
}