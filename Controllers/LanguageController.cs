using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

public class LanguageController : Controller
{
    public IActionResult SetLanguage(string culture, string returnUrl)
    {
        // Eğer culture değeri boşsa veya geçersizse, varsayılan olarak Türkçeyi ayarla
        if (string.IsNullOrEmpty(culture) || !new[] { "tr", "en", "de", "fr", "ru", "ja", "ko" }.Contains(culture))
        {
            culture = "tr";
        }

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTime.UtcNow.AddYears(1) } // ✅ 1 yıl boyunca çerez olarak sakla
        );

        return LocalRedirect(returnUrl);
    }

}
