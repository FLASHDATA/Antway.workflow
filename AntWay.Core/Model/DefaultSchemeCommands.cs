using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntWay.Core.Model
{
    public class DefaultSchemeCommandNames
    {
        public static DefaultSchemeCommandNames Single
                                       => new DefaultSchemeCommandNames();

        //public string Next => "Next";
        public string Siguiente => "Siguiente";
        public string Anterior => "Anterior";
    }
}
