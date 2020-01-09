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
        private readonly IFavoriteRepository _favoriteRepository;

        public FavouriteController(IFavoriteRepository favoriteRepository)
        {
            _favoriteRepository = favoriteRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetFavourites()
        {
            var user = HttpContext.GetTokenUser();
            var favouriteDrinks = await _favoriteRepository.GetFavouriteDrinks(user.UserId);
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
                drinkId = await _favoriteRepository.AddFavourite(user.UserId, newNewFavourite)
            });
        }

        [HttpDelete("{drinkId:int}")]
        public async Task<IActionResult> RemoveFavorite([FromRoute]int drinkId)
        {
            var user = HttpContext.GetTokenUser();
            await _favoriteRepository.RemoveFavouriteDrink(user.UserId, drinkId);
            return Ok();
        }
    }
}