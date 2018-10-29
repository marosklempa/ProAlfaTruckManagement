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
    public class Sapvendors_rampController : Controller
    {
        // GET: api/<controller>
        [HttpGet]
        public string Get()
        {
            return "value";
        }

        // GET: /api/<controller>/GetByVendorNum/445696
        [Route("GetByVendorNum/{num}")]
        public IEnumerable<Sapvendor_ramp> GetByVendorNum(string num)
        {
            return Sapvendor_rampDataManagement.GetData(1, num);
        }

        // GET: /api/<controller>/GetByRampId/1
        [Route("GetByRampId/{rampid}")]
        public IEnumerable<Sapvendor_ramp> GetByRampId(string rampid)
        {
            return Sapvendor_rampDataManagement.GetData(2, rampid);
        }

        // GET: /api/<controller>/AddDelete/NEW/455256/1/
        [Route("AddDelete/{mode}/{num}/{rampid}")]
        public int AddDelete(string mode, string num, string rampid)
        {
            int result = 0;
            switch (mode.ToUpper())
            {
                case string xaction when xaction=="NEW":
                    SqlConnection conn = new SqlConnection(ConnString.Value);
                    SqlCommand cmd = new SqlCommand();
                    try
                    {
                        cmd.CommandText = "INSERT INTO sapvendors_ramp (num,rampid) VALUES (@Num, @Rampid)";
                        cmd.Parameters.Add("@Num", SqlDbType.NVarChar).Value = num;
                        cmd.Parameters.Add("@Rampid", SqlDbType.NVarChar).Value = rampid;
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
                    break;
                case "DELETE":
                    result = DeleteRamp(num, rampid);
                    break;
                default:
                    break;
            }
            return result;
        }

        // GET: /api/<controller>/DeleteByVendorNum/456888
        [Route("DeleteByVendorNum/{num}")]
        public int DeleteByVendorNum(string num)
        {
            return DeleteRamp( num, "" );
        }

        // GET: /api/<controller>/DeleteByRampID/1
        [Route("DeleteByRampID/{rampid}")]
        public int DeleteByRampID(string rampid)
        {
            return DeleteRamp( "", rampid);
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

        private int DeleteRamp(string num, string rampid)
        {
            int result = 0;
            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd.CommandText = "DELETE FROM sapvendors_ramp WHERE ";
                if (num.Trim() != "")
                {
                    cmd.CommandText = cmd.CommandText + "num=@Num ";
                }
                if (num.Trim() != "" && rampid.Trim() != "")
                {
                    cmd.CommandText = cmd.CommandText + "AND ";
                }
                if (rampid.Trim() != "")
                {
                    cmd.CommandText = cmd.CommandText + "rampid=@Rampid ";
                }
                cmd.Parameters.Add("@Num", SqlDbType.NVarChar).Value = num;
                cmd.Parameters.Add("@Rampid", SqlDbType.NVarChar).Value = rampid;
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

    }
}
