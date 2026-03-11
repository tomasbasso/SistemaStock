using System.Text.Json;

namespace Sistema_de_Stock.Services
{
    public class JsonRepository<T> where T : class
    {
        private readonly string _filePath;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public JsonRepository(string fileName)
        {
            _filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
        }

        public async Task<List<T>> GetAllAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (!File.Exists(_filePath))
                {
                    return new List<T>();
                }

                string content = await File.ReadAllTextAsync(_filePath);
                if (string.IsNullOrWhiteSpace(content))
                {
                    return new List<T>();
                }

                return JsonSerializer.Deserialize<List<T>>(content) ?? new List<T>();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task SaveAllAsync(List<T> items)
        {
            await _semaphore.WaitAsync();
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string content = JsonSerializer.Serialize(items, options);
                await File.WriteAllTextAsync(_filePath, content);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task GenerateMockDataIfEmpty(List<T> mockData)
        {
            // Only seed if file doesn't exist to prevent overriding
            if (!File.Exists(_filePath))
            {
                await SaveAllAsync(mockData);
            }
        }
    }
}
