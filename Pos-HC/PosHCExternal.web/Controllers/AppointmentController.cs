using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;

namespace PosHCExternal.web.Controllers
{
    [ApiController]
    [Route("api/appointments")]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet]
        [Authorize(Roles = "Administrator,Receptionist,Doctor")]
        public async Task<IActionResult> GetPage(DateTime? from = null, DateTime? to = null, int page = 1, CancellationToken cancellationToken = default, int pageSize = 50, string? sortBy = null, string? sortDirection = null)
        {
            var result = await _appointmentService.GetPageAsync(from, to, page, cancellationToken, pageSize, sortBy, sortDirection);
            return Ok(result);
        }
        [HttpPost]
        [Authorize(Roles = "Administrator,Receptionist")]
        public async Task<IActionResult> Create(AppointmentDetailsDto input, CancellationToken cancellationToken)
        {
            var result = await _appointmentService.SaveAsync(Guid.Empty, input, cancellationToken);
            return Ok(result);
        }
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Administrator,Receptionist,Doctor")]
        public async Task<IActionResult> Update(Guid id, AppointmentDetailsDto input, CancellationToken cancellationToken)
        {
            var result = await _appointmentService.SaveAsync(id, input, cancellationToken);
            return Ok(result);
        }

    }
}
