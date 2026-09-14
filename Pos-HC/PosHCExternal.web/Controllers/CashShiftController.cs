using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;

namespace PosHCExternal.web.Controllers
{
    [ApiController]
    [Route("api/shifts")]
    [Authorize(Roles = "Administrator,Cashier")]
    public class CashShiftController : ControllerBase
    {
        private readonly ICashShiftService _cashShiftService;

        public CashShiftController(ICashShiftService cashShiftService)
        {
            _cashShiftService = cashShiftService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPage(int page = 1, CancellationToken cancellationToken = default, int pageSize = 50, string? sortBy = null, string? sortDirection = null)
        {
            var result = await _cashShiftService.GetPageAsync(page, cancellationToken, pageSize, sortBy, sortDirection);
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> Open(OpenShiftInput input, CancellationToken cancellationToken)
        {
            var result = await _cashShiftService.OpenShift(input.Currency, input.OpeningAmount, cancellationToken);
            return Ok(result);
        }
        [HttpPost("{id:guid}/close")]
        public async Task<IActionResult> Close(Guid id, CashMovementInput input, CancellationToken cancellationToken)
        {
            var result = await _cashShiftService.CloseShift(id, input.Amount, cancellationToken);
            return Ok(result);
        }
        [HttpPost("{id:guid}/movements")]
        public async Task<IActionResult> MoveCash(Guid id, CashMovementInput input, CancellationToken cancellationToken)
        {
            var result = await _cashShiftService.MoveCash(id, input.Amount, input.Reason, cancellationToken);
            return Ok(result);
        }
        [HttpGet("{id:guid}/movements")]
        public async Task<IActionResult> GetMovements(Guid id, CancellationToken cancellationToken)
        {
            var result = await _cashShiftService.GetMovementsAsync(id, cancellationToken);
            return Ok(result);
        }

    }
}
