using System;
using System.Collections.Concurrent;
using System.Linq;
using OptimaJet.Workflow.Core.License;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Cache
{
    /// <summary>
    /// Default cache <see cref="IParsedProcessCache"/> for parced processes <see cref="ProcessDefinition"/>
    /// </summary>
    public sealed class DefaultParcedProcessCache : IParsedProcessCache
    {
        private readonly ConcurrentDictionary<Guid, ProcessDefinition> _cache = new ConcurrentDictionary<Guid, ProcessDefinition>();

        /// <summary>
        /// Clear the cache
        /// </summary>
        public void Clear()
        {
            _cache.Clear();
        }

        /// <summary>
        /// Returns process definition from the cache by scheme id, if process definition is not exists in the cache returns null
        /// </summary>
        /// <param name="schemeId">Id of the scheme</param>
        /// <returns>ProcessDefinition object</returns>
        public ProcessDefinition GetProcessDefinitionBySchemeId(Guid schemeId)
        {
            ProcessDefinition result;
            _cache.TryGetValue(schemeId, out result);
            return result;
        }

        /// <summary>
        /// Adds process definition to the cache with scheme id as the key
        /// </summary>
        /// <param name="schemeId">Id of the scheme</param>
        /// <param name="processDefinition">ProcessDefinition object</param>
        public void AddProcessDefinition(Guid schemeId, ProcessDefinition processDefinition)
        {
            _cache.AddOrUpdate(schemeId, processDefinition, (guid, definition) => processDefinition);

            var maxNumberOfSchemes = Licensing.GetLicenseRestrictions<WorkflowEngineNetRestrictions>().MaxNumberOfSchemes;
            if (maxNumberOfSchemes > 0)
            {
                var uniqueSchemes = _cache.Values.GroupBy(pi => pi.Name).Count();
                if (uniqueSchemes > maxNumberOfSchemes)
                    throw new LicenseException(
                        string.Format(
                            "Maximum number of schemes for your license {0}.",
                            maxNumberOfSchemes));
            }
        }
    }
}
