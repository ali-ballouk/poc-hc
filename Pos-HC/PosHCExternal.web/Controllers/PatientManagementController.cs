using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;

namespace PosHCExternal.web.Controllers;

[ApiController]
[Route("api/clinic/patients")]
public class PatientManagementController(IPatientService patients) : ControllerBase
{
    [HttpGet, Authorize(Roles = "Administrator,Receptionist,Doctor")]
    public Task<PagedResult<Patient>> GetPage(string search = "", int page = 1, CancellationToken ct = default)
        => patients.GetPageAsync(search, page, ct);

    [HttpPost, Authorize(Roles = "Administrator,Receptionist")]
    public Task<Patient> Create(Patient input, CancellationToken ct)
        => patients.SaveAsync(Guid.Empty, input, ct);

    [HttpPut("{id:guid}"), Authorize(Roles = "Administrator,Receptionist")]
    public Task<Patient> Update(Guid id, Patient input, CancellationToken ct)
        => patients.SaveAsync(id, input, ct);
}
