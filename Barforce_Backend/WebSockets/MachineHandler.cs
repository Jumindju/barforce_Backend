using Barforce_Backend.Interface.Repositories;
using Barforce_Backend.Model.Websocket;
using Microsoft.Extensions.Logging;
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
        int lastOrderId;
        List<DrinkCommand> lastCommand = new List<DrinkCommand>();
        List<MachineQueue> machineMessages = new List<MachineQueue>();
        Dictionary<string, int> connections = new Dictionary<string, int>();
        private readonly ILogger _logger;
        private readonly IFinishOrderRepository _finishOrderRepository;

        public MachineHandler(WebSocketConnectionManager webSocketConnectionManager, ILoggerFactory loggerFactory,  IFinishOrderRepository finishOrderRepository) : base(webSocketConnectionManager)
        {
            _logger = loggerFactory.CreateLogger<MachineHandler>();
            _finishOrderRepository = finishOrderRepository;
        }
        public override async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            string messageString = Encoding.UTF8.GetString(buffer, 0, result.Count);
            _logger.LogInformation($"Websocketiddleware, Received Websocket Text: {messageString}");
            AdruinoMessage message = null;
            try
            {
                message = JsonConvert.DeserializeObject<AdruinoMessage>(messageString);
            }
            catch
            {
                _logger.LogError("Invalid Adruino Message: " + messageString);
            }
            if (message != null)
            {
                string socketId = null;
                socketId = WebSocketConnectionManager.GetId(socket);
                switch (message.Action)
                {
                    case "init":
                        int.TryParse(message.Data.ToString(), out int dbId);
                        string tmpSocketId = connections.FirstOrDefault(x => x.Value == dbId).Key;
                        if (tmpSocketId != null)
                        {
                            _logger.LogError("Machine already inited: " + dbId);
                        }
                        else
                        {
                            connections.Add(socketId, dbId);
                            _logger.LogInformation("init Machine: " + dbId);
                        }
                        MachineQueue queue = machineMessages.Find(x => x.DBId == dbId);
                        if (queue == null)
                        {
                            machineMessages.Add(new MachineQueue() { DBId = dbId, Messages = new Queue<string>() });
                        }
                        else if (queue.Messages.Count > 0)
                        {
                            string msg = queue.Messages.First();
                            await SendMessageAsync(socketId, msg);
                        }
                        break;
                    case "finished":
                        try
                        {
                            _finishOrderRepository.FinishOrder(lastOrderId, lastCommand);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"FinishOrder failed after Arduino finished Drink (lastOrderId: {lastOrderId}, lastCommand: {lastCommand})");
                        }
                        connections.TryGetValue(socketId, out int machineId);
                        MachineQueue queue1 = machineMessages.Find(x => x.DBId == machineId);
                        if (queue1 != null)
                        {
                            queue1.Messages.Dequeue();
                            if (queue1.Messages.Count > 0)
                            {
                                string msg = queue1.Messages.First();
                                await SendMessageAsync(socketId, msg);
                            }
                        }
                        break;
                    default: break;
                }
            }
        }
        public override async Task<int> SendMessageToMachine(int machineId, int orderId, List<DrinkCommand> _message)
        {
            string message = JsonConvert.SerializeObject(_message);
            if (!string.IsNullOrEmpty(message))
            {
                string socketId = connections.FirstOrDefault(x => x.Value == machineId).Key;
                if (socketId != null)
                {
                    MachineQueue queue = machineMessages.Find(x => x.DBId == machineId);
                    queue.Messages.Enqueue(message);
                    if (queue.Messages.Count == 1)
                    {
                        await SendMessageAsync(socketId, message);
                        lastCommand = _message;
                        lastOrderId = orderId;
                    }
                    return queue.Messages.Count - 1; // Position in Warteschlange
                }
                else
                {
                    _logger.LogError("Cannot Get SocketId by machineId: " + machineId + " ,Message: " + message);
                }
            }
            else
            {
                _logger.LogError("Invalid Message: " + message);
            }
            return -1;
        }
        public override async Task OnDisconnected(WebSocket socket)
        {
            string socketId = WebSocketConnectionManager.GetId(socket);
            await WebSocketConnectionManager.RemoveSocket(socketId);
            if (connections.TryGetValue(socketId, out int dbId))
            {
                connections.Remove(socketId);
                _logger.LogInformation("Machine Disconnected (DBId): " + dbId);
            }
        }
    }
}
