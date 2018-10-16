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
        //public async Task<string> Get(int id)
        //{
        //    await Task.Run(() => System.Threading.Thread.Sleep(30000));
        //    return "value";
        //}


        //GET api/home/5 
        public string Get(int id)
        {
            System.Threading.Thread.Sleep(30000);
            return "value";
        }
    }
}
