using System.Threading.Tasks;
using Barforce_Backend.Interface.Repositories;
using Barforce_Backend.Model.Helper.Middleware;
using Barforce_Backend.Model.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Barforce_Backend.Controllers
{
    [Route("api/user")]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody]UserRegister newUser)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse
                {
                    Message = "Invalid user object send"
                });
            await _userRepository.Register(newUser);
            return StatusCode(201);
        }
    }
}