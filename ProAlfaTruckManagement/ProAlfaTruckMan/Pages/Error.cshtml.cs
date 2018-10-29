using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace ProAlfaTruckMan.Pages
{
    public class ErrorModel : PageModel
    {
        public string RequestId { get; set; }
        public string ErrorMessage { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            ErrorMessage = HttpContext.Session.GetString(SessionVariables.LastError).Trim();
        }

        public IActionResult OnPostOK()
        {
            return Redirect("/Index" + AppCulture.UrlCultureSuffix);
        }
    }
}
