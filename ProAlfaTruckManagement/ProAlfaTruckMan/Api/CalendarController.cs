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
    public class CalendarController : Controller
    {
        // GET: api/<controller>
        [HttpGet]
        public string Get()
        {
            return "value";
        }

        // GET: /api/<controller>/GetByYearMonth/2018/05
        [Route("GetByYearMonth/{year:int}/{month:int}")]
        public IEnumerable<Models.Calendar> GetByYearMonth(int year, int month)
        {
            return CalendarDataManagement.GetData(1, year, month, 0, 0, null, null);
        }

        // GET: /api/<controller>/GetByYearWeek/2018/05
        [Route("GetByYearWeek/{year:int}/{week:int}")]
        public IEnumerable<Models.Calendar> GetByYearWeek(int year, int week)
        {
            return CalendarDataManagement.GetData(1, year, 0, 0, week, null, null);
        }

        // GET: /api/<controller>/GetByYearMonthDay/2018/05/10
        [Route("GetByYearMonthDay/{date:datetime}")]
        public IEnumerable<Models.Calendar> GetByYearMonthDay(DateTime date)
        {
            return CalendarDataManagement.GetData(1, date.Year, date.Month, date.Day, 0, null, null);
        }


        // GET: /api/<controller>/GetByDateRange/2018-05-01/2018-05-07
        [Route("GetByDateRange/{datefrom:datetime}/{dateto:datetime}")]
        public IEnumerable<Models.Calendar> GetByDateRange(DateTime datefrom, DateTime dateto)
        {
            return CalendarDataManagement.GetData(2, 0, 0, 0, 0, datefrom, dateto);
        }

        // GET: /api/<controller>/AddUpdateDelete/NEW/4/rampa_4/1/05:00
        [Route("AddUpdateDelete/{date:datetime}/{flag2:int}")]
        public int AddUpdateDelete(DateTime date, int flag2)
        {
            int result = 0;
            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader dr;
            try
            {
                cmd.CommandText = "SELECT flag2 FROM calendar WHERE date=@Date";
                cmd.Parameters.Add("@Date", SqlDbType.Date).Value = date;
                cmd.Connection = conn;
                cmd.Connection.Open();
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    cmd.CommandText = "UPDATE calendar SET flag2=@Flag2 WHERE date=@Date";
                }
                else
                {
                    cmd.CommandText = "INSERT INTO calendar (date, flag2) VALUES (@Date, @Flag2)";
                }
                cmd.Parameters.Add("@Flag2", SqlDbType.TinyInt).Value = flag2;
                dr.Close();
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
