using PosHC.Application.Interfaces;
using PosHC.Application.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
namespace PosHC.Infrastructure.Pdf;
public class ReceiptPdfGenerator : IReceiptPdfGenerator
{
    public byte[] Generate(ReceiptGenerateDto receipt, string language = "en")
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
            column.Item().Element(header => PdfBrand.Header(header, l));
            column.Item().Text(receipt.ClinicName).FontSize(20).Bold().FontColor(PdfBrand.Navy);
            column.Item().BorderBottom(2).BorderColor(PdfBrand.Primary).PaddingBottom(12).Text(receipt.Kind == "Refund" ? l.Text("REFUND RECEIPT", "إيصال استرداد") : l.Text("PAYMENT RECEIPT", "إيصال دفع")).Bold();
            column.Item().Text($"{l.Text("Receipt", "الإيصال")}: {receipt.Id}");
            column.Item().Text($"{l.Text("Invoice", "الفاتورة")}: {receipt.InvoiceNumber}");
            column.Item().Text($"{l.Text("Patient", "المريض")}: {receipt.PatientName}");
            column.Item().Text($"{l.Text("Date (UTC)", "التاريخ (UTC)")}: {receipt.PaymentDate.ToString("yyyy-MM-dd HH:mm", l.Culture)}");
            column.Item().Background(PdfBrand.Background).Padding(12).Text($"{l.Text("Amount", "المبلغ")}: {l.Number(Math.Abs(receipt.Amount))} {receipt.Currency}").FontSize(16).Bold();
            column.Item().Text($"{l.Text("Method", "الطريقة")}: {receipt.PaymentTypeId switch { 1 => l.Text("Cash", "نقداً"), 2 => l.Text("Card terminal", "جهاز دفع بالبطاقة"), 3 => l.Text("Bank transfer", "تحويل مصرفي"), _ => l.Text("Deferred", "آجل") }}");
            column.Item().Text($"{l.Text("Reference", "المرجع")}: {receipt.Reference}");
        });
        page.Footer().Element(footer => PdfBrand.Footer(footer, l));
    })).GeneratePdf();
    }
}
