using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProAlfaTruckMan.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProAlfaTruckMan
{
    [Route("api/[controller]")]
    [Authorize]
    public class Sapvendors_buyController : Controller
    {
        // GET: api/<controller>
        [HttpGet]
        public string Get()
        {
            return "value";
        }

        // GET: /api/<controller>/GetByRampId/1
        [Route("GetByVendorNum/{num}")]
        public IEnumerable<Sapvendor_buy> GetByVendorNum(string num)
        {
            return Sapvendor_buyDataManagement.GetData(1, num, "");
        }

        // GET: /api/<controller>/GetByRampIdValidDate/1/2018-05-15
//        [Route("GetByVendorNumUserId/{num}/{userid}")]
        [Route("GetByVendorNumUserId/{num}")]
        public IEnumerable<Sapvendor_buy> GetByVendorNumUserId(string num, string userid)
        {
            return Sapvendor_buyDataManagement.GetData(2, num, userid);
        }

        // GET: /api/<controller>/AddDelete/NEW/31100055/maros@foo.com
//        [Route("AddUpdateDelete/{mode}/{num}/{userid}/{email}")]
        [Route("AddUpdateDelete/{mode}")]
        public int AddUpdateDelete(string mode, string num, string userid, string email)
        {
            int result = 0;
            num = num.Trim();
            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            try
            {
                switch (mode.ToUpper())
                {
                    case "NEW":
                        cmd.CommandText = "INSERT INTO sapvendors_buy (num, userid, email) VALUES (@Num, @Userid, @Email)";
                        break;
                    case "MODIFY":
                        cmd.CommandText = "UPDATE sapvendors_buy SET email=@Email WHERE num=@Num AND userid=@Userid";
                        break;
                    case "DELETE":
                        cmd.CommandText = "DELETE FROM sapvendors_buy WHERE num=@Num AND userid=@Userid";
                        break;
                    default:
                        break;
                }
                cmd.Parameters.Add("@Num", SqlDbType.NVarChar).Value = num;
                cmd.Parameters.Add("@Userid", SqlDbType.NVarChar).Value = userid;
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
