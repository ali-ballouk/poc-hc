using Microsoft.AspNetCore.Mvc;

namespace PosHCExternal.web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HelloController : ControllerBase
    {
        [HttpGet("greet")]
        public IActionResult Greet()
        {
            return Ok(new { message = "Hello from .NET Core Web API!" });
        }
    }
}
