using PosHC.Application.DTOs;
using PosHC.Domain.Entities;
using PosHC.Infrastructure.Pdf;

namespace PosHC.Tests;

public class PdfLocalizationTests
{
    [Theory]
    [InlineData("en", "USD", 3)]
    [InlineData("ar", "LBP", 3)]
    [InlineData("ar", "USD", 70)]
    [InlineData("unknown", "USD", 3)]
    public void Mixed_language_invoices_and_receipts_render_without_missing_glyphs(string language, string currency, int itemCount)
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = true;
        var items = Enumerable.Range(1, itemCount).Select(i => new InvoiceItemGenerateDto(Guid.NewGuid(), $"استشارة طبية / Consultation {i}", 2, 20, 40)).ToList();
        var invoice = new InvoiceGenerateDto(Guid.NewGuid(), "د. ليلى Haddad", "مريم Smith", 25, 5,
            new DateTime(2026, 9, 10), 40 * itemCount + 25, 40 * itemCount + 20, items,
            1042, currency, "Issued", 0, "عيادة الأمل / Amal Clinic", "بيروت - Beirut", "+961 1 234 567");
        var bytes = new InvoicePdfGenerator().GenerateInvoicePdf(invoice, language);
        Assert.StartsWith("%PDF", System.Text.Encoding.ASCII.GetString(bytes.Take(8).ToArray()));
        var receipt = new ReceiptPdfGenerator().Generate(new Payment { Id = Guid.NewGuid(), Amount = 100, Currency = currency, Kind = "Refund", PaymentTypeId = 3, PaymentDate = new DateTime(2026, 9, 10), Reference = "TX-1042" },
            new Invoice { Number = 1042, ClinicName = invoice.ClinicName, PatientName = invoice.PatientName }, language);
        Assert.StartsWith("%PDF", System.Text.Encoding.ASCII.GetString(receipt.Take(8).ToArray()));
        // Optional local visual QA output; tests do not normally write artifacts.
        var output = Environment.GetEnvironmentVariable("POSHC_PDF_QA_DIR");
        if (!string.IsNullOrEmpty(output) && itemCount == 3 && language != "unknown")
        {
            Directory.CreateDirectory(output);
            File.WriteAllBytes(Path.Combine(output, $"invoice-{language}.pdf"), bytes);
            File.WriteAllBytes(Path.Combine(output, $"receipt-{language}.pdf"), receipt);
        }
    }
}
