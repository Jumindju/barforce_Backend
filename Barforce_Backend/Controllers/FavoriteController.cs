using System.Net.Http;
using System.Threading.Tasks;
using Barforce_Backend.Helper;
using Barforce_Backend.Interface.Repositories;
using Barforce_Backend.Model.Drink.Favorite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Barforce_Backend.Controllers
{
    [Authorize]
    [Route("api/favorite")]
    public class FavoriteController : Controller
    {
        private readonly IDrinkRepository _drinkRepository;

        public FavoriteController(IDrinkRepository drinkRepository)
        {
            _drinkRepository = drinkRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetFavorites()
        {
            return StatusCode(StatusCodes.Status501NotImplemented);
        }

        [HttpPost]
        public async Task<IActionResult> AddFavorite([FromBody] FavoriteDrink newFavorite)
        {
            var user = HttpContext.GetTokenUser();
            return StatusCode(201, new
            {
                drinkId = await _drinkRepository.AddFavorite(user.UserId, newFavorite)
            });
        }
    }
}