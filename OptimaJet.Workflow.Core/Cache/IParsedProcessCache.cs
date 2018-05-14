using System;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Cache
{
    /// <summary>
    /// Interface of a cache for parced processes <see cref="ProcessDefinition"/>
    /// </summary>
    public interface IParsedProcessCache
    {
        /// <summary>
        /// Clear the cache
        /// </summary>
        void Clear();

        /// <summary>
        /// Returns process definition from the cache by scheme id
        /// </summary>
        /// <param name="schemeId">Id of the scheme</param>
        /// <returns>ProcessDefinition object</returns>
        ProcessDefinition GetProcessDefinitionBySchemeId(Guid schemeId);

        /// <summary>
        /// Adds process definition to the cache with scheme id as the key
        /// </summary>
        /// <param name="schemeId">Id of the scheme</param>
        /// <param name="processDefinition">ProcessDefinition object</param>
        void AddProcessDefinition(Guid schemeId, ProcessDefinition processDefinition);
    }
}
