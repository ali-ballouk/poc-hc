using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;

namespace PosHCExternal.web.Controllers
{
    [ApiController]
    [Route("api/staff")]
    [Authorize(Roles = "Administrator")]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;

        public StaffController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPage(int page = 1, CancellationToken cancellationToken = default, int pageSize = 50, string? sortBy = null, string? sortDirection = null)
        {
            var result = await _staffService.GetPageAsync(page, cancellationToken, pageSize, sortBy, sortDirection);
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> Create(StaffInput input, CancellationToken cancellationToken)
        {
            var result = await _staffService.Save(Guid.Empty, input, cancellationToken);
            return Ok(result);
        }
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, StaffInput input, CancellationToken cancellationToken)
        {
            var result = await _staffService.Save(id, input, cancellationToken);
            return Ok(result);
        }

        [HttpPost("{id:guid}/reset")]
        public async Task<IActionResult> ResetPassword(Guid id, CancellationToken cancellationToken)
        {
            var token = await _staffService.IssueReset(id, cancellationToken);
            return Ok(new PasswordResetDto(token));
        }

    }
}
