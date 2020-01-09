using System;

namespace Barforce_Backend.Model.User
{
    public abstract class AlcoholStats
    {
        public DateTime ServeTime { get; set; }
        public double AlcAmount { get; set; }
    }
}