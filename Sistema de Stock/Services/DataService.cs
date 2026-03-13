using Microsoft.EntityFrameworkCore;
using Sistema_de_Stock.Data;
using Sistema_de_Stock.Models;

namespace Sistema_de_Stock.Services
{
    /// <summary>
    /// Servicio principal de acceso a datos, ahora basado en EF Core / SQLite.
    /// Mantiene el mismo contrato público que antes para compatibilidad con los componentes Blazor.
    /// </summary>
    public class DataService
    {
        private readonly StockDbContext _db;

        public DataService(StockDbContext db)
        {
            _db = db;
        }

        // ─────────────────────────────────────────────────────────
        // INICIALIZACIÓN
        // ─────────────────────────────────────────────────────────

        /// <summary>
        /// Inicializa la base de datos y aplica migraciones pendientes.
        /// Se llama desde MauiProgram o App.xaml.cs al iniciar.
        /// </summary>
        public async Task InitializeAsync()
        {
            await _db.InitializeDatabaseAsync();
        }

        // ─────────────────────────────────────────────────────────
        // CONFIGURACIÓN
        // ─────────────────────────────────────────────────────────

        public async Task<ConfiguracionApp?> GetConfiguracionAsync()
            => await _db.Configuraciones.FirstOrDefaultAsync();

        public async Task SaveConfiguracionAsync(ConfiguracionApp config)
        {
            var existing = await _db.Configuraciones.FindAsync(config.Id);
            if (existing == null)
                _db.Configuraciones.Add(config);
            else
            {
                existing.NombreNegocio = config.NombreNegocio;
                existing.Moneda = config.Moneda;
                existing.DireccionNegocio = config.DireccionNegocio;
                existing.Telefono = config.Telefono;
            }
            await _db.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────────────────
        // CATEGORÍAS
        // ─────────────────────────────────────────────────────────

        public async Task<List<Categoria>> GetCategoriasAsync()
            => await _db.Categorias.OrderBy(c => c.Name).ToListAsync();

        public async Task SaveCategoriaAsync(Categoria c)
        {
            var existing = await _db.Categorias.FindAsync(c.Id);
            if (existing == null)
                _db.Categorias.Add(c);
            else
                existing.Name = c.Name;

            await _db.SaveChangesAsync();
        }

        public async Task DeleteCategoriaAsync(Guid id)
        {
            var entity = await _db.Categorias.FindAsync(id);
            if (entity != null)
            {
                _db.Categorias.Remove(entity);
                await _db.SaveChangesAsync();
            }
        }

        // ─────────────────────────────────────────────────────────
        // PRODUCTOS
        // ─────────────────────────────────────────────────────────

        public async Task<List<Producto>> GetProductosAsync()
            => await _db.Productos.OrderBy(p => p.Name).ToListAsync();

        public async Task<int> GetTotalProductosAsync(string searchTerm = "")
        {
            var query = _db.Productos.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var words = searchTerm.ToLower()
                                      .Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in words)
                {
                    var w = word; // captura local para la lambda
                    query = query.Where(p =>
                        p.Name.ToLower().Contains(w) ||
                        (p.SKU != null && p.SKU.ToLower().Contains(w)));
                }
            }
            return await query.CountAsync();
        }

        public async Task<List<Producto>> GetProductosPaginadosAsync(int page, int pageSize, string searchTerm = "")
        {
            var query = _db.Productos.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var words = searchTerm.ToLower()
                                      .Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in words)
                {
                    var w = word; // captura local para la lambda
                    query = query.Where(p =>
                        p.Name.ToLower().Contains(w) ||
                        (p.SKU != null && p.SKU.ToLower().Contains(w)));
                }
            }
            return await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task SaveProductoAsync(Producto p)
        {
            var existing = await _db.Productos.FindAsync(p.Id);
            if (existing == null)
                _db.Productos.Add(p);
            else
            {
                existing.Name = p.Name;
                existing.SKU = p.SKU;
                existing.CategoryId = p.CategoryId;
                existing.Stock = p.Stock;
                existing.StockMinimo = p.StockMinimo;
                existing.Price = p.Price;
                existing.UnidadMedida = p.UnidadMedida;
                existing.Ubicacion = p.Ubicacion;
            }
            await _db.SaveChangesAsync();
        }

        public async Task AjustarPreciosPorcentajeAsync(List<Guid> productoIds, decimal porcentaje)
        {
            var productos = await _db.Productos.Where(p => productoIds.Contains(p.Id)).ToListAsync();
            foreach (var p in productos)
                p.Price = Math.Round(p.Price * (1 + porcentaje / 100), 2);
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Importa productos desde un stream de Excel (.xlsx).
        /// Columnas esperadas: SKU, Nombre, Precio (en ese orden o por nombre de cabecera).
        /// Si ya existe un producto con el mismo SKU, actualiza precio y nombre.
        /// Devuelve Result con (importados, actualizados, errores).
        /// </summary>
        public async Task<Result<(int Importados, int Actualizados, List<string> Errores)>> ImportarProductosDesdeExcelAsync(Stream stream, Guid categoriaDefaultId)
        {
            int importados = 0, actualizados = 0;
            var errores = new List<string>();

            using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
            var ws = workbook.Worksheets.First();

            // Detectar si la primera fila es cabecera (usamos GetValue<string>() para seguridad)
            var firstCell = ws.Cell(1, 1).GetValue<string>()?.Trim().ToLower() ?? "";
            bool esEncabezado = firstCell.Contains("sku") || firstCell.Contains("cod") || firstCell.Contains("nombre") || firstCell.Contains("producto");
            int startRow = esEncabezado ? 2 : 1;

            // Detectar índices de columnas por cabecera (si hay)
            int colSku = 1, colNombre = 2, colPrecio = 3;
            if (startRow == 2)
            {
                for (int c = 1; c <= ws.LastColumnUsed().ColumnNumber(); c++)
                {
                    var h = ws.Cell(1, c).GetValue<string>()?.Trim().ToLower() ?? "";
                    if (h.Contains("sku") || h.Contains("cod")) colSku = c;
                    else if (h.Contains("nombre") || h.Contains("producto") || h.Contains("descrip")) colNombre = c;
                    else if (h.Contains("precio") || h.Contains("price") || h.Contains("costo") || h.Contains("valor")) colPrecio = c;
                }
            }

            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;
            var skusExistentes = await _db.Productos.ToDictionaryAsync(p => (p.SKU ?? "").ToLower(), p => p);

            for (int row = startRow; row <= lastRow; row++)
            {
                try
                {
                    var sku = ws.Cell(row, colSku).GetValue<string>()?.Trim() ?? "";
                    var nombre = ws.Cell(row, colNombre).GetValue<string>()?.Trim() ?? "";
                    var rawPrecio = ws.Cell(row, colPrecio).GetValue<string>() ?? "";
                    var precioStr = rawPrecio.Trim().Replace("$", "").Replace(".", "").Replace(",", ".");

                    if (string.IsNullOrWhiteSpace(nombre)) continue;

                    if (!decimal.TryParse(precioStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal precio) || precio < 0)
                    {
                        errores.Add($"Fila {row}: precio inválido '{rawPrecio}'");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(sku))
                        sku = $"IMP-{row:D4}";

                    if (skusExistentes.TryGetValue(sku.ToLower(), out var existing))
                    {
                        existing.Name = nombre;
                        existing.Price = precio;
                        actualizados++;
                    }
                    else
                    {
                        var nuevo = new Producto
                        {
                            Name = nombre,
                            SKU = sku,
                            Price = precio,
                            CategoryId = categoriaDefaultId,
                            Stock = 5,
                            StockMinimo = 0,
                            UnidadMedida = "u."
                        };
                        _db.Productos.Add(nuevo);
                        importados++;
                    }
                }
                catch (Exception ex)
                {
                    errores.Add($"Fila {row}: {ex.Message}");
                }
            }

            await _db.SaveChangesAsync();
            return Result<(int, int, List<string>)>.Ok((importados, actualizados, errores));
        }

        // ── Presupuestos ────────────────────────────────────────────
        public async Task<List<Presupuesto>> GetPresupuestosAsync()
            => await _db.Presupuestos.OrderByDescending(p => p.Date).ToListAsync();

        public async Task<List<PresupuestoDetalle>> GetPresupuestoDetallesAsync(Guid presupuestoId)
            => await _db.PresupuestoDetalles.Where(d => d.PresupuestoId == presupuestoId).ToListAsync();

        public async Task<Presupuesto> SavePresupuestoAsync(Presupuesto presupuesto, List<PresupuestoDetalle> detalles)
        {
            // Número secuencial
            int maxNum = await _db.Presupuestos.AnyAsync()
                ? await _db.Presupuestos.MaxAsync(p => p.NumeroPresupuesto)
                : 0;
            presupuesto.NumeroPresupuesto = maxNum + 1;
            presupuesto.Total = detalles.Sum(d => d.UnitPrice * d.Quantity);

            _db.Presupuestos.Add(presupuesto);
            foreach (var d in detalles)
            {
                d.PresupuestoId = presupuesto.Id;
                _db.PresupuestoDetalles.Add(d);
            }
            await _db.SaveChangesAsync();
            return presupuesto;
        }

        public async Task DeletePresupuestoAsync(Guid id)
        {
            var entity = await _db.Presupuestos.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                // Eliminar detalles de presupuesto
                var detalles = await _db.PresupuestoDetalles.Where(d => d.PresupuestoId == id).ToListAsync();
                _db.PresupuestoDetalles.RemoveRange(detalles);
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeleteProductoAsync(Guid id)
        {
            var entity = await _db.Productos.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                await _db.SaveChangesAsync();
            }
        }

        public async Task<bool> ExisteProductoPorSKUAsync(string sku, Guid excludeId)
        {
            return await _db.Productos.AnyAsync(p => p.SKU == sku && p.Id != excludeId);
        }

        // ─────────────────────────────────────────────────────────
        // CLIENTES & CUENTAS CORRIENTES
        // ─────────────────────────────────────────────────────────

        public async Task<List<Cliente>> GetClientesAsync()
            => await _db.Clientes.OrderBy(c => c.Name).ToListAsync();

        public async Task SaveClienteAsync(Cliente c)
        {
            var existing = await _db.Clientes.FindAsync(c.Id);
            if (existing == null)
            {
                _db.Clientes.Add(c);
                // Crear cuenta corriente automáticamente al crear un cliente nuevo
                _db.CuentasCorrientes.Add(new CuentaCorriente { ClienteId = c.Id });
            }
            else
            {
                existing.Name = c.Name;
                existing.Phone = c.Phone;
                existing.Address = c.Address;
            }
            await _db.SaveChangesAsync();
        }

        public async Task DeleteClienteAsync(Guid id)
        {
            var entity = await _db.Clientes.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                // Eliminar cuenta corriente asociada
                var cc = await _db.CuentasCorrientes.FirstOrDefaultAsync(x => x.ClienteId == id);
                if (cc != null) _db.CuentasCorrientes.Remove(cc);

                await _db.SaveChangesAsync();
            }
        }

        public async Task<CuentaCorriente?> GetCuentaCorrienteAsync(Guid clienteId)
            => await _db.CuentasCorrientes.FirstOrDefaultAsync(x => x.ClienteId == clienteId);

        public async Task<List<CuentaCorriente>> GetCuentasCorrientesAsync()
            => await _db.CuentasCorrientes.ToListAsync();

        public async Task SaveCuentaCorrienteAsync(CuentaCorriente cc)
        {
            var existing = await _db.CuentasCorrientes.FindAsync(cc.Id);
            if (existing == null)
                _db.CuentasCorrientes.Add(cc);
            else
                existing.Balance = cc.Balance;

            await _db.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────────────────
        // MOVIMIENTOS FINANCIEROS
        // ─────────────────────────────────────────────────────────

        public async Task<List<MovimientoFinanciero>> GetMovimientosAsync()
            => await _db.MovimientosFinancieros.OrderByDescending(m => m.Date).ToListAsync();

        public async Task<int> GetTotalMovimientosAsync(string searchTerm = "")
        {
            var query = _db.MovimientosFinancieros.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(m => m.Description != null && m.Description.ToLower().Contains(term));
            }
            return await query.CountAsync();
        }

        public async Task<(decimal Ingresos, decimal Egresos)> GetTotalesMovimientosAsync()
        {
            var ingresos = (await _db.MovimientosFinancieros
                .Where(m => m.Type == TipoMovimiento.Ingreso)
                .Select(m => m.Amount)
                .ToListAsync())
                .Sum();

            var egresos = (await _db.MovimientosFinancieros
                .Where(m => m.Type == TipoMovimiento.Egreso)
                .Select(m => m.Amount)
                .ToListAsync())
                .Sum();
                
            return (ingresos, egresos);
        }

        public async Task<(decimal TotalVentas, int CantidadVentas, decimal TotalDeuda, decimal ValorInventario, int TotalProductos, List<Producto> BajoStock, List<MovimientoFinanciero> UltimosMovimientos)> GetDashboardDataAsync()
        {
            var hoyInicio = DateTime.Today;
            var hoyFin = DateTime.Today.AddDays(1).AddTicks(-1);

            var ventasHoy = await _db.Ventas
                .Where(v => v.Date >= hoyInicio && v.Date <= hoyFin)
                .Select(v => new { v.Total })
                .ToListAsync();

            var totalVentas = ventasHoy.Sum(v => v.Total);
            var cantidadVentas = ventasHoy.Count;

            var totalDeuda = (await _db.CuentasCorrientes
                .Select(c => c.Balance)
                .ToListAsync())
                .Sum();

            var productosData = await _db.Productos
                .Select(p => new { p.Price, p.Stock, p.StockMinimo })
                .ToListAsync();

            var valorInventario = productosData.Sum(p => p.Price * Math.Max(0, p.Stock));
            var totalProductos = productosData.Count;

            var bajoStock = await _db.Productos
                .Where(p => p.Stock <= p.StockMinimo)
                .OrderBy(p => p.Stock)
                .ToListAsync();

            var ultimosMovimientos = await _db.MovimientosFinancieros
                .Where(m => m.Date >= hoyInicio && m.Date <= hoyFin)
                .OrderByDescending(m => m.Date)
                .Take(10)
                .ToListAsync();

            return (totalVentas, cantidadVentas, totalDeuda, valorInventario, totalProductos, bajoStock, ultimosMovimientos);
        }

        public async Task<List<MovimientoFinanciero>> GetMovimientosPaginadosAsync(int page, int pageSize, string searchTerm = "")
        {
            var query = _db.MovimientosFinancieros.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(m => m.Description != null && m.Description.ToLower().Contains(term));
            }
            return await query.OrderByDescending(m => m.Date).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task AddMovimientoAsync(MovimientoFinanciero m)
        {
            _db.MovimientosFinancieros.Add(m);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteMovimientoAsync(Guid id)
        {
            var entity = await _db.MovimientosFinancieros.FindAsync(id);
            if (entity != null)
            {
                _db.MovimientosFinancieros.Remove(entity);
                await _db.SaveChangesAsync();
            }
        }

        // ─────────────────────────────────────────────────────────
        // VENTAS
        // ─────────────────────────────────────────────────────────

        public async Task<List<Venta>> GetVentasAsync()
            => await _db.Ventas.OrderByDescending(v => v.Date).ToListAsync();

        public async Task<List<VentaDetalle>> GetVentaDetallesAsync(Guid ventaId)
            => await _db.VentaDetalles.Where(d => d.VentaId == ventaId).ToListAsync();

        /// <summary>
        /// Obtiene el historial de ventas en cuenta corriente de un cliente (sólo fiado) 
        /// junto con sus detalles proyectados a texto para armar el PDF.
        /// </summary>
        public async Task<List<VentaFiadaDetalle>> GetVentasFiadasPorClienteAsync(Guid clienteId)
        {
            var ventas = await _db.Ventas
                .Where(v => v.ClienteId == clienteId && v.IsFiado)
                .OrderByDescending(v => v.Date)
                .ToListAsync();

            var r = new List<VentaFiadaDetalle>();
            foreach (var v in ventas)
            {
                var detalles = await _db.VentaDetalles
                    .Where(d => d.VentaId == v.Id)
                    .Join(_db.Productos, d => d.ProductoId, p => p.Id, (d, p) => $"{d.Quantity}x {p.Name} ({d.UnitPrice:C})")
                    .ToListAsync();

                r.Add(new VentaFiadaDetalle
                {
                    NumeroVenta = v.NumeroVenta,
                    Fecha = v.Date,
                    Total = v.Total,
                    Items = detalles
                });
            }
            return r;
        }

        /// <summary>
        /// Procesa una venta de forma completamente atómica usando una transacción EF Core.
        /// Si cualquier paso falla, todos los cambios se revierten (rollback automático).
        /// </summary>
        public async Task<bool> ProcesarVentaAsync(Venta venta, List<VentaDetalle> detalles)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // 1. Validar stock de todos los productos antes de hacer ningún cambio
                foreach (var d in detalles)
                {
                    var producto = await _db.Productos.FindAsync(d.ProductoId)
                        ?? throw new InvalidOperationException($"Producto con ID {d.ProductoId} no encontrado.");

                    if (producto.Stock < d.Quantity)
                        throw new InvalidOperationException($"Stock insuficiente para \"{producto.Name}\". Disponible: {producto.Stock}, solicitado: {d.Quantity}.");
                }

                // 2. Descontar stock
                foreach (var d in detalles)
                {
                    var producto = await _db.Productos.FindAsync(d.ProductoId)!;
                    producto!.Stock -= d.Quantity;
                }

                // 3. Asignar número de venta secuencial
                int maxNumero = await _db.Ventas.AnyAsync()
                    ? await _db.Ventas.MaxAsync(v => v.NumeroVenta)
                    : 0;
                venta.NumeroVenta = maxNumero + 1;

                _db.Ventas.Add(venta);

                // 4. Manejo de pago: Fiado vs Contado
                if (venta.IsFiado && venta.ClienteId.HasValue)
                {
                    var cc = await _db.CuentasCorrientes.FirstOrDefaultAsync(x => x.ClienteId == venta.ClienteId.Value)
                        ?? throw new InvalidOperationException("El cliente no tiene cuenta corriente asociada.");
                    cc.Balance += venta.Total;
                }
                else
                {
                    // Pago contado → registrar ingreso financiero
                    _db.MovimientosFinancieros.Add(new MovimientoFinanciero
                    {
                        Type = TipoMovimiento.Ingreso,
                        Amount = venta.Total,
                        Description = $"Venta #{venta.NumeroVenta}",
                        VentaId = venta.Id
                    });
                }

                // 5. Guardar detalles de venta
                foreach (var d in detalles)
                {
                    d.VentaId = venta.Id;
                    _db.VentaDetalles.Add(d);
                }

                // 6. Commit atómico
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw; // Re-lanzar para que el componente muestre el error al usuario
            }
        }
    }
}