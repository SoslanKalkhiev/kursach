using AlbumManager.Models;

namespace AlbumManager.Services;
public class DataSeeder
{
    private readonly UserStore _users;
    private readonly AlbumStore _albums;

    public DataSeeder(UserStore users, AlbumStore albums)
    {
        _users = users;
        _albums = albums;
    }

    private static Album New(string title, string artist, List<string> genres, DateTime release,
                             double? rating, AlbumStatus status, bool fav, string cover) =>
        new()
        {
            Title = title, Artist = artist, Genres = genres, ReleaseDate = release,
            Rating = rating, Status = status, IsFavorite = fav, CoverUrl = cover
        };

    public void Seed()
    {
        if (_users.All().Count > 0)
            return;

        var demo = _users.Create("demo", "demo");
        if (demo is null) return;

        var samples = new List<Album>
        {
            New("Wish You Were Here", "Pink Floyd", new() { "Progressive Rock", "Psychedelic" }, new DateTime(1975, 9, 12), 5, AlbumStatus.Listened, true,
                "https://is1-ssl.mzstatic.com/image/thumb/Music211/v4/aa/e0/ab/aae0ab6a-d906-a189-81bf-70b56aa43f7a/886445635843.jpg/600x600bb.jpg"),
            New("The Wall", "Pink Floyd", new() { "Progressive Rock", "Rock Opera" }, new DateTime(1979, 11, 30), 4.5, AlbumStatus.Listened, false,
                "https://is1-ssl.mzstatic.com/image/thumb/Music221/v4/3e/17/ec/3e17ec6d-f980-c64f-19e0-a6fd8bbf0c10/886445635850.jpg/600x600bb.jpg"),
            New("Abbey Road", "The Beatles", new() { "Rock", "Pop" }, new DateTime(1969, 9, 26), 5, AlbumStatus.Listened, true,
                "https://is1-ssl.mzstatic.com/image/thumb/Music211/v4/48/53/43/485343e3-dd6a-0034-faec-f4b6403f8108/13UMGIM63890.rgb.jpg/600x600bb.jpg"),
            New("Thriller", "Michael Jackson", new() { "Pop", "Funk", "Disco" }, new DateTime(1982, 11, 30), 5, AlbumStatus.Listened, false,
                "https://is1-ssl.mzstatic.com/image/thumb/Music115/v4/32/4f/fd/324ffda2-9e51-8f6a-0c2d-c6fd2b41ac55/074643811224.jpg/600x600bb.jpg"),
            New("OK Computer", "Radiohead", new() { "Alternative Rock", "Art Rock" }, new DateTime(1997, 5, 21), 4.5, AlbumStatus.Listened, true,
                "https://is1-ssl.mzstatic.com/image/thumb/Music116/v4/07/60/ba/0760ba0f-148c-b18f-d0ff-169ee96f3af5/634904078164.png/600x600bb.jpg"),
            New("In Rainbows", "Radiohead", new() { "Alternative Rock", "Electronic" }, new DateTime(2007, 10, 10), 4.5, AlbumStatus.Listened, false,
                "https://is1-ssl.mzstatic.com/image/thumb/Music126/v4/dd/50/c7/dd50c790-99ac-d3d0-5ab8-e3891fb8fd52/634904032463.png/600x600bb.jpg"),
            New("Random Access Memories", "Daft Punk", new() { "Electronic", "Disco", "Funk" }, new DateTime(2013, 5, 17), 4, AlbumStatus.Listened, false,
                "https://is1-ssl.mzstatic.com/image/thumb/Music115/v4/e8/43/5f/e8435ffa-b6b9-b171-40ab-4ff3959ab661/886443919266.jpg/600x600bb.jpg"),
            New("Discovery", "Daft Punk", new() { "Electronic", "House" }, new DateTime(2001, 3, 12), 5, AlbumStatus.Listened, true,
                "https://is1-ssl.mzstatic.com/image/thumb/Music221/v4/fd/4a/77/fd4a77db-0ebc-d043-41a2-f32fa1bb0fb4/dj.qrikkdwj.jpg/600x600bb.jpg"),
            New("To Pimp a Butterfly", "Kendrick Lamar", new() { "Hip-Hop", "Jazz Rap" }, new DateTime(2015, 3, 15), 5, AlbumStatus.Listened, false,
                "https://is1-ssl.mzstatic.com/image/thumb/Music112/v4/b5/a6/91/b5a69171-5232-3d5b-9c15-8963802f83dd/15UMGIM15814.rgb.jpg/600x600bb.jpg"),
            New("good kid, m.A.A.d city", "Kendrick Lamar", new() { "Hip-Hop", "West Coast" }, new DateTime(2012, 10, 22), 4.5, AlbumStatus.Listened, false,
                "https://is1-ssl.mzstatic.com/image/thumb/Music112/v4/9a/50/a1/9a50a1d8-01c2-2504-8d99-3f2fc7e5c2ff/12UMGIM52988.rgb.jpg/600x600bb.jpg"),
            New("Rumours", "Fleetwood Mac", new() { "Soft Rock", "Pop Rock" }, new DateTime(1977, 2, 4), 4, AlbumStatus.Listened, false,
                "https://is1-ssl.mzstatic.com/image/thumb/Music124/v4/4d/13/ba/4d13bac3-d3d5-7581-2c74-034219eadf2b/081227970949.jpg/600x600bb.jpg"),
            New("Back to Black", "Amy Winehouse", new() { "Soul", "R&B", "Jazz" }, new DateTime(2006, 10, 27), 4.5, AlbumStatus.Listened, true,
                "https://is1-ssl.mzstatic.com/image/thumb/Music112/v4/cf/3f/09/cf3f0994-980d-d8ed-088d-ae89af256b73/15UMGIM24224.rgb.jpg/600x600bb.jpg"),
            New("After Hours", "The Weeknd", new() { "R&B", "Synth-Pop" }, new DateTime(2020, 3, 20), 4, AlbumStatus.Listened, false,
                "https://is1-ssl.mzstatic.com/image/thumb/Music125/v4/2b/b9/fe/2bb9fef5-d7f3-8345-25a9-db0e79fde4e4/20UMGIM11048.rgb.jpg/600x600bb.jpg"),
            New("AM", "Arctic Monkeys", new() { "Indie Rock", "Alternative Rock" }, new DateTime(2013, 9, 9), 4.5, AlbumStatus.Listened, false,
                "https://is1-ssl.mzstatic.com/image/thumb/Music211/v4/69/9c/b5/699cb5d6-115c-ff73-9d26-e57ea4350d72/887828031795.png/600x600bb.jpg"),
            New("Demon Days", "Gorillaz", new() { "Alternative", "Trip-Hop", "Art Pop" }, new DateTime(2005, 5, 11), 4.5, AlbumStatus.Listened, true,
                "https://is1-ssl.mzstatic.com/image/thumb/Music125/v4/1c/0f/81/1c0f818a-e458-dd84-6f1b-ccbdf5fe14d6/825646291045.jpg/600x600bb.jpg"),
            New("My Beautiful Dark Twisted Fantasy", "Kanye West", new() { "Hip-Hop" }, new DateTime(2010, 11, 22), 4.5, AlbumStatus.Listened, false,
                "https://is1-ssl.mzstatic.com/image/thumb/Music124/v4/d1/74/da/d174dacf-5782-dfe2-19f7-ce037dcd0237/00602527584935.rgb.jpg/600x600bb.jpg"),
            New("The Rise and Fall of Ziggy Stardust", "David Bowie", new() { "Glam Rock", "Rock" }, new DateTime(1972, 6, 16), 4.5, AlbumStatus.Listened, false,
                "https://is1-ssl.mzstatic.com/image/thumb/Music114/v4/5f/fa/56/5ffa56c2-ea1f-7a17-6bad-192ff9b6476d/825646124206.jpg/600x600bb.jpg"),
            New("Mezzanine", "Massive Attack", new() { "Trip-Hop", "Electronic" }, new DateTime(1998, 4, 20), 4, AlbumStatus.Listened, false,
                "https://is1-ssl.mzstatic.com/image/thumb/Music115/v4/0a/98/55/0a98555b-8d9d-3b46-660a-b91261557d17/00724384559953.rgb.jpg/600x600bb.jpg"),
            New("The Slow Rush", "Tame Impala", new() { "Psychedelic", "Synth-Pop" }, new DateTime(2020, 2, 14), null, AlbumStatus.Planned, false,
                "https://is1-ssl.mzstatic.com/image/thumb/Music115/v4/65/e3/e7/65e3e740-b69f-f5cb-f2e6-7dedb5265ac9/19UMGIM96748.rgb.jpg/600x600bb.jpg"),
            New("Lonerism", "Tame Impala", new() { "Psychedelic Rock" }, new DateTime(2012, 10, 5), 4, AlbumStatus.Listened, false,
                "https://is1-ssl.mzstatic.com/image/thumb/Music115/v4/b7/40/9b/b7409bc6-24fa-b956-5613-4be8dc62be06/12UMGIM64219.rgb.jpg/600x600bb.jpg"),
            New("SOS", "SZA", new() { "R&B", "Pop" }, new DateTime(2022, 12, 9), null, AlbumStatus.Planned, false,
                "https://is1-ssl.mzstatic.com/image/thumb/Music122/v4/62/93/13/6293132e-20ff-67ab-3d1f-96bb6797a6ba/196589564955.jpg/600x600bb.jpg"),
            New("Born to Die", "Lana Del Rey", new() { "Baroque Pop", "Alternative" }, new DateTime(2012, 1, 27), null, AlbumStatus.Planned, false,
                "https://is1-ssl.mzstatic.com/image/thumb/Music211/v4/59/10/66/591066ea-3c85-3dfe-ef82-ffdbbcdfc8b9/12UMGIM00033.rgb.jpg/600x600bb.jpg"),
            New("Melodrama", "Lorde", new() { "Art Pop", "Electropop" }, new DateTime(2017, 6, 16), 4, AlbumStatus.Listened, true,
                "https://is1-ssl.mzstatic.com/image/thumb/Music124/v4/58/11/b1/5811b172-e180-25a6-69e6-4385fbbfb5dc/17UM1IM02207.rgb.jpg/600x600bb.jpg"),
        };

        var now = DateTime.UtcNow;
        for (int i = 0; i < samples.Count; i++)
        {
            samples[i].OwnerUserId = demo.Id;
            samples[i].DateAdded = now.AddDays(-(samples.Count - i));
            _albums.Add(samples[i]);
        }
    }
}
