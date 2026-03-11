using Microsoft.EntityFrameworkCore;
using Sistema_de_Stock.Data;
using Sistema_de_Stock.Models;
using Sistema_de_Stock.Services;

namespace Sistema_de_Stock.Tests;

/// <summary>
/// Helper para crear un DataService con base de datos SQLite en memoria para cada test.
/// Cada test obtiene su propia DB aislada via el nombre único del test.
/// </summary>
public static class TestDbHelper
{
    public static (DataService service, StockDbContext context) Create(string dbName)
    {
        var options = new DbContextOptionsBuilder<StockDbContext>()
            .UseSqlite($"DataSource=file:{dbName}?mode=memory&cache=shared")
            .Options;

        var context = new StockDbContext(options);
        context.Database.EnsureCreated();

        var service = new DataService(context);
        return (service, context);
    }
}
