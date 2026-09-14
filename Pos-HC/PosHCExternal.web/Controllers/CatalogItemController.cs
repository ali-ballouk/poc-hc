using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;

namespace PosHCExternal.web.Controllers
{
    [ApiController]
    [Route("api/catalogitem")]
    public class CatalogItemController : ControllerBase
    {
        private readonly ICatalogItemService _catalogItemService;

        public CatalogItemController(ICatalogItemService catalogItemService)
        {
            _catalogItemService = catalogItemService;
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> GetLookup(CancellationToken cancellationToken)
        {
            var result = await _catalogItemService.GetAllCatalogItemsAsync(cancellationToken);
            return Ok(result);
        }
        [HttpGet]
        [Authorize(Roles = "Administrator,Receptionist,Cashier,Doctor")]
        public async Task<IActionResult> GetPage(string search = "", int page = 1, CancellationToken cancellationToken = default, int pageSize = 50, string? sortBy = null, string? sortDirection = null)
        {
            var result = await _catalogItemService.GetPageAsync(search, page, cancellationToken, pageSize, sortBy, sortDirection);
            return Ok(result);
        }
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(CatalogItemDetailsDto input, CancellationToken cancellationToken)
        {
            var result = await _catalogItemService.SaveAsync(Guid.Empty, input, cancellationToken);
            return Ok(result);
        }
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Update(Guid id, CatalogItemDetailsDto input, CancellationToken cancellationToken)
        {
            var result = await _catalogItemService.SaveAsync(id, input, cancellationToken);
            return Ok(result);
        }

    }
}
