using System.Collections.Generic;

namespace Barforce_Backend.Model.Websocket
{
    public class MachineQueue
    {
        public int DBId { get; set; }
        public Queue<string> Messages { get; set; }
    }
}
