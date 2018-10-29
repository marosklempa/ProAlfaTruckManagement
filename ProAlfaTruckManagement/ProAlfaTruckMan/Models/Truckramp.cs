using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;

namespace ProAlfaTruckMan.Models
{
    public class Truckramp
    {
        public string Id { get; set; }
        public string Dsc { get; set; }
        public int Vendres { get; set; }
        public string Paltime { get; set; }

        public Truckramp( string Idx, string Dscx, int Vendresx, string Paltimex)
        {
            Id = Idx;
            Dsc = Dscx;
            Vendres = Vendresx;
            Paltime = Paltimex;
        }
    }

    public static class TruckrampDataManagement
    {
        public static List<Truckramp> GetData(string rampid)
        {
            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader dr;
            List<Truckramp> ramps = new List<Truckramp>();
            try
            {
                rampid = rampid.Trim();
                cmd.CommandText = "SELECT id,dsc,vendres,paltime FROM truckramps " + (rampid == "" ? "" : "WHERE id=@Id");
                cmd.Parameters.Add("@Id", SqlDbType.NVarChar).Value = rampid;
                cmd.Connection = conn;
                cmd.Connection.Open();
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        //Get Function
                        ramps.Add(new Truckramp(dr.GetValue(0).ToString().Trim(), dr.GetValue(1).ToString().Trim(), int.Parse(dr.GetValue(2).ToString()), dr.GetValue(3).ToString().Trim()));
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

            return ramps;
        }

    }
}
