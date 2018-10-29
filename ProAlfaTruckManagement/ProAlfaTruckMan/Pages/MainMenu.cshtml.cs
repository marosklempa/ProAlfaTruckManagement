using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Http;
using ProAlfaTruckMan.Models;

namespace ProAlfaTruckMan.Pages
{
    public class MainMenuModel : PageModel
    {
        private readonly IHtmlLocalizer _loc;

        [BindProperty]
        public VendorPersFooterInfo PersInfo { get; set; }

        public MainMenuModel(IHtmlLocalizerFactory htmlLocalizerFactory)
        {
            _loc = htmlLocalizerFactory.Create(Includes.AppShortName + ".Pages.MainMenu", Includes.AppShortName);
            PersInfo = new VendorPersFooterInfo();
        }

        public IActionResult OnGet()
        {
            //Message = _loc["Name"];
            PersInfo.Text = Sapvendors_persDataManagement.GetPersInfo(CurrentUser.InetName);

            if (HttpContext.Session.GetString(SessionVariables.IsLogout).Trim() != "1")
            {
                return Page();
            }
            else
            {
                return Redirect("/Index" + AppCulture.UrlCultureSuffix);
            }
        }

        public void OnPost()
        {
        }

    }

    public class VendorPersFooterInfo
    {
        public string Text { get; set; }
    }

}
