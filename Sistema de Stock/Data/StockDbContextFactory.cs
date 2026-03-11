using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Sistema_de_Stock.Data
{
    /// <summary>
    /// Factory utilizada por EF Core Tools (dotnet ef) en tiempo de diseño para instanciar el StockDbContext.
    /// Esto permite generar y aplicar migraciones sin depender del DI container de MAUI.
    /// </summary>
    public class StockDbContextFactory : IDesignTimeDbContextFactory<StockDbContext>
    {
        public StockDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<StockDbContext>();
            // Use a local path for design-time migrations
            optionsBuilder.UseSqlite("Data Source=stock_design.db");
            return new StockDbContext(optionsBuilder.Options);
        }
    }
}
