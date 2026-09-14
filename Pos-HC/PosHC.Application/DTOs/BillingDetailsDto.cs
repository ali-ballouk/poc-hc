namespace PosHC.Application.DTOs
{
    public class InvoiceListRow
    {
        public Guid Id
        {
            get; set;
        }
        public long Number
        {
            get; set;
        }
        public DateTime CreatedAt
        {
            get; set;
        }
        public string PatientName { get; set; } = "";
        public string DoctorName { get; set; } = "";
        public string Currency { get; set; } = "";
        public string Status { get; set; } = "";
        public decimal Total
        {
            get; set;
        }
        public decimal Paid
        {
            get; set;
        }
        public decimal Credits
        {
            get; set;
        }
        public decimal Balance
        {
            get; set;
        }
        public string PaymentStatus { get; set; } = "";
    }

    public record InvoiceBalanceDto(InvoiceDetailsDto Invoice, decimal Paid, decimal Credits,
        decimal Balance, string PaymentStatus);

    public record InvoiceHistoryDto(InvoiceBalanceDto Summary, List<PaymentDetailsDto> Payments,
        List<CreditNoteDetailsDto> CreditNotes);

    public record InvoiceStatusInput(string Status, string Reason = "");
    public record AdjustmentInput(decimal Amount, string Reason, Guid RequestId, int PaymentTypeId = 1);
}
