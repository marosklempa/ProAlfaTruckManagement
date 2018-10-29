using System;
using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProAlfaTruckMan.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProAlfaTruckMan
{
    [Route("api/[controller]")]
    [Authorize]
    public class Sapvendors_persController : Controller
    {
        // GET: api/<controller>
        [HttpGet]
        public string Get()
        {
            return "";
        }

        // GET: /api/<controller>/GetByVendorNum/445696
        [Route("GetByVendorNum/{num}")]
        public IEnumerable<Sapvendor_pers> GetByVendorNum(string num)
        {
            return Sapvendors_persDataManagement.GetData(1, num);
        }

        // GET: /api/<controller>/GetByInetName/jen@sak.sk
//        [Route("GetByInetName/{inetname}")]
        [Route("GetByInetName")]
        public IEnumerable<Sapvendor_pers> GetByInetName(string inetname)
        {
            return Sapvendors_persDataManagement.GetData(2, inetname);
        }

        // GET: /api/<controller>/AddUpdateDelete/NEW/455256/Jan/Bano/jbano/hskj8w/0908710035/jbano@ehs.sk
//        [Route("AddUpdateDelete/{mode}/{num}/{name}/{surname}/{inetname}/{inetpwd}/{mobil}/{email}")]
        [Route("AddUpdateDelete/{mode}")]
        public int AddUpdateDelete(string mode, string num, string name, string surname, string inetname, string inetpwd, string mobil, string email)
        {
            int result = 0;
            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            num = num.Trim();
            try
            {
                switch (mode.ToUpper())
                {
                    case "NEW":
                        cmd.CommandText = "INSERT INTO sapvendors_pers (num, name, surname, inetname, inetpwd, mobil, email, changepwd) ";
                        cmd.CommandText = cmd.CommandText + "VALUES (@Num, @Name, @Surname, @Inetname, @Inetpwd, @Mobil, @Email, 1)";
                        break;
                    case "MODIFY":
                        cmd.CommandText = "UPDATE sapvendors_pers SET num=@Num, name=@Name, surname=@Surname, mobil=@Mobil, email=@Email WHERE inetname=@Inetname";
                        break;
                    case "DELETE":
                        cmd.CommandText = "DELETE FROM sapvendors_pers WHERE inetname=@Inetname";
                        break;
                    default:
                        break;
                }
                cmd.Parameters.Add("@Num", SqlDbType.NVarChar).Value = num;
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = name;
                cmd.Parameters.Add("@Surname", SqlDbType.NVarChar).Value = surname;
                cmd.Parameters.Add("@Inetname", SqlDbType.NVarChar).Value = inetname;
                cmd.Parameters.Add("@Inetpwd", SqlDbType.NVarChar).Value = inetpwd;
                cmd.Parameters.Add("@Mobil", SqlDbType.NVarChar).Value = mobil;
                cmd.Parameters.Add("@Email", SqlDbType.NVarChar).Value = email;
                cmd.Connection = conn;
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            catch
            {
                result = -1;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
            return result;
        }

        // GET: /api/<controller>/ChangePwd/jbano/wlp44qp
//        [Route("ChangePwd/{inetname}/{inetpwd}")]
        [Route("ChangePwd")]
        public int ChangePwd(string inetname, string inetpwd)
        {
            return Sapvendors_persDataManagement.ChangePersPwd(inetname, inetpwd, "1");
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

    }
}
