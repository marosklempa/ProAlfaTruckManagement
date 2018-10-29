using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using ProAlfaTruckMan.Models;

namespace ProAlfaTruckMan.Pages
{
    public class UnloadsListModel : PageModel
    {
        private readonly IHtmlLocalizer _loc;
        private readonly IEmailService _emailService;
        private readonly IHostingEnvironment _hostingEnviroment;
        public List<Unload> Unlds { get; set; }
        public bool HasPreviousPage = false;
        public bool HasNextPage = false;
        public int PageIndex = 1, PageSize = 5;

        [BindProperty]
        public ViewData UnloadsListData { get; set; }

        public UnloadsListModel(IHtmlLocalizerFactory htmlLocalizerFactory, IEmailService emailService, IHostingEnvironment env)
        {
            _loc = htmlLocalizerFactory.Create(Includes.AppShortName + ".Pages.UnloadsList", Includes.AppShortName);
            _emailService = emailService;
            _hostingEnviroment = env;
            UnloadsListData = new ViewData();
        }

        public IActionResult OnGet(int? pgidx)
        {
            //Message = _loc["Name"];
            GetFooter();
            if (HttpContext.Session.GetString(SessionVariables.IsLogout).Trim() != "1")
            {
                // Nahranie všetkých vykládok firmy prihláseného užívateľa od dnes do nekonečna
                Tuple<List<Truck_unload>, string> retval = Truck_unloadDataManagement.GetData("", CurrentUser.Num, DateTime.Now, DateTime.Now.AddYears(100));
                List<Truck_unload> trunlds = retval.Item1;
                Unlds = new List<Unload>();
                PageIndex = (pgidx ?? 1);
                HasPreviousPage = (PageIndex > 1);
                HasNextPage = (PageIndex < (double)trunlds.Count / PageSize);   // int delený iným int-0 vždy dáva int (5/2=2), preto pretypujeme jeden int na double a potom už double delené int-om dáva double (5/2=2.5)
                for (int i = 0; i <= trunlds.Count - 1; i++)
                {
                    if (i >= (PageIndex * PageSize) - PageSize && i <= (PageIndex * PageSize) - 1 )
                    {
                        Unlds.Add(new Unload(trunlds[i].Rampid, Strings.Decrypt(trunlds[i].Dsc), trunlds[i].Vendnum, Strings.Decrypt(trunlds[i].Comp), trunlds[i].Datefrom, trunlds[i].Dateto, trunlds[i].Palqty,
                            trunlds[i].Src, trunlds[i].Blocktype, trunlds[i].Remark, trunlds[i].Startrowid, trunlds[i].Rowid, Strings.Decrypt(trunlds[i].Inetname), Strings.Decrypt(trunlds[i].Name),
                            Strings.Decrypt(trunlds[i].Surname)));
                    }
                }
                return Page();
            }
            else
            {
                return Redirect("/Index" + AppCulture.UrlCultureSuffix);
            }
        }

        public IActionResult OnGetGoToPage(string Culture, int PgIdx)
        {
            return OnGet(PgIdx);
        }

        public void OnPost()
        {
        }

        public IActionResult OnPostClose()
        {
            return Redirect("/MainMenu" + AppCulture.UrlCultureSuffix);
        }

        private void GetFooter()
        {
            UnloadsListData.Text = Sapvendors_persDataManagement.GetPersInfo(CurrentUser.InetName);
        }
    }

    public class ViewData
    {
        public string Text { get; set; }
    }

    public class Unload : Truck_unload
    {
        public string Datefrom2
        {
            get
            {
                return base.Datefrom.ToString("d.M.yyyy");
            }
        }

        public string TimeFromTo
        {
            get
            {
                return base.Datefrom.ToString("H:mm") + "-" + base.Dateto.AddSeconds(1).ToString("H:mm");
            }
        }

        public Unload() : base()
        { }

        public Unload(string Rampidx, string Dscx, string Vendnumx, string Compx, DateTime Datefromx, DateTime Datetox, int Palqtyx, int Srcx, int Blocktypex, string Remarkx,
            int Startrowidx, int Rowidx, string Inetnamex, string Namex, string Surnamex)
            : base(Rampidx, Dscx, Vendnumx, Compx, Datefromx, Datetox, Palqtyx, Srcx, Blocktypex, Remarkx, Startrowidx, Rowidx, Inetnamex, Namex, Surnamex)
        {
        }


    }

}
