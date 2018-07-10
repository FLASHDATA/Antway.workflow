using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntWay.Core.Model
{
    // 200 (ok), 500 (error)

    public class AntWayResponseView
    {
        public const decimal CODE_RESPONSE_ERROR = 500;
        public const decimal CODE_RESPONSE_OK = 200;

        public decimal CodeResponse { get; set; }
        public object Value { get; set; }

        public string Description { get; set; }
        public List<string> DescriptionDetail { get; set; }

        public bool Success => (CodeResponse == CODE_RESPONSE_OK);
    }
}
