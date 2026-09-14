using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;

namespace PosHCExternal.web.Controllers
{
    [ApiController]
    [Route("api/invoice")]
    [Authorize(Roles = "Administrator,Cashier,Receptionist")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPage(string search = "", Guid? patientId = null, int page = 1, CancellationToken cancellationToken = default, int pageSize = 50, string? sortBy = null, string? sortDirection = null)
        {
            var result = await _invoiceService.GetPageAsync(search, patientId, page, pageSize, sortBy, sortDirection, cancellationToken);
            return Ok(result);
        }
        [HttpGet("lookup")]
        public async Task<IActionResult> GetAllInvoices(CancellationToken cancellationToken)
        {
            var result = await _invoiceService.GetAllInvoicesDto(cancellationToken);
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> CreateInvoice(CreateInvoiceDto input, CancellationToken cancellationToken)
        {
            var result = await _invoiceService.SaveInvoiceAsync(input, cancellationToken);
            return Ok(result);
        }
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetDetails(Guid id, CancellationToken cancellationToken)
        {
            var result = await _invoiceService.GetDetailsAsync(id, cancellationToken);
            return Ok(result);
        }
        [HttpPost("{id:guid}/status")]
        [Authorize(Roles = "Administrator,Cashier")]
        public async Task<IActionResult> ChangeStatus(Guid id, InvoiceStatusInput input, CancellationToken cancellationToken)
        {
            var result = await _invoiceService.ChangeStatusAsync(id, input, cancellationToken);
            return Ok(result);
        }
        [HttpPost("{id:guid}/credits")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Credit(Guid id, AdjustmentInput input, CancellationToken cancellationToken)
        {
            var result = await _invoiceService.CreditAsync(id, input, cancellationToken);
            return Ok(result);
        }
        [HttpGet("{id:guid}/print")]
        public async Task<IActionResult> Print(Guid id, CancellationToken cancellationToken, [FromQuery] string language = "en")
        {
            var bytes = await _invoiceService.PrintAsync(id, language, cancellationToken);
            if (bytes == null)
            {
                return NotFound();
            }

            return File(bytes, "application/pdf", $"Invoice-{id}.pdf");
        }

    }
}
