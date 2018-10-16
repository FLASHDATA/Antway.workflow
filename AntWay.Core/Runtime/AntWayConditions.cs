using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AntWay.Core.Mapping;
using AntWay.Core.Model;
using AntWay.Core.Scheme;
using Newtonsoft.Json;
using OptimaJet.Workflow.Core.Model;

namespace AntWay.Core.Runtime
{
    //public class AntWayConditions
    //{
    //    public static AntWayConditions Single => new AntWayConditions();

    //    public bool EvaluateExpression<TA, TPO>(ProcessInstance processInstance,
    //                                     Expression<Func<TPO, bool>> condition) where TA : IAntWayRuntimeActivity
    //    {
    //        Type type = typeof(TA);
    //        string activityId = GetActivityIdFromClassType(type);
    //        bool result = EvaluateExpression<TA, TPO>(processInstance, activityId, condition);

    //        return result;
    //    }


    //    public bool EvaluateExpression<TA,TPO>(ProcessInstance processInstance, string activityId,
    //                                      Expression<Func<TPO, bool>> condition) where TA: IAntWayRuntimeActivity
    //    {
    //        List<TPO> l = new List<TPO>();
    //        List<ActivityExecution> paramValues = AntWayRuntime.Single
    //                                                .GetActivityExecutionObject<List<ActivityExecution>>
    //                                                        (processInstance, activityId);
    //        TPO parametersOut = JsonConvert.DeserializeObject<TPO>
    //                                (paramValues.LastOrDefault().ParametersOut.ToString());

    //        IAntWayRuntimeActivity activityInstance = WorkflowClient.AntWayRunTime
    //                                                   .GetActivityExecutionObject<List<TA>>
    //                                                        (processInstance, activityId)
    //                                                   .LastOrDefault();

    //        if (activityInstance == null) return false;

    //        activityInstance.ParametersBind = MappingReflection
    //                                          .GetParametersBind(processInstance.ProcessId.ToString(),
    //                                                             activityInstance, parametersOut);
    //        l.Add((TPO) activityInstance.ParametersBind);

    //        var f = from g in l.
    //                Where(condition.Compile())
    //                select g;
    //        f.ToList();

    //        bool result = f.Any();

    //        return result;
    //    }

    //    protected string GetActivityIdFromClassType(Type type)
    //    {
    //        Type myType1 = Type.GetType($"{type.FullName}, {type.Assembly.ManifestModule.Name.Replace(".dll", "")}");
    //        if (myType1 == null) return null;

    //        var result = myType1.GetAttributeValue((ActivityAttribute a) => a.Id);

    //        return result;
    //    }
    //}
}
