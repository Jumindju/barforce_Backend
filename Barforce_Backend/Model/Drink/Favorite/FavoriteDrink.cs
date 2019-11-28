using System.Collections.Generic;
using Barforce_Backend.Model.Ingredient;

namespace Barforce_Backend.Model.Drink.Favorite
{
    public class FavoriteDrink : NewFavoriteDrink
    {
        public int GlassSize { get; set; }
        public int DrinkId { get; set; }
        public new List<DrinkIngredient> Ingredients { get; set; }
    }
}