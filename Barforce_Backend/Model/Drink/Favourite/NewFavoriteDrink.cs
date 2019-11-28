using System.ComponentModel.DataAnnotations;

namespace Barforce_Backend.Model.Drink.Favourite
{
    public class NewFavouriteDrink : CreateDrink
    {
        [StringLength(64)]
        public string Name { get; set; }
    }
}