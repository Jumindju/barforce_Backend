using Barforce_Backend.Model.Websocket;
using Newtonsoft.Json;
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

        List<MachineQueue> machineMessages = new List<MachineQueue>();
        Dictionary<string, int> connections = new Dictionary<string, int>();
        public MachineHandler(WebSocketConnectionManager webSocketConnectionManager) : base(webSocketConnectionManager) { }
        public override async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            string messageString = Encoding.UTF8.GetString(buffer, 0, result.Count);
            AdruinoMessage message = null;
            try
            {
                message = JsonConvert.DeserializeObject<AdruinoMessage>(messageString);

            }
            catch
            {
                Console.WriteLine("Invalid Adruino Message: " + messageString);
            }
            if (message != null)
            {
                string socketId = null;
                socketId = WebSocketConnectionManager.GetId(socket);
                switch (message.Action)
                {
                    case "init":
                        int.TryParse(message.Data.ToString(), out int dbId);
                        connections.Add(socketId, dbId);
                        MachineQueue queue = machineMessages.Find(x => x.socketId == socketId);
                        if (queue == null)
                        {
                            machineMessages.Add(new MachineQueue() { socketId = socketId, messages = new Queue<string>() });
                        }
                        else if(queue.messages.Count > 0)
                        {
                            string msg = queue.messages.First();
                            await SendMessageAsync(queue.socketId, msg);
                        }
                        break;
                    case "finished":
                        MachineQueue queue1 = machineMessages.Find(x => x.socketId == socketId );
                        queue1.messages.Dequeue();
                        if (queue1.messages.Count > 0)
                        {
                            string msg = queue1.messages.First();
                            await SendMessageAsync(queue1.socketId, msg);
                        }
                        break;
                    default: break;
                }
                Console.WriteLine("Message: " + message);
            }
        }
        public override async Task SendMessageToMachine(int machineId, string message)
        {
            string socketId = connections.First(x => x.Value == machineId).Key;
            if (socketId != null)
            {
                MachineQueue queue = machineMessages.Find(x => x.socketId == socketId);
                queue.messages.Enqueue(message);
                if (queue.messages.Count == 1)
                {
                    await SendMessageAsync(socketId, message);
                }
            }
            else
            {
                Console.WriteLine("Cannot Get SocketId by machineId: " + machineId + " ,Message: " + message);
            }
        }
        public override async Task OnDisconnected(WebSocket socket)
        {
            string socketId = WebSocketConnectionManager.GetId(socket);
            await WebSocketConnectionManager.RemoveSocket(WebSocketConnectionManager.GetId(socket));
            if (connections.TryGetValue(socketId, out int dbId))
            {
                connections.Remove(socketId);
                Console.WriteLine("Machine Disconnected (DBId): " + dbId);
            }
        }
    }
}
