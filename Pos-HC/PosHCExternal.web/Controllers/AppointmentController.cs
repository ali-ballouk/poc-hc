using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;

namespace PosHCExternal.web.Controllers;

[ApiController, Route("api/clinic/appointments")]
public class AppointmentController(IAppointmentService appointments) : ControllerBase
{
    [HttpGet, Authorize(Roles = "Administrator,Receptionist,Doctor")]
    public Task<PagedResult<Appointment>> GetPage(DateTime? from = null, DateTime? to = null, int page = 1, CancellationToken ct = default, int pageSize = 50, string? sortBy = null, string? sortDirection = null)
        => appointments.GetPageAsync(from, to, page, ct, pageSize, sortBy, sortDirection);

    [HttpPost, Authorize(Roles = "Administrator,Receptionist")]
    public Task<Appointment> Create(Appointment input, CancellationToken ct)
        => appointments.SaveAsync(Guid.Empty, input, ct);

    [HttpPut("{id:guid}"), Authorize(Roles = "Administrator,Receptionist,Doctor")]
    public Task<Appointment> Update(Guid id, Appointment input, CancellationToken ct)
        => appointments.SaveAsync(id, input, ct);
}
