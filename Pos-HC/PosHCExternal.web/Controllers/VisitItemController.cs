// Controllers/DoctorsController.cs
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.Interfaces;
namespace PosHCExternal.web.Controllers
{
    [ApiController]
    [Route("api/visititem")]
    public class VisitItemController : ControllerBase
    {
        private readonly IVisitItemService _visitItemService;

        public VisitItemController(IVisitItemService service)
        {
            _visitItemService = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllVisitItems(CancellationToken cancellationToken)
        {
            var result = await _visitItemService.GetAllVisitItems(cancellationToken);
            return Ok(result);
        }
    }
}
