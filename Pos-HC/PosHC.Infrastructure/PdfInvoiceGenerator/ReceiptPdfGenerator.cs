using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
namespace PosHC.Infrastructure.Pdf;
public class ReceiptPdfGenerator : IReceiptPdfGenerator
{
    public byte[] Generate(Payment payment, Invoice invoice, string language = "en")
    {
        var l = new PdfLanguage(language);
        return Document.Create(document => document.Page(page =>
    {
        page.Size(PageSizes.A5);
        page.Margin(25);
        l.Configure(page);
        page.Content().Column(column =>
        {
            column.Spacing(10);
            column.Item().Text(invoice.ClinicName).FontSize(20).Bold().FontColor("#087F82");
            column.Item().BorderBottom(2).BorderColor("#087F82").PaddingBottom(12).Text(payment.Kind == "Refund" ? l.Text("REFUND RECEIPT", "إيصال استرداد") : l.Text("PAYMENT RECEIPT", "إيصال دفع")).Bold();
            column.Item().Text($"{l.Text("Receipt", "الإيصال")}: {payment.Id}");
            column.Item().Text($"{l.Text("Invoice", "الفاتورة")}: {invoice.Number}");
            column.Item().Text($"{l.Text("Patient", "المريض")}: {invoice.PatientName}");
            column.Item().Text($"{l.Text("Date (UTC)", "التاريخ (UTC)")}: {payment.PaymentDate.ToString("yyyy-MM-dd HH:mm", l.Culture)}");
            column.Item().Background("#E2F3EF").Padding(12).Text($"{l.Text("Amount", "المبلغ")}: {l.Number(Math.Abs(payment.Amount))} {payment.Currency}").FontSize(16).Bold();
            column.Item().Text($"{l.Text("Method", "الطريقة")}: {payment.PaymentTypeId switch { 1 => l.Text("Cash", "نقداً"), 2 => l.Text("Card terminal", "جهاز دفع بالبطاقة"), 3 => l.Text("Bank transfer", "تحويل مصرفي"), _ => l.Text("Deferred", "آجل") }}");
            column.Item().Text($"{l.Text("Reference", "المرجع")}: {payment.Reference}");
        });
    })).GeneratePdf();
    }
}
