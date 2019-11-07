using Barforce_Backend.WebSockets;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Barforce_Backend.Controllers
{
    [Route("api/test")]
    public class TestController : Controller
    {
        private MachineHandler _machineHandler { get; set; }
        public TestController(MachineHandler machineHandler)
        {
            _machineHandler = machineHandler;
        }
        [HttpGet]
        public  IActionResult Test()
        {
            return Ok("Hello World");
        }
        [HttpPost]
        public async Task<IActionResult> TestWebsocketAsync()
        {
            await _machineHandler.SendMessageToMachine(1, "Hello from Server");
            return Ok("Send Message to Client");
        }
    }
}
