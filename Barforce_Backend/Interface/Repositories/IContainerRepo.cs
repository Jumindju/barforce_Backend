using System.Collections.Generic;
using System.Threading.Tasks;
using Barforce_Backend.Model.Container;
using Barforce_Backend.Model.Ingredient;

namespace Barforce_Backend.Interface.Repositories
{
    public interface IContainerRepo
    {
        Task<IEnumerable<ContainerDto>> ReadAll(int machineId);
        Task<bool> IngredientsInContainer(int machineId, int glassSize, List<NewDrinkIngredient> ingredients);
    }
}