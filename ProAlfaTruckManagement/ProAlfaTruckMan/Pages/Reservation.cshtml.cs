using System;
using System.Threading;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Globalization;
using System.Net.Mail;
using System.Net.Mime;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using ProAlfaTruckMan.Models;

namespace ProAlfaTruckMan.Pages
{
    public class ReservationModel : PageModel
    {
        private readonly IHtmlLocalizer _loc;
        private readonly IEmailService _emailService;
        private readonly IHostingEnvironment _hostingEnviroment;
        private readonly IHtmlLocalizerFactory _htmlLocalizerFactory;

        [BindProperty]
        public ReservationData ResData { get; set; }

        public ReservationModel(IHtmlLocalizerFactory htmlLocalizerFactory, IEmailService emailService, IHostingEnvironment env)
        {
            _loc = htmlLocalizerFactory.Create(Includes.AppShortName + ".Pages.Reservation", Includes.AppShortName);
            _emailService = emailService;
            _hostingEnviroment = env;
            _htmlLocalizerFactory = htmlLocalizerFactory;
            ResData = new ReservationData();
        }

        public IActionResult OnGet()
        {
            //Message = _loc["Name"];
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
            if (!ModelState.IsValid)
            {
                GetFooter();
                return Page();
            }
            ResData.Remark = (ResData.Remark != null ? ResData.Remark.Trim() : "");
            bool success = true;
            DateTime resdate;
            int unloadrowid = 0;
            List<TimeSlice> freetimeslices = new List<TimeSlice>();
            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader dr;
            try
            {
                resdate = DateTime.ParseExact(ResData.Date, "yyyy-MM-dd", null);

                // Detaily prihlásenho užívateľa
                List<Sapvendor_pers> pers = Sapvendors_persDataManagement.GetData(2, Strings.Encrypt(CurrentUser.InetName));
                if (pers.Count > 0)   // Vždy by malo byť
                {
                    List<Truckramp> truckramps = new List<Truckramp>();
                    cmd.Connection = conn;
                    cmd.Connection.Open();

                    // Nájdeme rampy, ktoré sú priradené firme prihláseného užívateľa
                    cmd.CommandText = "SELECT sapvendors_ramp.rampid, truckramps.dsc, truckramps.vendres, truckramps.paltime FROM sapvendors_ramp LEFT OUTER JOIN truckramps ON sapvendors_ramp.rampid=truckramps.id ";
                    cmd.CommandText = cmd.CommandText + "WHERE sapvendors_ramp.num=@Num";
                    cmd.Parameters.Add("@Num", SqlDbType.NVarChar).Value = pers[0].Num.Trim();
                    dr = cmd.ExecuteReader();
                    if (dr.HasRows == false)
                    {
                        // Firma prihláseného užívateľa NEMÁ priradené žiadne rampy, preto skusime nájsť všetky rampy, ktoré nie su priradene žiadnej firme
                        dr.Close();
                        cmd.Dispose();
                        cmd.CommandText = "SELECT truckramps.id, truckramps.dsc, truckramps.vendres, truckramps.paltime FROM truckramps ";
                        cmd.CommandText = cmd.CommandText + "EXCEPT SELECT truckramps.id, truckramps.dsc, truckramps.vendres, truckramps.paltime FROM truckramps JOIN sapvendors_ramp ON truckramps.id = sapvendors_ramp.rampid";
                        dr = cmd.ExecuteReader();
                    }
                    if (dr.HasRows)
                    {
                        // Našli sa rampy
                        while (dr.Read())
                        {
                            truckramps.Add(new Truckramp(dr.GetValue(0).ToString().Trim(), Strings.Decrypt(dr.GetValue(1).ToString().Trim()), Int32.Parse(dr.GetValue(2).ToString().Trim()), dr.GetValue(3).ToString().Trim()));
                        }
                    }
                    dr.Close();
                    cmd.Dispose();
                    if (truckramps.Count == 0)
                    {
                        ResData.ErrorMsg = _loc["There is no available ramp for your company"];
                        success = false;
                    }

                    if (success == true)
                    {
                        // Kontrola u každej rampy, že v daný deň je na nej v kalenáry smien zadefinovaná aspoň jedna pracovná smena
                        for (int i = truckramps.Count - 1; i >= 0; i--)
                        {
                            if (WorkCalendar.IsCalWorkDayShiftInDate(truckramps[i].Id, resdate) == 0)
                            {
                                // V daný deň na danej rampe nie je žiadna pracovná smena, vyradíme teda rampu zo zoznamu
                                // !!! Keďže prechádzame zoznam rámp od konca po prvú, môžeme v cykle použiť metódu RemoveAt
                                truckramps.RemoveAt(i);
                            }
                        }
                        if (truckramps.Count == 0)
                        {
                            // Na žiadnej z rámp možných použiť pre firmu prihláseného užívateľa nie je v daný deň pracovná smena
                            ResData.ErrorMsg = _loc["There is no working shifts for selected day on available ramps for your company"];
                            success = false;
                        }
                    }

                    List<TimeSlice> tslices = new List<TimeSlice>();
                    if (success == true)
                    {
                        // Vytvorenie zoznamu časových dielikov (po 10 minútach) pre každú rampu a jej pracovné smeny. Zoznam je zoradeny podľa dátumu/času. Prvý dielik je napr. 2018-06-28 06:00:00-06:09:59 a posledný môže byť ďalší deň o 2018-06-29 05:50:00-05:59:59
                        for (int i = 0; i <= truckramps.Count - 1; i++)
                        {
                            if (WorkCalendar.IsCalWorkDayShiftInDateTime(truckramps[i].Id, resdate.AddHours(6)) == 1)
                            {
                                tslices = WorkCalendar.CreateTimeSlices(truckramps[i].Id, truckramps[i].Dsc, resdate.AddHours(6), 1, Constants.TimeSlicesPerShift, tslices, false);
                            }
                            if (WorkCalendar.IsCalWorkDayShiftInDateTime(truckramps[i].Id, resdate.AddHours(14)) == 1)
                            {
                                tslices = WorkCalendar.CreateTimeSlices(truckramps[i].Id, truckramps[i].Dsc, resdate.AddHours(14), 2, Constants.TimeSlicesPerShift, tslices, false);
                            }
                            if (WorkCalendar.IsCalWorkDayShiftInDateTime(truckramps[i].Id, resdate.AddHours(22)) == 1)
                            {
                                tslices = WorkCalendar.CreateTimeSlices(truckramps[i].Id, truckramps[i].Dsc, resdate.AddHours(22), 3, Constants.TimeSlicesPerShift, tslices, false);
                            }
                        }

                        // Ak má firma minimálny čas príjazdu, tak treba vyradiť u každej rampy časové dieliky, ktoré sú pred minimálnym časom príjazdu
                        List<Sapvendor> vendor = SapvendorDataManagement.GetData(pers[0].Num.Trim());   // Táto metóda vždy vráti práve jeden element zoznamu
                        if (vendor.Count > 0)
                        {
                            // Kontrola, či má firma zapnutý príznak minimálneho času príjazdu
                            if (vendor[0].Xftimeflag == 1)
                            {
                                // Pre minimálny čas príjazdu platí, že môže byť len z rozsahu 06:00-23:59. Nikdy nesmie byť z rozsahu 00:00-05:59
                                DateTime mindate = DateTime.ParseExact(ResData.Date + " " + vendor[0].Xftimeval + ":00", "yyyy-MM-dd HH:mm:ss", null);
                                for (int i = 0; i <= truckramps.Count - 1; i++)
                                {
                                    // V dielikoch rampy nájdeme tie, ktoré majú čas nižší ako minimálny čas príjazdu a také vymažeme zo zoznamu
                                    // Prechádzame dieliky všetkých rámp a hľadáme tie, ktoré patria aktuálnej rampe z nadradeného cyklu
                                    for (int j = tslices.Count - 1; j >= 0; j--)
                                    {
                                        // Zistenie či časový dielik patrí aktuálnej rampe z nadradeného cyklu a zároveň či jeho čas je nižší ako minimálny čas príjazdu
                                        if (truckramps[i].Id == tslices[j].rampid  &&  tslices[j].startdt < mindate)
                                        {
                                            // Ak či časový dielik patrí aktuálnej rampe z nadradeného cyklu a zároveň je jeho čas nižší ako minimálny čas príjazdu, vymažeme ho zo zoznamu dielikov
                                            // !!! Keďže prechádzame zoznam rámp od konca po prvú, môžeme v cykle použiť metódu RemoveAt
                                            tslices.RemoveAt(j);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Nenašla sa firma prihláseného užívateľa
                            // Toto by nemalo nikdy nastať
                            ResData.ErrorMsg = _loc["There is no vendor for user"];
                            success = false;
                        }

                        // Po vyradení časových dielikov pred minimálnym časom príjazdu treba skontrolovať, či ostal v aspoň jednej rampe nejaký čas,
                        // situácia keď neostane žiadny čas môže nastať napr. v prípade, že firma má jednu rampu, čas príchodu je napr. 15:00 a v daný
                        // deň rampa pracuje iba na rannú smenu
                        if (tslices.Count == 0)
                        {
                            // Na žiadnej z rámp možných použiť pre firmu prihláseného užívateľa nie je v daný deň k dispozícii pracovný čas po minimálnom čase príjazdu
                            ResData.ErrorMsg = _loc["There is no working times after minimal arrive time for selected day on available ramps for your company"];
                            success = false;
                        }
                    }

                    if (success == true)
                    {
                        // Zaciatocný čas pracovného dňa, napr. 2018-07-04 06:00:00
                        DateTime workdaystart = DateTime.ParseExact(ResData.Date + " 06:00:00", "yyyy-MM-dd HH:mm:ss", null);
                        // Koncový čas pracovného dňa, napr. 2018-07-05 05:59:59
                        DateTime workdayend = DateTime.ParseExact(ResData.Date + " 06:00:00", "yyyy-MM-dd HH:mm:ss", null).AddHours(24).AddSeconds(-1);
                        // Označenie časových dielikov ako 'obsadené' blokovanými časmi, pevnými rezerváciami a dynamickými rezerváciami
                        foreach (Truckramp ramp in truckramps)
                        {
                            // Nájdenie blokovaných časov každej rampy a uznačenie patričných časových dielikov ako obsadených pre blokované časy
                            TruckrampFuncs.GetBlockResSlices("truckramps_block", ramp.Id, ramp.Dsc, resdate, tslices, cmd, dr, true);
                            // Nájdenie pevne rezervovaných časov každej rampy a uznačenie patričných časových dielikov ako obsadených pre pevne rezervované časy
                            TruckrampFuncs.GetBlockResSlices("sapvendors_res", ramp.Id, ramp.Dsc, resdate, tslices, cmd, dr, false);
                            // Nájdenie dynamicky rezervovaných časov každej rampy a uznačenie patričných časových dielikov ako obsadených pre dynamicky rezervované časy
                            TruckrampFuncs.GetUnloadSlices(ramp.Id, ramp.Dsc, workdaystart, workdayend, tslices, cmd, dr);
                        }

                        // Nájdenie dostatočne veľkého volného časového okna na vykládku v pracovnom čase každej rampy a nakoniec vybratie okna s najnižším časom (časom najbližším k začiatku pracovného dňa)
                        int loadtslices, numoffree, begin_j;
                        bool sliceok;
                        for (int i = 0; i <= truckramps.Count - 1; i++)
                        {
                            // Výpočet počtu 10 minútových dielikov potrebných pre vykládku daného počtu paliet.
                            // Prevedieme čas vykládky jednej palety na danej rampe v tvare MM:SS na sekundy, vynásobíme daným počtom paliet, dostaneme počet sekúnd nutný na vyloženie daného počtu paliet
                            // Počet sekúnd nutný na vyloženie daného počtu paliet vydelíme počtom sekúnd jedného 10 minútového dielika a výsledok zaokrúhlime na nabližšie vyššie celé číslo
                            // Výsledok predstavuje počet 10 minútových dielikov potrebných na vyloženie daného počtu paliet
                            loadtslices = (int)Math.Ceiling((double)TimeFuncs.ConvToS("00:" + truckramps[i].Paltime) * (int)ResData.PalQnty / (Constants.TimeSliceLenghtInMinutes * 60));
                            numoffree = 0; begin_j = -1;
                            // V dielikoch rampy nájdeme prvý súvislý blok (okno) voľných 10 minútových dielikov, ktorých počet je rovný alebo väčší ako počet dielikov potrebných na vykládku daného počtu paliet
                            // Prechádzame dieliky všetkých rámp a hľadáme tie, ktoré patria aktuálnej rampe z nadradeného cyklu
                            for (int j = 0; j <= tslices.Count - 1; j++)
                            {
                                // Zistenie či časový dielik patrí aktuálnej rampe z nadradeného cyklu
                                if (truckramps[i].Id == tslices[j].rampid)
                                {
                                    // Zistenie, či časový dielik je voľný
                                    if (tslices[j].loaded == false)
                                    {
                                        // Časový dielik je voľný
                                        if (begin_j == -1)
                                        {
                                            // Ak je nájdený voľný časový dielik prvý, tak si zapämatáme jeho index
                                            begin_j = j;
                                            sliceok = true;
                                        }
                                        else
                                        {
                                            // Ak je nájdený dielik druhý a ďalší v okne, tak zistíme, či náhodou nie je už z ďalšej smeny oproti prvému dieliku
                                            if (tslices[j].shift - tslices[begin_j].shift <= 1)
                                            {
                                                // Ak je nájdený dielik z tej istej smeny alebo z nasledujúcej smeny, tak je to ok, lebo okno môže začínať na konci prvej a pokračovať do druhej smeny (alebo z druhej do tretej)
                                                sliceok = true;
                                            }
                                            else
                                            {
                                                // Aj je nájdený dielik z nenadväzujúcej smeny, tak ho zahodíme a vynulujeme okno (toto rieši nepravdepodobný prípad, že je pracovná prvá smena a a potom až tretia. T. j. druhá smena je nepracovná.
                                                // Ak by sa okno začínalo na konci prvej smeny a pokračovalo na začiatku tretej, tak to nesmie byť
                                                sliceok = false;
                                            }
                                        }
                                        if (sliceok == true)
                                        {
                                            numoffree = numoffree + 1;
                                            if (numoffree >= loadtslices)
                                            {
                                                freetimeslices.Add(new TimeSlice(truckramps[i].Id, truckramps[i].Dsc, tslices[begin_j].startdt, tslices[j].enddt, 0, false, false));
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            numoffree = 0; begin_j = -1;
                                        }
                                    }
                                    else
                                    {
                                        // Dielik je obsadený. Vtedy zistíme, či je obskočitelný. Ak nie, tak vynulujeme pocitadlo. Ak áno, tak nič nenulujeme.
                                        if (tslices[j].jumpable == false)
                                        {
                                            numoffree = 0; begin_j = -1;
                                        }
                                    }
                                }
                            }
                        }

                        if (freetimeslices.Count > 0)
                        {
                            // Ak sa našli nejaké voľné okná v rampách v daný deň, zoradíme voľné okná podľa začiatočného času a prvá rampa bude tá s najnižším časom voľného okna
                            freetimeslices = freetimeslices.OrderBy(o => o.startdt).ToList();
                            // Zapíšeme do databázy vykládku do nájdeného voľného okna
                            Tuple<int, string> retval = Truck_unloadDataManagement.AddUpdateDelete("NEW", 0, freetimeslices[0].rampid, pers[0].Num, freetimeslices[0].startdt, freetimeslices[0].enddt, (int)ResData.PalQnty, 1, 0,
                                Strings.Encrypt(ResData.Remark), 0, Strings.Encrypt(CurrentUser.InetName), false);
                            unloadrowid = retval.Item1;
                            if (unloadrowid > 0)
                            {

                            }
                            else
                            {
                                // Vyskytla sa neznáma chyba pri zápise dát
                                ResData.ErrorMsg = _loc["Uknown error occured ({0}), ({1})", unloadrowid, retval.Item2];
                                success = false;
                            }
                        }
                        else
                        {
                            // Nenašlo sa žiadne voľné okno na žiadnej rampe v daný deň
                            ResData.ErrorMsg = _loc["There is no available time for selected day on available ramps for your company, try another day"];
                            success = false;
                        }
                    }

                    if (success == true && pers[0].Email.Trim() != "")
                    {
                        // Rezervácia prebehla v poriadku a preto užívateľovi pošleme o tom e-mail
                        string htmlcode = System.IO.File.ReadAllText(_hostingEnviroment.ContentRootPath + "/EmailHTMLTemplate_NewReservation.html");

                        IHtmlLocalizer translate = _htmlLocalizerFactory.Create(Includes.AppShortName + ".Pages.Reservation", Includes.AppShortName).WithCulture(CultureInfo.CreateSpecificCulture(AppCulture.Name));
                        htmlcode = htmlcode.Replace("#$EMAIL-RESERVATION$#", translate["EMAIL-RESERVATION"].Value);
                        htmlcode = htmlcode.Replace("#$EMAIL-TITLE$#", translate["EMAIL-TITLE"].Value);
                        htmlcode = htmlcode.Replace("#$EMAIL-UNLOAD-NUMBER$#", translate["EMAIL-UNLOAD-NUMBER"].Value);
                        htmlcode = htmlcode.Replace("#$EMAIL-UNLOAD-START$#", translate["EMAIL-UNLOAD-START"].Value);
                        htmlcode = htmlcode.Replace("#$EMAIL-UNLOAD-END$#", translate["EMAIL-UNLOAD-END"].Value);
                        htmlcode = htmlcode.Replace("#$EMAIL-PALETTE-QNTY$#", translate["EMAIL-PALETTE-QNTY"].Value);
                        htmlcode = htmlcode.Replace("#$EMAIL-RAMP$#", translate["EMAIL-RAMP"].Value);
                        htmlcode = htmlcode.Replace("#$EMAIL-REMARK$#", translate["EMAIL-REMARK"].Value);
                        htmlcode = htmlcode.Replace("#$EMAIL-FOOTER$#", translate["EMAIL-FOOTER"].Value);

                        htmlcode = htmlcode.Replace("#$VENDOR$#", Sapvendors_persDataManagement.GetPersInfo(CurrentUser.InetName));
                        htmlcode = htmlcode.Replace("#$ROWID$#", unloadrowid.ToString());
                        htmlcode = htmlcode.Replace("#$STARTDATETIME$#", freetimeslices[0].startdt.ToString("d.M.yyyy H:mm"));
                        htmlcode = htmlcode.Replace("#$ENDDATETIME$#", freetimeslices[0].enddt.AddSeconds(1).ToString("d.M.yyyy H:mm"));
                        htmlcode = htmlcode.Replace("#$PALETTES$#", ResData.PalQnty.ToString());
                        htmlcode = htmlcode.Replace("#$RAMP$#", freetimeslices[0].rampdsc);
                        htmlcode = htmlcode.Replace("#$REMARK$#", ResData.Remark);
                        AlternateView avHtml = AlternateView.CreateAlternateViewFromString(htmlcode, null, MediaTypeNames.Text.Html);

                        LinkedResource inline = new LinkedResource(_hostingEnviroment.WebRootPath + "/images/vg_logo.jpg", MediaTypeNames.Image.Jpeg);
                        inline.ContentId = "vg_logo.jpg";
                        inline.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
                        avHtml.LinkedResources.Add(inline);

                        MailMessage mailMessage = new MailMessage();
                        mailMessage.From = new MailAddress(Constants.ProAlfaTruckManagementEmailAddress);
                        mailMessage.To.Add(Strings.Decrypt(pers[0].Email.Trim()));

                        List<Sapvendor_buy> buyers = Sapvendor_buyDataManagement.GetData(1, pers[0].Num, "");
                        foreach (Sapvendor_buy buyer in buyers)
                        {
                            if (Strings.Decrypt(buyer.Email.Trim()).Trim() != "")
                            {
                                mailMessage.CC.Add(Strings.Decrypt(buyer.Email.Trim()));
                            }
                        }

                        mailMessage.Subject = translate["EMAIL-RESERVATION"].Value;
                        mailMessage.AlternateViews.Add(avHtml);
                        mailMessage.IsBodyHtml = true;

                        _emailService.Send(mailMessage, _hostingEnviroment);
                    }
                }
                else
                {
                    success = false;
                }
            }
            catch (Exception ex)
            {
                success = false;
                HttpContext.Session.SetString(SessionVariables.LastError, ex.Message);
                return RedirectToPage("/Error");
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            if (success == true)
            {
                //Presmerovanie na stránku s informáciami o rezervácii
                return RedirectToPage("/ReservationSummary", "Values", new
                {
                    Rowid = unloadrowid,
                    Start = freetimeslices[0].startdt.ToString("d.M.yyyy H:mm"),
                    End = freetimeslices[0].enddt.AddSeconds(1).ToString("d.M.yyyy H:mm"),
                    Qnty = ResData.PalQnty,
                    RampDsc = freetimeslices[0].rampdsc,
                    Remark = ResData.Remark,
                    culture = AppCulture.Name
                });
            }
            else
            {
                GetFooter();
                return Page();
            }
        }

        public IActionResult OnPostCancel()
        {
            return Redirect("/MainMenu" + AppCulture.UrlCultureSuffix);
        }

        private void GetFooter()
        {
            ResData.Text = Sapvendors_persDataManagement.GetPersInfo(CurrentUser.InetName);
        }
    }

    public class ReservationData
    {
        [Required(ErrorMessage = "Palette quantity is required")]
        [Range(1, 99, ErrorMessage = "Palette quantity have to be from range 1 to 99")]
        [DataType(DataType.Text)]
        public int? PalQnty { get; set; }

        [Required(ErrorMessage = "Unload date is required")]
        [DataType(DataType.Date)]
        [CheckCalWorkDay(ErrorMessage = "Non working warehouse day")]
        public string Date { get; set; }

        [DataType(DataType.Text)]
        public string Remark { get; set; }

        public string Text { get; set; }
        public LocalizedHtmlString ErrorMsg { get; set; }
    }

    public class CheckCalWorkDay : ValidationAttribute
    {
        public CheckCalWorkDay()
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                ReservationData rsd = (ReservationData)validationContext.ObjectInstance;
                if (rsd.Date == null)
                {
                    // if date is empty, we do not check whether it is working day
                    return ValidationResult.Success;
                }
                bool workday = (WorkCalendar.IsCalWorkDay(DateTime.ParseExact(rsd.Date + " 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)) == 1);
                if (workday)
                {
                    return ValidationResult.Success;
                }
                else
                {
                    var errorMessage = FormatErrorMessage(validationContext.DisplayName);
                    return new ValidationResult(errorMessage);
                }
            }
            else
            {
                return ValidationResult.Success;
            }
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name);
        }
    }

    public class CalWorkDayAdapter : AttributeAdapterBase<CheckCalWorkDay>
    {
        public CalWorkDayAdapter(CheckCalWorkDay attribute, IStringLocalizer stringLocalizer) : base(attribute, stringLocalizer)
        {
        }

        public override void AddValidation(ClientModelValidationContext context)
        {
        }

        public override string GetErrorMessage(ModelValidationContextBase validationContext)
        {
            return GetErrorMessage(validationContext.ModelMetadata, validationContext.ModelMetadata.GetDisplayName());
        }
    }

}
