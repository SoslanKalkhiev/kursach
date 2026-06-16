using System.Security.Claims;
using AlbumManager.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace AlbumManager.Controllers;

public class AccountController : Controller
{
    private readonly UserStore _users;

    public AccountController(UserStore users) => _users = users;

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError("", "Введите имя и пароль.");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        var user = _users.Validate(username, password);
        if (user is null)
        {
            ModelState.AddModelError("", "Неверное имя пользователя или пароль.");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        await SignInAsync(user.Id, user.Username);
        return RedirectToLocal(returnUrl);
    }

    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string username, string password, string confirmPassword, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError("", "Введите имя и пароль.");
            return View();
        }
        if (password != confirmPassword)
        {
            ModelState.AddModelError("", "Пароли не совпадают.");
            return View();
        }

        var user = _users.Create(username.Trim(), password);
        if (user is null)
        {
            ModelState.AddModelError("", "Имя пользователя уже занято.");
            return View();
        }

        await SignInAsync(user.Id, user.Username);
        return RedirectToLocal(returnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    private async Task SignInAsync(string userId, string username)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, username)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }

    private IActionResult RedirectToLocal(string? returnUrl) =>
        Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl!) : RedirectToAction("Index", "Albums");
}
