using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using ProAlfaTruckMan.Models;

namespace ProAlfaTruckMan.Pages
{
    public class ReservationSummaryModel : PageModel
    {
        private readonly IHtmlLocalizer _loc;

        [BindProperty]
        public ReservationSummaryData ResSummData { get; set; }

        public ReservationSummaryModel(IHtmlLocalizerFactory htmlLocalizerFactory)
        {
            _loc = htmlLocalizerFactory.Create(Includes.AppShortName + ".Pages.ReservationSummary", Includes.AppShortName);
            ResSummData = new ReservationSummaryData();
        }

        public void OnGetValues(int Rowid, string Start, string End, int Qnty, string RampDsc, string Remark)
        {
            ResSummData.Rowid = Rowid.ToString();
            ResSummData.DateStart = Start;
            ResSummData.DateEnd = End;
            ResSummData.PalQnty = Qnty;
            ResSummData.Ramp = RampDsc;
            ResSummData.Remark = Remark;
            GetFooter();
        }

        public IActionResult OnGet()
        {
            GetFooter();

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

        public IActionResult OnPostOK()
        {
            return Redirect("/MainMenu" + AppCulture.UrlCultureSuffix);
        }

        private void GetFooter()
        {
            ResSummData.Text = Sapvendors_persDataManagement.GetPersInfo(CurrentUser.InetName);
        }
    }

    public class ReservationSummaryData
    {
        [DataType(DataType.Text)]
        public int? PalQnty { get; set; }

        [DataType(DataType.Text)]
        public string Rowid { get; set; }

        [DataType(DataType.Text)]
        public string DateStart { get; set; }

        [DataType(DataType.Text)]
        public string DateEnd { get; set; }

        [DataType(DataType.Text)]
        public string Ramp { get; set; }

        [DataType(DataType.Text)]
        public string Remark { get; set; }

        public string Text { get; set; }
    }

}
