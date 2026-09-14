using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.DTOs;
using PosHC.Application.Services;

namespace PosHCExternal.web.Controllers;

[ApiController, Route("api/clinic/patients/{patientId:guid}/visits")]
[Authorize(Roles = "Administrator,Receptionist,Doctor")]
public class PatientVisitsController(PatientVisitService visits) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<PatientVisitDto>> GetPage(Guid patientId, int page = 1, int pageSize = 20, CancellationToken ct = default, string? sortBy = null, string? sortDirection = null)
        => visits.GetPageAsync(patientId, page, pageSize, ct, sortBy, sortDirection);

    [HttpPut("{visitId:guid}")]
    public Task<PatientVisitDto> Update(Guid patientId, Guid visitId, VisitNotesInput input, CancellationToken ct)
        => visits.UpdateAsync(patientId, visitId, input, ct);
}
