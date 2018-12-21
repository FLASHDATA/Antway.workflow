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
            var result = new List<ActivityManager>();
            return result;
        }
    }

    public class ActivityManagerVoid : ActivityManagerBase, IActivityManager
    {
    }


    public class ActivityManager
    {
        public int Priority { get; set; }
        public Type ClassActivityType { get; set; }


        public ActivityManager(Type classActivityType)
        {
            ClassActivityType = classActivityType;
        }
    }
}
