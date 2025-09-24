using PosHC.Domain.Entities;

namespace PosHC.Application.Interfaces
{

    public interface IPOSHCRepository
    {
        Task<List<Doctor>> GetAllDoctorsAsync(CancellationToken cancellationToken = default);

        Task<List<Patient>> GetAllPatientsAsync(CancellationToken cancellationToken = default);
        Task<List<CatalogItem>> GetAllItemsAsync(CancellationToken cancellationToken = default);

        Task<Invoice> SaveInvoice(Invoice invoice, CancellationToken cancellationToken = default);

        Task<List<Invoice>> GetAllInvoiceAsync(CancellationToken cancellationToken = default);

        Task<List<PaymentType>> GetAllPaymentTypeAsync(CancellationToken cancellationToken = default);

        Task<Payment> SavePayment(Payment payment, CancellationToken cancellationToken = default);

        Task<List<Payment>> GetAllPaymentAsync(CancellationToken cancellationToken = default);

        Task<Invoice> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default);

    }
}
