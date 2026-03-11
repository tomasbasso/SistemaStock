using Sistema_de_Stock.Services;

namespace Sistema_de_Stock
{
    public partial class App : Application
    {
        public App(DataService dataService)
        {
            InitializeComponent();
            MainPage = new MainPage();

            // Inicializar la base de datos (aplica migraciones y seeding si es necesario)
            // Se ejecuta al inicio para garantizar que la DB está lista antes de que cualquier
            // componente intente leer datos.
            Task.Run(async () =>
            {
                try
                {
                    await dataService.InitializeAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] Fallo al inicializar la base de datos: {ex.Message}");
                }
            });
        }
    }
}
