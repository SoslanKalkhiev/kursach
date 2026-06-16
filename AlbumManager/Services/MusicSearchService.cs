using System.Text.Json;

namespace AlbumManager.Services;

public record AlbumSearchResult(string Title, string Artist, string CoverUrl, string Genre, string? ReleaseDate);

// Поиск альбомов через iTunes Search API.
public class MusicSearchService
{
    private readonly HttpClient _http;
    private readonly ILogger<MusicSearchService> _logger;

    public MusicSearchService(HttpClient http, ILogger<MusicSearchService> logger)
    {
        _http = http;
        _logger = logger;
        _http.Timeout = TimeSpan.FromSeconds(8);
    }

    public async Task<List<AlbumSearchResult>> SearchAsync(string query, int limit = 12)
    {
        var results = new List<AlbumSearchResult>();
        if (string.IsNullOrWhiteSpace(query)) return results;

        var url = $"https://itunes.apple.com/search?term={Uri.EscapeDataString(query)}" +
                  $"&entity=album&limit={limit}";

        try
        {
            using var stream = await _http.GetStreamAsync(url);
            using var doc = await JsonDocument.ParseAsync(stream);

            if (!doc.RootElement.TryGetProperty("results", out var items))
                return results;

            foreach (var item in items.EnumerateArray())
            {
                var title = GetString(item, "collectionName");
                var artist = GetString(item, "artistName");
                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(artist))
                    continue;
                var cover = GetString(item, "artworkUrl100").Replace("100x100bb", "600x600bb");
                var genre = GetString(item, "primaryGenreName");
                var release = GetString(item, "releaseDate");
                var releaseDate = release.Length >= 10 ? release[..10] : null; // yyyy-MM-dd

                results.Add(new AlbumSearchResult(title, artist, cover, genre, releaseDate));
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "iTunes search failed for query '{Query}'", query);
        }

        return results;
    }

    private static string GetString(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? (v.GetString() ?? "") : "";
}
