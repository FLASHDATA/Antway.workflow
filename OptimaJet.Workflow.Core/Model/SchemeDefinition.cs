using System;
using System.Collections.Generic;
using OptimaJet.Workflow.Core.Parser;

namespace OptimaJet.Workflow.Core.Model
{
    /// <summary>
    /// Represent a not parsed process scheme
    /// </summary>
    /// <typeparam name="T">Type of not parsed scheme</typeparam>
    public class SchemeDefinition<T> where T : class 
    {
        /// <summary>
        /// Not parsed process scheme
        /// </summary>
        public T Scheme { get; private set; }
        /// <summary>
        /// Name of the the scheme
        /// </summary>
        public string SchemeCode { get; set; }
        /// <summary>
        /// Name of the root scheme if subprocess
        /// </summary>
        public string RootSchemeCode { get; set; }

        /// <summary>
        /// Id of the the scheme
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>
        /// Id of the the root scheme if subprocess
        /// </summary>
        public Guid? RootSchemeId { get; private set; }

        public List<string> AllowedActivities { get; set; }

        public string StartingTransition { get; set; }

        /// <summary>
        /// Sign that the scheme is obsolete
        /// </summary>
        public bool  IsObsolete { get; private set; }
        /// <summary>
        /// Sign that parameters for creating scheme was changed
        /// </summary>
        public bool IsDeterminingParametersChanged { get; set; }
        /// <summary>
        /// Parameters for creating the scheme of the process
        /// </summary>
        public string DefiningParameters { get; set; }

        /// <summary>
        /// Create SchemeDefinition object
        /// </summary>
        /// <param name="id">Id of the the scheme</param>
        /// <param name="schemeCode">Name of the the scheme</param>
        /// <param name="parentSchemeCode">Name of the parent scheme if subprocess</param>
        /// <param name="rootSchemeCode">Name of the root scheme if subprocess</param>
        /// <param name="scheme">Not parsed process scheme</param>
        /// <param name="isObsolete">Sign that the scheme is obsolete</param>
        /// <param name="isDeterminingParametersChanged">Sign that parameters for creating scheme was changed</param>
        /// <param name="definingParameters">Parameters for creating the scheme of the process</param>
        public SchemeDefinition(Guid id, Guid? rootSchemeId, string schemeCode, string rootSchemeCode, T scheme,
            bool isObsolete, bool isDeterminingParametersChanged, List<string> allowedActivities, string startingTransition,string definingParameters = null)
        {
            Id = id;
            RootSchemeId = rootSchemeId;
            Scheme = scheme;
            SchemeCode = schemeCode;
            RootSchemeCode = rootSchemeCode;
            AllowedActivities = allowedActivities;
            StartingTransition = startingTransition;
            IsObsolete = isObsolete;
            IsDeterminingParametersChanged = isDeterminingParametersChanged;
            DefiningParameters = definingParameters;
        }

        /// <summary>
        /// Create SchemeDefinition object
        /// </summary>
        /// <param name="processDefinition">ProcessDefinition object</param>
        /// <param name="parser">WorkflowParser to serialize the scheme</param>
        public SchemeDefinition(ProcessDefinition processDefinition, IWorkflowParser<T> parser)
        {
            Id = processDefinition.Id;
            RootSchemeId = processDefinition.RootSchemeId;
            Scheme = parser.SerializeToSchemeMedium(processDefinition);
            SchemeCode = processDefinition.Name;
            RootSchemeCode = processDefinition.RootSchemeCode;
            AllowedActivities = processDefinition.AllowedActivities;
            StartingTransition = processDefinition.StartingTransition;
            IsObsolete = processDefinition.IsObsolete;
            IsDeterminingParametersChanged = false;
            DefiningParameters = processDefinition.DefiningParametersString;
        }

    }
}
