// Controllers/DoctorsController.cs
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.Interfaces;
namespace PosHCExternal.web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisitItemController : ControllerBase
    {
        private readonly IVisitItemService _visitItemService;

        public VisitItemController(IVisitItemService service)
        {
            _visitItemService = service;
        }

        [HttpGet("visititem")]
        public async Task<IActionResult> GetAllVisitItems(CancellationToken cancellationToken)
        {
            var result = await _visitItemService.GetAllVisitItems(cancellationToken);
            return Ok(result);
        }
    }
}
