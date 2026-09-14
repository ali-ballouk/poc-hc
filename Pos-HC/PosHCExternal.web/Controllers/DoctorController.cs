using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;

namespace PosHCExternal.web.Controllers
{
    [ApiController]
    [Route("api/doctor")]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> GetLookup(CancellationToken cancellationToken)
        {
            var result = await _doctorService.GetAllDoctorInfo(cancellationToken);
            return Ok(result);
        }
        [HttpGet]
        [Authorize(Roles = "Administrator,Receptionist,Cashier,Doctor")]
        public async Task<IActionResult> GetPage(string search = "", int page = 1, CancellationToken cancellationToken = default, int pageSize = 50, string? sortBy = null, string? sortDirection = null)
        {
            var result = await _doctorService.GetPageAsync(search, page, cancellationToken, pageSize, sortBy, sortDirection);
            return Ok(result);
        }
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(DoctorDetailsDto input, CancellationToken cancellationToken)
        {
            var result = await _doctorService.SaveAsync(Guid.Empty, input, cancellationToken);
            return Ok(result);
        }
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Update(Guid id, DoctorDetailsDto input, CancellationToken cancellationToken)
        {
            var result = await _doctorService.SaveAsync(id, input, cancellationToken);
            return Ok(result);
        }
        [HttpGet("availability")]
        [Authorize(Roles = "Administrator,Receptionist,Doctor")]
        public async Task<IActionResult> GetAvailability(int page = 1, CancellationToken cancellationToken = default, int pageSize = 50, string? sortBy = null, string? sortDirection = null)
        {
            var result = await _doctorService.GetAvailabilityAsync(page, cancellationToken, pageSize, sortBy, sortDirection);
            return Ok(result);
        }
        [HttpPost("availability")]
        [Authorize(Roles = "Administrator,Receptionist")]
        public async Task<IActionResult> AddAvailability(DoctorAvailabilityDetailsDto input, CancellationToken cancellationToken)
        {
            var result = await _doctorService.SaveAvailabilityAsync(input, cancellationToken);
            return Ok(result);
        }

    }
}
