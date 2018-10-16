using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Winforms.Demos
{
    public class Mensaje
    {
        public string Localizador { get; set; }
        public string Parameter { get; set; }
        public string Value { get; set; }

        public string Display => $"{Localizador}: {Parameter}/{Value}";
    }
}
