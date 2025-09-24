using PosHC.Application.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PosHC.Application.Interfaces;

namespace PosHC.Infrastructure.Pdf
{
    public class InvoicePdfGenerator : IInvoicePdfGenerator
    {
        public byte[] GenerateInvoicePdf(InvoiceGenerateDto inv)
        {
            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // Header
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Clinic / POS HC").Bold().FontSize(16);
                            col.Item().Text("Street 123, City");
                            col.Item().Text("Phone: +000 000 000");
                        });
                        row.ConstantItem(200).Column(col =>
                        {
                            col.Item().Text($"INVOICE #{inv.Id.ToString()[..8]}").Bold();
                            col.Item().Text($"Date: {inv.CreatedAt:yyyy-MM-dd}");
                            col.Item().Text($"Doctor: {inv.DoctorName}");
                            col.Item().Text($"Patient: {inv.PatientName}");
                        });
                    });

                    // Body
                    page.Content().Column(col =>
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
                                h.Cell().Element(H).Text("Item");
                                h.Cell().Element(H).AlignRight().Text("Qty");
                                h.Cell().Element(H).AlignRight().Text("Unit Price");
                                h.Cell().Element(H).AlignRight().Text("Line Total");

                                static IContainer H(IContainer x) =>
                                    x.DefaultTextStyle(s => s.SemiBold())
                                     .BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                     .PaddingVertical(5);
                            });

                            // Rows
                            foreach (var it in inv.Items)
                            {
                                t.Cell().Text(it.Name);
                                t.Cell().AlignRight().Text(it.Quantity);
                                t.Cell().AlignRight().Text(it.UnitPrice.ToString("0.00"));
                                t.Cell().AlignRight().Text(it.LineTotal.ToString("0.00"));
                            }


                            // Summary
                            t.Cell().ColumnSpan(2);
                            t.Cell().AlignRight().Text("Doctor Fee:");
                            t.Cell().AlignRight().Text(inv.DoctorFee.ToString("0.00"));
                            // Summary
                            t.Cell().ColumnSpan(2);
                            t.Cell().AlignRight().Text("Subtotal:");
                            t.Cell().AlignRight().Text(inv.Subtotal.ToString("0.00"));

                            t.Cell().ColumnSpan(2);
                            t.Cell().AlignRight().Text("Discount:");
                            t.Cell().AlignRight().Text($"-{inv.Discount ?? 0:0.00}");

                            t.Cell().ColumnSpan(2);
                            t.Cell().AlignRight().Text(txt => txt.Span("TOTAL:").SemiBold());
                            t.Cell().AlignRight().Text(txt => txt.Span(inv.Total.ToString("0.00")).SemiBold());
                        });
                    });

                    // Footer
                    page.Footer().AlignCenter().Text($"Printed {DateTime.Now:yyyy-MM-dd HH:mm}");
                });
            });

            return doc.GeneratePdf();
        }
    }
}
