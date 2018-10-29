using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ProAlfaTruckMan.Models
{
    public class Sapvendor_res
    {
        public string Num { get; set; }
        public string Comp { get; set; }
        public string Rampid { get; set; }
        public string Dsc { get; set; }
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
        public int Rowid { get; set; }

        public Sapvendor_res( string Numx, string Compx, string Rampidx, string Dscx, DateTime Validfromx, DateTime Validtox, string Timefromx, string Timetox, int Monx, int Tuex, int Wedx, int Thux, int Frix, int Satx, int Sunx, int Rowidx )
        {
            Num = Numx;
            Comp = Compx;
            Rampid = Rampidx;
            Dsc = Dscx;
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
            Rowid = Rowidx;
        }
    }

    public static class Sapvendor_resDataManagement
    {
        public static List<Sapvendor_res> GetData(int mode, string num, string rampid, DateTime? date, DateTime? date2)
        {
            List<Sapvendor_res> sapvendor_res = new List<Sapvendor_res>();
            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader dr;
            try
            {
                num = num.Trim();
                rampid = rampid.Trim();
                cmd.CommandText = "SELECT sapvendors_res.num, sapvendors.comp, sapvendors_res.rampid, truckramps.dsc, sapvendors_res.validfrom, sapvendors_res.validto, sapvendors_res.timefrom, sapvendors_res.timeto,";
                cmd.CommandText = cmd.CommandText + "sapvendors_res.mon, sapvendors_res.tue, sapvendors_res.wed, sapvendors_res.thu, sapvendors_res.fri, sapvendors_res.sat, sapvendors_res.sun, sapvendors_res.rowid ";
                cmd.CommandText = cmd.CommandText + "FROM sapvendors_res LEFT OUTER JOIN sapvendors ON sapvendors_res.num=sapvendors.num LEFT OUTER JOIN truckramps ON sapvendors_res.rampid=truckramps.id WHERE ";
                cmd.CommandText = cmd.CommandText + (num.Trim() != "" ? "sapvendors_res.num=@Num AND " : "");
                cmd.CommandText = cmd.CommandText + (rampid.Trim() != "" ? "sapvendors_res.rampid=@Rampid AND " : "");
                if (mode == 1)
                {
                    if (date.HasValue)
                    {
                        DateTime realdate = (DateTime)date;   // trick to convert nullable datetime to non-nullable datetime
                        cmd.CommandText = cmd.CommandText + "sapvendors_res.validfrom<=@Realdate AND sapvendors_res.validto>=@Realdate AND ";
                        cmd.Parameters.Add("@Realdate", SqlDbType.Date).Value = realdate;
                    }
                }
                else
                {
                    if (date.HasValue && date2.HasValue)
                    {
                        DateTime realdate = (DateTime)date;   // trick to convert nullable datetime to non-nullable datetime
                        DateTime realdate2 = (DateTime)date2;   // trick to convert nullable datetime to non-nullable datetime
                        cmd.CommandText = cmd.CommandText + "NOT (sapvendors_res.validfrom>@Realdate2 OR sapvendors_res.validto<@Realdate) AND ";
                        cmd.Parameters.Add("@Realdate", SqlDbType.Date).Value = realdate;
                        cmd.Parameters.Add("@Realdate2", SqlDbType.Date).Value = realdate2;
                    }
                }
                cmd.CommandText = cmd.CommandText + "1=1 ORDER BY sapvendors_res.validfrom ";
                cmd.Parameters.Add("@Num", SqlDbType.NVarChar).Value = num;
                cmd.Parameters.Add("@Rampid", SqlDbType.NVarChar).Value = rampid;
                cmd.Connection = conn;
                cmd.Connection.Open();
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        //Get Function
                        //sapvendor_res.Add(new Truckramp_block("451233", "Sanberg s.r.o.", "1", DateTime.Now, DateTime.Now, "05:00", "05:06", 0, 1, 2, 3,
                        //    4, 5, 6, 99));
                        sapvendor_res.Add(new Sapvendor_res(dr.GetValue(0).ToString().Trim(), Strings.Decrypt(dr.GetValue(1).ToString().Trim()), dr.GetValue(2).ToString().Trim(), Strings.Decrypt(dr.GetValue(3).ToString().Trim()),
                            DateTime.Parse(dr.GetValue(4).ToString()), DateTime.Parse(dr.GetValue(5).ToString()), dr.GetValue(6).ToString().Trim(), dr.GetValue(7).ToString().Trim(), Int32.Parse(dr.GetValue(8).ToString()),
                            Int32.Parse(dr.GetValue(9).ToString()), Int32.Parse(dr.GetValue(10).ToString()), Int32.Parse(dr.GetValue(11).ToString()), Int32.Parse(dr.GetValue(12).ToString()), Int32.Parse(dr.GetValue(13).ToString()),
                            Int32.Parse(dr.GetValue(14).ToString()), Int32.Parse(dr.GetValue(15).ToString())));
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

            return sapvendor_res;
        }
    }

}
