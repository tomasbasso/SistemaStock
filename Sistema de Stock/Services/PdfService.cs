using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sistema_de_Stock.Models;

namespace Sistema_de_Stock.Services
{
    /// <summary>
    /// Modelo con los datos necesarios para generar el estado de cuenta de un cliente.
    /// </summary>
    public class EstadoCuentaData
    {
        public Cliente Cliente { get; set; } = new();
        public CuentaCorriente CuentaCorriente { get; set; } = new();
        public ConfiguracionApp Config { get; set; } = new();
        public List<VentaFiadaDetalle> VentasFiadas { get; set; } = new();
        public DateTime FechaGeneracion { get; set; } = DateTime.Now;
    }

    public class VentaFiadaDetalle
    {
        public int NumeroVenta { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public List<string> Items { get; set; } = new();
    }

    /// <summary>
    /// Genera documentos PDF de estados de cuenta de clientes usando QuestPDF.
    /// </summary>
    public class PdfService
    {
        public PdfService()
        {
            // Configurar licencia comunitaria de QuestPDF (gratuita para uso no comercial / proyectos pequeños)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        /// <summary>
        /// Genera el PDF del estado de cuenta de un cliente.
        /// Retorna los bytes del PDF para ser guardados en disco.
        /// </summary>
        public byte[] GenerarEstadoCuenta(EstadoCuentaData data)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(c => ComposeContent(c, data));
                    page.Footer().Element(ComposeFooter);

                    void ComposeHeader(QuestPDF.Infrastructure.IContainer header)
                    {
                        header.Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                // Left: Store name
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text(data.Config.NombreNegocio)
                                        .FontSize(22).Bold().FontColor("#1e293b");
                                    if (!string.IsNullOrEmpty(data.Config.DireccionNegocio))
                                        c.Item().Text(data.Config.DireccionNegocio).FontSize(9).FontColor("#64748b");
                                    if (!string.IsNullOrEmpty(data.Config.Telefono))
                                        c.Item().Text($"Tel: {data.Config.Telefono}").FontSize(9).FontColor("#64748b");
                                });

                                // Right: Document title
                                row.ConstantItem(180).Column(c =>
                                {
                                    c.Item().Background("#1e40af").Padding(12).Column(inner =>
                                    {
                                        inner.Item().Text("ESTADO DE CUENTA")
                                            .FontSize(14).Bold().FontColor("#ffffff").AlignCenter();
                                        inner.Item().Text($"C/C — {data.FechaGeneracion:dd/MM/yyyy}")
                                            .FontSize(9).FontColor("#bfdbfe").AlignCenter();
                                    });
                                });
                            });

                            col.Item().PaddingTop(12).LineHorizontal(1.5f).LineColor("#1e40af");
                        });
                    }

                    void ComposeContent(QuestPDF.Infrastructure.IContainer content, EstadoCuentaData d)
                    {
                        content.PaddingTop(16).Column(col =>
                        {
                            // ── Cliente Info ──────────────────────────
                            col.Item().Background("#f1f5f9").Padding(14).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("CLIENTE").FontSize(8).FontColor("#64748b").Bold();
                                    c.Item().Text(d.Cliente.Name).FontSize(14).Bold().FontColor("#1e293b");
                                    if (!string.IsNullOrEmpty(d.Cliente.Phone))
                                        c.Item().Text($"Tel: {d.Cliente.Phone}").FontSize(9).FontColor("#475569");
                                    if (!string.IsNullOrEmpty(d.Cliente.Address))
                                        c.Item().Text($"Dir: {d.Cliente.Address}").FontSize(9).FontColor("#475569");
                                });

                                row.ConstantItem(160).Column(c =>
                                {
                                    c.Item().Text("DEUDA TOTAL").FontSize(8).FontColor("#64748b").Bold().AlignRight();
                                    var balanceColor = d.CuentaCorriente.Balance > 0 ? "#dc2626" : "#16a34a";
                                    c.Item().Text($"{d.CuentaCorriente.Balance:C}")
                                        .FontSize(20).Bold().FontColor(balanceColor).AlignRight();
                                    var statusText = d.CuentaCorriente.Balance > 0 ? "DEUDA PENDIENTE"
                                                   : d.CuentaCorriente.Balance < 0 ? "A FAVOR"
                                                   : "SIN DEUDA";
                                    c.Item().Text(statusText).FontSize(8).FontColor(balanceColor).AlignRight();
                                });
                            });

                            col.Item().PaddingTop(20).Text("HISTORIAL DE VENTAS EN CUENTA CORRIENTE")
                                .FontSize(10).Bold().FontColor("#1e40af");

                            col.Item().PaddingTop(8).LineHorizontal(0.5f).LineColor("#cbd5e1");

                            if (!d.VentasFiadas.Any())
                            {
                                col.Item().PaddingTop(16).AlignCenter()
                                    .Text("No hay ventas en cuenta corriente registradas para este cliente.")
                                    .FontColor("#94a3b8").Italic();
                            }
                            else
                            {
                                // Table header
                                col.Item().PaddingTop(8).Table(table =>
                                {
                                    table.ColumnsDefinition(cols =>
                                    {
                                        cols.ConstantColumn(60);   // Nro
                                        cols.ConstantColumn(90);   // Fecha
                                        cols.RelativeColumn();     // Detalle
                                        cols.ConstantColumn(90);   // Total
                                    });

                                    // Header
                                    table.Header(header =>
                                    {
                                        static void HeaderCell(QuestPDF.Infrastructure.IContainer c, string text) =>
                                            c.Background("#1e40af").Padding(8)
                                             .Text(text).Bold().FontColor("#ffffff").FontSize(9);

                                        header.Cell().Element(c => HeaderCell(c, "VENTA #"));
                                        header.Cell().Element(c => HeaderCell(c, "FECHA"));
                                        header.Cell().Element(c => HeaderCell(c, "DETALLE"));
                                        header.Cell().Element(c => HeaderCell(c, "MONTO"));
                                    });

                                    // Rows
                                    bool alternate = false;
                                    foreach (var v in d.VentasFiadas.OrderByDescending(x => x.Fecha))
                                    {
                                        var bg = alternate ? "#f8fafc" : "#ffffff";
                                        alternate = !alternate;

                                        static QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer c, string bg) =>
                                            c.Background(bg).BorderBottom(0.5f).BorderColor("#e2e8f0").Padding(7);

                                        table.Cell().Element(c => CellStyle(c, bg))
                                            .Text($"#{v.NumeroVenta:D4}").FontSize(9).FontColor("#334155");

                                        table.Cell().Element(c => CellStyle(c, bg))
                                            .Text(v.Fecha.ToString("dd/MM/yyyy")).FontSize(9).FontColor("#334155");

                                        table.Cell().Element(c => CellStyle(c, bg)).Column(itemCol =>
                                        {
                                            foreach (var item in v.Items)
                                                itemCol.Item().Text($"• {item}").FontSize(8.5f).FontColor("#475569");
                                        });

                                        table.Cell().Element(c => CellStyle(c, bg)).AlignRight()
                                            .Text($"{v.Total:C}").FontSize(9).Bold().FontColor("#1e293b");
                                    }
                                });

                                // Totals row
                                col.Item().PaddingTop(4).Background("#1e293b").Padding(10).Row(row =>
                                {
                                    row.RelativeItem().Text("TOTAL DEUDA EN CUENTA CORRIENTE")
                                        .FontSize(10).Bold().FontColor("#ffffff");
                                    row.ConstantItem(90).AlignRight()
                                        .Text($"{d.VentasFiadas.Sum(v => v.Total):C}")
                                        .FontSize(11).Bold().FontColor("#fbbf24");
                                });
                            }

                            // Nota al pie
                            col.Item().PaddingTop(24).Text(
                                "Este documento es un resumen informativo del estado de cuenta corriente. " +
                                "No tiene validez como comprobante fiscal.")
                                .FontSize(8).FontColor("#94a3b8").Italic();
                        });
                    }

                    void ComposeFooter(QuestPDF.Infrastructure.IContainer footer)
                    {
                        footer.BorderTop(0.5f).BorderColor("#cbd5e1").PaddingTop(8).Row(row =>
                        {
                            row.RelativeItem().Text($"Generado el {data.FechaGeneracion:dd/MM/yyyy HH:mm}")
                                .FontSize(8).FontColor("#94a3b8");
                            row.RelativeItem().AlignRight()
                                .Text(x =>
                                {
                                    x.Span("Pág. ").FontSize(8).FontColor("#94a3b8");
                                    x.CurrentPageNumber().FontSize(8).FontColor("#94a3b8");
                                    x.Span(" de ").FontSize(8).FontColor("#94a3b8");
                                    x.TotalPages().FontSize(8).FontColor("#94a3b8");
                                });
                        });
                    }
                });
            }).GeneratePdf();
        }
    }
}
