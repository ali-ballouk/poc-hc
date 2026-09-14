using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;

namespace PosHCExternal.web.Controllers;

[ApiController, Route("api/clinic/staff"), Authorize(Roles = "Administrator")]
public class StaffController(IStaffService staff) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<StaffUser>> GetPage(int page = 1, CancellationToken ct = default, int pageSize = 50, string? sortBy = null, string? sortDirection = null)
        => staff.GetPageAsync(page, ct, pageSize, sortBy, sortDirection);

    [HttpPost]
    public Task<StaffUser> Create(StaffInput input, CancellationToken ct)
        => staff.Save(Guid.Empty, input, ct);

    [HttpPut("{id:guid}")]
    public Task<StaffUser> Update(Guid id, StaffInput input, CancellationToken ct)
        => staff.Save(id, input, ct);

    [HttpPost("{id:guid}/reset")]
    public async Task<object> ResetPassword(Guid id, CancellationToken ct)
        => new
        {
            Token = await staff.IssueReset(id, ct),
            ExpiresInMinutes = 30
        };
}
