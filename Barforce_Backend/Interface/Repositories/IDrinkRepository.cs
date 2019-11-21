using System.Collections.Generic;
using System.Threading.Tasks;
using Barforce_Backend.Model.Drink;

namespace Barforce_Backend.Interface.Repositories
{
    public interface IDrinkRepository
    {
        Task<IEnumerable<GlassSize>> ReadGlassSizes();
        Task<int> CreateDrink(CreateDrink newDrink);
    }
}