using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
