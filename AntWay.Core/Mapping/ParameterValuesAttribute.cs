﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AntWay.Core.Mapping
{
    public class ParameterValuesAttribute : Attribute
    {
        //public string Name;
        public string[] Values { get; set; }
    }
}
