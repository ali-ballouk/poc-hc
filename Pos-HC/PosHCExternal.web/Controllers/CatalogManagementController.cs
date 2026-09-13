using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;

namespace PosHCExternal.web.Controllers;

[ApiController]
[Route("api/clinic/catalog")]
public class CatalogManagementController(ICatalogItemService catalog) : ControllerBase
{
    [HttpGet, Authorize(Roles = "Administrator,Receptionist,Cashier,Doctor")]
    public Task<PagedResult<CatalogItem>> GetPage(string search = "", int page = 1, CancellationToken ct = default, int pageSize = 50)
        => catalog.GetPageAsync(search, page, ct, pageSize);

    [HttpPost, Authorize(Roles = "Administrator")]
    public Task<CatalogItem> Create(CatalogItem input, CancellationToken ct)
        => catalog.SaveAsync(Guid.Empty, input, ct);

    [HttpPut("{id:guid}"), Authorize(Roles = "Administrator")]
    public Task<CatalogItem> Update(Guid id, CatalogItem input, CancellationToken ct)
        => catalog.SaveAsync(id, input, ct);
}
