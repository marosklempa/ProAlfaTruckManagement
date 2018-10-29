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
    public class SapvendorsController : Controller
    {
        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<Sapvendor> Get()
        {
            return SapvendorDataManagement.GetData("");
        }

        // GET api/<controller>/456875
        [HttpGet("{num}")]
        public IEnumerable<Sapvendor> Get(string num)
        {
            return SapvendorDataManagement.GetData(num);
        }

        // GET: /api/<controller>/AddUpdateDelete/NEW/457421/Abc s.r.o./Pod rampou 2/90901/Skalica/1/1/1/09:00/1
//        [Route("AddUpdateDelete/{mode}/{num}/{comp}/{str}/{zip}/{city}/{countrycode}/{xactive:int}/{xtrtype:int}/{xftimeflag:int}/{xftimeval}/{rampres:int}")]
        [Route("AddUpdateDelete/{mode}")]
        public int AddUpdateDelete(string mode, string num, string comp, string str, string zip, string city, string countrycode, int xactive, int xtrtype, int xftimeflag, string xftimeval, int rampres)
        {
            int result = 0;
            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            try
            {
                num = num.Trim();
                comp = comp.Trim() != "-" ? comp.Trim() : "";
                str = str.Trim() != "-" ? str.Trim() : "";
                zip = zip.Trim() != "-" ? zip.Trim() : "";
                city = city.Trim() != "-" ? city.Trim() : "";
                countrycode = countrycode.Trim() != "-" ? countrycode.Trim() : "";
                xftimeval = xftimeval.Trim() != "-" ? xftimeval.Trim() : "";
                switch (mode.ToUpper())
                {
                    case "NEW":
                        cmd.CommandText = "INSERT INTO sapvendors (num, comp, str, zip, city, countrycode, xactive, xtrtype, xftimeflag, xftimeval, rampres) ";
                        cmd.CommandText = cmd.CommandText + "VALUES (@Num, @Comp, @Str, @Zip, @City, @Countrycode, @Xactive, @Xtrtype, @Xftimeflag, @Xftimeval, @Rampres)";
                        break;
                    case "MODIFY":
                        cmd.CommandText = "UPDATE sapvendors SET comp=@Comp, str=@Str, zip=@Zip, city=@City, countrycode=@Countrycode, xactive=@Xactive, xtrtype=@Xtrtype, ";
                        cmd.CommandText = cmd.CommandText + "xftimeflag=@Xftimeflag, xftimeval=@Xftimeval, rampres=@Rampres WHERE num=@Num";
                        break;
                    case "DELETE":
                        cmd.CommandText = "DELETE FROM sapvendors WHERE num='" + num + "'";
                        break;
                    default:
                        break;
                }
                cmd.Parameters.Add("@Num", SqlDbType.NVarChar).Value = num;
                cmd.Parameters.Add("@Comp", SqlDbType.NVarChar).Value = comp;
                cmd.Parameters.Add("@Str", SqlDbType.NVarChar).Value = str;
                cmd.Parameters.Add("@Zip", SqlDbType.NVarChar).Value = zip;
                cmd.Parameters.Add("@City", SqlDbType.NVarChar).Value = city;
                cmd.Parameters.Add("@Countrycode", SqlDbType.NChar).Value = countrycode;
                cmd.Parameters.Add("@Xactive", SqlDbType.TinyInt).Value = xactive;
                cmd.Parameters.Add("@Xtrtype", SqlDbType.TinyInt).Value = xtrtype;
                cmd.Parameters.Add("@Xftimeflag", SqlDbType.TinyInt).Value = xftimeflag;
                cmd.Parameters.Add("@Xftimeval", SqlDbType.NVarChar).Value = xftimeval;
                cmd.Parameters.Add("@Rampres", SqlDbType.TinyInt).Value = rampres;
                cmd.Connection = conn;
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                if (mode == "DELETE")
                {
                    cmd.CommandText = "DELETE FROM sapvendors_pers WHERE num=@Num";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "DELETE FROM sapvendors_buy WHERE num=@Num";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "DELETE FROM sapvendors_ramp WHERE num=@Num";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "DELETE FROM sapvendors_res WHERE num=@Num";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "DELETE FROM trucks_unloads WHERE vendnum=@Num";
                    cmd.ExecuteNonQuery();
                }
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
