using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Localization;

namespace ProAlfaTruckMan.Pages
{
    public class PrivacyModel : PageModel
    {
        private readonly IHtmlLocalizer _loc;

        public PrivacyModel(IHtmlLocalizerFactory htmlLocalizerFactory)
        {
            _loc = htmlLocalizerFactory.Create(Includes.AppShortName + ".Pages.MainMenu", Includes.AppShortName);
        }

        public void OnGet()
        {
        }
    }
}