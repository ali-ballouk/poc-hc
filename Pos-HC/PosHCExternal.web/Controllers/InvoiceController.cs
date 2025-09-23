// Controllers/DoctorsController.cs
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Application.Services;
namespace PosHCExternal.web.Controllers
{
    [ApiController]
    [Route("api/invoice")]
    public class InvoiceControllerController : ControllerBase
    {
        private readonly IInvoiceService _invoiceServiceService;

        public InvoiceControllerController(IInvoiceService service)
        {
            _invoiceServiceService = service;
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
    }
}
