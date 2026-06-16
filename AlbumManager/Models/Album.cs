namespace AlbumManager.Models;
public class Album
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string OwnerUserId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Artist { get; set; } = "";
    public DateTime? ReleaseDate { get; set; }
    public List<string> Genres { get; set; } = new();
    public double? Rating { get; set; }
    public AlbumStatus Status { get; set; } = AlbumStatus.Planned;
    public bool IsFavorite { get; set; }
    public string CoverUrl { get; set; } = "";
    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
}
