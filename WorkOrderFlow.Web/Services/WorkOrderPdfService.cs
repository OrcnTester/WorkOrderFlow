using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WorkOrderFlow.Web.Models;

namespace WorkOrderFlow.Web.Services;

public class WorkOrderPdfService
{
    public byte[] Generate(WorkOrder workOrder, List<WorkOrderMaterial> materials)
    {
        var materialTotal = materials.Sum(m => m.QuantityUsed * m.UnitPrice);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Column(column =>
                {
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            left.Item().Text("WORKORDERFLOW")
                                .FontSize(22)
                                .Bold()
                                .FontColor(Colors.Blue.Darken3);

                            left.Item().Text("Work Order Report")
                                .FontSize(11)
                                .FontColor(Colors.Grey.Darken2);
                        });

                        row.ConstantItem(180).AlignRight().Column(right =>
                        {
                            right.Item().AlignRight().Text("WORK ORDER")
                                .FontSize(22)
                                .Bold()
                                .FontColor(Colors.Grey.Darken3);

                            right.Item().AlignRight().Text($"No: #{workOrder.Id}").Bold();
                            right.Item().AlignRight().Text($"Created: {workOrder.CreatedAt.ToLocalTime():dd.MM.yyyy}");

                            if (workOrder.DueDate.HasValue)
                                right.Item().AlignRight().Text($"Due: {workOrder.DueDate.Value:dd.MM.yyyy}");
                        });
                    });

                    column.Item().PaddingTop(16).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().PaddingVertical(20).Column(column =>
                {
                    column.Spacing(18);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Element(Card).Column(left =>
                        {
                            left.Item().Text("CUSTOMER").FontSize(9).Bold().FontColor(Colors.Grey.Darken2);
                            left.Item().PaddingTop(6).Text(workOrder.Customer?.FullName ?? "-").FontSize(14).Bold();

                            if (!string.IsNullOrWhiteSpace(workOrder.Customer?.CompanyName))
                                left.Item().Text(workOrder.Customer.CompanyName);

                            if (!string.IsNullOrWhiteSpace(workOrder.Customer?.Phone))
                                left.Item().Text($"Phone: {workOrder.Customer.Phone}");

                            if (!string.IsNullOrWhiteSpace(workOrder.Customer?.Email))
                                left.Item().Text($"Email: {workOrder.Customer.Email}");
                        });

                        row.ConstantItem(20);

                        row.RelativeItem().Element(Card).Column(right =>
                        {
                            right.Item().Text("WORK ORDER SUMMARY").FontSize(9).Bold().FontColor(Colors.Grey.Darken2);
                            right.Item().PaddingTop(6).Text(workOrder.Title).FontSize(14).Bold();
                            right.Item().Text($"Status: {workOrder.Status}");
                            right.Item().Text($"Priority: {workOrder.Priority}");

                            if (!string.IsNullOrWhiteSpace(workOrder.Description))
                            {
                                right.Item().PaddingTop(6).Text(workOrder.Description);
                            }
                        });
                    });

                    column.Item().Text("Materials Used")
                        .FontSize(15)
                        .Bold()
                        .FontColor(Colors.Grey.Darken3);

                    if (!materials.Any())
                    {
                        column.Item().Element(Card).Text("No materials were used for this work order.");
                    }
                    else
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(TableHeader).Text("Item");
                                header.Cell().Element(TableHeader).AlignRight().Text("Qty");
                                header.Cell().Element(TableHeader).AlignRight().Text("Unit");
                                header.Cell().Element(TableHeader).AlignRight().Text("Total");
                            });

                            foreach (var material in materials)
                            {
                                table.Cell().Element(TableCell).Text(material.InventoryItem?.Name ?? "-");
                                table.Cell().Element(TableCell).AlignRight().Text(material.QuantityUsed.ToString());
                                table.Cell().Element(TableCell).AlignRight().Text($"{material.UnitPrice:N2} TL");
                                table.Cell().Element(TableCell).AlignRight().Text($"{material.LineTotal:N2} TL");
                            }
                        });

                        column.Item().AlignRight().Width(230).Element(container =>
                        {
                            container.Background(Colors.Blue.Darken3)
                                .Padding(14)
                                .Column(totalBox =>
                                {
                                    totalBox.Item().Text("MATERIAL TOTAL")
                                        .FontSize(9)
                                        .FontColor(Colors.White);

                                    totalBox.Item().Text($"{materialTotal:N2} TL")
                                        .FontSize(20)
                                        .Bold()
                                        .FontColor(Colors.White);
                                });
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(workOrder.ResolutionNote))
                    {
                        column.Item().Element(Card).Column(notes =>
                        {
                            notes.Item().Text("Resolution Note").FontSize(11).Bold();
                            notes.Item().PaddingTop(5).Text(workOrder.ResolutionNote);
                        });
                    }
                });

                page.Footer().Column(footer =>
                {
                    footer.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    footer.Item().PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem().Text("Generated by WorkOrderFlow")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken1);

                        row.RelativeItem().AlignRight().Text(DateTime.Now.ToString("dd.MM.yyyy HH:mm"))
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken1);
                    });
                });
            });
        }).GeneratePdf();
    }

    private static IContainer Card(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Background(Colors.Grey.Lighten5)
            .Padding(12);
    }

    private static IContainer TableHeader(IContainer container)
    {
        return container
            .Background(Colors.Grey.Darken3)
            .PaddingVertical(8)
            .PaddingHorizontal(10)
            .DefaultTextStyle(x => x.FontColor(Colors.White).Bold());
    }

    private static IContainer TableCell(IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(8)
            .PaddingHorizontal(10);
    }
}