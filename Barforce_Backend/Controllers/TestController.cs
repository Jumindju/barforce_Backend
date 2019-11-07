using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Barforce_Backend.Controllers
{
    [Route("api/test")]
    public class TestController : Controller
    {
        [Authorize]
        [HttpGet]
        public IActionResult Test()
        {
            return Ok("Hello World");
        }
    }
}
