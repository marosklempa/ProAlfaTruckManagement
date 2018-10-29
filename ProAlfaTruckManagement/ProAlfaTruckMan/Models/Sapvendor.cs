using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ProAlfaTruckMan.Models
{
    public class Sapvendor
    {
        public string Num { get; set; }
        public string Comp { get; set; }
        public string Str { get; set; }
        public string ZIP { get; set; }
        public string City { get; set; }
        public string Countrycode { get; set; }
        public int Xactive { get; set; }
        public int Xtrtype { get; set; }
        public int Xftimeflag { get; set; }
        public string Xftimeval { get; set; }
        public int Rampres { get; set; }

        public Sapvendor( string Numx, string Compx, string Strx, string ZIPx, string Cityx, string Countrycodex, int Xactivex, int Xtrtypex, int Xftimeflagx, string Xftimevalx, int Rampresx)
        {
            Num = Numx;
            Comp = Compx;
            Str = Strx;
            ZIP = ZIPx;
            City = Cityx;
            Countrycode = Countrycodex;
            Xactive = Xactivex;
            Xtrtype = Xtrtypex;
            Xftimeflag = Xftimeflagx;
            Xftimeval = Xftimevalx;
            Rampres = Rampresx;
        }
    }

    public static class SapvendorDataManagement
    {
        public static List<Sapvendor> GetData(string num)
        {
            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader dr;
            List<Sapvendor> sapvendors = new List<Sapvendor>();
            try
            {
                cmd.CommandText = "SELECT num,comp,str,zip,city,countrycode,xactive,xtrtype,xftimeflag,xftimeval,rampres FROM sapvendors " + (num == "" ? "" : "WHERE num=@Num");
                cmd.Parameters.Add("@Num", SqlDbType.NVarChar).Value = num;
                cmd.Connection = conn;
                cmd.Connection.Open();
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        //Get Function
                        sapvendors.Add(new Sapvendor(dr.GetValue(0).ToString().Trim(), dr.GetValue(1).ToString().Trim(), dr.GetValue(2).ToString().Trim(), dr.GetValue(3).ToString().Trim(),
                            dr.GetValue(4).ToString().Trim(), dr.GetValue(5).ToString().Trim(), int.Parse(dr.GetValue(6).ToString()), int.Parse(dr.GetValue(7).ToString()),
                            int.Parse(dr.GetValue(8).ToString()), dr.GetValue(9).ToString().Trim(), int.Parse(dr.GetValue(10).ToString())));
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

            return sapvendors;
        }

    }
}
