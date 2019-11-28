using System.Collections.Generic;

namespace Barforce_Backend.Model.Drink
{
    public class CreateDrink
    {
        public int GlassSizeId { get; set; }
        public List<DrinkIngredient> Ingredients { get; set; }
    }
}