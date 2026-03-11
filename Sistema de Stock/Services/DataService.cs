using Sistema_de_Stock.Models;

namespace Sistema_de_Stock.Services
{
    public class DataService
    {
        public JsonRepository<ConfiguracionApp> ConfigRepo { get; } = new("config.json");
        public JsonRepository<Categoria> CategoriaRepo { get; } = new("categorias.json");
        public JsonRepository<Producto> ProductoRepo { get; } = new("productos.json");
        public JsonRepository<Cliente> ClienteRepo { get; } = new("clientes.json");
        public JsonRepository<CuentaCorriente> CuentaCorrienteRepo { get; } = new("cuentas_corrientes.json");
        public JsonRepository<MovimientoFinanciero> MovimientoRepo { get; } = new("movimientos.json");
        public JsonRepository<Venta> VentaRepo { get; } = new("ventas.json");
        public JsonRepository<VentaDetalle> VentaDetalleRepo { get; } = new("ventas_detalles.json");

        public async Task InitializeAsync()
        {
            var conf = await ConfigRepo.GetAllAsync();
            if (!conf.Any())
            {
                await ConfigRepo.SaveAllAsync(new List<ConfiguracionApp> 
                { 
                    new ConfiguracionApp 
                    { 
                        NombreNegocio = "Mi Negocio Stock"
                    } 
                });
            }

            await SeedMockDataAsync();
        }

        private async Task SeedMockDataAsync()
        {
            var categorias = await CategoriaRepo.GetAllAsync();
            if (categorias.Any()) return; // Already seeded

            // --- Categorias ---
            var catHerramientas = new Categoria { Name = "Herramientas Manuales" };
            var catElectricas = new Categoria { Name = "Herramientas Eléctricas" };
            var catConstruccion = new Categoria { Name = "Materiales de Construcción" };
            
            await CategoriaRepo.SaveAllAsync(new List<Categoria> { catHerramientas, catElectricas, catConstruccion });

            // --- Productos ---
            var prod1 = new Producto { Name = "Martillo Carpintero 600g", SKU = "HT-001", Price = 8500.00m, Stock = 50, StockMinimo = 10, CategoryId = catHerramientas.Id };
            var prod2 = new Producto { Name = "Destornillador Phillips Pz2", SKU = "HT-002", Price = 3200.00m, Stock = 120, StockMinimo = 20, CategoryId = catHerramientas.Id };
            var prod3 = new Producto { Name = "Taladro Percutor 700W Bosch", SKU = "EL-001", Price = 125000.00m, Stock = 15, StockMinimo = 5, CategoryId = catElectricas.Id };
            var prod4 = new Producto { Name = "Amoladora Angular 115mm", SKU = "EL-002", Price = 89000.00m, Stock = 8, StockMinimo = 10, CategoryId = catElectricas.Id }; // Low stock
            var prod5 = new Producto { Name = "Bolsa Cemento Loma Negra 50kg", SKU = "MT-001", Price = 9800.00m, Stock = 200, StockMinimo = 50, CategoryId = catConstruccion.Id };
            var prod6 = new Producto { Name = "Arena Fina x Metro", SKU = "MT-002", Price = 15000.00m, Stock = 0, StockMinimo = 10, CategoryId = catConstruccion.Id }; // Out of stock
            
            await ProductoRepo.SaveAllAsync(new List<Producto> { prod1, prod2, prod3, prod4, prod5, prod6 });

            // --- Clientes ---
            var cli1 = new Cliente { Name = "Juan Pérez", Phone = "11-4567-8910", Address = "Av. Siempre Viva 123" };
            var cli2 = new Cliente { Name = "Constructora El Sol S.A.", Phone = "11-9876-5432", Address = "Calle Comercial 456" };

            await ClienteRepo.SaveAllAsync(new List<Cliente> { cli1, cli2 });

            // --- Cuentas Corrientes ---
            var cc1 = new CuentaCorriente { ClienteId = cli1.Id, Balance = 0 };
            var cc2 = new CuentaCorriente { ClienteId = cli2.Id, Balance = 45500.00m }; // Has debt

            await CuentaCorrienteRepo.SaveAllAsync(new List<CuentaCorriente> { cc1, cc2 });
            
            // --- Movimientos (Initial state) ---
            var mov1 = new MovimientoFinanciero { Type = TipoMovimiento.Ingreso, Amount = 150000, Description = "Capital Inicial" };
            
            // Set to today so it appears in Dashboard
            mov1.Date = DateTime.Today.AddHours(9);

            await MovimientoRepo.SaveAllAsync(new List<MovimientoFinanciero> { mov1 });
        }

        // --- Categories ---
        public async Task<List<Categoria>> GetCategoriasAsync() => await CategoriaRepo.GetAllAsync();
        public async Task SaveCategoriaAsync(Categoria c)
        {
            var list = await GetCategoriasAsync();
            var existing = list.FirstOrDefault(x => x.Id == c.Id);
            if (existing != null) { existing.Name = c.Name; } else { list.Add(c); }
            await CategoriaRepo.SaveAllAsync(list);
        }
        public async Task DeleteCategoriaAsync(Guid id)
        {
            var list = await GetCategoriasAsync();
            list.RemoveAll(x => x.Id == id);
            await CategoriaRepo.SaveAllAsync(list);
        }

        // --- Products ---
        public async Task<List<Producto>> GetProductosAsync() => await ProductoRepo.GetAllAsync();
        public async Task SaveProductoAsync(Producto p)
        {
            var list = await GetProductosAsync();
            var existing = list.FirstOrDefault(x => x.Id == p.Id);
            if (existing != null)
            {
                existing.Name = p.Name;
                existing.SKU = p.SKU;
                existing.CategoryId = p.CategoryId;
                existing.Stock = p.Stock;
                existing.StockMinimo = p.StockMinimo;
                existing.Price = p.Price;
            }
            else { list.Add(p); }
            await ProductoRepo.SaveAllAsync(list);
        }
        public async Task DeleteProductoAsync(Guid id)
        {
            var list = await GetProductosAsync();
            list.RemoveAll(x => x.Id == id);
            await ProductoRepo.SaveAllAsync(list);
        }

        // --- Clients & CC ---
        public async Task<List<Cliente>> GetClientesAsync() => await ClienteRepo.GetAllAsync();
        public async Task SaveClienteAsync(Cliente c)
        {
            var list = await GetClientesAsync();
            var existing = list.FirstOrDefault(x => x.Id == c.Id);
            if (existing != null)
            {
                existing.Name = c.Name;
                existing.Phone = c.Phone;
                existing.Address = c.Address;
            }
            else 
            { 
                list.Add(c); 
                // Create CC automatically
                var ccList = await CuentaCorrienteRepo.GetAllAsync();
                ccList.Add(new CuentaCorriente { ClienteId = c.Id });
                await CuentaCorrienteRepo.SaveAllAsync(ccList);
            }
            await ClienteRepo.SaveAllAsync(list);
        }
        
        public async Task<CuentaCorriente?> GetCuentaCorrienteAsync(Guid clienteId)
        {
            var list = await CuentaCorrienteRepo.GetAllAsync();
            return list.FirstOrDefault(x => x.ClienteId == clienteId);
        }

        // --- Finances ---
        public async Task<List<MovimientoFinanciero>> GetMovimientosAsync() => await MovimientoRepo.GetAllAsync();
        public async Task AddMovimientoAsync(MovimientoFinanciero m)
        {
            var list = await GetMovimientosAsync();
            list.Add(m);
            await MovimientoRepo.SaveAllAsync(list);
        }

        // --- POS / Ventas ---
        public async Task<bool> ProcesarVentaAsync(Venta venta, List<VentaDetalle> detalles)
        {
            // Lock everything sequentially to prevent race conditions during POS
            // Since our JSON repo has a semaphore inside for reading/writing, 
            // reading all and saving all within a single application-level operation isn't purely atomic across files, 
            // but for a local MAUI app, it's sufficient unless another thread is calling at the exact same millisecond.
            
            var productos = await GetProductosAsync();
            
            // 1. Validate Stock strictly
            foreach (var d in detalles)
            {
                var p = productos.FirstOrDefault(x => x.Id == d.ProductoId);
                if (p == null || p.Stock < d.Quantity)
                {
                    throw new Exception($"Stock insuficiente para el producto {(p?.Name ?? "Desconocido")}.");
                }
            }

            // 2. Reduce Stock
            foreach (var d in detalles)
            {
                var p = productos.First(x => x.Id == d.ProductoId);
                p.Stock -= d.Quantity;
            }
            
            // Generate NumeroVenta
            var ventas = await VentaRepo.GetAllAsync();
            venta.NumeroVenta = ventas.Count > 0 ? ventas.Max(x => x.NumeroVenta) + 1 : 1;
            
            ventas.Add(venta);

            // Handle Payment Logic (Fiado vs Contado)
            if (venta.IsFiado && venta.ClienteId.HasValue)
            {
                var ccs = await CuentaCorrienteRepo.GetAllAsync();
                var cc = ccs.FirstOrDefault(x => x.ClienteId == venta.ClienteId.Value);
                if (cc != null)
                {
                    cc.Balance += venta.Total; // Add Debt
                    await CuentaCorrienteRepo.SaveAllAsync(ccs);
                }
            }
            else
            {
                // Contado - Ingreso
                var movs = await MovimientoRepo.GetAllAsync();
                movs.Add(new MovimientoFinanciero
                {
                    Type = TipoMovimiento.Ingreso,
                    Amount = venta.Total,
                    Description = $"Venta #{venta.NumeroVenta}",
                    VentaId = venta.Id
                });
                await MovimientoRepo.SaveAllAsync(movs);
            }

            // Add Details
            var vDetalles = await VentaDetalleRepo.GetAllAsync();
            vDetalles.AddRange(detalles);

            // Save all states
            await ProductoRepo.SaveAllAsync(productos);
            await VentaRepo.SaveAllAsync(ventas);
            await VentaDetalleRepo.SaveAllAsync(vDetalles);

            return true;
        }
    }
}
