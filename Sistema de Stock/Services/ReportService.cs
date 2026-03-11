using ClosedXML.Excel;
using Sistema_de_Stock.Models;

namespace Sistema_de_Stock.Services
{
    public class ReportService
    {
        public byte[] GenerateInventoryReport(List<Producto> productos, List<Categoria> categorias)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Inventario");

            // Header
            ws.Cell(1, 1).Value = "SKU";
            ws.Cell(1, 2).Value = "Producto";
            ws.Cell(1, 3).Value = "Categoría";
            ws.Cell(1, 4).Value = "Precio";
            ws.Cell(1, 5).Value = "Stock";
            ws.Cell(1, 6).Value = "Valor Total (ARS)";

            var headerRow = ws.Range("A1:F1");
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.AirForceBlue;
            headerRow.Style.Font.FontColor = XLColor.White;

            // Data
            int row = 2;
            foreach (var p in productos)
            {
                var cat = categorias.FirstOrDefault(c => c.Id == p.CategoryId);
                ws.Cell(row, 1).Value = p.SKU;
                ws.Cell(row, 2).Value = p.Name;
                ws.Cell(row, 3).Value = cat?.Name ?? "Sin Categoría";
                ws.Cell(row, 4).Value = p.Price;
                ws.Cell(row, 4).Style.NumberFormat.Format = "$ #,##0.00";
                ws.Cell(row, 5).Value = p.Stock;
                
                decimal total = p.Stock > 0 ? p.Stock * p.Price : 0;
                ws.Cell(row, 6).Value = total;
                ws.Cell(row, 6).Style.NumberFormat.Format = "$ #,##0.00";
                
                if (p.Stock <= p.StockMinimo)
                {
                    ws.Cell(row, 5).Style.Font.FontColor = XLColor.Red;
                    ws.Cell(row, 5).Style.Font.Bold = true;
                }

                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] GenerateSalesReport(List<Venta> ventas, List<Cliente> clientes)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Ventas");

            ws.Cell(1, 1).Value = "Nro. Venta";
            ws.Cell(1, 2).Value = "Fecha y Hora";
            ws.Cell(1, 3).Value = "Cliente";
            ws.Cell(1, 4).Value = "Tipo Pago";
            ws.Cell(1, 5).Value = "Total (ARS)";

            var headerRow = ws.Range("A1:E1");
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.DarkGreen;
            headerRow.Style.Font.FontColor = XLColor.White;

            int row = 2;
            foreach (var v in ventas.OrderByDescending(x => x.Date))
            {
                string clienteName = "Consumidor Final";
                if (v.ClienteId.HasValue)
                {
                    var c = clientes.FirstOrDefault(x => x.Id == v.ClienteId.Value);
                    if (c != null) clienteName = c.Name;
                }

                ws.Cell(row, 1).Value = v.NumeroVenta;
                ws.Cell(row, 2).Value = v.Date.ToString("yyyy-MM-dd HH:mm");
                ws.Cell(row, 3).Value = clienteName;
                ws.Cell(row, 4).Value = v.IsFiado ? "Fiado (C/C)" : "Contado";
                
                ws.Cell(row, 5).Value = v.Total;
                ws.Cell(row, 5).Style.NumberFormat.Format = "$ #,##0.00";

                row++;
            }
            
            // Total Row
            ws.Cell(row, 4).Value = "TOTAL VENTAS:";
            ws.Cell(row, 4).Style.Font.Bold = true;
            ws.Cell(row, 5).FormulaA1 = $"=SUM(E2:E{row-1})";
            ws.Cell(row, 5).Style.Font.Bold = true;
            ws.Cell(row, 5).Style.NumberFormat.Format = "$ #,##0.00";

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] GenerateFinancialReport(List<MovimientoFinanciero> movimientos)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Finanzas");

            ws.Cell(1, 1).Value = "Fecha y Hora";
            ws.Cell(1, 2).Value = "Concepto";
            ws.Cell(1, 3).Value = "Tipo";
            ws.Cell(1, 4).Value = "Ingreso (+) / Egreso (-)";

            var headerRow = ws.Range("A1:D1");
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.DarkBlue;
            headerRow.Style.Font.FontColor = XLColor.White;

            int row = 2;
            foreach (var m in movimientos.OrderByDescending(x => x.Date))
            {
                ws.Cell(row, 1).Value = m.Date.ToString("yyyy-MM-dd HH:mm");
                ws.Cell(row, 2).Value = m.Description;
                ws.Cell(row, 3).Value = m.Type.ToString();
                
                decimal displayAmount = m.Type == TipoMovimiento.Egreso ? -m.Amount : m.Amount;
                ws.Cell(row, 4).Value = displayAmount;
                ws.Cell(row, 4).Style.NumberFormat.Format = "$ #,##0.00;[Red]-$ #,##0.00";

                row++;
            }
            
            // Total Row
            ws.Cell(row, 3).Value = "BALANCE NETO:";
            ws.Cell(row, 3).Style.Font.Bold = true;
            ws.Cell(row, 4).FormulaA1 = $"=SUM(D2:D{row-1})";
            ws.Cell(row, 4).Style.Font.Bold = true;
            ws.Cell(row, 4).Style.NumberFormat.Format = "$ #,##0.00;[Red]-$ #,##0.00";

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
