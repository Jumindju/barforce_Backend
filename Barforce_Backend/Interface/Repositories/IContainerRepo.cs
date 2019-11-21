using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Barforce_Backend.Model.Container;

namespace Barforce_Backend.Interface.Repositories
{
    public interface IContainerRepo
    {
        Task<IEnumerable<ContainerDto>> ReadAll(int machineId);
        Task<bool> IngredientInContainer(int machineId, List<int> ingredients);
    }
}