using System.Linq;
using System.Threading.Tasks;
using Barforce_Backend.Helper;
using Barforce_Backend.Interface.Repositories;
using Barforce_Backend.Model.Drink;
using Barforce_Backend.Model.Helper.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Barforce_Backend.Controllers
{
    [Authorize]
    [Route("api/drink")]
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

        [HttpGet("history")]
        public async Task<IActionResult> ReadHistory([FromQuery] int? take, [FromQuery] int? skip)
        {
            var user = HttpContext.GetTokenUser();
            var history = await _drinkRepository.ReadUsersHistory(user.UserId, take ?? 10, skip ?? 0);
            if (history.Any())
                return Ok(history);
            return NoContent();
        }

        [HttpPost("{machineId:int}")]
        public async Task<IActionResult> OrderDrink([FromRoute] int machineId, [FromBody] CreateDrink newDrink)
        {
            if (machineId == 0)
                return BadRequest(new ErrorResponse
                {
                    Message = "Invalid machineId"
                });

            var user = HttpContext.GetTokenUser();
            var drinksInQueue = await _drinkRepository.CreateOrder(user.UserId, machineId, newDrink);
            return Ok(new
            {
                drinksInQueue
            });
        }
    }
}