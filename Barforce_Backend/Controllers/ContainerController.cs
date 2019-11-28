using System.Linq;
using System.Threading.Tasks;
using Barforce_Backend.Interface.Repositories;
using Barforce_Backend.Model.Helper.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Barforce_Backend.Controllers
{
    [Route("api/{machineId:int}/container")]
    public class ContainerController : Controller
    {
        private readonly IContainerRepo _containerRepo;

        public ContainerController(IContainerRepo containerRepo)
        {
            _containerRepo = containerRepo;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllContainer([FromRoute] int machineId)
        {
            if (machineId <= 0)
                return BadRequest(new ErrorResponse
                {
                    Message = "Invalid MachineId"
                });
            var containers = await _containerRepo.ReadAll(machineId);
            return !containers.Any()
                ? (IActionResult) StatusCode(StatusCodes.Status204NoContent)
                : StatusCode(200, containers);
        }
    }
}