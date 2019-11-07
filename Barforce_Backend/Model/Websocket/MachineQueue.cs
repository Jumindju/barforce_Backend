using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Barforce_Backend.Model.Websocket
{
    public class MachineQueue
    {
        public string socketId { get; set; }
        public Queue<string> messages { get; set; }
    }
}
