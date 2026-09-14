using PosHC.Application.DTOs;

namespace PosHC.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentDetailsDto> SavePayment(PaymentRequestDto input, CancellationToken cancellationToken = default);
        Task<PaymentDetailsDto> RefundAsync(Guid invoiceId, AdjustmentInput input, CancellationToken cancellationToken);
        Task<byte[]?> PrintReceiptAsync(Guid id, string language, CancellationToken cancellationToken);
    }
}
