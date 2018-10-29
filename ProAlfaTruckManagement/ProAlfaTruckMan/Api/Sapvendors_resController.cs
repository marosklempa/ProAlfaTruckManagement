using System;
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
    public class Sapvendors_resController : Controller
    {
        // GET: api/<controller>
        [HttpGet]
        public string Get()
        {
            return "value";
        }

        // GET: /api/<controller>/GetByNum/452366/1
        [Route("GetByVendorNum/{num}/{rampid}")]
        public IEnumerable<Sapvendor_res> GetByVendorNum(string num, string rampid)
        {
            return Sapvendor_resDataManagement.GetData(1, num, (rampid != "-" ? rampid : ""), null, null);
        }

        // GET: /api/<controller>/GetByNumValidDate/451233/1/2018-05-15
        [Route("GetByVendorNumValidDate/{num}/{rampid}/{date:datetime}")]
        public IEnumerable<Sapvendor_res> GetByVendorNumValidDate(string num, string rampid, DateTime date)
        {
            return Sapvendor_resDataManagement.GetData(1, num, (rampid != "-" ? rampid : ""), date, null);
        }

        // GET: /api/<controller>/GetByRampId/1
        [Route("GetByRampId/{rampid}")]
        public IEnumerable<Sapvendor_res> GetByRampId(string rampid)
        {
            return Sapvendor_resDataManagement.GetData(1, "", rampid, null, null);
        }

        // GET: /api/<controller>/GetByRampIdValidDate/1/2015-05-20
        [Route("GetByRampIdValidDate/{rampid}/{date:datetime}")]
        public IEnumerable<Sapvendor_res> GetByRampIdValidDate(string rampid, DateTime date)
        {
            return Sapvendor_resDataManagement.GetData(1, "", rampid, date, null);
        }

        // GET: /api/<controller>/GetByRampIdDateRange/1/2018-05-15/2018-05-21
        [Route("GetByRampIdDateRange/{rampid}/{date:datetime}/{dateto:datetime}")]
        public IEnumerable<Sapvendor_res> GetByRampIdDateRange(string rampid, DateTime datefrom, DateTime dateto)
        {
            return Sapvendor_resDataManagement.GetData(2, "", rampid, datefrom, dateto);
        }

        // GET: /api/<controller>/AddUpdateDelete/MODIFY/25/1/2018-04-20/2018-04-31/07:00/08:00/0/0/1/1/0/0/0/Poznamka
        [Route("AddUpdateDelete/{mode}/{rowid:int}/{num}/{rampid}/{validfrom}/{validto}/{timefrom}/{timeto}/{mon:int}/{tue:int}/{wed:int}/{thu:int}/{fri:int}/{sat:int}/{sun:int}")]
        public int AddUpdateDelete(string mode, int rowid, string num, string rampid, string validfrom, string validto, string timefrom, string timeto, int mon, int tue, int wed, int thu, int fri, int sat, int sun)
        {
            int result = 0;

            if (mode.ToUpper() == "NEW" || mode.ToUpper() == "MODIFY")
            {
                result = TruckrampFuncs.DetectBlockResColision(mode, rowid, rampid, validfrom, validto, timefrom, timeto, mon, tue, wed, thu, fri, sat, sun);
            }
            if (result == 0  && (mode.ToUpper() == "NEW" || mode.ToUpper() == "MODIFY"))
            {
                // Tento dodavatel sa na tejto rampe nevyklada! (1) (pripad, ze firmu, co ma vyhradenu svoju jednu rampu alebo viac ramp, sa pokusame zapisat na inu rampu)
                result = TruckrampFuncs.AllowReservationVendOnRamp(1, rampid, num) == true ? 0 : -98;
            }
            if (result == 0 && (mode.ToUpper() == "NEW" || mode.ToUpper() == "MODIFY"))
            {
                // Tento dodavatel sa na tejto rampe nevyklada! (2) (pripad, ze firmu, co nema vyhradenu ziadnu svoju rampu, sa pokusame zapisat na vyhradenu rampu inych firiem)
                result = TruckrampFuncs.AllowReservationVendOnRamp(2, rampid, num) == true ? 0 : -99;
            }
            if (result == 0)
            {
                SqlConnection conn = new SqlConnection(ConnString.Value);
                SqlCommand cmd = new SqlCommand();
                try
                {
                    num = num.Trim();
                    rampid = rampid.Trim();
                    timefrom = timefrom.Trim();
                    timeto = timeto.Trim();
                    cmd.Connection = conn;
                    cmd.Connection.Open();
                    cmd.Parameters.Add("@Num", SqlDbType.NVarChar).Value = num;
                    cmd.Parameters.Add("@Rampid", SqlDbType.NVarChar).Value = rampid;
                    cmd.Parameters.Add("@Validfrom", SqlDbType.Date).Value = validfrom;
                    cmd.Parameters.Add("@Validto", SqlDbType.Date).Value = validto;
                    cmd.Parameters.Add("@Timefrom", SqlDbType.NVarChar).Value = timefrom;
                    cmd.Parameters.Add("@Timeto", SqlDbType.NVarChar).Value = timeto;
                    cmd.Parameters.Add("@Mon", SqlDbType.TinyInt).Value = mon;
                    cmd.Parameters.Add("@Tue", SqlDbType.TinyInt).Value = tue;
                    cmd.Parameters.Add("@Wed", SqlDbType.TinyInt).Value = wed;
                    cmd.Parameters.Add("@Thu", SqlDbType.TinyInt).Value = thu;
                    cmd.Parameters.Add("@Fri", SqlDbType.TinyInt).Value = fri;
                    cmd.Parameters.Add("@Sat", SqlDbType.TinyInt).Value = sat;
                    cmd.Parameters.Add("@Sun", SqlDbType.TinyInt).Value = sun;
                    cmd.Parameters.Add("@Rowid", SqlDbType.SmallInt).Value = rowid;
                    switch (mode.ToUpper())
                    {
                        case "NEW":
                            cmd.CommandText = "INSERT INTO sapvendors_res (num,rampid,validfrom,validto,timefrom,timeto,mon,tue,wed,thu,fri,sat,sun) VALUES (@Num, @Rampid, ";
                            cmd.CommandText = cmd.CommandText + "@Validfrom, @Validto, @Timefrom, @Timeto, @Mon, @Tue, @Wed, @Thu, @Fri, @Sat, @Sun);";
                            cmd.CommandText = cmd.CommandText + "SELECT CAST(scope_identity() AS int) AS rowid";
                            result = (Int32)cmd.ExecuteScalar();
                            break;
                        case "MODIFY":
                            cmd.CommandText = "UPDATE sapvendors_res SET num=@Num, rampid=@Rampid, validfrom=@Validfrom, validto=@Validto, timefrom=@Timefrom, timeto=@Timeto, ";
                            cmd.CommandText = cmd.CommandText + "mon=@Mon, tue=@Tue, wed=@Wed, thu=@Thu, fri=@Fri, sat=@Sat, sun=@Sun WHERE rowid=" + rowid.ToString();
                            cmd.ExecuteNonQuery();
                            break;
                        case "DELETE":
                            cmd.CommandText = "DELETE FROM sapvendors_res WHERE rowid=@Rowid";
                            cmd.ExecuteNonQuery();
                            break;
                    }
                    cmd.Dispose();
                }
                catch
                {
                    result = -12;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
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
