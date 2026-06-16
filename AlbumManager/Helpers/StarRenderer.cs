using System.Globalization;

namespace AlbumManager.Helpers;
public static class StarRenderer
{
    public static string Render(double? rating, bool showEmptyLabel = true)
    {
        if (rating is null)
            return showEmptyLabel ? "<span class=\"no-rating\">без оценки</span>" : "";

        var pct = Math.Clamp(rating.Value / 5.0 * 100.0, 0, 100);
        var label = rating.Value.ToString("0.#", CultureInfo.InvariantCulture);
        return
            $"<span class=\"stars\" title=\"{label} из 5\">" +
            "<span class=\"stars-base\">★★★★★</span>" +
            $"<span class=\"stars-fill\" style=\"width:{pct.ToString(CultureInfo.InvariantCulture)}%\">★★★★★</span>" +
            "</span>";
    }
}
