using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Barforce_Backend.Model.Websocket
{
    public class UserDrink
    {
        public UserDrink(string userName, List<DrinkCommand> drinkList)
        {
            UserName = userName;
            DrinkList = drinkList;
        }
        public string UserName { get; set; }
        public List<DrinkCommand> DrinkList { get; set; }
    }
}
