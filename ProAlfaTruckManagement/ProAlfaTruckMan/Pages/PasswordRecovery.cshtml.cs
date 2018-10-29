using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace ProAlfaTruckMan.Pages
{
    public class PasswordRecoveryModel : PageModel
    {

        public IActionResult OnGet()
        {
            return Page();
        }

        public IActionResult OnPostOK()
        {
            return Redirect("/Index" + AppCulture.UrlCultureSuffix);
        }
    }
}