namespace PosHC.Application.DTOs
{
    public record ReceiptGenerateDto(Guid Id, long InvoiceNumber, string PatientName, string ClinicName,
        DateTime PaymentDate, decimal Amount, string Currency, string Kind, int PaymentTypeId, string Reference);
}
