using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;

namespace AntWay.Core.Mapping
{
    public class CommandsMapping : ICommandsMapping
    {
        private object ObjectCommandSchemeMapping { get; set; }

        public CommandsMapping(object objectCommandSchemeMapping)
        {
            ObjectCommandSchemeMapping = objectCommandSchemeMapping;
        }

        public List<CommandDefinition> GetMappedCommands()
        {
            var result = new List<CommandDefinition>();

            var properties = ObjectCommandSchemeMapping.GetType()
                            .GetProperties()
                            .Where(n => n.Name.ToLower() != "single");

            foreach (var p in properties)
            {
                result.Add(new CommandDefinition
                {
                    Name = p.GetValue(ObjectCommandSchemeMapping).ToString(),
                    InputParameters = new List<ParameterDefinitionReference>(),
                });
            }
            
            return result;
        }
    }
}
