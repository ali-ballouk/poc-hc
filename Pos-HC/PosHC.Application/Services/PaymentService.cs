using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Application.Mapping;
using PosHC.Domain.Entities;

namespace PosHC.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IBillingService _billingService;
        private readonly IClinicStore _store;
        private readonly IClinicSettingsService _settingsService;
        private readonly IReceiptPdfGenerator _receiptPdfGenerator;

        public PaymentService(IBillingService billingService, IClinicStore store,
            IClinicSettingsService settingsService, IReceiptPdfGenerator receiptPdfGenerator)
        {
            _billingService = billingService;
            _store = store;
            _settingsService = settingsService;
            _receiptPdfGenerator = receiptPdfGenerator;
        }

        public async Task<PaymentDetailsDto> SavePayment(PaymentRequestDto input, CancellationToken cancellationToken = default)
        {
            var payment = await _billingService.Collect(input, cancellationToken);
            return ClinicDtoMapper.ToDto(payment);
        }

        public async Task<PaymentDetailsDto> RefundAsync(Guid invoiceId, AdjustmentInput input, CancellationToken cancellationToken)
        {
            var payment = await _billingService.Refund(invoiceId, input.Amount, input.PaymentTypeId,
                input.Reason, input.RequestId, cancellationToken);
            return ClinicDtoMapper.ToDto(payment);
        }

        public async Task<byte[]?> PrintReceiptAsync(Guid id, string language, CancellationToken cancellationToken)
        {
            var payment = await _store.Find<Payment>(row => row.Id == id, cancellationToken);
            if (payment == null)
            {
                return null;
            }

            var invoice = await _billingService.Invoice(payment.InvoiceId, cancellationToken);
            var settings = await _settingsService.GetAsync(cancellationToken);
            var receipt = new ReceiptGenerateDto(payment.Id, invoice.Number, invoice.PatientName, settings.Name,
                payment.PaymentDate, payment.Amount, payment.Currency, payment.Kind, payment.PaymentTypeId, payment.Reference);
            return _receiptPdfGenerator.Generate(receipt, language);
        }
    }
}
