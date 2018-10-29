using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ProAlfaTruckMan.Models
{
    public class Sapvendor_ramp
    {
        public string Num { get; set; }
        public string Comp { get; set; }
        public string RampId { get; set; }
        public string Dsc { get; set; }

        public Sapvendor_ramp( string Numx, string Compx, string RampIdx, string Dscx)
        {
            Num = Numx;
            Comp = Compx;
            RampId = RampIdx;
            Dsc = Dscx;
        }
    }

    public static class Sapvendor_rampDataManagement
    {
        public static List<Sapvendor_ramp> GetData(int type, string ident)
        {
            List<Sapvendor_ramp> sapvends_ramp = new List<Sapvendor_ramp>();
            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader dr;
            try
            {
                cmd.CommandText = "SELECT sapvendors_ramp.num, sapvendors.comp, sapvendors_ramp.rampid, truckramps.dsc FROM sapvendors_ramp LEFT OUTER JOIN sapvendors ON sapvendors_ramp.num=sapvendors.num ";
                cmd.CommandText = cmd.CommandText + "LEFT OUTER JOIN truckramps ON sapvendors_ramp.rampid=truckramps.id WHERE sapvendors_ramp.";
                if (type == 1)
                {
                    cmd.CommandText = cmd.CommandText + "num";
                }
                else
                {
                    cmd.CommandText = cmd.CommandText + "rampid";
                }
                cmd.CommandText = cmd.CommandText + "=@Ident";
                cmd.Parameters.Add("@Ident", SqlDbType.NVarChar).Value = ident;
                cmd.Connection = conn;
                cmd.Connection.Open();
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        //Get Function
                        sapvends_ramp.Add(new Sapvendor_ramp(dr.GetValue(0).ToString().Trim(), Strings.Decrypt(dr.GetValue(1).ToString().Trim()), dr.GetValue(2).ToString().Trim(), Strings.Decrypt(dr.GetValue(3).ToString().Trim())));
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

            return sapvends_ramp;
        }

    }
}
