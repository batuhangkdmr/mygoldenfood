using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using MyGoldenFood.Services;
using System;

namespace MyGoldenFood.Controllers
{
    public class LanguageController : Controller
    {
        private readonly LocalizationCacheService _translationCacheService;

        public LanguageController(LocalizationCacheService translationCacheService)
        {
            _translationCacheService = translationCacheService;
        }

        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }

    }

}

