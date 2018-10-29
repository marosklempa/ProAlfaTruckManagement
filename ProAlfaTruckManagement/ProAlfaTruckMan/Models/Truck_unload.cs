using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;


using Microsoft.AspNetCore.Hosting;

namespace ProAlfaTruckMan.Models
{
    public class Truck_unload
    {
        public string Rampid { get; set; }
        public string Dsc { get; set; }
        public string Vendnum { get; set; }
        public string Comp { get; set; }
        public DateTime Datefrom { get; set; }
        public DateTime Dateto { get; set; }
        public int Palqty { get; set; }
        public int Src { get; set; }
        public int Blocktype { get; set; }
        public string Remark { get; set; }
        public int Startrowid { get; set; }
        public int Rowid { get; set; }
        public string Inetname { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }

        public Truck_unload()
        { }

        public Truck_unload(string Rampidx, string Dscx, string Vendnumx, string Compx, DateTime Datefromx, DateTime Datetox, int Palqtyx, int Srcx, int Blocktypex, string Remarkx,
            int Startrowidx, int Rowidx, string Inetnamex, string Namex, string Surnamex)
        {
            Rampid = Rampidx;
            Dsc = Dscx;
            Vendnum = Vendnumx;
            Comp = Compx;
            Datefrom = Datefromx;
            Dateto = Datetox;
            Palqty = Palqtyx;
            Src = Srcx;
            Blocktype = Blocktypex;
            Remark = Remarkx;
            Startrowid = Startrowidx;
            Rowid = Rowidx;
            Inetname = Inetnamex;
            Name = Namex;
            Surname = Surnamex;
        }
    }

    public static class Truck_unloadDataManagement
    {
        public static Tuple<List<Truck_unload>, string> GetData(string rampid, string vendnum, DateTime date, DateTime date2)
        {
            string errmsg = "";
            List<Truck_unload> sapvendor_res = new List<Truck_unload>();
            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader dr;
            try
            {
                rampid = rampid.Trim();
                vendnum = vendnum.Trim();
                cmd.CommandText = "SELECT trucks_unloads.rampid, truckramps.dsc, trucks_unloads.vendnum, sapvendors.comp, trucks_unloads.datefrom, trucks_unloads.dateto, trucks_unloads.palqnty, trucks_unloads.src, ";
                cmd.CommandText = cmd.CommandText + "trucks_unloads.blocktype, trucks_unloads.remark, trucks_unloads.startrowid, trucks_unloads.rowid, trucks_unloads.inetname, trucks_unloads.name, trucks_unloads.surname ";
                cmd.CommandText = cmd.CommandText + "FROM trucks_unloads LEFT OUTER JOIN sapvendors ON trucks_unloads.vendnum=sapvendors.num LEFT OUTER JOIN truckramps ON trucks_unloads.rampid=truckramps.id WHERE ";
                cmd.CommandText = cmd.CommandText + (rampid != "" ? "trucks_unloads.rampid=@Rampid AND " : "");
                cmd.CommandText = cmd.CommandText + (vendnum != "" ? "trucks_unloads.vendnum=@Vendnum AND " : "");
                cmd.CommandText = cmd.CommandText + "NOT (trucks_unloads.datefrom>@ToDate OR trucks_unloads.dateto<@FromDate) ";

                cmd.CommandText = cmd.CommandText + "ORDER BY trucks_unloads.datefrom";
                cmd.Parameters.Add("@Rampid", SqlDbType.NVarChar).Value = rampid;
                cmd.Parameters.Add("@Vendnum", SqlDbType.NVarChar).Value = vendnum;
                cmd.Parameters.Add("@FromDate", SqlDbType.DateTime).Value = date;
                cmd.Parameters.Add("@ToDate", SqlDbType.DateTime).Value = date2;
                cmd.Connection = conn;
                cmd.Connection.Open();
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        sapvendor_res.Add(new Truck_unload(dr.GetValue(0).ToString().Trim(), dr.GetValue(1).ToString().Trim(), dr.GetValue(2).ToString().Trim(), dr.GetValue(3).ToString().Trim(),
                            DateTime.Parse(dr.GetValue(4).ToString()), DateTime.Parse(dr.GetValue(5).ToString()), Int32.Parse(dr.GetValue(6).ToString()), Int32.Parse(dr.GetValue(7).ToString()), Int32.Parse(dr.GetValue(8).ToString()),
                            dr.GetValue(9).ToString(), Int32.Parse(dr.GetValue(10).ToString()), Int32.Parse(dr.GetValue(11).ToString()), dr.GetValue(12).ToString().Trim(),
                            dr.GetValue(13).ToString().Trim(), dr.GetValue(14).ToString().Trim()));
                    }
                }
                dr.Close();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                errmsg = ex.Message;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            return Tuple.Create(sapvendor_res, errmsg);
        }

        public static Tuple<int, string> AddUpdateDelete(string mode, int rowid, string rampid, string vendnum, DateTime datefrom, DateTime dateto, int palqnty, int src, int blocktype,
            string remark, int startrowid, string inetname, bool checkblockcolision)
        {
            int result = 0; string result2 = "";
            DateTime tmpdatetime;
            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader dr;
            try
            {
                rampid = rampid.Trim();
                vendnum = vendnum.Trim();
                string name = "";
                string surname = "";
                cmd.Connection = conn;
                cmd.Connection.Open();
                if (mode.ToUpper() == "NEW" || mode.ToUpper() == "MODIFY")
                {
                    // 1. je to aktivny dodavatel?
                    cmd.CommandText = "SELECT xftimeflag, xftimeval FROM sapvendors WHERE num=@Num AND xactive=1";
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@Num", SqlDbType.NVarChar).Value = vendnum;
                    dr = cmd.ExecuteReader();
                    if (dr.HasRows)
                    {
                        dr.Read();
                        if (dr.GetValue(0).ToString().Trim() == "1")
                        {
                            // 2. datum/cas zaciatku vykladky je vacsi alebo rovny minimalnemu casu vykladky danej firmy
                            if (datefrom < DateTime.ParseExact(datefrom.ToString("yyyy-MM-dd") + " " + dr.GetValue(1).ToString().Trim() + ":00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))
                            {
                                // Cas zaciatku vykladky je nizsi ako minimalny cas vykladky tohto dodavatela!
                                result = -1;
                            }
                        }
                    }
                    else
                    {
                        // Neaktivny dodavatel sa neda vykladat!
                        result = -2;
                    }
                    dr.Close();
                    cmd.Dispose();

                    if (result == 0)
                    {
                        // 3. na tejto rampe moze byt vylozena tato firma (pripad, ze firmu, co ma vyhradenu svoju jednu rampu alebo viac ramp, sa pokusame zapisat na inu rampu)
                        if (TruckrampFuncs.AllowReservationVendOnRamp(1, rampid, vendnum) == false)
                        {
                            // Tento dodavatel sa na tejto rampe nevyklada! (1)
                            result = -3;
                        }
                        dr.Close();
                        cmd.Dispose();
                    }

                    if (result == 0)
                    {
                        // 4. na tejto rampe moze byt vylozena tato firma (pripad, ze firmu, co nema vyhradenu ziadnu svoju rampu, sa pokusame zapisat na vyhradenu rampu inych firiem)
                        if (TruckrampFuncs.AllowReservationVendOnRamp(2, rampid, vendnum) == false)
                        {
                            // Tento dodavatel sa na tejto rampe nevyklada! (2)
                            result = -4;
                        }
                    }

                    if (result == 0)
                    {
                        // 5. datum zaciatku vykladky je pracovny den
                        tmpdatetime = DateTime.ParseExact(datefrom.ToString("yyyy-MM-dd") + " 06:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        if (WorkCalendar.IsCalWorkDay(DateTime.ParseExact(datefrom.ToString("yyyy-MM-dd") + " 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).AddDays(datefrom < tmpdatetime ? -1 : 0)) != 1)
                        {
                            // Zaciatok vykladky spada do nepracovneho dna!
                            result = -5;
                        }
                    }

                    if (result == 0)
                    {
                        // 6. datum konca vykladky je pracovny den
                        tmpdatetime = DateTime.ParseExact(dateto.ToString("yyyy-MM-dd") + " 06:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        if (WorkCalendar.IsCalWorkDay(DateTime.ParseExact(dateto.ToString("yyyy-MM-dd") + " 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).AddDays(dateto < tmpdatetime ? -1 : 0)) != 1)
                        {
                            // Koniec vykladky spada do nepracovneho dna!
                            result = -6;
                        }
                    }

                    if (result == 0)
                    {
                        // 7. datum/cas zaciatku vykladky je pracovna smena
                        if (WorkCalendar.IsCalWorkDayShiftInDateTime(rampid.Trim(), datefrom) != 1)
                        {
                            // Cas zaciatku vykladky spada do nepracovnej smeny tejto linky!
                            result = -7;
                        }
                    }

                    if (result == 0)
                    {
                        // 8. datum/cas konca vykladky je pracovna smena
                        if (WorkCalendar.IsCalWorkDayShiftInDateTime(rampid.Trim(), datefrom) != 1)
                        {
                            // Cas konca vykladky spada do nepracovnej smeny tejto linky!
                            result = -8;
                        }
                    }

                    if (result == 0  && checkblockcolision == true)
                    {
                        // 9. nekoliduje s blokovanym casom rampy
                        if (datefrom.ToString("yyyy-MM-dd") == dateto.ToString("yyyy-MM-dd"))
                        {
                            // ak je zaciatok a koniec vykladky v jednom dni
                            if (TruckrampFuncs.BlockResColision("truckramps_block", rampid, datefrom, datefrom.ToString("HH:mm:ss"), dateto.ToString("HH:mm:ss"), 0) != 0)
                            {
                                // Vykladka koliduje s niektorym blokovanym casom rampy! (1)
                                result = -9;
                            }
                        }
                        else
                        {
                            // ak je zaciatok a koniec vykladky v roznych dnoch, teda zacina pred pol nocou a konci po nej
                            // skontrolujeme najprv jej cast od zaciatku po polnoc
                            if (TruckrampFuncs.BlockResColision("truckramps_block", rampid, datefrom, datefrom.ToString("HH:mm:ss"), "23:59:59", 0) != 0)
                            {
                                // Vykladka koliduje s niektorym blokovanym casom rampy! (2)
                                result = -10;
                            }
                            else
                            {
                                // a potom cast od polnoci po koniec
                                if (TruckrampFuncs.BlockResColision("truckramps_block", rampid, dateto, "00:00:00", dateto.ToString("HH:mm:ss"), 0) != 0)
                                {
                                    // Vykladka koliduje s niektorym blokovanym casom rampy! (3)
                                    result = -11;
                                }
                            }
                        }
                    }

                    if (result == 0)
                    {
                        // 10. nekoliduje s pevnou rezervaciou rampy
                        if (datefrom.ToString("yyyy-MM-dd") == dateto.ToString("yyyy-MM-dd"))
                        {
                            // ak je zaciatok a koniec vykladky v jednom dni
                            if (TruckrampFuncs.BlockResColision("sapvendors_res", rampid, datefrom, datefrom.ToString("HH:mm:ss"), dateto.ToString("HH:mm:ss"), 0) != 0)
                            {
                                // Vykladka koliduje s niektorou pevnou rezervaciou dodavatela! (1)
                                result = -12;
                            }
                        }
                        else
                        {
                            // ak je zaciatok a koniec vykladky v roznych dnoch, teda zacina pred pol nocou a konci po nej
                            // skontrolujeme najprv jej cast od zaciatku po polnoc
                            if (TruckrampFuncs.BlockResColision("sapvendors_res", rampid, datefrom, datefrom.ToString("HH:mm:ss"), "23:59:59", 0) != 0)
                            {
                                // Vykladka koliduje s niektorou pevnou rezervaciou dodavatela! (2)
                                result = -13;
                            }
                            else
                            {
                                // a potom cast od polnoci po koniec
                                if (TruckrampFuncs.BlockResColision("sapvendors_res", rampid, dateto, "00:00:00", dateto.ToString("HH:mm:ss"), 0) != 0)
                                {
                                    // Vykladka koliduje s niektorou pevnou rezervaciou dodavatela! (3)
                                    result = -14;
                                }
                            }
                        }
                    }

                    if (result == 0)
                    {
                        // 11. nekoliduje s dynamickou rezervaciou
                        if (TruckrampFuncs.UnloadColision(rampid, datefrom, dateto, rowid) != 0)
                        {
                            // Vykladka koliduje s inou vykladkou!
                            result = -15;
                        }
                    }

                    if (result == 0  &&  Strings.Decrypt( inetname ).Trim() != "")
                    {
                        // 12. pracovnik existuje
                        List<Sapvendor_pers> pers = Sapvendors_persDataManagement.GetData(2, inetname.Trim());
                        if (pers.Count == 0)
                        {
                            // Neexistujúci pracovnik
                            result = -16;
                        }
                        else
                        {
                            name = pers[0].Name;
                            surname = pers[0].Surname;
                        }
                    }
                }

                if (result == 0)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@Rampid", SqlDbType.NVarChar).Value = rampid;
                    cmd.Parameters.Add("@Vendnum", SqlDbType.NVarChar).Value = vendnum;
                    cmd.Parameters.Add("@FromDate", SqlDbType.DateTime).Value = datefrom;
                    cmd.Parameters.Add("@ToDate", SqlDbType.DateTime).Value = dateto;
                    cmd.Parameters.Add("@Palqnty", SqlDbType.SmallInt).Value = palqnty;
                    cmd.Parameters.Add("@Src", SqlDbType.TinyInt).Value = src;
                    cmd.Parameters.Add("@Blocktype", SqlDbType.TinyInt).Value = blocktype;
                    cmd.Parameters.Add("@Remark", SqlDbType.NVarChar).Value = remark;
                    cmd.Parameters.Add("@Startrowid", SqlDbType.SmallInt).Value = startrowid;
                    cmd.Parameters.Add("@Rowid", SqlDbType.Int).Value = rowid;
                    cmd.Parameters.Add("@Inetname", SqlDbType.NVarChar).Value = inetname;
                    cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = name;
                    cmd.Parameters.Add("@Surname", SqlDbType.NVarChar).Value = surname;
                    switch (mode.ToUpper())
                    {
                        case "NEW":
                            cmd.CommandText = "INSERT INTO trucks_unloads (rampid,vendnum,datefrom,dateto,palqnty,src,blocktype,remark,startrowid,inetname,name,surname) ";
                            cmd.CommandText = cmd.CommandText + "VALUES (@Rampid, @Vendnum, @FromDate, @ToDate, @Palqnty, @Src, @Blocktype, @Remark, @Startrowid, @Inetname, @Name, @Surname);";
                            cmd.CommandText = cmd.CommandText + "SELECT CAST(scope_identity() AS int) AS rowid";
                            result = (Int32)cmd.ExecuteScalar();
                            break;
                        case "MODIFY":
                            cmd.CommandText = "UPDATE trucks_unloads SET rampid=@Rampid, vendnum=@Vendnum, datefrom=@FromDate, dateto=@ToDate, palqnty=@Palqnty, src=@Src, blocktype=@Blocktype, ";
                            cmd.CommandText = cmd.CommandText + "remark=@Remark, startrowid=@Startrowid, inetname=@Inetname, name=@Name, surname=@Surname WHERE rowid=@Rowid";
                            cmd.ExecuteNonQuery();
                            break;
                        case "DELETE":
                            cmd.CommandText = "DELETE FROM trucks_unloads WHERE rowid=@Rowid";
                            cmd.ExecuteNonQuery();
                            break;
                    }
                    cmd.Dispose();
                }
            }
            catch (Exception ex)
            {
                result = -16;
                result2 = ex.Message;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
            return Tuple.Create(result,result2);
        }
    }
}
