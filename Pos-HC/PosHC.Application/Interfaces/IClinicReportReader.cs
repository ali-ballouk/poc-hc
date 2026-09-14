using PosHC.Domain.Entities;

namespace PosHC.Application.Interfaces
{
    public record CreditReportRow(string Currency, decimal Amount);
    public record ClinicReportData(List<Invoice> Invoices, List<Payment> Payments, List<Invoice> Outstanding,
        Dictionary<Guid, decimal> Paid, Dictionary<Guid, decimal> Credits, List<CreditReportRow> PeriodCredits);

    public class ClinicExportData
    {
        public List<Patient> Patients { get; set; } = new();
        public List<Doctor> Doctors { get; set; } = new();
        public List<CatalogItem> Catalog { get; set; } = new();
        public List<Invoice> Invoices { get; set; } = new();
        public List<InvoiceItem> InvoiceItems { get; set; } = new();
        public List<Payment> Payments { get; set; } = new();
        public List<CreditNote> CreditNotes { get; set; } = new();
        public List<Appointment> Appointments { get; set; } = new();
        public List<DoctorAvailability> Availability { get; set; } = new();
        public List<CashShift> CashShifts { get; set; } = new();
        public List<CashMovement> CashMovements { get; set; } = new();
        public List<ClinicSettings> Settings { get; set; } = new();
        public List<AuditEntry> Audit { get; set; } = new();
    }

    public interface IClinicReportReader
    {
        Task<ClinicReportData> ReadAsync(DateTime from, DateTime to, CancellationToken cancellationToken);
        Task<ClinicExportData> ReadExportAsync(CancellationToken cancellationToken);
    }
}
