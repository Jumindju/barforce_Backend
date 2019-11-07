using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Barforce_Backend.Helper;
using Barforce_Backend.Interface.Helper;
using Barforce_Backend.Interface.Repositories;
using Barforce_Backend.Model.Helper.Middleware;
using Barforce_Backend.Model.User;
using Microsoft.AspNetCore.Authorization;
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

        [HttpGet("userName")]
        public async Task<IActionResult> CheckUsername([FromQuery] string userName)
        {
            if (await _userRepository.UsernameExists(userName))
                return Ok();
            return NoContent();
        }

        [HttpGet("email")]
        public async Task<IActionResult> CheckEmail([FromQuery] string email)
        {
            return await _userRepository.EMailExists(email)
                ? Conflict()
                : StatusCode(200);
        }

        [HttpGet("login")]
        public async Task<IActionResult> LoginUser()
        {
            var header = Request.Headers["Authorization"];
            if (header.Count == 0)
                return Unauthorized();
            var baseAuth = header.ToString().Substring(6);
            var encryptedAuth = Convert.FromBase64String(baseAuth);
            var authString = Encoding.UTF8.GetString(encryptedAuth);
            var split = authString.Split(":", StringSplitOptions.RemoveEmptyEntries);
            if (split.Length != 2)
                return BadRequest(new ErrorResponse
                {
                    Message = "Invalid auth header"
                });
            return Ok(await _userRepository.Login(split[0], split[1]));
        }

        [HttpGet("verify")]
        public async Task<IActionResult> VerifyUserMail([FromQuery] int userId, [FromQuery] Guid verifyToken)
        {
            return Ok(await _userRepository.VerifyMail(userId, verifyToken));
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] UserRegister newUser)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(val => val.Errors)
                    .Select(err => err.ErrorMessage);
                return BadRequest(new ErrorResponse
                {
                    Message = $"Invalid user object send: {string.Join(',', errors)}"
                });
            }

            await _userRepository.Register(newUser);
            return StatusCode(201);
        }

        [Authorize]
        [HttpPost("changePw")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPassword newPw)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse
                {
                    Message = "Invalid new password"
                });
            var user = HttpContext.GetTokenUser();
            if (user == null)
                return Unauthorized(new ErrorResponse
                {
                    Message = "No user found in token"
                });
            await _userRepository.ResetPassword(user.UserId, newPw.NewPassword);
            return Ok();
        }
    }
}