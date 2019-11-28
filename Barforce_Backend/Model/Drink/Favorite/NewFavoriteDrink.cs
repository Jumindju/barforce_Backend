using System.ComponentModel.DataAnnotations;

namespace Barforce_Backend.Model.Drink.Favorite
{
    public class NewFavoriteDrink : CreateDrink
    {
        [StringLength(64)]
        public string Name { get; set; }
    }
}