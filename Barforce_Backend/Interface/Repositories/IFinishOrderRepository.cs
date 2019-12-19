using System.Collections.Generic;
using System.Threading.Tasks;
using Barforce_Backend.Model.Drink;
using Barforce_Backend.Model.Drink.Favourite;
using Barforce_Backend.Model.Drink.Overview;
using Barforce_Backend.Model.Websocket;

namespace Barforce_Backend.Interface.Repositories
{
    public interface IFinishOrderRepository
    {
        Task FinishOrder(int orderId, List<DrinkCommand> drinks, bool aborted = false);
    }
}