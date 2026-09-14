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
    public Task<PagedResult<CatalogItem>> GetPage(string search = "", int page = 1, CancellationToken ct = default, int pageSize = 50, string? sortBy = null, string? sortDirection = null)
        => catalog.GetPageAsync(search, page, ct, pageSize, sortBy, sortDirection);

    [HttpPost, Authorize(Roles = "Administrator")]
    public Task<CatalogItem> Create(CatalogItem input, CancellationToken ct)
        => catalog.SaveAsync(Guid.Empty, input, ct);

    [HttpPut("{id:guid}"), Authorize(Roles = "Administrator")]
    public Task<CatalogItem> Update(Guid id, CatalogItem input, CancellationToken ct)
        => catalog.SaveAsync(id, input, ct);
}
