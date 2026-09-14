using PosHC.Application.DTOs;
using PosHC.Domain.Entities;

namespace PosHC.Application.Interfaces
{
    public record InvoiceBalance(Invoice Invoice, decimal Paid, decimal Credits, decimal Balance, string PaymentStatus);

    public interface IBillingService
    {
        Task<Invoice> Invoice(Guid id, CancellationToken cancellationToken);
        Task<InvoiceBalance> Balance(Guid id, CancellationToken cancellationToken);
        Task<Invoice> Create(CreateInvoiceDto input, CancellationToken cancellationToken);
        Task<Invoice> ChangeStatus(Guid id, string status, string reason, CancellationToken cancellationToken);
        Task<Payment> Collect(PaymentRequestDto input, CancellationToken cancellationToken);
        Task<CreditNote> Credit(Guid id, decimal amount, string reason, Guid requestId, CancellationToken cancellationToken);
        Task<Payment> Refund(Guid id, decimal amount, int method, string reason, Guid requestId, CancellationToken cancellationToken);
    }
}
