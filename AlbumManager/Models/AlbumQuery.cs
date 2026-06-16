namespace AlbumManager.Models;
public enum LibraryTab
{
    All = 0,
    Listened = 1,
    Planned = 2,
    Favorites = 3
}
public enum AlbumSort
{
    Title = 0,      
    DateAdded = 1,  
    Artist = 2,     
    Rating = 3,    
    Genre = 4      
}

public class AlbumQuery
{
    public string? Search { get; set; }
    public AlbumSort Sort { get; set; } = AlbumSort.DateAdded;
    public LibraryTab Tab { get; set; } = LibraryTab.All;
    public string? Genre { get; set; }
}

public class AlbumIndexViewModel
{
    public AlbumQuery Query { get; set; } = new();
    public List<Album> Albums { get; set; } = new();

    public List<string> AllGenres { get; set; } = new();

    public int CountAll { get; set; }
    public int CountListened { get; set; }
    public int CountPlanned { get; set; }
    public int CountFavorites { get; set; }
    public double? AverageRating { get; set; }
    public List<string> TopGenres { get; set; } = new();
}
