using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntWay.EFDAL;
using AntWay.Views;

namespace AntWay.BLL
{
    public class AntWayBLL
    {
        public IDALAntWay IDAL { get; set; }

        public AntWayWorkflowLocatorView Fetch(string locator)
        {
            var dataView = IDAL.Fetch<AntWayWorkflowLocatorView>(locator);
            return dataView;
        }

        public AntWayWorkflowLocatorView Update(AntWayWorkflowLocatorView view)
        {
            var result = IDAL.Update(view);
            return result;
        }

        public AntWayWorkflowLocatorView Insert(AntWayWorkflowLocatorView view)
        {
            var result = IDAL.Insert(view);
            return result;
        }
    }
}
