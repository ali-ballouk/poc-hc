using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace PosHC.Infrastructure.Pdf;

internal static class PdfBrand
{
    public const string Primary = "#007CAF";
    public const string Navy = "#064779";
    public const string Background = "#E8F6FC";
    public const string Border = "#D8E8F2";
    public const string Text = "#315978";
    private static readonly Lazy<string> Logo = new(() =>
    {
        using var stream = typeof(PdfBrand).Assembly.GetManifestResourceStream("ClinicSol.Logo.svg")
            ?? throw new InvalidOperationException("The ClinicSol logo is missing.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    });

    public static void Header(IContainer container, PdfLanguage language)
        => (language.Arabic ? container.AlignRight() : container.AlignLeft())
            .ShrinkHorizontal().PaddingBottom(10).ContentFromLeftToRight().Row(row =>
        {
            row.ConstantItem(30).Height(33).Svg(Logo.Value).FitArea();
            row.AutoItem().PaddingLeft(8).AlignMiddle().Text("ClinicSol")
                .Bold().FontSize(23).FontColor(Navy);
        });

    public static void Footer(IContainer container, PdfLanguage language, bool pageNumbers = false)
        => container.BorderTop(1).BorderColor(Border).PaddingTop(8).Column(column =>
        {
            column.Item().ContentFromLeftToRight().AlignCenter().Text("Powered by ClinicSol")
                .FontSize(9).FontColor(Primary);
            if (pageNumbers)
                column.Item().AlignCenter().Text(text =>
                {
                    text.DefaultTextStyle(style => style.FontSize(8).FontColor(Text));
                    text.Span(language.Text("Page ", "صفحة "));
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
        });
}
