using PosHC.Application.DTOs;
using PosHC.Domain.Entities;
using PosHC.Infrastructure.Pdf;

namespace PosHC.Tests;

public class PdfLocalizationTests
{
    [Theory]
    [InlineData("en")]
    [InlineData("ar")]
    public void Multiline_clinic_details_do_not_exhaust_the_repeating_header(string language)
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        var invoice = new InvoiceGenerateDto(Guid.NewGuid(), "Doctor", "Patient", 25, 0,
            DateTime.UtcNow, 25, 25, [], 1042, "USD", "Issued", 0,
            "Clinic", string.Join("\n", Enumerable.Repeat("Beirut", 60)), "1234");
        var bytes = new InvoicePdfGenerator().GenerateInvoicePdf(invoice, language);
        Assert.StartsWith("%PDF", System.Text.Encoding.ASCII.GetString(bytes.Take(8).ToArray()));
    }
    [Theory]
    [InlineData("en", 8950000)]
    [InlineData("ar", 999999999999)]
    public void Large_lbp_amounts_fit_invoice_columns(string language, decimal amount)
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        var invoice = new InvoiceGenerateDto(Guid.NewGuid(), "Doctor", "Patient", amount, 0,
            DateTime.UtcNow, amount * 2, amount * 2,
            [new InvoiceItemGenerateDto(Guid.NewGuid(), "Consultation", 1, amount, amount)],
            1042, "LBP", "Issued", 0, "Clinic", "Beirut", "1234");
        var bytes = new InvoicePdfGenerator().GenerateInvoicePdf(invoice, language);
        Assert.StartsWith("%PDF", System.Text.Encoding.ASCII.GetString(bytes.Take(8).ToArray()));
        var output = Environment.GetEnvironmentVariable("POSHC_PDF_QA_DIR");
        if (!string.IsNullOrEmpty(output))
        {
            Directory.CreateDirectory(output);
            File.WriteAllBytes(Path.Combine(output, $"invoice-large-{language}.pdf"), bytes);
        }
    }
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
        var receipt = new ReceiptPdfGenerator().Generate(new ReceiptGenerateDto(Guid.NewGuid(), 1042,
            invoice.PatientName, invoice.ClinicName, new DateTime(2026, 9, 10), 100, currency, "Refund", 3, "TX-1042"), language);
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
