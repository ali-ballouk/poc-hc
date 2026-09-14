using System.Globalization;
using QuestPDF.Drawing;
using QuestPDF.Fluent;

namespace PosHC.Infrastructure.Pdf;
internal sealed class PdfLanguage
{
    private static readonly Lazy<bool> Font = new(() =>
    {
        using var stream = typeof(PdfLanguage).Assembly.GetManifestResourceStream(
            "PosHC.Infrastructure.PdfInvoiceGenerator.Fonts.NotoSansArabic-Regular.ttf")
            ?? throw new InvalidOperationException("The bundled Arabic font is missing.");
        FontManager.RegisterFont(stream);
        return true;
    });
    public bool Arabic { get; }
    public CultureInfo Culture { get; }
    public PdfLanguage(string language)
    {
        _ = Font.Value;
        Arabic = string.Equals(language, "ar", StringComparison.OrdinalIgnoreCase);
        Culture = CultureInfo.GetCultureInfo(Arabic ? "ar-LB" : "en-US");
    }
    public string Text(string english, string arabic) => Arabic ? arabic : english;
    public string Number(decimal value) => value.ToString("N2", Culture);
    public string Status(string value) => value switch
    {
        "Draft" => Text("Draft", "مسودة"), "Issued" => Text("Issued", "صادرة"),
        "Void" => Text("Void", "ملغاة"), _ => value
    };
    public void Configure(PageDescriptor page)
    {
        page.DefaultTextStyle(x => x.FontFamily("Lato", "Noto Sans Arabic").FontSize(10).FontColor(PdfBrand.Text));
        if (Arabic) page.ContentFromRightToLeft();
    }
}
