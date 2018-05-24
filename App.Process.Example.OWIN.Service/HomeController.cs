using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace IMHab.PreventBlanqueo.OWIN.Service
{
    public class HomeController : ApiController
    {
        // GET api/home/5 
        public string Get(int id)
        {
            return "value";
        }
    }
}
