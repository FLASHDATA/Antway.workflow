namespace OptimaJet.Workflow.Core.Model
{
    /// <summary>
    /// Classifier of  the direction of the transition
    /// </summary>
    public enum TransitionClassifier
    {
        AntWay,
        /// <summary>
        /// Direction not specified
        /// </summary>
        NotSpecified,
        /// <summary>
        /// Direct transition
        /// </summary>
        Direct,
        /// <summary>
        /// Reverse transition
        /// </summary>
        Reverse,
    }
}
