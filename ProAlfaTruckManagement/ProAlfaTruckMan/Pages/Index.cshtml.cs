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

namespace ProAlfaTruckMan.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHtmlLocalizer _loc;

        [BindProperty]
        public LoginCredentials LoginCreds { get; set; }

        public IndexModel(IHtmlLocalizerFactory htmlLocalizerFactory)
        {
            _loc = htmlLocalizerFactory.Create(Includes.AppShortName + ".Pages.Index", Includes.AppShortName);

            LoginCreds = new LoginCredentials();

        }

        public void OnGet()
        {
            //Message = _loc["Name"];

            //LoginCreds.Name = Request.Cookies[Includes.AppShortName + CookieNames.LoginUserName];
            //LoginCreds.RememberPassword = (Request.Cookies[Includes.AppShortName + CookieNames.RememberLoginUserName] == "1" ? true : false);
            HttpContext.Session.SetString(SessionVariables.IsLogout, "1");
            string culturename = HttpContext.Request.Query["culture"].ToString().Trim();
            AppCulture.Name = ( culturename != "" ? culturename : "sk-SK" );
        }

        public void OnPost()
        {
        }

        public IActionResult OnPostOK()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            HttpContext.Session.SetString(SessionVariables.IsLogout, "0");

            string Num="", ChangePwd = "0";
            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader dr;
            try
            {
                cmd.CommandText = "SELECT num, changepwd FROM sapvendors_pers WHERE inetname=@Inetname";
                cmd.Parameters.Add("@Inetname", SqlDbType.Char).Value = Strings.Encrypt(LoginCreds.Name.Trim());
                cmd.Connection = conn;
                cmd.Connection.Open();
                dr = cmd.ExecuteReader();
                if (dr.Read() && dr.HasRows)   // vždy sa musí nájsť
                {
                    //Get Function
                    Num = dr[0].ToString().Trim();
                    ChangePwd = dr[1].ToString().Trim();
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

            if (Num.Trim() != "")
            {
                CurrentUser.InetName = LoginCreds.Name.Trim();
                CurrentUser.Num = Num;
                return Redirect((ChangePwd == "0" ? "/MainMenu" : "/PasswordChange") + AppCulture.UrlCultureSuffix);
            }
            else
            {
                // Toto by nikdy nemalo nastat
                HttpContext.Session.SetString(SessionVariables.LastError, "Unknown user found in logon procedure");
                return Redirect("/Error" + AppCulture.UrlCultureSuffix);
            }
        }

    }

    public class LoginCredentials
    {
        [Required(ErrorMessage = "Name is required")]
        [DataType(DataType.Text)]
        [CheckUserPwd(ErrorMessage = "Wrong name or password")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

    }

    public class CheckUserPwd : ValidationAttribute
    {
        public CheckUserPwd()
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            LoginCredentials lgc = (LoginCredentials)validationContext.ObjectInstance;
            bool Success = false;
            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader dr;
            try
            {
                cmd.CommandText = "SELECT inetpwd FROM sapvendors_pers pers LEFT JOIN sapvendors vend ON pers.num=vend.num WHERE pers.inetname=@Inetname AND vend.xactive=1";
                cmd.Parameters.Add("@Inetname", SqlDbType.Char).Value = Strings.Encrypt(lgc.Name.Trim());
                cmd.Connection = conn;
                cmd.Connection.Open();
                dr = cmd.ExecuteReader();
                if (dr.Read() && dr.HasRows)
                {
                    //Get Function
                    string PwdFromDb = dr[0].ToString().Trim();
                    string PwdFromInput = lgc.Password.Trim();
                    if (PwdFromDb.Length == 139 && PwdFromDb.Substring(128, 1) == "$")
                    {
                        Success = ( Strings.SHA512( PwdFromInput + Strings.Right(PwdFromDb, 10 ) ).ToLower() == Strings.Left(PwdFromDb, 128) );
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

        public override string FormatErrorMessage(string name)
        {
            return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name);
        }
    }

    public class UserNameExistAdapter : AttributeAdapterBase<CheckUserPwd>
    {
        public UserNameExistAdapter(CheckUserPwd attribute, IStringLocalizer stringLocalizer) : base(attribute, stringLocalizer)
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
