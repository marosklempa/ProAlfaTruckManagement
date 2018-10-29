using System;
using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace ProAlfaTruckMan.Models
{
    public class Sapvendor_pers
    {
        public string Num { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Inetname { get; set; }
        public string Inetpwd { get; set; }
        public string Mobil { get; set; }
        public string Email { get; set; }


        public Sapvendor_pers(string Numx, string Namex, string Surnamex, string Inetnamex, string Inetpwdx, string Mobilx, string Emailx)
        {
            Num = Numx;
            Name = Namex;
            Surname = Surnamex;
            Inetname = Inetnamex;
            Inetpwd = Inetpwdx;
            Mobil = Mobilx;
            Email = Emailx;
        }
    }

    public static class Sapvendors_persDataManagement
    {
        public static List<Sapvendor_pers> GetData(int type, string ident)
        {
            List<Sapvendor_pers> sapvends_pers = new List<Sapvendor_pers>();
            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader dr;
            try
            {
                cmd.CommandText = "SELECT num, name, surname, inetname, inetpwd, mobil, email FROM sapvendors_pers WHERE ";
                if (type == 1)
                {
                    cmd.CommandText = cmd.CommandText + "num";
                }
                else
                {
                    cmd.CommandText = cmd.CommandText + "inetname";
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
                        sapvends_pers.Add(new Sapvendor_pers(dr.GetValue(0).ToString().Trim(), dr.GetValue(1).ToString().Trim(), dr.GetValue(2).ToString().Trim(),
                            dr.GetValue(3).ToString().Trim(), dr.GetValue(4).ToString().Trim(), dr.GetValue(5).ToString().Trim(), dr.GetValue(6).ToString().Trim()));
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

            return sapvends_pers;
        }

        public static string GetPersInfo(string PersIdent)
        {
            string RetVal = "";
            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader dr;
            try
            {
                cmd.CommandText = "SELECT Pers.name, Pers.surname, Vend.comp, Vend.str, Vend.zip, Vend.city, Vend.countrycode FROM sapvendors_pers Pers " +
                    "LEFT JOIN sapvendors Vend ON Pers.num=Vend.num WHERE Pers.inetname=@Inetname AND Vend.xactive=1";
                cmd.Parameters.Add("@Inetname", SqlDbType.Char).Value = Strings.Encrypt(PersIdent);
                cmd.Connection = conn;
                cmd.Connection.Open();
                dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    //Get Function
                    RetVal = Strings.Decrypt(dr[0].ToString().Trim()) + " " + Strings.Decrypt(dr[1].ToString().Trim()) + ", " + Strings.Decrypt(dr[2].ToString().Trim()) + ", " + Strings.Decrypt(dr[3].ToString().Trim()) + ", " +
                        Strings.Decrypt(dr[4].ToString().Trim()) + " " + Strings.Decrypt(dr[5].ToString().Trim()) + ", " + Strings.Decrypt(dr[6].ToString().Trim());
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
            return RetVal;
        }

        public static int ChangePersPwd(string inetname, string inetpwd, string changepwd)
        {
            int result = 0;
            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd.CommandText = "UPDATE sapvendors_pers SET inetpwd=@Inetpwd, changepwd=@Changepwd WHERE inetname=@Inetname";
                cmd.Parameters.Add("@Inetname", SqlDbType.NVarChar).Value = inetname;
                cmd.Parameters.Add("@Inetpwd", SqlDbType.NVarChar).Value = inetpwd;
                cmd.Parameters.Add("@Changepwd", SqlDbType.NVarChar).Value = changepwd;
                cmd.Connection = conn;
                cmd.Connection.Open();
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
    }
}
