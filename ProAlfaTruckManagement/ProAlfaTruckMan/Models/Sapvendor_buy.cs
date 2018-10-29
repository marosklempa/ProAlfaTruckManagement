using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;

namespace ProAlfaTruckMan.Models
{
    public class Sapvendor_buy
    {
        public string Num { get; set; }
        public string UserID { get; set; }
        public string Email { get; set; }

        public Sapvendor_buy(string Numx, string UserIDx, string Emailx)
        {
            Num = Numx;
            UserID = UserIDx;
            Email = Emailx;
        }
    }

    public static class Sapvendor_buyDataManagement
    {
        public static List<Sapvendor_buy> GetData(int type, string num, string userid)
        {
            List<Sapvendor_buy> sapvendor_buy = new List<Sapvendor_buy>();
            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader dr;
            try
            {
                cmd.CommandText = "SELECT num,userid,email FROM sapvendors_buy WHERE num=@Num ";
                if (type == 2)
                {
                    cmd.CommandText = cmd.CommandText + "AND userid=@Userid";
                    cmd.Parameters.Add("@Userid", SqlDbType.NVarChar).Value = userid;
                }
                cmd.Parameters.Add("@Num", SqlDbType.NVarChar).Value = num;
                cmd.Connection = conn;
                cmd.Connection.Open();
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        sapvendor_buy.Add(new Sapvendor_buy(dr.GetValue(0).ToString().Trim(), dr.GetValue(1).ToString().Trim(), dr.GetValue(2).ToString().Trim()));
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

            return sapvendor_buy;
        }
    }

}
