using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ProAlfaTruckMan.Models
{
    public class Truckramp_cal
    {
        public string Rampid { get; set; }
        public DateTime Date { get; set; }
        public int Shift_morn { get; set; }
        public int Shift_aftl { get; set; }
        public int Shift_night { get; set; }

        public Truckramp_cal( string Rampidx, DateTime Datex, int Shift_mornx, int Shift_aftlx, int Shift_nightx )
        {
            Rampid = Rampidx;
            Date = Datex;
            Shift_morn = Shift_mornx;
            Shift_aftl = Shift_aftlx;
            Shift_night = Shift_nightx;
        }
    }

    public static class Truckramp_calDataManagement
    {
        public static List<Truckramp_cal> GetData(int mode, string rampid, int year, int month, int day, DateTime? datefrom, DateTime? dateto)
        {
            List<Truckramp_cal> truckramp_cal = new List<Truckramp_cal>();
            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader dr;
            try
            {
                cmd.CommandText = "SELECT rampid,date,shift_morn,shift_aftl,shift_night FROM truckramps_cal WHERE rampid='" + rampid.Trim() + "' ";
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
                cmd.Connection = conn;
                cmd.Connection.Open();
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        //Get Function
                        //truckramp_cal.Add(new Truckramp_cal("1", DateTime.Now, "1", "0", "0"));
                        truckramp_cal.Add(new Truckramp_cal(dr.GetValue(0).ToString().Trim(), DateTime.Parse(dr.GetValue(1).ToString()), Int32.Parse(dr.GetValue(2).ToString()),
                            Int32.Parse(dr.GetValue(3).ToString()), Int32.Parse(dr.GetValue(4).ToString())));
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

            return truckramp_cal;
        }

    }
}
