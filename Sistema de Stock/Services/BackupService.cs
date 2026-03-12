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
                        FileTypes = customFileType
                    });
                });

                if (pickResult == null)
                    return (false, "Cancelado por el usuario");

                string ext = Path.GetExtension(pickResult.FileName).ToLower();
                if (ext != ".db" && ext != ".sqlite" && ext != ".sqlite3")
                    return (false, $"El archivo '{pickResult.FileName}' no tiene una extensión válida (.db, .sqlite, .sqlite3)");

                // 1. IMPORTANTE: Limpiar los pools de conexiones de SQLite
                Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

                // 2. Copiar el archivo elegido a una ubicación temporal de caché
                var tempPath = Path.Combine(FileSystem.CacheDirectory, "temp_restore.db");
                
                using (var stream = await pickResult.OpenReadAsync())
                {
                    if (stream.Length == 0)
                        return (false, "El archivo seleccionado está vacío.");

                    using (var tempFile = File.Create(tempPath))
                    {
                        await stream.CopyToAsync(tempFile);
                    }
                }

                // 3. Eliminar archivos "sidecar" de SQLite (WAL mode) si existen
                string walPath = _dbPath + "-wal";
                string shmPath = _dbPath + "-shm";

                if (File.Exists(walPath)) File.Delete(walPath);
                if (File.Exists(shmPath)) File.Delete(shmPath);

                // 4. Mover archivo (sobrescribir)
                File.Copy(tempPath, _dbPath, overwrite: true);

                // Limpiar temporal
                if (File.Exists(tempPath)) File.Delete(tempPath);

                return (true, $"Respaldo '{pickResult.FileName}' restaurado correctamente. La aplicación DEBE reiniciarse.");
            }
            catch (Exception ex)
            {
                return (false, $"Error al restaurar: {ex.Message}");
            }
        }
    }
}
