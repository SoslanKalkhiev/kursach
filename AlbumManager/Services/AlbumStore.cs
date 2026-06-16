using AlbumManager.Models;

namespace AlbumManager.Services;

public class AlbumStore
{
    private readonly string _root;

    public AlbumStore(IHostEnvironment env)
    {
        _root = Path.Combine(env.ContentRootPath, "App_Data", "albums");
        Directory.CreateDirectory(_root);
    }

    private JsonStore<Album> StoreFor(string userId) =>
        new(Path.Combine(_root, $"{userId}.json"));

    public List<Album> GetForUser(string userId) => StoreFor(userId).ReadAll();

    public List<Album> GetShowcase(string userId, int count) =>
        GetForUser(userId)
            .OrderByDescending(a => a.IsFavorite)
            .ThenByDescending(a => a.Rating ?? 0)
            .Take(count)
            .ToList();

    public Album? Get(string userId, string albumId) =>
        GetForUser(userId).FirstOrDefault(a => a.Id == albumId);

    public void Add(Album album)
    {
        var store = StoreFor(album.OwnerUserId);
        var list = store.ReadAll();
        list.Add(album);
        store.WriteAll(list);
    }

    public bool Update(Album album)
    {
        var store = StoreFor(album.OwnerUserId);
        var list = store.ReadAll();
        var idx = list.FindIndex(a => a.Id == album.Id);
        if (idx < 0) return false;
        list[idx] = album;
        store.WriteAll(list);
        return true;
    }

    public bool Delete(string userId, string albumId)
    {
        var store = StoreFor(userId);
        var list = store.ReadAll();
        var removed = list.RemoveAll(a => a.Id == albumId) > 0;
        if (removed) store.WriteAll(list);
        return removed;
    }

    public AlbumIndexViewModel Query(string userId, AlbumQuery query)
    {
        var all = GetForUser(userId);

        var vm = new AlbumIndexViewModel
        {
            Query = query,
            CountAll = all.Count,
            CountListened = all.Count(a => a.Status == AlbumStatus.Listened),
            CountPlanned = all.Count(a => a.Status == AlbumStatus.Planned),
            CountFavorites = all.Count(a => a.IsFavorite),
            AllGenres = all.SelectMany(a => a.Genres)
                           .Select(g => g.Trim())
                           .Where(g => g.Length > 0)
                           .Distinct(StringComparer.OrdinalIgnoreCase)
                           .OrderBy(g => g, StringComparer.OrdinalIgnoreCase)
                           .ToList()
        };

        var rated = all.Where(a => a.Rating.HasValue).Select(a => a.Rating!.Value).ToList();
        vm.AverageRating = rated.Count > 0 ? Math.Round(rated.Average(), 1) : null;

        vm.TopGenres = all.SelectMany(a => a.Genres)
                          .Select(g => g.Trim())
                          .Where(g => g.Length > 0)
                          .GroupBy(g => g, StringComparer.OrdinalIgnoreCase)
                          .OrderByDescending(g => g.Count())
                          .Take(3)
                          .Select(g => g.Key)
                          .ToList();

        IEnumerable<Album> items = all;

        items = query.Tab switch
        {
            LibraryTab.Listened => items.Where(a => a.Status == AlbumStatus.Listened),
            LibraryTab.Planned => items.Where(a => a.Status == AlbumStatus.Planned),
            LibraryTab.Favorites => items.Where(a => a.IsFavorite),
            _ => items
        };

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            items = items.Where(a =>
                a.Title.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                a.Artist.Contains(s, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Genre))
        {
            items = items.Where(a => a.Genres.Any(g =>
                string.Equals(g.Trim(), query.Genre.Trim(), StringComparison.OrdinalIgnoreCase)));
        }

        items = query.Sort switch
        {
            AlbumSort.Title => items.OrderBy(a => a.Title, StringComparer.OrdinalIgnoreCase),
            AlbumSort.Artist => items.OrderBy(a => a.Artist, StringComparer.OrdinalIgnoreCase),
            AlbumSort.Rating => items.OrderByDescending(a => a.Rating ?? -1),
            AlbumSort.Genre => items.OrderBy(a => a.Genres.FirstOrDefault() ?? "", StringComparer.OrdinalIgnoreCase),
            _ => items.OrderByDescending(a => a.DateAdded)
        };

        vm.Albums = items.ToList();
        return vm;
    }
}
