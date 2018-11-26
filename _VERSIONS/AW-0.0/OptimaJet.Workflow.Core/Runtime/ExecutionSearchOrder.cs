namespace OptimaJet.Workflow.Core.Runtime
{
    public enum ExecutionSearchOrder
    {
        LocalGlobalProvider,
        GlobalLocalProvider,
        LocalProviderGlobal,
        GlobalProviderLocal,
        ProviderLocalGlobal,
        ProviderGlobalLocal
    }

    internal static class ExecutionSearchOrderExtensions
    {
        public static bool IsProviderFirst (this ExecutionSearchOrder order)
        {
            return order == ExecutionSearchOrder.ProviderGlobalLocal || order == ExecutionSearchOrder.ProviderLocalGlobal;
        }
        
        public static bool IsProviderLast (this ExecutionSearchOrder order)
        {
            return order == ExecutionSearchOrder.LocalGlobalProvider || order == ExecutionSearchOrder.GlobalLocalProvider;
        }

        public static bool IsLocalPrevaleGlobal(this ExecutionSearchOrder order)
        {
            return order == ExecutionSearchOrder.LocalGlobalProvider || order == ExecutionSearchOrder.LocalProviderGlobal ||
                   order == ExecutionSearchOrder.ProviderLocalGlobal;
        }
    }
}