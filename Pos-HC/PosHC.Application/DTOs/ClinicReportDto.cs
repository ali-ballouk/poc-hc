namespace PosHC.Application.DTOs
{
    public record CurrencyReportDto(string Currency, int InvoiceCount, decimal Invoiced, decimal Discounts,
        decimal Tax, decimal Collected, decimal Refunded, decimal Credits, decimal Outstanding);
    public record DoctorReportDto(string DoctorName, string Currency, int Invoices, decimal Billed, decimal ConsultationFees);
    public record ServiceReportDto(string Name, string Currency, int Quantity, decimal GrossSales);
    public record PaymentMethodReportDto(string Currency, string Method, decimal NetCollected);
    public record ClinicReportDto(DateTime From, DateTime To, List<CurrencyReportDto> Summary,
        List<DoctorReportDto> ByDoctor, List<ServiceReportDto> ByService, List<PaymentMethodReportDto> ByPaymentMethod);
}
