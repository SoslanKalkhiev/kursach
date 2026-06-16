using System.Text.Json;

namespace AlbumManager.Services;


public class JsonStore<T>
{
    private readonly string _path;
    private readonly object _lock = new();

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public JsonStore(string path)
    {
        _path = path;
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
    }

    public List<T> ReadAll()
    {
        lock (_lock)
        {
            if (!File.Exists(_path))
                return new List<T>();

            var json = File.ReadAllText(_path);
            if (string.IsNullOrWhiteSpace(json))
                return new List<T>();

            return JsonSerializer.Deserialize<List<T>>(json, Options) ?? new List<T>();
        }
    }

    public void WriteAll(List<T> items)
    {
        lock (_lock)
        {
            var json = JsonSerializer.Serialize(items, Options);
            File.WriteAllText(_path, json);
        }
    }
}
