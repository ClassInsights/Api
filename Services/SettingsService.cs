using System.Text.Json;

namespace Api.Services;

public class SettingsService
{
    private readonly ILogger<SettingsService> _logger;
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    private const string FilePath = "data/settings.json";
    private Dictionary<string, object> _settingsCache = new();
    private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true, PropertyNameCaseInsensitive = true};
    
    public SettingsService(ILogger<SettingsService> logger)
    {
        _logger = logger;
        _ = LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        if (!File.Exists(FilePath))
            return;

        try
        {
            var json = await File.ReadAllTextAsync(FilePath);
            var settings = JsonSerializer.Deserialize<Dictionary<string, object>>(json, _serializerOptions);
            if (settings != null)
            {
                _settingsCache = settings;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load settings");
        }
    }

    public Task<T?> GetSettingAsync<T>(string key)
    {
        if (!_settingsCache.TryGetValue(key, out var jsonValue))
            return Task.FromResult(default(T));
        try
        {
            return Task.FromResult(JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(jsonValue, _serializerOptions)));
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse settings");
        }
        return Task.FromResult(default(T));
    }

    public async Task SetSettingAsync<T>(string key, T value)
    {
        await _cacheLock.WaitAsync();
        try
        {
            _settingsCache[key] = value!;
            await SaveSettingsToFileAsync();
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    private async Task SaveSettingsToFileAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_settingsCache, _serializerOptions);

            // Atomic Write
            const string tempFile = FilePath + ".tmp";
            await File.WriteAllTextAsync(tempFile, json);
            File.Move(tempFile, FilePath, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings");
        }
    }
}
