using System.Collections.Generic;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Runtime
{
    public interface ICommandsMapping
    {
        List<CommandDefinition> GetMappedCommands();
    }
}