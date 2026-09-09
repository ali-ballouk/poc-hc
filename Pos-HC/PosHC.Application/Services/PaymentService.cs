using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
namespace PosHC.Application.Services;
public class PaymentService(BillingService billing) : IPaymentService
{
    public async Task<PaymentResultDto> SavePayment(PaymentRequestDto input, CancellationToken cancellationToken = default)
    {
        var payment = await billing.Collect(input, cancellationToken);
        return new PaymentResultDto { Id = payment.Id, InvoiceId = payment.InvoiceId, PaymentDate = payment.PaymentDate, PaymentTypeId = payment.PaymentTypeId };
    }
}
