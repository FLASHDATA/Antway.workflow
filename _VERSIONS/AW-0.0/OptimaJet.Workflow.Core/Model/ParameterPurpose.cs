using OptimaJet.Workflow.Core.Persistence;

namespace OptimaJet.Workflow.Core.Model
{

    /// <summary>
    /// Specifies the method of storing parameters
    /// </summary>
    public enum ParameterPurpose
    {
        /// <summary>
        /// Is not stored and only exists during transition execution
        /// </summary>
        Temporary,

        /// <summary>
        /// Is stored in persistence store <see cref="IPersistenceProvider"/>
        /// </summary>
        Persistence,

        /// <summary>
        /// Is system, storing can be different
        /// </summary>
        System
    }
}
