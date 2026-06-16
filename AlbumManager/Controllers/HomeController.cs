using System.Diagnostics;
using AlbumManager.Models;
using AlbumManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlbumManager.Controllers;

public class HomeController : Controller
{
    private readonly UserStore _users;
    private readonly AlbumStore _albums;

    public HomeController(UserStore users, AlbumStore albums)
    {
        _users = users;
        _albums = albums;
    }

    public IActionResult Index()
    {
        var demo = _users.FindByName("demo");
        var showcase = demo is not null ? _albums.GetShowcase(demo.Id, 12) : new List<Album>();
        return View(showcase);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
