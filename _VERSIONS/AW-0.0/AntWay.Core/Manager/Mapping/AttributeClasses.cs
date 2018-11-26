using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntWay.Core.Mapping
{
    public class ActivityAttribute : Attribute
    {
        public string Id;
    }


    public class ParameterBindingAttribute : Attribute
    {
        public string InputBindMethod { get; set; }
        public string OutputBindMethod { get; set; }
        public string IOBindMethod
        {
            get { return InputBindMethod; }
            set
            {
                InputBindMethod = value;
                OutputBindMethod = value;
            }
        }

        public bool InputChecksum { get; set; }
        public bool OutputChecksum { get; set; }

        public bool IOCheckSum
        {
            //get
            //{
            //    return InputChecksum && OutputChecksum;
            //}
            set
            {
                if (value == true)
                {
                    InputChecksum = true;
                    OutputChecksum = true;
                }
            }
        }
    }


    public class ParameterValuesAttribute : Attribute
    {
        //public string Name;
        public string[] Values { get; set; }
    }

}
