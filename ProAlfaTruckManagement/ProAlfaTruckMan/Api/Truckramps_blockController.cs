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
    public class Truckramps_blockController : Controller
    {
        // GET: api/<controller>
        [HttpGet]
        public string Get()
        {
            return "value";
        }

        // GET: /api/<controller>/GetByRampId/1
        [Route("GetByRampId/{rampid}")]
        public IEnumerable<Truckramp_block> GetByRampId(string rampid)
        {
            return Truckramp_blockDataManagement.GetData(1, rampid, null, null);
        }

        // GET: /api/<controller>/GetByRampIdValidDate/1/2018-05-15
        [Route("GetByRampIdValidDate/{rampid}/{date:datetime}")]
        public IEnumerable<Truckramp_block> GetByRampIdValidDate(string rampid, DateTime date)
        {
            return Truckramp_blockDataManagement.GetData(1, rampid, date, null);
        }

        // GET: /api/<controller>/GetByRampIdDateRange/1/2018-05-15/2018-05-21
        [Route("GetByRampIdDateRange/{rampid}/{datefrom:datetime}/{dateto:datetime}")]
        public IEnumerable<Truckramp_block> GetByRampIdDateRange(string rampid, DateTime datefrom, DateTime dateto)
        {
            return Truckramp_blockDataManagement.GetData(2, rampid, datefrom, dateto);
        }

        // GET: /api/<controller>/AddUpdateDelete/MODIFY/25/1/2018-04-20/2018-04-31/07:00/08:00/0/0/1/1/0/0/0/1/Poznamka
//        [Route("AddUpdateDelete/{mode}/{rowid:int}/{rampid}/{validfrom}/{validto}/{timefrom}/{timeto}/{mon:int}/{tue:int}/{wed:int}/{thu:int}/{fri:int}/{sat:int}/{sun:int}/{jumpable:int}/{remark}")]
        [Route("AddUpdateDelete/{mode}")]
        public int AddUpdateDelete(string mode, int rowid, string rampid, string validfrom, string validto, string timefrom, string timeto, int mon, int tue, int wed, int thu, int fri, int sat, int sun, int jumpable, string remark)
        {
            int result = 0;

            if (mode.ToUpper() == "NEW" || mode.ToUpper() == "MODIFY")
            {
                result = TruckrampFuncs.DetectBlockResColision(mode, rowid, rampid, validfrom, validto, timefrom, timeto, mon, tue, wed, thu, fri, sat, sun);
            }
            if (result == 0)
            {
                SqlConnection conn = new SqlConnection(ConnString.Value);
                SqlCommand cmd = new SqlCommand();
                try
                {
                    rampid = rampid.Trim();
                    timefrom = timefrom.Trim();
                    timeto = timeto.Trim();
                    remark = (remark.Trim() != "-" ? remark.Trim() : "");
                    cmd.Connection = conn;
                    cmd.Connection.Open();
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
                    cmd.Parameters.Add("@Jumpable", SqlDbType.TinyInt).Value = jumpable;
                    cmd.Parameters.Add("@Remark", SqlDbType.NVarChar).Value = remark;
                    cmd.Parameters.Add("@Rowid", SqlDbType.SmallInt).Value = rowid;
                    switch (mode.ToUpper())
                    {
                        case "NEW":
                            cmd.CommandText = "INSERT INTO truckramps_block (rampid,validfrom,validto,timefrom,timeto,mon,tue,wed,thu,fri,sat,sun,jumpable,remark) ";
                            cmd.CommandText = cmd.CommandText + "VALUES (@Rampid, @Validfrom, @Validto, @Timefrom, @Timeto, @Mon, @Tue, @Wed, @Thu, @Fri, @Sat, @Sun, @Jumpable, @Remark);";
                            cmd.CommandText = cmd.CommandText + "SELECT CAST(scope_identity() AS int) AS rowid";
                            result = (Int32)cmd.ExecuteScalar();
                            cmd.Dispose();
                            break;
                        case "MODIFY":
                            cmd.CommandText = "UPDATE truckramps_block SET rampid=@Rampid, validfrom=@Validfrom, validto=@Validto, timefrom=@Timefrom, ";
                            cmd.CommandText = cmd.CommandText + "timeto=@Timeto, mon=@Mon, tue=@Tue, wed=@Wed, thu=@Thu, ";
                            cmd.CommandText = cmd.CommandText + "fri=@Fri, sat=@Sat, sun=@Sun, jumpable=@Jumpable, remark=@Remark";
                            cmd.CommandText = cmd.CommandText + "' WHERE rowid=@Rowid";
                            cmd.ExecuteNonQuery();
                            cmd.Dispose();
                            break;
                        case "DELETE":
                            cmd.CommandText = "DELETE FROM truckramps_block WHERE rowid=@Rowid";
                            cmd.ExecuteNonQuery();
                            cmd.Dispose();
                            break;
                    }
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
