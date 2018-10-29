using System.Globalization;
using System.Data;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using ProAlfaTruckMan.Models;

namespace ProAlfaTruckMan.Pages
{
    public class PasswordChangeModel : PageModel
    {
        private readonly IHtmlLocalizer _loc;

        [BindProperty]
        public NewPassword NewPwd { get; set; }

        public PasswordChangeModel(IHtmlLocalizerFactory htmlLocalizerFactory)
        {
            _loc = htmlLocalizerFactory.Create(Includes.AppShortName + ".Pages.Index", Includes.AppShortName);

            NewPwd = new NewPassword();
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
            GetFooter();
            if (!ModelState.IsValid)
            {
                return Page();
            }
            string rndstring = Strings.GenerateRandomString(10);
            Sapvendors_persDataManagement.ChangePersPwd(Strings.Encrypt(CurrentUser.InetName), Strings.SHA512(NewPwd.Password1.Trim() + rndstring).ToLower() + "$" + rndstring, "0");
            return Redirect("/MainMenu" + AppCulture.UrlCultureSuffix);
        }

        public IActionResult OnPostCancel()
        {
            return Redirect("/Index" + AppCulture.UrlCultureSuffix);
        }

        private void GetFooter()
        {
            NewPwd.Text = Sapvendors_persDataManagement.GetPersInfo(CurrentUser.InetName);
        }
    }

    public class NewPassword
    {
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(15, MinimumLength = 5, ErrorMessage = "Password lenght have to be from 5 to 15 characters")]
        [PwdNotEqual(ErrorMessage = "Passwords are not equal")]
        [PwdSameAsOld(ErrorMessage = "New password have to be different than old password")]
        public string Password1 { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(15, MinimumLength = 5, ErrorMessage = "Password lenght have to be from 5 to 15 characters")]
        public string Password2 { get; set; }

        public string Text { get; set; }
    }

    public class PwdNotEqual : ValidationAttribute
    {
        public PwdNotEqual()
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                NewPassword lgc = (NewPassword)validationContext.ObjectInstance;

                if (lgc.Password1.Trim() == lgc.Password2.Trim())
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

    public class PwdNotEqualAdapter : AttributeAdapterBase<PwdNotEqual>
    {
        public PwdNotEqualAdapter(PwdNotEqual attribute, IStringLocalizer stringLocalizer) : base(attribute, stringLocalizer)
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

    public class PwdSameAsOld : ValidationAttribute
    {
        public PwdSameAsOld()
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                NewPassword lgc = (NewPassword)validationContext.ObjectInstance;

                bool Success = false;
                SqlConnection conn = new SqlConnection(ConnString.Value);
                SqlCommand cmd = new SqlCommand();
                SqlDataReader dr;
                try
                {
                    cmd.CommandText = "SELECT inetpwd FROM sapvendors_pers pers LEFT JOIN sapvendors vend ON pers.num=vend.num WHERE pers.inetname=@Inetname AND vend.xactive=1";
                    cmd.Parameters.Add("@Inetname", SqlDbType.Char).Value = Strings.Encrypt(CurrentUser.InetName);
                    cmd.Connection = conn;
                    cmd.Connection.Open();
                    dr = cmd.ExecuteReader();
                    if (dr.Read() && dr.HasRows)
                    {
                        //Get Function
                        string PwdFromDb = dr[0].ToString().Trim();
                        string PwdFromInput = lgc.Password1.Trim();
                        if (PwdFromDb.Length == 139 && PwdFromDb.Substring(128, 1) == "$")
                        {
                            Success = !(Strings.SHA512(PwdFromInput + Strings.Right(PwdFromDb, 10)).ToLower() == Strings.Left(PwdFromDb, 128));
                        }
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

                if (Success)
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

    public class PwdSameAsOldAdapter : AttributeAdapterBase<PwdSameAsOld>
    {
        public PwdSameAsOldAdapter(PwdSameAsOld attribute, IStringLocalizer stringLocalizer) : base(attribute, stringLocalizer)
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
