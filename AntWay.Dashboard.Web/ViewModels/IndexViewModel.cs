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
}