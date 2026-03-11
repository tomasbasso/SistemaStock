using Microsoft.EntityFrameworkCore;
using Sistema_de_Stock.Models;

namespace Sistema_de_Stock.Data
{
    /// <summary>
    /// Contexto principal de Entity Framework Core para la base de datos SQLite de la aplicación.
    /// Define el esquema y el seeding inicial de datos.
    /// </summary>
    public class StockDbContext : DbContext
    {
        public StockDbContext(DbContextOptions<StockDbContext> options) : base(options) { }

        public DbSet<ConfiguracionApp> Configuraciones { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<CuentaCorriente> CuentasCorrientes { get; set; }
        public DbSet<MovimientoFinanciero> MovimientosFinancieros { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<VentaDetalle> VentaDetalles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Configuracion ---
            modelBuilder.Entity<ConfiguracionApp>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NombreNegocio).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Moneda).IsRequired().HasMaxLength(10);
                entity.Property(e => e.DireccionNegocio).HasMaxLength(300);
                entity.Property(e => e.Telefono).HasMaxLength(50);
            });

            // --- Categorias ---
            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // --- Productos ---
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.SKU).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.SKU).IsUnique();
                entity.Property(e => e.Price).HasColumnType("TEXT"); // SQLite stores decimals as TEXT
                entity.Property(e => e.CategoryId).IsRequired();
            });

            // --- Clientes ---
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Phone).HasMaxLength(50);
                entity.Property(e => e.Address).HasMaxLength(300);
            });

            // --- Cuentas Corrientes ---
            modelBuilder.Entity<CuentaCorriente>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Balance).HasColumnType("TEXT");
                entity.HasIndex(e => e.ClienteId).IsUnique(); // One CC per client
            });

            // --- Movimientos Financieros ---
            modelBuilder.Entity<MovimientoFinanciero>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasColumnType("TEXT");
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Type).HasConversion<string>(); // Store enum as readable string
            });

            // --- Ventas ---
            modelBuilder.Entity<Venta>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Total).HasColumnType("TEXT");
                entity.HasIndex(e => e.NumeroVenta).IsUnique();
            });

            // --- Venta Detalles ---
            modelBuilder.Entity<VentaDetalle>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitPrice).HasColumnType("TEXT");
            });
        }

        /// <summary>
        /// Crea la base de datos (si no existe) directamente desde el modelo EF Core y ejecuta el seeding inicial.
        /// Se usa EnsureCreatedAsync en lugar de MigrateAsync porque dotnet ef no puede ejecutarse
        /// en proyectos MAUI multi-target. Para una app de usuario único offline, esto es equivalente.
        /// </summary>
        public async Task InitializeDatabaseAsync()
        {
            await Database.EnsureCreatedAsync();

            // === APLICAR MIGRACIONES MANUALES ===
            var connection = Database.GetDbConnection();
            bool wasClosed = connection.State == System.Data.ConnectionState.Closed;
            if (wasClosed) await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA table_info(Productos);";
                bool hasUnidadMedida = false;
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var colName = reader.GetString(1);
                        if (string.Equals(colName, "UnidadMedida", StringComparison.OrdinalIgnoreCase))
                        {
                            hasUnidadMedida = true;
                            break;
                        }
                    }
                }

                if (!hasUnidadMedida)
                {
                    command.CommandText = "ALTER TABLE Productos ADD COLUMN UnidadMedida TEXT NOT NULL DEFAULT 'u.';";
                    await command.ExecuteNonQueryAsync();
                }
            }

            if (wasClosed) await connection.CloseAsync();
            // ====================================

            await SeedInitialDataAsync();
        }

        private async Task SeedInitialDataAsync()
        {
            // Only seed if everything is empty (first run)
            if (await Configuraciones.AnyAsync()) return;

            // --- Configuración inicial ---
            Configuraciones.Add(new ConfiguracionApp
            {
                NombreNegocio = "Mi Ferretería",
                Moneda = "ARS"
            });

            // --- Categorías de ferretería ---
            var catHerramientas = new Categoria { Name = "Herramientas Manuales" };
            var catElectricas = new Categoria { Name = "Herramientas Eléctricas" };
            var catConstruccion = new Categoria { Name = "Materiales de Construcción" };

            Categorias.AddRange(catHerramientas, catElectricas, catConstruccion);

            // --- Productos de muestra ---
            Productos.AddRange(
                new Producto { Name = "Martillo Carpintero 600g", SKU = "HT-001", Price = 8500.00m, Stock = 50, StockMinimo = 10, CategoryId = catHerramientas.Id },
                new Producto { Name = "Destornillador Phillips Pz2", SKU = "HT-002", Price = 3200.00m, Stock = 120, StockMinimo = 20, CategoryId = catHerramientas.Id },
                new Producto { Name = "Taladro Percutor 700W Bosch", SKU = "EL-001", Price = 125000.00m, Stock = 15, StockMinimo = 5, CategoryId = catElectricas.Id },
                new Producto { Name = "Amoladora Angular 115mm", SKU = "EL-002", Price = 89000.00m, Stock = 8, StockMinimo = 10, CategoryId = catElectricas.Id },
                new Producto { Name = "Bolsa Cemento 50kg", SKU = "MT-001", Price = 9800.00m, Stock = 200, StockMinimo = 50, CategoryId = catConstruccion.Id },
                new Producto { Name = "Arena Fina x Metro", SKU = "MT-002", Price = 15000.00m, Stock = 30, StockMinimo = 10, CategoryId = catConstruccion.Id }
            );

            // --- Clientes de muestra ---
            var cli1 = new Cliente { Name = "Juan Pérez", Phone = "11-4567-8910", Address = "Av. Siempre Viva 123" };
            var cli2 = new Cliente { Name = "Constructora El Sol S.A.", Phone = "11-9876-5432", Address = "Calle Comercial 456" };

            Clientes.AddRange(cli1, cli2);

            // --- Cuentas Corrientes (una por cliente) ---
            CuentasCorrientes.AddRange(
                new CuentaCorriente { ClienteId = cli1.Id, Balance = 0 },
                new CuentaCorriente { ClienteId = cli2.Id, Balance = 45500.00m }
            );

            // --- Movimiento financiero inicial ---
            MovimientosFinancieros.Add(new MovimientoFinanciero
            {
                Type = TipoMovimiento.Ingreso,
                Amount = 150000,
                Description = "Capital Inicial",
                Date = DateTime.Today.AddHours(9)
            });

            await SaveChangesAsync();
        }
    }
}
