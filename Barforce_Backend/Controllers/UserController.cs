using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Barforce_Backend.Helper;
using Barforce_Backend.Helper.CustomPropertyValidator;
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
            var userNameValidator = new UserNameValidator();
            if (!userNameValidator.IsValid(userName))
                return BadRequest(new ErrorResponse
                {
                    Message = "No valid userName send"
                });
            if (await _userRepository.UsernameExists(userName))
                return Ok();
            return NoContent();
        }

        [HttpGet("email")]
        public async Task<IActionResult> CheckEmail([FromQuery] string email)
        {
            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(email))
                return BadRequest(new ErrorResponse
                {
                    Message = "No valid email send"
                });
            if (await _userRepository.EMailExists(email))
                return Ok();
            return NoContent();
        }

        [HttpGet("login")]
        public async Task<IActionResult> LoginUser()
        {
            try
            {
                var userData = Request.GetBasicAuth();
                var userToken = await _userRepository.Login(userData[0], userData[1]);
                return Ok(userToken);
            }
            catch (InvalidOperationException e)
            {
                return Unauthorized(new ErrorResponse
                {
                    Message = e.Message
                });
            }
            catch (IndexOutOfRangeException e)
            {
                return BadRequest(new ErrorResponse
                {
                    Message = e.Message
                });
            }
        }

        [HttpGet("verify")]
        public async Task<IActionResult> VerifyUserMail([FromQuery] int userId, [FromQuery] int verifyNumber)
        {
            return Ok(await _userRepository.VerifyMail(verifyNumber));
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

        [Authorize]
        [HttpPost("Logoff")]
        public async Task<IActionResult> LogOff()
        {
            var user = HttpContext.GetTokenUser();
            if (user == null)
                return Unauthorized(new ErrorResponse
                {
                    Message = "No user found in token"
                });
            await _userRepository.LogOff(user.UserId);
            return Ok();
        }
    }
}