using CommunityToolkit.Maui.Storage;
using Sistema_de_Stock.Data;
using Sistema_de_Stock.Models;
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

        public async Task<Result<string>> ExportBackupAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (!File.Exists(_dbPath))
                    return Result<string>.Fail("No se encontró el archivo de la base de datos.");

                // Create a temporary path for the backup
                var tempBackupPath = Path.Combine(FileSystem.CacheDirectory, $"Backup_Temp_{Guid.NewGuid()}.db");

                // Use SQLite's online backup API to ensure a fully consistent snapshot
                // This correctly handles WAL (Write-Ahead Logging) mode, merging everything into one file.
                using (var sourceConnection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={_dbPath}"))
                using (var destinationConnection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={tempBackupPath}"))
                {
                    await sourceConnection.OpenAsync(cancellationToken);
                    await destinationConnection.OpenAsync(cancellationToken);

                    sourceConnection.BackupDatabase(destinationConnection);
                    
                    // Cerrar explícitamente antes de salir del bloque using
                    await destinationConnection.CloseAsync();
                    await sourceConnection.CloseAsync();
                }

                // Liberar los handles de archivo de SQLite antes de que FileStream intente abrirlo
                Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
                await Task.Delay(200, cancellationToken); // Pequeño respiro para el OS

                var fileName = $"Backup_Stock_{DateTime.Now:yyyyMMdd_HHmm}.db";
                bool isSuccessful = false;
                Exception? saveException = null;

                // Open the newly created, complete backup file
                using (var stream = new FileStream(tempBackupPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    // MAUI dialogs deben ejecutarse en el MainThread
                    try 
                    {
                        var fileSaverResult = await MainThread.InvokeOnMainThreadAsync(async () => 
                        {
                            return await FileSaver.Default.SaveAsync(fileName, stream, cancellationToken);
                        });
                        isSuccessful = fileSaverResult.IsSuccessful;
                    } 
                    catch (Exception ex) 
                    {
                        saveException = ex;
                    }
                }

                // Clean up the temporary backup file
                if (File.Exists(tempBackupPath))
                {
                    try { File.Delete(tempBackupPath); } catch { /* Ignore cleanup errors */ }
                }

                if (saveException != null)
                    return Result<string>.Fail($"Error interno al guardar: {saveException.Message}");
                    
                if (!isSuccessful)
                    return Result<string>.Fail("La operación fue cancelada por el usuario o falló.");

                return Result<string>.Ok("Backup exportado correctamente");
            }
            catch (Exception ex)
            {
                // Manejar / loguear excepción
                Console.WriteLine($"Error al exportar: {ex.Message}");
                return Result<string>.Fail(ex.Message);
            }
        }

        public async Task<Result<string>> RestoreBackupAsync()
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
                    return Result<string>.Fail("Cancelado por el usuario");

                string ext = Path.GetExtension(pickResult.FileName).ToLower();
                if (ext != ".db" && ext != ".sqlite" && ext != ".sqlite3")
                    return Result<string>.Fail($"El archivo '{pickResult.FileName}' no tiene una extensión válida (.db, .sqlite, .sqlite3)");

                // 1. Limpieza AGRESIVA de conexiones y memoria
                // Forzamos el GC para que disponga de cualquier DbContext o SqliteConnection que haya quedado boyando
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

                // 2. Copiar el archivo elegido a una ubicación temporal de caché
                var tempPath = Path.Combine(FileSystem.CacheDirectory, "temp_restore.db");
                
                using (var stream = await pickResult.OpenReadAsync())
                {
                    if (stream.Length == 0)
                        return Result<string>.Fail("El archivo seleccionado está vacío.");

                    using (var tempFile = File.Create(tempPath))
                    {
                        await stream.CopyToAsync(tempFile);
                    }
                }

                // 3. Reintentos para mover/sobreescribir (evitar "file in use")
                int retries = 5;
                bool success = false;
                string lastError = "";

                while (retries > 0 && !success)
                {
                    try
                    {
                        // Eliminar archivos "sidecar" de SQLite (WAL mode)
                        string walPath = _dbPath + "-wal";
                        string shmPath = _dbPath + "-shm";

                        if (File.Exists(walPath)) File.Delete(walPath);
                        if (File.Exists(shmPath)) File.Delete(shmPath);

                        // Sobrescribir base de datos principal
                        File.Copy(tempPath, _dbPath, overwrite: true);
                        success = true;
                    }
                    catch (IOException)
                    {
                        retries--;
                        if (retries > 0)
                        {
                            // Esperar un poco antes de reintentar
                            await Task.Delay(500); 
                            // Intentar limpiar pools de nuevo
                            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
                        }
                    }
                    catch (Exception ex)
                    {
                        lastError = ex.Message;
                        break;
                    }
                }

                // Limpiar temporal de caché
                if (File.Exists(tempPath)) File.Delete(tempPath);

                if (!success)
                    return Result<string>.Fail($"No se pudo completar la restauración. El archivo sigue bloqueado. {lastError}");

                return Result<string>.Ok($"Respaldo '{pickResult.FileName}' restaurado correctamente. La aplicación DEBE reiniciarse.");
            }
            catch (Exception ex)
            {
                return Result<string>.Fail($"Error al restaurar: {ex.Message}");
            }
        }
    }
}
