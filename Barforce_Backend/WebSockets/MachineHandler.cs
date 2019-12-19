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
        List<MachineQueue> machineMessages = new List<MachineQueue>();
        Dictionary<string, int> connections = new Dictionary<string, int>();
        private readonly ILogger _logger;
        private readonly IFinishOrderRepository _finishOrderRepository;

        public MachineHandler(WebSocketConnectionManager webSocketConnectionManager, ILoggerFactory loggerFactory, IFinishOrderRepository finishOrderRepository) : base(webSocketConnectionManager)
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
                            _logger.LogError("Machine already inited: " + dbId + " ==> deleto old create new");
                            connections.Remove(tmpSocketId);
                            connections.Add(socketId, dbId);
                        }
                        else
                        {
                            connections.Add(socketId, dbId);
                            _logger.LogInformation("init Machine: " + dbId);
                        }
                        MachineQueue queue = machineMessages.Find(x => x.DBId == dbId);
                        if (queue == null)
                        {
                            machineMessages.Add(new MachineQueue() { DBId = dbId, Orders = new Queue<Order>() });
                        }
                        else if (queue.Orders.Count > 0)
                        {
                            Order order = queue.Orders.First();
                            await SendMessageAsync(socketId, order.Message);
                        }
                        break;
                    case "finished":
                    case "aborted":
                        connections.TryGetValue(socketId, out int machineId);
                        MachineQueue queue1 = machineMessages.Find(x => x.DBId == machineId);
                        if (queue1 != null && queue1.Orders.Count > 0)
                        {
                            Order order = queue1.Orders.First();
                            try
                            {
                                _finishOrderRepository.FinishOrder(order.OrderId, order.MessageObject, message.Action == "aborted");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"FinishOrder failed after Arduino finished Drink (lastOrderId: {order.OrderId}, lastCommand: {order.MessageObject}, aborted: {message.Action == "aborted"})");
                            }
                            queue1.Orders.Dequeue();
                            if (queue1.Orders.Count > 0)
                            {
                                Order newOrder = queue1.Orders.First();
                                await SendMessageAsync(socketId, newOrder.Message);
                            }
                        }
                        break;
                    default: break;
                }
            }
        }
        public override async Task<int> SendMessageToMachine(int machineId, string userName, int orderId, List<DrinkCommand> _message)
        {
            string message = JsonConvert.SerializeObject(new UserDrink(userName,_message));
            if (!string.IsNullOrEmpty(message))
            {
                string socketId = connections.FirstOrDefault(x => x.Value == machineId).Key;
                if (socketId != null)
                {
                    MachineQueue queue = machineMessages.Find(x => x.DBId == machineId);
                    queue.Orders.Enqueue(new Order(orderId, userName, message, _message));
                    if (queue.Orders.Count == 1)
                    {
                        await SendMessageAsync(socketId, message);
                    }
                    return queue.Orders.Count - 1; // Position in Warteschlange
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
