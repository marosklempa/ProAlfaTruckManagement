using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ProAlfaTruckMan.Models
{
    public class Truckramp_block
    {
        public string Rampid { get; set; }
        public DateTime Validfrom { get; set; }
        public DateTime Validto { get; set; }
        public string Timefrom { get; set; }
        public string Timeto { get; set; }
        public int Mon { get; set; }
        public int Tue { get; set; }
        public int Wed { get; set; }
        public int Thu { get; set; }
        public int Fri { get; set; }
        public int Sat { get; set; }
        public int Sun { get; set; }
        public int Jumpable { get; set; }
        public string Remark { get; set; }
        public int Rowid { get; set; }

        public Truckramp_block( string Rampidx, DateTime Validfromx, DateTime Validtox, string Timefromx, string Timetox, int Monx, int Tuex, int Wedx, int Thux, int Frix, int Satx, int Sunx, int Jumpablex, string Remarkx, int Rowidx )
        {
            Rampid = Rampidx;
            Validfrom = Validfromx;
            Validto = Validtox;
            Timefrom = Timefromx;
            Timeto = Timetox;
            Mon = Monx;
            Tue = Tuex;
            Wed = Wedx;
            Thu = Thux;
            Fri = Frix;
            Sat = Satx;
            Sun = Sunx;
            Jumpable = Jumpablex;
            Remark = Remarkx;
            Rowid = Rowidx;
        }
    }

    public static class Truckramp_blockDataManagement
    {
        public static List<Truckramp_block> GetData(int mode, string rampid, DateTime? date, DateTime? date2)
        {
            rampid = rampid.Trim();
            List<Truckramp_block> truckramp_block = new List<Truckramp_block>();
            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader dr;
            try
            {
                cmd.CommandText = "SELECT rampid,validfrom,validto,timefrom,timeto,mon,tue,wed,thu,fri,sat,sun,jumpable,remark,rowid FROM truckramps_block WHERE rampid=@Rampid ";
                if (mode == 1)
                {
                    if (date.HasValue)
                    {
                        DateTime realdate = (DateTime)date;   // trick to convert nullable datetime to non-nullable datetime
                        cmd.CommandText = cmd.CommandText + "AND validfrom<=@Realdate AND validto>=@Realdate ";
                        cmd.Parameters.Add("@Realdate", SqlDbType.Date).Value = realdate;
                    }
                }
                else
                {
                    if (date.HasValue && date2.HasValue)   // if mode==2 then date and date2 should have always value, this "if" is here only to be sure that it contains value
                    {
                        DateTime realdate = (DateTime)date;   // trick to convert nullable datetime to non-nullable datetime
                        DateTime realdate2 = (DateTime)date2;   // trick to convert nullable datetime to non-nullable datetime
                        cmd.CommandText = cmd.CommandText + "AND NOT (validfrom>@Realdate2 OR validto<@Realdate)";
                        cmd.Parameters.Add("@Realdate", SqlDbType.Date).Value = realdate;
                        cmd.Parameters.Add("@Realdate2", SqlDbType.Date).Value = realdate2;
                    }
                }
                cmd.CommandText = cmd.CommandText + "ORDER BY validfrom";
                cmd.Parameters.Add("@Rampid", SqlDbType.NVarChar).Value = rampid;
                cmd.Connection = conn;
                cmd.Connection.Open();
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        //Get Function
                        //truckramp_block.Add(new Truckramp_block("1", DateTime.Now, DateTime.Now, "05:00", "05:06", 0, 1, 2, 3,
                        //    4, 5, 6, "rem", 99));
                        truckramp_block.Add(new Truckramp_block(dr.GetValue(0).ToString().Trim(), DateTime.Parse(dr.GetValue(1).ToString()), DateTime.Parse(dr.GetValue(2).ToString()), dr.GetValue(3).ToString().Trim(),
                            dr.GetValue(4).ToString().Trim(), Int32.Parse(dr.GetValue(5).ToString()), Int32.Parse(dr.GetValue(6).ToString()), Int32.Parse(dr.GetValue(7).ToString()), Int32.Parse(dr.GetValue(8).ToString()),
                            Int32.Parse(dr.GetValue(9).ToString()), Int32.Parse(dr.GetValue(10).ToString()), Int32.Parse(dr.GetValue(11).ToString()), Int32.Parse(dr.GetValue(12).ToString()), dr.GetValue(13).ToString().Trim(),
                            Int32.Parse(dr.GetValue(14).ToString())));
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

            return truckramp_block;
        }

    }
}
