using System;
using System.Collections.Generic;
using Barforce_Backend.Model.Ingredient;

namespace Barforce_Backend.Model.Drink.Overview
{
    public class OverviewDrink
    {
        public int DrinkId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime ServeTime { get; set; }
        public int GlassSize { get; set; }
        public List<DrinkIngredient> Ingredients { get; set; }
    }
}