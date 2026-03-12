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
                var term = searchTerm.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(term) || 
                                         (p.SKU != null && p.SKU.ToLower().Contains(term)));
            }
            return await query.CountAsync();
        }

        public async Task<List<Producto>> GetProductosPaginadosAsync(int page, int pageSize, string searchTerm = "")
        {
            var query = _db.Productos.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(term) || 
                                         (p.SKU != null && p.SKU.ToLower().Contains(term)));
            }
            return await query.OrderBy(p => p.Name).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
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

        public async Task DeleteProductoAsync(Guid id)
        {
            var entity = await _db.Productos.FindAsync(id);
            if (entity != null)
            {
                _db.Productos.Remove(entity);
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
            var entity = await _db.Clientes.FindAsync(id);
            if (entity != null)
            {
                // Eliminar cuenta corriente asociada
                var cc = await _db.CuentasCorrientes.FirstOrDefaultAsync(x => x.ClienteId == id);
                if (cc != null) _db.CuentasCorrientes.Remove(cc);

                _db.Clientes.Remove(entity);
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
            var movimientos = await _db.MovimientosFinancieros
                .Select(m => new { m.Type, m.Amount })
                .ToListAsync();

            var ingresos = movimientos
                .Where(m => m.Type == TipoMovimiento.Ingreso)
                .Sum(m => m.Amount);
                
            var egresos = movimientos
                .Where(m => m.Type == TipoMovimiento.Egreso)
                .Sum(m => m.Amount);
                
            return (ingresos, egresos);
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