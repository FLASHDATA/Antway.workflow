using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntWay.Core.Model
{
    public class ResponseCodes
    {
        public const decimal CODE_RESPONSE_ERROR = 500;
        public const decimal CODE_RESPONSE_OK = 200;
        public const decimal CODE_RESPONSE_NOT_FOUND = 404;
    }

    public class CallServiceResponseView
    {
        public decimal CodeResponse { get; set; }
        public bool Success => (CodeResponse == ResponseCodes.CODE_RESPONSE_OK);

        public object Value { get; set; }
        public string Description { get; set; }
        public List<string> DescriptionDetail { get; set; }
    }

    public class CallWorkFlowResponseView
    {
        public decimal CodeResponse { get; set; }
        public bool Success => (CodeResponse == ResponseCodes.CODE_RESPONSE_OK);

        public Guid? WorflowProcessId { get; set; }
        
    }
}
