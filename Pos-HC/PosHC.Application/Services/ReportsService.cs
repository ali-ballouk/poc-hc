using PosHC.Application.Exceptions;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Application.Mapping;

namespace PosHC.Application.Services
{
    public class ReportsService : IReportsService
    {
        private readonly IClinicReportReader _reader;
        private readonly IAuditService _auditService;

        public ReportsService(IClinicReportReader reader, IAuditService auditService)
        {
            _reader = reader;
            _auditService = auditService;
        }

        public async Task<ClinicReportDto> SummaryAsync(DateTime from, DateTime to, CancellationToken cancellationToken)
        {
            if (to <= from || to - from > TimeSpan.FromDays(366))
            {
                throw new BusinessException("Choose a report period of at most 366 days.");
            }

            var data = await _reader.ReadAsync(from, to, cancellationToken);
            var summary = new List<CurrencyReportDto>();
            foreach (var currency in new[] { "USD", "LBP" })
            {
                var invoices = data.Invoices.Where(invoice => invoice.Currency == currency).ToList();
                var payments = data.Payments.Where(payment => payment.Currency == currency).ToList();
                var outstanding = data.Outstanding.Where(invoice => invoice.Currency == currency)
                    .Sum(invoice => Math.Max(0, invoice.Total - data.Paid.GetValueOrDefault(invoice.Id) - data.Credits.GetValueOrDefault(invoice.Id)));
                summary.Add(new CurrencyReportDto(currency, invoices.Count, invoices.Sum(invoice => invoice.Total),
                    invoices.Sum(invoice => invoice.Discount ?? 0), invoices.Sum(invoice => invoice.Tax),
                    payments.Where(payment => payment.Amount > 0).Sum(payment => payment.Amount),
                    -payments.Where(payment => payment.Amount < 0).Sum(payment => payment.Amount),
                    data.PeriodCredits.Where(credit => credit.Currency == currency).Sum(credit => credit.Amount), outstanding));
            }

            var doctors = data.Invoices.GroupBy(invoice => new { invoice.DoctorId, invoice.DoctorName, invoice.Currency })
                .Select(group => new DoctorReportDto(group.Key.DoctorName, group.Key.Currency, group.Count(),
                    group.Sum(invoice => invoice.Total), group.Sum(invoice => invoice.DoctorFee))).ToList();
            var services = data.Invoices.SelectMany(invoice => invoice.Items.Select(item => new { invoice.Currency, item.Name, item.Quantity, item.LineTotal }))
                .GroupBy(item => new { item.Currency, item.Name })
                .Select(group => new ServiceReportDto(group.Key.Name, group.Key.Currency,
                    group.Sum(item => item.Quantity), group.Sum(item => item.LineTotal))).ToList();
            var methods = data.Payments.GroupBy(payment => new { payment.Currency, payment.PaymentTypeId })
                .Select(group => new PaymentMethodReportDto(group.Key.Currency,
                    group.Key.PaymentTypeId switch
                    {
                        1 => "Cash",
                        2 => "Card terminal",
                        3 => "Bank transfer",
                        _ => "Deferred"
                    },
                    group.Sum(payment => payment.Amount))).ToList();
            return new ClinicReportDto(from, to, summary, doctors, services, methods);
        }

        public async Task<ClinicExportDto> ExportAsync(CancellationToken cancellationToken)
        {
            await _auditService.RecordAndSaveAsync("Export", "Clinic", 1, cancellationToken);
            var data = await _reader.ReadExportAsync(cancellationToken);
            return new ClinicExportDto
            {
                ExportedAt = DateTime.UtcNow,
                Patients = data.Patients.Select(ClinicDtoMapper.ToDto).ToList(),
                Doctors = data.Doctors.Select(ClinicDtoMapper.ToDto).ToList(),
                Catalog = data.Catalog.Select(ClinicDtoMapper.ToDto).ToList(),
                Invoices = data.Invoices.Select(ClinicDtoMapper.ToDto).ToList(),
                InvoiceItems = data.InvoiceItems.Select(ClinicDtoMapper.ToDto).ToList(),
                Payments = data.Payments.Select(ClinicDtoMapper.ToDto).ToList(),
                CreditNotes = data.CreditNotes.Select(ClinicDtoMapper.ToDto).ToList(),
                Appointments = data.Appointments.Select(ClinicDtoMapper.ToDto).ToList(),
                Availability = data.Availability.Select(ClinicDtoMapper.ToDto).ToList(),
                CashShifts = data.CashShifts.Select(ClinicDtoMapper.ToDto).ToList(),
                CashMovements = data.CashMovements.Select(ClinicDtoMapper.ToDto).ToList(),
                Settings = data.Settings.Select(ClinicDtoMapper.ToDto).ToList(),
                Audit = data.Audit.Select(ClinicDtoMapper.ToDto).ToList()
            };
        }
    }
}
