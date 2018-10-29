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
    public class Truckramps_calController : Controller
    {
        // GET: api/<controller>
        [HttpGet]
        public string Get()
        {
            return "value";
        }

        // GET: /api/<controller>/GetByRampIdYearMonthDay/1/2018/06/20
        [Route("GetByRampIdYearMonthDay/{rampid}/{year:int}/{month:int}/{day:int}")]
        public IEnumerable<Truckramp_cal> GetByRampIdYearMonthDay(string rampid, int year, int month, int day)
        {
            return Truckramp_calDataManagement.GetData(1, rampid, year, month, day, null, null);
        }

        // GET: /api/<controller>/GetByRampIdDateRange/1/2018-06-20/2018-06-27
        [Route("GetByRampIdDateRange/{rampid}/{datefrom:datetime}/{dateto:datetime}")]
        public IEnumerable<Truckramp_cal> GetByRampIdDateRange(string rampid, DateTime datefrom, DateTime dateto)
        {
            return Truckramp_calDataManagement.GetData(2, rampid, 0, 0, 0, datefrom, dateto);
        }


        // GET: /api/<controller>/Save/1
        [Route("AddUpdateDelete/{rampid}/{date:datetime}/{shift_morn:int}/{shift_aftl:int}/{shift_night:int}")]
        public int AddUpdateDelete(string rampid, DateTime date, int shift_morn, int shift_aftl, int shift_night)
        {
            int retval = 0;
            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader dr;
            try
            {
                cmd.CommandText = "SELECT rampid FROM truckramps_cal WHERE rampid=@Rampid AND date=@Date";
                cmd.Parameters.Add("@Rampid", SqlDbType.NVarChar).Value = rampid;
                cmd.Parameters.Add("@Date", SqlDbType.Date).Value = date;
                cmd.Parameters.Add("@Shift_morn", SqlDbType.TinyInt).Value = shift_morn;
                cmd.Parameters.Add("@Shift_aftl", SqlDbType.TinyInt).Value = shift_aftl;
                cmd.Parameters.Add("@Shift_night", SqlDbType.TinyInt).Value = shift_night;
                cmd.Connection = conn;
                cmd.Connection.Open();
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    cmd.CommandText = "UPDATE truckramps_cal SET shift_morn=@Shift_morn, shift_aftl=@Shift_aftl, shift_night=@Shift_night WHERE rampid=@Rampid AND date=@Date";
                }
                else
                {
                    cmd.CommandText = "INSERT INTO truckramps_cal (rampid, date, shift_morn, shift_aftl, shift_night) VALUES (@Rampid, @Date, @Shift_morn, @Shift_aftl, @Shift_night)";
                }
                dr.Close();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            catch
            {
                retval = -1;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
            return retval;
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
