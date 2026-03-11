using CommunityToolkit.Maui.Storage;
using Sistema_de_Stock.Data;
using System.IO;

namespace Sistema_de_Stock.Services
{
    public class BackupService
    {
        private readonly string _dbPath;

        public BackupService()
        {
            _dbPath = Path.Combine(FileSystem.AppDataDirectory, "stock.db");
        }

        public async Task<bool> ExportBackupAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (!File.Exists(_dbPath))
                    return false;

                using var stream = new FileStream(_dbPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var fileName = $"Backup_Stock_{DateTime.Now:yyyyMMdd_HHmm}.db";
                // MAUI dialogs deben ejecutarse en el MainThread
                var fileSaverResult = await MainThread.InvokeOnMainThreadAsync(async () => 
                {
                    return await FileSaver.Default.SaveAsync(fileName, stream, cancellationToken);
                });
                return fileSaverResult.IsSuccessful;
            }
            catch (Exception ex)
            {
                // Manejar / loguear excepción
                Console.WriteLine($"Error al exportar: {ex.Message}");
                return false;
            }
        }

        public async Task<(bool Success, string Message)> RestoreBackupAsync()
        {
            try
            {
                // Permitir elegir el archivo .db
                var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".db", ".sqlite", ".sqlite3" } },
                    { DevicePlatform.Android, new[] { "application/octet-stream", "application/x-sqlite3" } }
                });

                // MAUI dialogs deben ejecutarse en el MainThread
                var pickResult = await MainThread.InvokeOnMainThreadAsync(async () => 
                {
                    return await FilePicker.Default.PickAsync(new PickOptions
                    {
                        PickerTitle = "Selecciona el respaldo a restaurar",
                    });
                });

                if (pickResult == null)
                    return (false, "Cancelado por el usuario");

                if (!pickResult.FileName.EndsWith(".db") && !pickResult.FileName.EndsWith(".sqlite"))
                    return (false, "El archivo no tiene el formato correcto (.db o .sqlite)");

                // Cerrar cualquier conexión temporal es difícil en caliente con EF Core si hay Scoped contexts
                // Sin embargo, en MAUI con DB local reescribir el archivo suele funcionar si aseguramos que no se esté escribiendo
                var tempPath = Path.Combine(FileSystem.CacheDirectory, pickResult.FileName);
                
                // Read from picker stream
                using (var stream = await pickResult.OpenReadAsync())
                using (var tempFile = File.OpenWrite(tempPath))
                {
                    await stream.CopyToAsync(tempFile);
                }

                // Move file (overwrite)
                File.Copy(tempPath, _dbPath, overwrite: true);

                return (true, "Respaldo restaurado correctamente. Es recomendable reiniciar la aplicación.");
            }
            catch (Exception ex)
            {
                return (false, $"Error al restaurar: {ex.Message}");
            }
        }
    }
}
