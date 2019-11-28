using System.Threading.Tasks;
using Barforce_Backend.Helper;
using Barforce_Backend.Interface.Helper;
using Barforce_Backend.Interface.Repositories;
using Barforce_Backend.Model.Drink;
using Barforce_Backend.Model.Helper.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Barforce_Backend.Controllers
{
    [Authorize]
    [Route("api/{machineId:int}/drink")]
    public class DrinkController : Controller
    {
        private readonly IDrinkRepository _drinkRepository;
        private readonly ITokenHelper _tokenHelper;

        public DrinkController(IDrinkRepository drinkRepository, ITokenHelper tokenHelper)
        {
            _drinkRepository = drinkRepository;
            _tokenHelper = tokenHelper;
        }

        [HttpGet("glasses")]
        public async Task<IActionResult> ReadGlassSizes()
        {
            return Ok(await _drinkRepository.ReadGlassSizes());
        }

        [HttpPost]
        public async Task<IActionResult> CreateDrink([FromRoute] int machineId, [FromBody] CreateDrink newDrink)
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