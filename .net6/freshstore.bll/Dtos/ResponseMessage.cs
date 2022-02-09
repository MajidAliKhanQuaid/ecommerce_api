using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace freshstore.bll.Dtos
{
    public class ResponseMessage
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; }
        public bool RequestRefresh { get; set; } = false;
        public dynamic Data { get; set; }
    }
}
