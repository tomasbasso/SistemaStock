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
        public DbSet<Presupuesto> Presupuestos { get; set; }
        public DbSet<PresupuestoDetalle> PresupuestoDetalles { get; set; }

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

            // --- Presupuestos ---
            modelBuilder.Entity<Presupuesto>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Total).HasColumnType("TEXT");
                entity.HasIndex(e => e.NumeroPresupuesto).IsUnique();
                entity.Property(e => e.Notas).HasMaxLength(500);
            });

            // --- Presupuesto Detalles ---
            modelBuilder.Entity<PresupuestoDetalle>(entity =>
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

                // Migración para columna Ubicacion
                command.CommandText = "PRAGMA table_info(Productos);";
                bool hasUbicacion = false;
                using (var reader2 = await command.ExecuteReaderAsync())
                {
                    while (await reader2.ReadAsync())
                    {
                        if (string.Equals(reader2.GetString(1), "Ubicacion", StringComparison.OrdinalIgnoreCase))
                        {
                            hasUbicacion = true;
                            break;
                        }
                    }
                }
                if (!hasUbicacion)
                {
                    command.CommandText = "ALTER TABLE Productos ADD COLUMN Ubicacion TEXT NOT NULL DEFAULT '';";
                    await command.ExecuteNonQueryAsync();
                }

                // Crear tablas de Presupuestos si no existen
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Presupuestos (
                        Id TEXT PRIMARY KEY,
                        NumeroPresupuesto INTEGER NOT NULL UNIQUE,
                        Date TEXT NOT NULL,
                        FechaVencimiento TEXT,
                        Total TEXT NOT NULL,
                        ClienteId TEXT,
                        Notas TEXT NOT NULL DEFAULT ''
                    );
                    CREATE TABLE IF NOT EXISTS PresupuestoDetalles (
                        Id TEXT PRIMARY KEY,
                        PresupuestoId TEXT NOT NULL,
                        ProductoId TEXT NOT NULL,
                        Quantity INTEGER NOT NULL,
                        UnitPrice TEXT NOT NULL
                    );";
                await command.ExecuteNonQueryAsync();
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
                NombreNegocio = "Comercial Kai Ken",
                Moneda = "ARS"
            });


            await SaveChangesAsync();
        }
    }
}