using PosHC.Application.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PosHC.Application.Interfaces;

namespace PosHC.Infrastructure.Pdf
{
    public class InvoicePdfGenerator : IInvoicePdfGenerator
    {
        public byte[] GenerateInvoicePdf(InvoiceGenerateDto inv, string language = "en")
        {
            var l = new PdfLanguage(language);
            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    l.Configure(page);

                    // Header
                    page.Header().BorderBottom(2).BorderColor("#087F82").PaddingBottom(18).Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(inv.ClinicName).Bold().FontSize(20).FontColor("#087F82");
                            col.Item().Text(inv.ClinicAddress);
                            col.Item().Text("\u2066" + inv.ClinicPhone + "\u2069");
                        });
                        row.ConstantItem(200).Column(col =>
                        {
                            col.Item().Text($"{l.Text("INVOICE", "فاتورة")} #{inv.Number} - {l.Status(inv.Status)}").Bold();
                            col.Item().Text($"{l.Text("Currency", "العملة")}: {inv.Currency}");
                            col.Item().Text($"{l.Text("Date", "التاريخ")}: \u2066{inv.CreatedAt.ToString("yyyy-MM-dd", l.Culture)}\u2069");
                            col.Item().Text($"{l.Text("Doctor", "الطبيب")}: {inv.DoctorName}");
                            col.Item().Text($"{l.Text("Patient", "المريض")}: {inv.PatientName}");
                        });
                    });

                    // Body
                    page.Content().PaddingTop(20).Column(col =>
                    {
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(6);
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                            });

                            // Header
                            t.Header(h =>
                            {
                                h.Cell().Element(H).Text(l.Text("Item", "البند"));
                                h.Cell().Element(H).Text(l.Text("Qty", "الكمية"));
                                h.Cell().Element(H).Text(l.Text("Unit Price", "سعر الوحدة"));
                                h.Cell().Element(H).Text(l.Text("Line Total", "إجمالي البند"));

                                static IContainer H(IContainer x) =>
                                    x.Background("#087F82").DefaultTextStyle(s => s.SemiBold().FontColor("#FFFFFF"))
                                     .Padding(8);
                            });

                            // Rows
                            foreach (var it in inv.Items)
                            {
                                t.Cell().Padding(8).Text(it.Name);
                                t.Cell().Padding(8).Text(it.Quantity.ToString(l.Culture));
                                t.Cell().Padding(8).Text(l.Number(it.UnitPrice));
                                t.Cell().Padding(8).Text(l.Number(it.LineTotal));
                            }


                            // Summary
                            t.Cell().ColumnSpan(2);
                            t.Cell().PaddingVertical(6).Text(l.Text("Doctor Fee:", "أتعاب الطبيب:"));
                            t.Cell().PaddingVertical(6).Text(l.Number(inv.DoctorFee));
                            // Summary
                            t.Cell().ColumnSpan(2);
                            t.Cell().PaddingVertical(6).Text(l.Text("Subtotal:", "المجموع الفرعي:"));
                            t.Cell().PaddingVertical(6).Text(l.Number(inv.Subtotal));

                            t.Cell().ColumnSpan(2);
                            t.Cell().PaddingVertical(6).Text(l.Text("Discount:", "الخصم:"));
                            t.Cell().PaddingVertical(6).Text("\u2066" + l.Number(-(inv.Discount ?? 0)) + "\u2069");

                            t.Cell().ColumnSpan(2);
                            t.Cell().PaddingVertical(6).Text(l.Text("Tax:", "الضريبة:"));
                            t.Cell().PaddingVertical(6).Text(l.Number(inv.Tax));
                            t.Cell().ColumnSpan(2);
                            t.Cell().Background("#E2F3EF").PaddingVertical(8).Text(l.Text("TOTAL:", "الإجمالي:")).SemiBold();
                            t.Cell().Background("#E2F3EF").PaddingVertical(8).Text(l.Number(inv.Total)).SemiBold();
                        });
                    });

                    // Footer
                    page.Footer().AlignCenter().Text(text => { text.Span(l.Text("Page ", "صفحة ")); text.CurrentPageNumber(); text.Span(" / "); text.TotalPages(); });
                });
            });

            return doc.GeneratePdf();
        }
    }
}
