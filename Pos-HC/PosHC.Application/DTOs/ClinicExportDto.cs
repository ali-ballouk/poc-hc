namespace PosHC.Application.DTOs
{
    public class ClinicExportDto
    {
        public DateTime ExportedAt
        {
            get; set;
        }
        public int Version { get; set; } = 1;
        public List<PatientDetailsDto> Patients { get; set; } = new();
        public List<DoctorDetailsDto> Doctors { get; set; } = new();
        public List<CatalogItemDetailsDto> Catalog { get; set; } = new();
        public List<InvoiceDetailsDto> Invoices { get; set; } = new();
        public List<InvoiceLineDto> InvoiceItems { get; set; } = new();
        public List<PaymentDetailsDto> Payments { get; set; } = new();
        public List<CreditNoteDetailsDto> CreditNotes { get; set; } = new();
        public List<AppointmentDetailsDto> Appointments { get; set; } = new();
        public List<DoctorAvailabilityDetailsDto> Availability { get; set; } = new();
        public List<CashShiftDetailsDto> CashShifts { get; set; } = new();
        public List<CashMovementDetailsDto> CashMovements { get; set; } = new();
        public List<ClinicSettingsDetailsDto> Settings { get; set; } = new();
        public List<AuditEntryDetailsDto> Audit { get; set; } = new();
    }
}
