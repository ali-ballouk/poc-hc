// Controllers/DoctorsController.cs
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

        public PaymentController(IPaymentService service)
        {
            _paymentService = service;
        }

        [HttpPost]
        public async Task<IActionResult> SavePayment(PaymentRequestDto paymentRequestDto, CancellationToken cancellationToken)
        {
            var result = await _paymentService.SavePayment(paymentRequestDto);
            return Ok(result);
        }
    }
}
