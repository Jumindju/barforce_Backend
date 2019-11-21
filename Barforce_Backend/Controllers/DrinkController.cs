using System.Threading.Tasks;
using Barforce_Backend.Interface.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Barforce_Backend.Controllers
{
    [Authorize]
    [Route("api/{machineId:int}/drink")]
    public class DrinkController : Controller
    {
        private readonly IDrinkRepository _drinkRepository;

        public DrinkController(IDrinkRepository drinkRepository)
        {
            _drinkRepository = drinkRepository;
        }

        [HttpGet("glasses")]
        public async Task<IActionResult> ReadGlassSizes()
        {
            return Ok(await _drinkRepository.ReadGlassSizes());
        }
    }
}