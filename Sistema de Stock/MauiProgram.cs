using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sistema_de_Stock.Data;
using CommunityToolkit.Maui;

namespace Sistema_de_Stock
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            // ── Base de datos SQLite con EF Core ──────────────────────────
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "stock.db");
            builder.Services.AddDbContext<StockDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"),
                ServiceLifetime.Transient);

            // ── Servicios de la aplicación ────────────────────────────────
            builder.Services.AddTransient<Sistema_de_Stock.Services.DataService>();
            builder.Services.AddSingleton<Sistema_de_Stock.Services.ReportService>();
            builder.Services.AddSingleton<Sistema_de_Stock.Services.NotificationService>();
            builder.Services.AddSingleton<Sistema_de_Stock.Services.PdfService>();
            builder.Services.AddSingleton<Sistema_de_Stock.Services.BackupService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
