using Barforce_Backend.WebSockets;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Barforce_Backend.Controllers
{
    [Route("api/test")]
    public class TestController : Controller
    {
        private MachineHandler MachineHandler { get; set; }
        public TestController(MachineHandler machineHandler)
        {
            MachineHandler = machineHandler;
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
            await MachineHandler.SendMessageToMachine(1, "Hello from Server");
            return Ok("Send Message to Client");
        }
    }
}
