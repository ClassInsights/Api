using System.Text.Json;

namespace Api.Services;

public class SettingsService
{
    private readonly string _filePath;
    private readonly Dictionary<string, string> _settingsCache;
    private readonly SemaphoreSlim _cacheLock = new(1, 1);

    public SettingsService(string filePath = "settings.json")
    {
        _filePath = filePath;

        // Load settings into the cache during initialization
        _settingsCache = File.Exists(_filePath)
            ? JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(_filePath)) ?? new Dictionary<string, string>()
            : new Dictionary<string, string>();
    }

    public async Task<string?> GetSettingAsync(string key)
    {
        await _cacheLock.WaitAsync();
        try
        {
            return _settingsCache.TryGetValue(key, out var value) ? value : null;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public async Task SetSettingAsync(string key, string value)
    {
        await _cacheLock.WaitAsync();
        try
        {
            _settingsCache[key] = value;
            await SaveSettingsToFileAsync();
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    private async Task SaveSettingsToFileAsync()
    {
        var json = JsonSerializer.Serialize(_settingsCache);
        await File.WriteAllTextAsync(_filePath, json);
    }
}