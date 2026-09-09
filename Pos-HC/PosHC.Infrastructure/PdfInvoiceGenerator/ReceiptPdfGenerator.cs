using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
namespace PosHC.Infrastructure.Pdf;
public class ReceiptPdfGenerator : IReceiptPdfGenerator
{
    public byte[] Generate(Payment payment, Invoice invoice) => Document.Create(document => document.Page(page =>
    {
        page.Size(PageSizes.A5);
        page.Margin(25);
        page.DefaultTextStyle(x => x.FontSize(11));
        page.Content().Column(column =>
        {
            column.Spacing(10);
            column.Item().Text(invoice.ClinicName).FontSize(18).Bold();
            column.Item().Text(payment.Kind == "Refund" ? "REFUND RECEIPT" : "PAYMENT RECEIPT").Bold();
            column.Item().Text($"Receipt: {payment.Id}");
            column.Item().Text($"Invoice: {invoice.Number}");
            column.Item().Text($"Patient: {invoice.PatientName}");
            column.Item().Text($"Date (UTC): {payment.PaymentDate:yyyy-MM-dd HH:mm}");
            column.Item().Text($"Amount: {Math.Abs(payment.Amount):N2} {payment.Currency}").FontSize(16).Bold();
            column.Item().Text($"Method: {payment.PaymentTypeId switch { 1 => "Cash", 2 => "Card terminal", 3 => "Bank transfer", _ => "Deferred" }}");
            column.Item().Text($"Reference: {payment.Reference}");
        });
    })).GeneratePdf();
}
