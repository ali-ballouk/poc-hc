using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;

namespace PosHCExternal.web.Controllers;

[ApiController, Route("api/clinic/settings")]
public class ClinicSettingsController(IClinicSettingsService settings) : ControllerBase
{
    [HttpGet]
    public Task<ClinicSettings> Get(CancellationToken ct) => settings.GetAsync(ct);

    [HttpPut, Authorize(Roles = "Administrator")]
    public Task<ClinicSettings> Update(ClinicSettings input, CancellationToken ct)
        => settings.SaveAsync(input, ct);
}
