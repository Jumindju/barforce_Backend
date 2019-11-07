using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Barforce_Backend.Model.Websocket
{
    public class AdruinoMessage
    {
        public string Action { get; set; }
        public object Data { get; set; }
    }
}
