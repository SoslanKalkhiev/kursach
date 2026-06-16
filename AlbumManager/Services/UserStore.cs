using AlbumManager.Models;

namespace AlbumManager.Services;

public class UserStore
{
    private readonly JsonStore<User> _store;

    public UserStore(IHostEnvironment env)
    {
        var path = Path.Combine(env.ContentRootPath, "App_Data", "users.json");
        _store = new JsonStore<User>(path);
    }

    public List<User> All() => _store.ReadAll();

    public User? FindByName(string username) =>
        All().FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

    public User? FindById(string id) =>
        All().FirstOrDefault(u => u.Id == id);

    public User? Create(string username, string password)
    {
        var users = _store.ReadAll();
        if (users.Any(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase)))
            return null;

        var (hash, salt) = PasswordHasher.Hash(password);
        var user = new User { Username = username, PasswordHash = hash, PasswordSalt = salt };
        users.Add(user);
        _store.WriteAll(users);
        return user;
    }

    public User? Validate(string username, string password)
    {
        var user = FindByName(username);
        if (user is null) return null;
        return PasswordHasher.Verify(password, user.PasswordHash, user.PasswordSalt) ? user : null;
    }
}
