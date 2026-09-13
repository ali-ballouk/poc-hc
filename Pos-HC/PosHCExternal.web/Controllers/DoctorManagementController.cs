using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;

namespace PosHCExternal.web.Controllers;

[ApiController]
[Route("api/clinic/doctors")]
public class DoctorManagementController(IDoctorService doctors) : ControllerBase
{
    [HttpGet, Authorize(Roles = "Administrator,Receptionist,Cashier,Doctor")]
    public Task<PagedResult<Doctor>> GetPage(string search = "", int page = 1, CancellationToken ct = default, int pageSize = 50)
        => doctors.GetPageAsync(search, page, ct, pageSize);

    [HttpPost, Authorize(Roles = "Administrator")]
    public Task<Doctor> Create(Doctor input, CancellationToken ct)
        => doctors.SaveAsync(Guid.Empty, input, ct);

    [HttpPut("{id:guid}"), Authorize(Roles = "Administrator")]
    public Task<Doctor> Update(Guid id, Doctor input, CancellationToken ct)
        => doctors.SaveAsync(id, input, ct);

    [HttpGet("/api/clinic/availability"), Authorize(Roles = "Administrator,Receptionist,Doctor")]
    public Task<PagedResult<DoctorAvailability>> GetAvailability(int page = 1, CancellationToken ct = default, int pageSize = 50)
        => doctors.GetAvailabilityAsync(page, ct, pageSize);

    [HttpPost("/api/clinic/availability"), Authorize(Roles = "Administrator,Receptionist")]
    public Task<DoctorAvailability> AddAvailability(DoctorAvailability input, CancellationToken ct)
        => doctors.SaveAvailabilityAsync(input, ct);
}
