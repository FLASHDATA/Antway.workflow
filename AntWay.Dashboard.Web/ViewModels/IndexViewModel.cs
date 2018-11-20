using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AntWay.Dashboard.Web.ViewModels
{
    public class IndexViewModel
    {
        public NewSchemeViewModel NewSchemeViewModel { get; set; }
    }

    public class NewSchemeViewModel
    {
        public string NewSchemeCode { get; set; }
        public string NewSchemeName { get; set; }
        public string NewSchemeDataBase { get; set; }


        public List<string> DBSchemes { get; set; }

        public List<SelectListItem> DBSchemesListItems
        {
            get
            {
                var result = new List<SelectListItem>();

                foreach (string scheme in DBSchemes)
                {
                    result.Add(new SelectListItem
                    {
                        Value = scheme,
                        Text = scheme
                    });
                }

                return result;
            }
        }
    }

    public class EditSchemeViewModel
    {
        public string SchemeCode { get; set; }
        public string SchemeName { get; set; }
        public string SchemeDataBase { get; set; }
        public string Description { get; set; }
        public bool WorkflowService { get; set; }
        public bool Active { get; set; }
    }

    public class ActivityManagerViewModel
    {
        public List<string> StateList { get; set; }
        public string TagDescription { get; set; }

        public string StateSelected { get; set; }
    }

    public class ActivityStatePostViewModel
    {
        public string scheme { get; set; }
        public string locator { get; set; }
        public string stateToSet { get; set; }
    }
}