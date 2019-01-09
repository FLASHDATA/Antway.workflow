using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntWay.Core.Model
{
    public class NullableDecimal : ActivityModelBase<NullableDecimal>
    {
        public decimal? Value { get; set; }

        public NullableDecimal() { }

        public NullableDecimal(decimal? value)
        {
            Value = value;
        }
    }
}
