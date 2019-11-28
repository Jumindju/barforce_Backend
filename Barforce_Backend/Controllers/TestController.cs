using Barforce_Backend.WebSockets;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Barforce_Backend.Model.Websocket;

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
        [Authorize]
        [HttpGet]
        public  IActionResult Test()
        {
            return Ok("Hello World");
        }
        [HttpPost]
        public async Task<IActionResult> TestWebsocketAsync()
        {
            List<DrinkCommand> msg = new List<DrinkCommand>()
            {
                new DrinkCommand(){Id=1,AmmountMl=50},
                new DrinkCommand(){Id=2,AmmountMl=150},
                new DrinkCommand(){Id=3,AmmountMl=100},
                new DrinkCommand(){Id=4,AmmountMl=0}
            };

            int queuePosition = await _machineHandler.SendMessageToMachine(1, msg); // 0 = dran, 1 = als nächstes ...
            return Ok($"Send Message to Client, QueuePosition: {queuePosition}");
        }
    }
}
