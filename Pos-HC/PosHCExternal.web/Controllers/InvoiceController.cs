// Controllers/DoctorsController.cs
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Application.Invoices.Queries;
using PosHC.Application.Services;
namespace PosHCExternal.web.Controllers
{
    [ApiController]
    [Route("api/invoice")]
    public class InvoiceControllerController : ControllerBase
    {
        private readonly IInvoiceService _invoiceServiceService;

        private readonly InvoiceForPrintService _invoiceForPrintService;
        private readonly IInvoicePdfGenerator _iInvoicePdfGenerator;

        public InvoiceControllerController(IInvoiceService service, InvoiceForPrintService invoiceForPrintService, IInvoicePdfGenerator invoicePdfGenerator)
        {
            _invoiceServiceService = service;
            _invoiceForPrintService = invoiceForPrintService;
            _iInvoicePdfGenerator = invoicePdfGenerator;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllInvoice( CancellationToken cancellationToken)
        {
            var result = await _invoiceServiceService.GetAllInvoicesDto(cancellationToken);
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> CreatInvoice(CreateInvoiceDto invoice,CancellationToken cancellationToken)
        {
            var result = await _invoiceServiceService.SaveInvoiceAsync(invoice, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}/print")]
        public async Task<IActionResult> Print(Guid id, CancellationToken ct)
        {
            var dto = await _invoiceForPrintService.GetInvoice(id, ct);
            if (dto is null) return NotFound();

            var bytes = _iInvoicePdfGenerator.GenerateInvoicePdf(dto);
            var fileName = $"Invoice-{dto.Id}.pdf";

            Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileName}\"");
            return File(bytes, "application/pdf", fileName);
        }
    }
}
