using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Barforce_Backend.Helper;
using Barforce_Backend.Interface.Repositories;
using Barforce_Backend.Model.Drink.Favourite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Barforce_Backend.Controllers
{
    [Authorize]
    [Route("api/favourite")]
    public class FavouriteController : Controller
    {
        private readonly IDrinkRepository _drinkRepository;

        public FavouriteController(IDrinkRepository drinkRepository)
        {
            _drinkRepository = drinkRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetFavourites()
        {
            var user = HttpContext.GetTokenUser();
            var favouriteDrinks = await _drinkRepository.GetFavouriteDrinks(user.UserId);
            if (favouriteDrinks.Any())
                return Ok(favouriteDrinks);
            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> AddFavourite([FromBody] NewFavouriteDrink newNewFavourite)
        {
            var user = HttpContext.GetTokenUser();
            return StatusCode(201, new
            {
                drinkId = await _drinkRepository.AddFavourite(user.UserId, newNewFavourite)
            });
        }
    }
}