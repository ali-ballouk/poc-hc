using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;

namespace PosHCExternal.web.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,Cashier")]
        public async Task<IActionResult> SavePayment(PaymentRequestDto input, CancellationToken cancellationToken)
        {
            var result = await _paymentService.SavePayment(input, cancellationToken);
            return Ok(result);
        }
        [HttpPost("invoice/{invoiceId:guid}/refunds")]
        [Authorize(Roles = "Administrator,Cashier")]
        public async Task<IActionResult> Refund(Guid invoiceId, AdjustmentInput input, CancellationToken cancellationToken)
        {
            var result = await _paymentService.RefundAsync(invoiceId, input, cancellationToken);
            return Ok(result);
        }
        [HttpGet("{id:guid}/receipt")]
        [Authorize(Roles = "Administrator,Cashier,Receptionist")]
        public async Task<IActionResult> Receipt(Guid id, CancellationToken cancellationToken, [FromQuery] string language = "en")
        {
            var bytes = await _paymentService.PrintReceiptAsync(id, language, cancellationToken);
            if (bytes == null)
            {
                return NotFound();
            }

            return File(bytes, "application/pdf", $"Receipt-{id}.pdf");
        }

    }
}
