using System.Collections.Generic;
using Barforce_Backend.Model.Ingredient;

namespace Barforce_Backend.Model.Drink
{
    public class CreateDrink
    {
        public int GlassSizeId { get; set; }
        public List<NewDrinkIngredient> Ingredients { get; set; }
    }
}