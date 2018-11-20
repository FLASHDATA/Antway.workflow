using AntWay.Core.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Model.Expedientes.Activities
{
    public class ExpedientesActivityManager: ActivityManagerBase, IActivityManager
    {
        protected override List<ActivityManager> SetActivitiesManager()
        {
            var result = new List<ActivityManager>();

            var activity1Mapping = new ActivityManager(typeof(ActivityExpedienteNew),
                                                       typeof(ActivityExpedienteNewModel))
            {
                Priority = 1,
                //MethodActivitiesBond = new List<MethodActivityBond>()
                //{
                //    new MethodActivityBond(nameof(ActivityExpedienteNew.BindingMethod_FechaCreacionExpediente)),
                //    new MethodActivityBond(nameof(ActivityExpedienteNew.BindingMethod_Canceled))
                //}
            };

            result.Add(activity1Mapping);


            return result
                    .OrderBy(p => p.Priority)
                    .ToList();
        }
    }
}
