using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Net.Mail;
using System.Net.Mime;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using ProAlfaTruckMan.Models;

namespace ProAlfaTruckMan.Pages
{
    public class UnloadEditModel : PageModel
    {
        private readonly IHtmlLocalizer _loc;
        private readonly IEmailService _emailService;
        private readonly IHostingEnvironment _hostingEnviroment;
        private readonly IHtmlLocalizerFactory _htmlLocalizerFactory;

        [BindProperty]
        public UnloadEditData UnloadData { get; set; }

        public string EditMode;
        private readonly string TmpSessionVarName1 = "pgidx_8w5r7t52d5wolx";
        private readonly string TmpSessionVarName2 = "rowid_28w2skftu22sd7";

        public UnloadEditModel(IHtmlLocalizerFactory htmlLocalizerFactory, IEmailService emailService, IHostingEnvironment env)
        {
            _loc = htmlLocalizerFactory.Create(Includes.AppShortName + ".Pages.UnloadEdit", Includes.AppShortName);
            UnloadData = new UnloadEditData();
            _emailService = emailService;
            _hostingEnviroment = env;
            _htmlLocalizerFactory = htmlLocalizerFactory;
        }

        public IActionResult OnGet(string Mode, int Rowid, int PgIdx)
        {
            UnloadData.Rowid = Rowid.ToString();
            EditMode = Mode;
            HttpContext.Session.SetString(TmpSessionVarName1, PgIdx.ToString());
            HttpContext.Session.SetString(TmpSessionVarName2, Rowid.ToString());
            GetFooter();

            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader dr;
            try
            {
                cmd.CommandText = "SELECT trucks_unloads.palqnty, trucks_unloads.datefrom, trucks_unloads.dateto, trucks_unloads.remark, truckramps.dsc FROM trucks_unloads ";
                cmd.CommandText = cmd.CommandText + "LEFT OUTER JOIN truckramps ON trucks_unloads.rampid=truckramps.id WHERE rowid=@Rowid";
                cmd.Parameters.Add("@Rowid", SqlDbType.Int).Value = Rowid;
                cmd.Connection = conn;
                cmd.Connection.Open();
                dr = cmd.ExecuteReader();
                if (dr.HasRows)   // vždy by mal mať práve jeden riadok
                {
                    dr.Read();
                    UnloadData.PalQnty = Int32.Parse( dr.GetValue(0).ToString() );
                    UnloadData.DateStart = DateTime.Parse( dr.GetValue(1).ToString(), CultureInfo.CurrentCulture).ToString("d.M.yyyy HH:mm");
                    UnloadData.DateEnd = DateTime.Parse( dr.GetValue(2).ToString(), CultureInfo.CurrentCulture).AddSeconds(1).ToString("d.M.yyyy HH:mm");
                    UnloadData.Remark = Strings.Decrypt(dr.GetValue(3).ToString());
                    UnloadData.Ramp = Strings.Decrypt(dr.GetValue(4).ToString());
                }
                dr.Close();
                cmd.Dispose();
            }
            catch
            {
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }


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

        public IActionResult OnPostDelete()
        {
            GetFooter();

            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader dr;
            try
            {
                // najdenie vykladky
                cmd.CommandText = "SELECT trucks_unloads.palqnty, trucks_unloads.datefrom, trucks_unloads.dateto, trucks_unloads.remark, truckramps.dsc FROM trucks_unloads ";
                cmd.CommandText = cmd.CommandText + "LEFT OUTER JOIN truckramps ON trucks_unloads.rampid=truckramps.id WHERE rowid=@Rowid";
                cmd.Parameters.Add("@Rowid", SqlDbType.Int).Value = Int32.Parse(HttpContext.Session.GetString(TmpSessionVarName2));
                cmd.Connection = conn;
                cmd.Connection.Open();
                dr = cmd.ExecuteReader();
                if (dr.HasRows)   // vždy by mal mať práve jeden riadok
                {
                    dr.Read();
                    UnloadData.PalQnty = Int32.Parse(dr.GetValue(0).ToString());
                    UnloadData.DateStart = DateTime.Parse(dr.GetValue(1).ToString(), CultureInfo.CurrentCulture).ToString("d.M.yyyy HH:mm");
                    UnloadData.DateEnd = DateTime.Parse(dr.GetValue(2).ToString(), CultureInfo.CurrentCulture).AddSeconds(1).ToString("d.M.yyyy HH:mm");
                    UnloadData.Remark = Strings.Decrypt(dr.GetValue(3).ToString());
                    UnloadData.Ramp = dr.GetValue(4).ToString();
                }
                dr.Close();

                cmd.CommandText = "DELETE FROM trucks_unloads WHERE rowid=@Rowid";
                cmd.ExecuteNonQuery();
                cmd.Dispose();

                // Zrušenie vykládky prebehlo v poriadku a preto užívateľovi pošleme o tom e-mail
                string htmlcode = System.IO.File.ReadAllText(_hostingEnviroment.ContentRootPath + "/EmailHTMLTemplate_DeleteReservation.html");

                IHtmlLocalizer translate = _htmlLocalizerFactory.Create(Includes.AppShortName + ".Pages.Reservation", Includes.AppShortName).WithCulture(CultureInfo.CreateSpecificCulture(AppCulture.Name));
                htmlcode = htmlcode.Replace("#$EMAIL-DELETE-RESERVATION$#", translate["EMAIL-DELETE-RESERVATION"].Value);
                htmlcode = htmlcode.Replace("#$EMAIL-TITLE$#", translate["EMAIL-TITLE"].Value);
                htmlcode = htmlcode.Replace("#$EMAIL-UNLOAD-NUMBER$#", translate["EMAIL-UNLOAD-NUMBER"].Value);
                htmlcode = htmlcode.Replace("#$EMAIL-UNLOAD-START$#", translate["EMAIL-UNLOAD-START"].Value);
                htmlcode = htmlcode.Replace("#$EMAIL-UNLOAD-END$#", translate["EMAIL-UNLOAD-END"].Value);
                htmlcode = htmlcode.Replace("#$EMAIL-PALETTE-QNTY$#", translate["EMAIL-PALETTE-QNTY"].Value);
                htmlcode = htmlcode.Replace("#$EMAIL-RAMP$#", translate["EMAIL-RAMP"].Value);
                htmlcode = htmlcode.Replace("#$EMAIL-REMARK$#", translate["EMAIL-REMARK"].Value);
                htmlcode = htmlcode.Replace("#$EMAIL-FOOTER$#", translate["EMAIL-FOOTER"].Value);

                htmlcode = htmlcode.Replace("#$VENDOR$#", Sapvendors_persDataManagement.GetPersInfo(CurrentUser.InetName));
                htmlcode = htmlcode.Replace("#$ROWID$#", HttpContext.Session.GetString(TmpSessionVarName2));
                htmlcode = htmlcode.Replace("#$STARTDATETIME$#", UnloadData.DateStart.ToString(CultureInfo.CurrentCulture));
                htmlcode = htmlcode.Replace("#$ENDDATETIME$#", UnloadData.DateEnd.ToString(CultureInfo.CurrentCulture));
                htmlcode = htmlcode.Replace("#$PALETTES$#", UnloadData.PalQnty.ToString());
                htmlcode = htmlcode.Replace("#$RAMP$#", UnloadData.Ramp);
                htmlcode = htmlcode.Replace("#$REMARK$#", UnloadData.Remark);
                AlternateView avHtml = AlternateView.CreateAlternateViewFromString(htmlcode, null, MediaTypeNames.Text.Html);

                LinkedResource inline = new LinkedResource(_hostingEnviroment.WebRootPath + "/images/vg_logo.jpg", MediaTypeNames.Image.Jpeg);
                inline.ContentId = "vg_logo.jpg";
                inline.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
                avHtml.LinkedResources.Add(inline);

                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(Constants.ProAlfaTruckManagementEmailAddress);

                bool IsEmailAddress = false;
                List<Sapvendor_pers> persons = Sapvendors_persDataManagement.GetData(2, Strings.Encrypt(CurrentUser.InetName));
                foreach (Sapvendor_pers person in persons)
                {
                    if (Strings.Decrypt(person.Email.Trim()).Trim() != "")
                    {
                        mailMessage.To.Add(Strings.Decrypt(person.Email.Trim()));
                        IsEmailAddress = true;
                    }
                }
                List<Sapvendor_buy> buyers = Sapvendor_buyDataManagement.GetData(1, CurrentUser.Num, "");
                foreach (Sapvendor_buy buyer in buyers)
                {
                    if (Strings.Decrypt(buyer.Email.Trim()).Trim() != "")
                    {
                        mailMessage.CC.Add(Strings.Decrypt(buyer.Email.Trim()));
                        IsEmailAddress = true;
                    }
                }
                mailMessage.Subject = translate["EMAIL-DELETE-RESERVATION"].Value;
                mailMessage.AlternateViews.Add(avHtml);
                mailMessage.IsBodyHtml = true;

                if (IsEmailAddress)
                {
                    _emailService.Send(mailMessage, _hostingEnviroment);
                }
            }
            catch
            {
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            return OnPostClose();
        }

        public IActionResult OnPostClose()
        {
            return RedirectToPage("/UnloadsList", "GoToPage",
                new
                {
                    PgIdx = Int32.Parse(HttpContext.Session.GetString(TmpSessionVarName1)),
                    culture = AppCulture.Name
                });
        }

        private void GetFooter()
        {
            UnloadData.Text = Sapvendors_persDataManagement.GetPersInfo(CurrentUser.InetName);
        }
    }

    public class UnloadEditData
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
