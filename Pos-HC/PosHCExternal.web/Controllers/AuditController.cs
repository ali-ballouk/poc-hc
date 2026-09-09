using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;

namespace PosHCExternal.web.Controllers;

[ApiController, Route("api/clinic/audit"), Authorize(Roles = "Administrator")]
public class AuditController(IAuditService audit) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<AuditEntry>> GetPage(string search = "", int page = 1, CancellationToken ct = default)
        => audit.GetPageAsync(search, page, ct);
}
