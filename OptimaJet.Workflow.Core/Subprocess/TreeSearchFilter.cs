using System;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Subprocess
{
    /// <summary>
    /// Specifies starting point of search something in the Process Instance Tree
    /// </summary>
    public enum TreeSearchStart
    {
        /// <summary>
        /// From specified node
        /// </summary>
        FromMe,
        /// <summary>
        /// From tree root
        /// </summary>
        FromRoot,
        /// <summary>
        /// From parent of specified node
        /// </summary>
        FromParent
    }

    /// <summary>
    /// Specifies which kind of nodes will be included in search
    /// </summary>
    public enum TreeSearchInclude
    {
        /// <summary>
        /// All children nodes and start point
        /// </summary>
        AllDownIncludeStartPoint,
        /// <summary>
        /// All parent nodes and start point
        /// </summary>
        AllUpIncludeStartPoint,
        /// <summary>
        /// Only start point
        /// </summary>
        OnlyStartPoint,
        /// <summary>
        /// All children nodes
        /// </summary>
        AllDownExcludeStartPoint,
        /// <summary>
        /// All parent nodes
        /// </summary>
        AllUpExcludeWithoutStartPoint,
    }

    /// <summary>
    /// Specifies the current process and the method of searching in the Process Instances tree
    /// </summary>
    public class TreeSearchFilter
    {
        private Guid _processId;

        /// <summary>
        /// Process id, which will be specified to search
        /// </summary>
        public Guid ProcessId
        {
            get
            {
                if (ProcessInstance != null)
                {
                    return ProcessInstance.ProcessId;
                }
                return _processId;
            }
            set
            {
                if (ProcessInstance != null)
                {
                    throw new InvalidOperationException("ProcessInstance has been specified. You can't set ProcessId");
                }
                _processId = value;
            }
        }

        /// <summary>
        /// Specifies starting point of search something in the Process Instance Tree
        /// </summary>
        public TreeSearchStart StartFrom { get;set; }

        /// <summary>
        /// ProcessInstance, wich will be specified to search
        /// </summary>
        public ProcessInstance ProcessInstance { get; set; }

        /// <summary>
        /// Specifies which kind of nodes will be included in search
        /// </summary>
        public TreeSearchInclude Include { get; set; }

        /// <summary>
        /// Use this constructor when you know ProcessId only
        /// </summary>
        /// <param name="processId">Process id, which will be specified to search</param>
        public TreeSearchFilter(Guid processId)
        {
            _processId = processId;
        }

        /// <summary>
        /// Use this constructor when you have an access to a  ProcessInstance
        /// </summary>
        /// <param name="processInstance">ProcessInstance , wich will be specified to search</param>
        public TreeSearchFilter(ProcessInstance processInstance)
        {
            ProcessInstance = processInstance;
        } 
    }
}
