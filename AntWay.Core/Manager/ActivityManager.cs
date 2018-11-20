using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntWay.Core.Manager
{
    public interface IActivityManager
    {
        List<ActivityManager> GetActivitiesManager();
    }

    public abstract class ActivityManagerBase
    {
        public virtual List<ActivityManager> GetActivitiesManager()
        {
            return SetActivitiesManager();
        }

        protected virtual List<ActivityManager> SetActivitiesManager()
        {
            var result = new List<ActivityManager>();
            return result;
        }
    }

    public class ActivityManagerVoid: ActivityManagerBase, IActivityManager
    {
    }


    public class ActivityManager
    {
        public int Priority { get; set; }
        public Type ClassActivityType { get; set; }
        public Type ClassActivityModelType { get; set; }
        //public List<MethodActivityBond> MethodActivitiesBond;

        public ActivityManager(Type classActivityType, Type classActivityModelType)
        {
            ClassActivityType = classActivityType;
            ClassActivityModelType = classActivityModelType;
        }
    }



    //public class MethodActivityBond
    //{
    //    public enum EAction
    //    {
    //        SkipToActivityTag,
    //        Stop
    //    }
        
    //    public string MethodName { get; set; }
    //    public int Priority { get; set; }
    //    public EAction Action { get; set; }

    //    public MethodActivityBond(string methodName)
    //    {
    //        MethodName = methodName;
    //        Priority = 0;
    //        Action = EAction.Stop;
    //    }
    //}
}
