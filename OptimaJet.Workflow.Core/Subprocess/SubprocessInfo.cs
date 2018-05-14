using System.Collections.Generic;
using System.Linq;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Subprocess
{
    //public class SubprocessInfo
    //{
    //    public ProcessDefinition ProcessDefinition { get; private set; }

    //    public bool IsAllowedForAllActivities { get; private set; }

    //    public IEnumerable<string> AllowedActivities
    //    {
    //        get { return _allowedActivities; }
    //    }

    //    private readonly List<string> _allowedActivities;

    //    public SubprocessInfo(ProcessDefinition processDefinition)
    //    {
    //        IsAllowedForAllActivities = true;
    //        ProcessDefinition = processDefinition;
    //    }

    //    public SubprocessInfo(ProcessDefinition processDefinition, List<string> allowedActivities)
    //    {
    //        ProcessDefinition = processDefinition;
    //        if (allowedActivities == null || !allowedActivities.Any())
    //        {
    //            IsAllowedForAllActivities = true;
    //        }
    //        else
    //        {
    //            IsAllowedForAllActivities = false;
    //            _allowedActivities = allowedActivities;
    //        }
    //    }

    //}
}
