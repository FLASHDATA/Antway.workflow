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
        public IDALLocator IDALLocator { get; set; }
        public IDALSchema IDALSchema { get; set; }

        public WorkflowLocatorView GetWorkflowLocator(string locator)
        {
            var dataView = IDALLocator.Fetch<WorkflowLocatorView>(locator);
            return dataView;
        }

        //public WorkflowLocatorView AddWorkflowLocator(string locator)
        //{
        //    IDALLocator.Insert
        //}
    }
}
