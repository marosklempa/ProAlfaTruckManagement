using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Http;

namespace ProAlfaTruckMan.Pages
{
    public class LogoutModel : PageModel
    {

        public void OnGet()
        {
            HttpContext.Session.SetString(SessionVariables.IsLogout, "1");
        }

        public IActionResult OnPostOK()
        {
            return Redirect("/Index" + AppCulture.UrlCultureSuffix);
        }
    }
}