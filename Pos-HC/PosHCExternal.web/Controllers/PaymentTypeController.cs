// Controllers/DoctorsController.cs
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.Interfaces;
namespace PosHCExternal.web.Controllers
{
    [ApiController]
    [Route("api/paymenttype")]
    public class PaymentTypeController : ControllerBase
    {
        private readonly IPaymentTypeService _paymentTypeService;

        public PaymentTypeController(IPaymentTypeService service)
        {
            _paymentTypeService = service;
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> GetLookup(CancellationToken cancellationToken)
        {
            var result = await _paymentTypeService.GetPaymentTypeLookupDtos();
            return Ok(result);
        }
    }
}
