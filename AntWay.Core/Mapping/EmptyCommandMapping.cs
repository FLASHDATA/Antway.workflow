using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Runtime
{
    public class EmptyCommandMapping : ICommandsMapping
    {
        public List<CommandDefinition> GetMappedCommands()
        {
            return null;
        }
    }
}
