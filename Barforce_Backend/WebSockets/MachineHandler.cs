using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Barforce_Backend.WebSockets
{
    public class MachineHandler : WebSocketHandler
    {
        public MachineHandler(WebSocketConnectionManager webSocketConnectionManager) : base(webSocketConnectionManager)
        {

        }
        public override async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
        }
    }
}
