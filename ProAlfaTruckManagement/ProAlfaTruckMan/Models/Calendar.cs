using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ProAlfaTruckMan.Models
{
    public class Calendar
    {
        public DateTime Date { get; set; }
        public int Week { get; set; }
        public int Flag2 { get; set; }

        public Calendar( DateTime Datex, int Weekx, int Flag2x )
        {
            Date = Datex;
            Week = Weekx;
            Flag2 = Flag2x;
        }
    }

    public static class CalendarDataManagement
    {
        public static List<Models.Calendar> GetData(int mode, int year, int month, int day, int week, DateTime? datefrom, DateTime? dateto)
        {
            List<Models.Calendar> calendar = new List<Models.Calendar>();
            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader dr;
            try
            {
                cmd.CommandText = "SELECT date,week,flag2 FROM calendar WHERE 1=1 ";
                if (mode == 1)
                {
                    if (year > 0)
                    {
                        cmd.CommandText = cmd.CommandText + "AND YEAR(date)=@Year ";
                    }
                    if (month > 0)
                    {
                        cmd.CommandText = cmd.CommandText + "AND MONTH(date)=@Month ";
                    }
                    if (day > 0)
                    {
                        cmd.CommandText = cmd.CommandText + "AND DAY(date)=@Day ";
                    }
                    if (week > 0)
                    {
                        cmd.CommandText = cmd.CommandText + "AND week=@Week ";
                    }
                }
                else
                {
                    DateTime realdatefrom = (DateTime)datefrom;   // trick to convert nullable datetime to non-nullable datetime
                    DateTime realdateto = (DateTime)dateto;   // trick to convert nullable datetime to non-nullable datetime
                    cmd.CommandText = cmd.CommandText + "AND date>=@Realdatefrom AND date<=@Realdateto ";
                    cmd.Parameters.Add("@Realdatefrom", SqlDbType.Date).Value = realdatefrom;
                    cmd.Parameters.Add("@Realdateto", SqlDbType.Date).Value = realdateto;
                }
                cmd.CommandText = cmd.CommandText + "ORDER BY date";
                cmd.Parameters.Add("@Year", SqlDbType.SmallInt).Value = year;
                cmd.Parameters.Add("@Month", SqlDbType.TinyInt).Value = month;
                cmd.Parameters.Add("@Day", SqlDbType.TinyInt).Value = day;
                cmd.Parameters.Add("@Week", SqlDbType.TinyInt).Value = week;

                cmd.Connection = conn;
                cmd.Connection.Open();
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        //Get Function
                        //calendar.Add(new Calendar("1", DateTime.Now, "1", "0", "0"));
                        calendar.Add(new Calendar(DateTime.Parse(dr.GetValue(0).ToString()), Int32.Parse(dr.GetValue(1).ToString()), Int32.Parse(dr.GetValue(2).ToString())));
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

            return calendar;
        }
    }
}
