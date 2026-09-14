using PosHC.Application.Mapping;
using PosHC.Application.Services;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;
namespace PosHC.Application.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IBillingService _billing;
        private readonly IClinicStore _store;
        private readonly IInvoiceReader _reader;
        private readonly IInvoiceForPrintService _printService;
        private readonly IInvoicePdfGenerator _pdfGenerator;

        public InvoiceService(IBillingService billing, IClinicStore store, IInvoiceReader reader, IInvoiceForPrintService printService, IInvoicePdfGenerator pdfGenerator)
        {
            _billing = billing;
            _store = store;
            _reader = reader;
            _printService = printService;
            _pdfGenerator = pdfGenerator;
        }

        public async Task<InvoiceResultDto> SaveInvoiceAsync(CreateInvoiceDto dto, CancellationToken cancellationToken = default)
        {
            var invoice = await _billing.Create(dto, cancellationToken);
            return new InvoiceResultDto { InvoiceId = invoice.Id, InvoiceDate = invoice.CreatedAt, Total = invoice.Total };
        }
        public async Task<List<InvoiceDto>> GetAllInvoicesDto(CancellationToken cancellationToken = default)
        {
            var invoices = await _store.List<Invoice>(limit: 500, ct: cancellationToken);
            var result = new List<InvoiceDto>();
            foreach (var row in invoices)
            {
                var invoice = await _billing.Invoice(row.Id, cancellationToken);
                result.Add(new InvoiceDto { InvoiceId = row.Id, InvoiceDate = row.CreatedAt, DoctorId = row.DoctorId, DoctorName = row.DoctorName, PatientId = row.PatientId, PatientName = row.PatientName, Discount = row.Discount, DoctorFee = row.DoctorFee, Total = invoice.Total });
            }
            return result.OrderByDescending(x => x.InvoiceDate).ToList();
        }

        public Task<PagedResult<InvoiceListRow>> GetPageAsync(string search, Guid? patientId, int page,
            int pageSize, string? sortBy, string? sortDirection, CancellationToken cancellationToken)
        {
            return _reader.GetPageAsync(search, patientId, page, pageSize, sortBy, sortDirection, cancellationToken);
        }

        public async Task<InvoiceHistoryDto> GetDetailsAsync(Guid id, CancellationToken cancellationToken)
        {
            var balance = await _billing.Balance(id, cancellationToken);
            var payments = await _store.List<Payment>(payment => payment.InvoiceId == id, int.MaxValue, ct: cancellationToken);
            var credits = await _store.List<CreditNote>(credit => credit.InvoiceId == id, int.MaxValue, ct: cancellationToken);
            var summary = new InvoiceBalanceDto(ClinicDtoMapper.ToDto(balance.Invoice), balance.Paid,
                balance.Credits, balance.Balance, balance.PaymentStatus);
            return new InvoiceHistoryDto(summary, payments.Select(ClinicDtoMapper.ToDto).ToList(), credits.Select(ClinicDtoMapper.ToDto).ToList());
        }

        public async Task<InvoiceDetailsDto> ChangeStatusAsync(Guid id, InvoiceStatusInput input, CancellationToken cancellationToken)
        {
            var invoice = await _billing.ChangeStatus(id, input.Status, input.Reason, cancellationToken);
            return ClinicDtoMapper.ToDto(invoice);
        }

        public async Task<CreditNoteDetailsDto> CreditAsync(Guid id, AdjustmentInput input, CancellationToken cancellationToken)
        {
            var credit = await _billing.Credit(id, input.Amount, input.Reason, input.RequestId, cancellationToken);
            return ClinicDtoMapper.ToDto(credit);
        }

        public async Task<byte[]?> PrintAsync(Guid id, string language, CancellationToken cancellationToken)
        {
            var invoice = await _printService.GetInvoice(id, cancellationToken);
            return invoice == null ? null : _pdfGenerator.GenerateInvoicePdf(invoice, language);
        }
    }
}
