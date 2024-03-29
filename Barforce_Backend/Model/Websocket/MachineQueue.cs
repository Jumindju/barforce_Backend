using System.Collections.Generic;

namespace Barforce_Backend.Model.Websocket
{
    public class MachineQueue
    {
        public int DBId { get; set; }
        public Queue<Order> Orders { get; set; }
    }
    public class Order
    {
        public Order (int orderId,string userName, string message, List<DrinkCommand> messageObject)
        {
            OrderId = orderId;
            UserName = userName;
            Message = message;
            MessageObject = messageObject;
        }
        public int OrderId { get; set; }
        public string Message { get; set; }
        public string UserName { get; set; }
        public List<DrinkCommand> MessageObject { get; set; }
    }
}
