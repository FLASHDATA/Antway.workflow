using System;
using System.Collections.Generic;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;

namespace AntWay.Core.Mapping
{
    public class EmptyCommandMapping : ICommandsMapping
    {
        public List<CommandDefinition> GetMappedCommands()
        {
            return null;
        }
    }
}
