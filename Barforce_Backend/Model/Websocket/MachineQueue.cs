using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Barforce_Backend.Model.Websocket
{
    public class MachineQueue
    {
        public int DBId { get; set; }
        public Queue<string> Messages { get; set; }
    }
}
