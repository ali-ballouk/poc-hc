// Controllers/DoctorsController.cs
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.Interfaces;
using PosHC.Application.Services;
namespace PosHCExternal.web.Controllers
{
    [ApiController]
    [Route("api/catalogtitems")]
    public class CatalogItemController : ControllerBase
    {
        private readonly ICatalogItemService _catalogItemService;

        public CatalogItemController(ICatalogItemService service)
        {
            _catalogItemService = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllItems(CancellationToken cancellationToken)
        {
            var result = await _catalogItemService.GetAllItems(cancellationToken);
            return Ok(result);
        }
    }
}
