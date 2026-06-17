using System.Globalization;
using System.Security.Claims;
using AlbumManager.Models;
using AlbumManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlbumManager.Controllers;

[Authorize]
public class AlbumsController : Controller
{
    private readonly AlbumStore _albums;
    private readonly MusicSearchService _search;
    private readonly IWebHostEnvironment _env;

   
    private static readonly Dictionary<string, string> AllowedImages = new()
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp",
        ["image/gif"] = ".gif",
    };

    public AlbumsController(AlbumStore albums, MusicSearchService search, IWebHostEnvironment env)
    {
        _albums = albums;
        _search = search;
        _env = env;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // Главная страница
    [HttpGet]
    public IActionResult Index(AlbumQuery query)
    {
        var vm = _albums.Query(UserId, query);
        return View(vm);
    }

    // Инфо об альбоме
    [HttpGet]
    public IActionResult Details(string id)
    {
        var album = _albums.Get(UserId, id);
        if (album is null) return NotFound();
        return PartialView("_AlbumDetail", album);
    }

    // Добавление/редактирование альбома
    [HttpGet]
    public IActionResult Form(string? id)
    {
        if (string.IsNullOrEmpty(id))
            return PartialView("_AlbumForm", new Album { Id = "" });

        var album = _albums.Get(UserId, id);
        if (album is null) return NotFound();
        return PartialView("_AlbumForm", album);
    }

    [HttpGet]
    public async Task<IActionResult> Search(string q)
    {
        var results = await _search.SearchAsync(q);
        return Json(results);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadCover(IFormFile? file)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Файл не выбран.");
        if (file.Length > 5 * 1024 * 1024)
            return BadRequest("Файл больше 5 МБ.");
        if (!AllowedImages.TryGetValue(file.ContentType, out var ext))
            return BadRequest("Поддерживаются только изображения JPG, PNG, WEBP, GIF.");

        var dir = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(dir);

        var name = $"{Guid.NewGuid():N}{ext}";
        var path = Path.Combine(dir, name);
        await using (var stream = System.IO.File.Create(path))
            await file.CopyToAsync(stream);

        return Json(new { url = $"/uploads/{name}" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Save(
        string? id, string title, string artist, DateTime? releaseDate,
        string? genres, string? rating, AlbumStatus status, bool isFavorite, string? coverUrl)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(artist))
            return BadRequest("Название и исполнитель обязательны.");

        var genreList = (genres ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        double? normRating = null;
        if (!string.IsNullOrEmpty(rating) &&
            double.TryParse(rating, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
        {
            normRating = Math.Clamp(Math.Round(parsed * 2) / 2, 0, 5);
        }

        if (string.IsNullOrEmpty(id))
        {
            var album = new Album
            {
                OwnerUserId = UserId,
                Title = title.Trim(),
                Artist = artist.Trim(),
                ReleaseDate = releaseDate,
                Genres = genreList,
                Rating = normRating,
                Status = status,
                IsFavorite = isFavorite,
                CoverUrl = (coverUrl ?? "").Trim()
            };
            _albums.Add(album);
        }
        else
        {
            var album = _albums.Get(UserId, id);
            if (album is null) return NotFound();

            album.Title = title.Trim();
            album.Artist = artist.Trim();
            album.ReleaseDate = releaseDate;
            album.Genres = genreList;
            album.Rating = normRating;
            album.Status = status;
            album.IsFavorite = isFavorite;
            album.CoverUrl = (coverUrl ?? "").Trim();
            _albums.Update(album);
        }

        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleFavorite(string id)
    {
        var album = _albums.Get(UserId, id);
        if (album is null) return NotFound();
        album.IsFavorite = !album.IsFavorite;
        _albums.Update(album);
        return Ok(new { isFavorite = album.IsFavorite });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(string id)
    {
        return _albums.Delete(UserId, id) ? Ok() : NotFound();
    }
}
