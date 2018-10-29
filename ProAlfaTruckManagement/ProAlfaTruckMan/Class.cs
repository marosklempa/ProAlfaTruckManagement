using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Globalization;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using IdentityServer4;
using IdentityServer4.Models;


namespace ProAlfaTruckMan
{
    public static class Includes
    {
        public static string AppShortName = "ProAlfaTruckMan";
        public static string AppLongName = "ProAlfa Truck Management";
    }

    public static class SessionVariables
    {
        public static string LastError = "";
        public static string IsLogout = "islogout";
    }

    public static class CurrentUser
    {
        public static string InetName;
        public static string Num;
    }

    public static class CookieNames
    {
        public static string LoginUserName = "_loginusername";
        public static string RememberLoginUserName = "_rememberloginusername";
    }

    public static class ConnString
    {
        public static string Value;
    }

    public static class Strings
    {
        public static string possibleChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        public static char[] possibleCharsArray = possibleChars.ToCharArray();
        public static int possibleCharsAvailable = possibleChars.Length;
        public static Random random = new Random();
        private static string passphrase = "---";
        private static string saltvalue = "---";
        private static string hashalgorithm = "---";
        private static int passworditerations = 0;
        private static string initvector = "---";
        private static int keysize = 0;

        public static string GenerateRandomString(int num)
        {
            var rBytes = new byte[num];
            random.NextBytes(rBytes);
            var rName = new char[num];
            while (num-- > 0)
                rName[num] = possibleCharsArray[rBytes[num] % possibleCharsAvailable];
            return new string(rName);
        }

        public static string Left(string input, int num)
        {
            if (num > input.Length)
            {
                num = input.Length;
            }
            return input.Substring(0, num);
        }

        public static string Right(string input, int num)
        {
            if (num > input.Length)
            {
                num = input.Length;
            }
            return input.Substring(input.Length - num);
        }

        public static string SHA512(string input)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            using (var hash = System.Security.Cryptography.SHA512.Create())
            {
                var hashedInputBytes = hash.ComputeHash(bytes);

                // Convert to text
                // StringBuilder Capacity is 128, because 512 bits / 8 bits in byte * 2 symbols for byte 
                var hashedInputStringBuilder = new System.Text.StringBuilder(128);
                foreach (var b in hashedInputBytes)
                    hashedInputStringBuilder.Append(b.ToString("X2"));
                return hashedInputStringBuilder.ToString();
            }
        }

        public static string Encrypt(string plaintext)
        {
            // Convert strings into byte arrays.
            // Let us assume that strings only contain ASCII codes.
            // If strings include Unicode characters, use Unicode, UTF7, or UTF8
            // encoding.
            byte[] initVectorBytes = Encoding.ASCII.GetBytes(initvector);
            byte[] saltValueBytes = Encoding.ASCII.GetBytes(saltvalue);

            // Convert our plaintext into a byte array.
            // Let us assume that plaintext contains UTF8-encoded characters.
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plaintext);

            // First, we must create a password, from which the key will be derived.
            // This password will be generated from the specified passphrase and
            // salt value. The password will be created using the specified hash
            // algorithm. Password creation can be done in several iterations.
            PasswordDeriveBytes password = default(PasswordDeriveBytes);
            password = new PasswordDeriveBytes(passphrase, saltValueBytes, hashalgorithm, passworditerations);
            
            // Use the password to generate pseudo-random bytes for the encryption
            // key. Specify the size of the key in bytes (instead of bits).
            byte[] keyBytes = password.GetBytes(keysize / 8);
            
            // Create uninitialized Rijndael encryption object.
            RijndaelManaged symmetricKey = default(RijndaelManaged);
            symmetricKey = new RijndaelManaged();

            // It is reasonable to set encryption mode to Cipher Block Chaining
            // (CBC). Use default options for other symmetric key parameters.
            symmetricKey.Mode = CipherMode.CBC;
            
            // Generate encryptor from the existing key bytes and initialization
            // vector. Key size will be defined based on the number of the key
            // bytes.
            ICryptoTransform encryptor = default(ICryptoTransform);
            encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);

            // Define memory stream which will be used to hold encrypted data.
            MemoryStream memoryStream = default(MemoryStream);
            memoryStream = new MemoryStream();

            // Define cryptographic stream (always use Write mode for encryption).
            CryptoStream cryptoStream = default(CryptoStream);
            cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

            // Start encrypting.
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);

            // Finish encrypting.
            cryptoStream.FlushFinalBlock();

            // Convert our encrypted data from a memory stream into a byte array.
            byte[] cipherTextBytes = memoryStream.ToArray();

            // Close both streams.
            memoryStream.Close();
            cryptoStream.Close();

            // Convert encrypted data into a base64-encoded string.
            return Convert.ToBase64String(cipherTextBytes);
        }

        public static string Decrypt(string ciphertext)
        {
            // Convert strings defining encryption key characteristics into byte
            // arrays. Let us assume that strings only contain ASCII codes.
            // If strings include Unicode characters, use Unicode, UTF7, or UTF8
            // encoding.
            byte[] initVectorBytes = Encoding.ASCII.GetBytes(initvector);
            byte[] saltValueBytes = Encoding.ASCII.GetBytes(saltvalue);

            // Convert our ciphertext into a byte array.
            byte[] cipherTextBytes = Convert.FromBase64String(ciphertext);

            // First, we must create a password, from which the key will be
            // derived. This password will be generated from the specified
            // passphrase and salt value. The password will be created using
            // the specified hash algorithm. Password creation can be done in
            // several iterations.
            PasswordDeriveBytes password = default(PasswordDeriveBytes);
            password = new PasswordDeriveBytes(passphrase, saltValueBytes, hashalgorithm, passworditerations);

            // Use the password to generate pseudo-random bytes for the encryption
            // key. Specify the size of the key in bytes (instead of bits).
            byte[] keyBytes = password.GetBytes(keysize / 8);

            // Create uninitialized Rijndael encryption object.
            RijndaelManaged symmetricKey = default(RijndaelManaged);
            symmetricKey = new RijndaelManaged();

            // It is reasonable to set encryption mode to Cipher Block Chaining
            // (CBC). Use default options for other symmetric key parameters.
            symmetricKey.Mode = CipherMode.CBC;

            // Generate decryptor from the existing key bytes and initialization
            // vector. Key size will be defined based on the number of the key
            // bytes.
            ICryptoTransform decryptor = default(ICryptoTransform);
            decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);

            // Define memory stream which will be used to hold encrypted data.
            MemoryStream memoryStream = default(MemoryStream);
            memoryStream = new MemoryStream(cipherTextBytes);

            // Define memory stream which will be used to hold encrypted data.
            CryptoStream cryptoStream = default(CryptoStream);
            cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

            // Since at this point we don't know what the size of decrypted data
            // will be, allocate the buffer long enough to hold ciphertext;
            // plaintext is never longer than ciphertext.
            byte[] plainTextBytes = new byte[cipherTextBytes.Length + 1];

            // Start decrypting.
            int decryptedByteCount = 0;
            decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

            // Close both streams.
            memoryStream.Close();
            cryptoStream.Close();

            // Convert decrypted data into a string.
            // Let us assume that the original plaintext string was UTF8-encoded.
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
        }

    }

    public static class Files
    {
        public static void Log(string file, string text, bool append)
        {
            if (!append)
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(file))
                {
                    sw.WriteLine(text);
                }
            }
            else
            {
                // This text is always added, making the file longer over time
                // if it is not deleted.
                using (StreamWriter sw = File.AppendText(file))
                {
                    sw.WriteLine(text);
                }
            }
            return;
        }
    }

    public static class TruckrampFuncs
    {

        public static int DetectBlockResColision(string mode, int rowid, string rampid, string validfrom, string validto, string timefrom, string timeto, int mon, int tue, int wed, int thu, int fri, int sat, int sun)
        {
            DateTime date;
            int result = 0, retval = 0;
            for (int i = 0; i <= (DateTime.Parse(validto) - DateTime.Parse(validfrom)).Days; i++)
            {
                date = DateTime.Parse(validfrom).AddDays(i);
                if ((date.DayOfWeek == DayOfWeek.Monday && mon == 1) || (date.DayOfWeek == DayOfWeek.Tuesday && tue == 1) || (date.DayOfWeek == DayOfWeek.Wednesday && wed == 1) ||
                    (date.DayOfWeek == DayOfWeek.Thursday && thu == 1) || (date.DayOfWeek == DayOfWeek.Friday && fri == 1) || (date.DayOfWeek == DayOfWeek.Saturday && sat == 1) ||
                    (date.DayOfWeek == DayOfWeek.Sunday && sun == 1))
                {
                    //date = DateTime.ParseExact(validfrom + " 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).AddDays(i);
                    // Kontrola na koliziu s blokovanymi casami rampy
                    retval = TruckrampFuncs.BlockResColision("truckramps_block", rampid, date, timefrom, timeto, (mode.ToUpper() == "NEW" ? 0 : rowid));
                    switch (retval)
                    {
                        case 1:
                            // Zistena kolizia
                            result = -1;
                            break;
                        case -1:
                            // Vynimka v metode IsRampBlockTimeColision
                            result = -2;
                            break;
                    }
                    if (result == 0)
                    {
                        // Kontrola na koliziu s pevnymi rezervaciami rampy
                        retval = TruckrampFuncs.BlockResColision("sapvendors_res", rampid, date, timefrom, timeto, 0);
                        switch (retval)
                        {
                            case 1:
                                // Zistena kolizia
                                result = -3;
                                break;
                            case -1:
                                // Vynimka v metode IsSAPVendResTimeColision
                                result = -4;
                                break;
                        }
                    }
                }

                if (result == 0)
                {
                    retval = WorkCalendar.IsCalWorkDay(date);   // Zistenie, ci je den pracovny
                    switch (retval)
                    {
                        case 0:
                            // Den nie je pracovny podla pracovneho kalendara firmy, kontrola nie je potrebna
                            break;
                        case 1:
                            // Kontrola zaciatku blokovaneho casu na koliziu s definiciou pracovných smien rampy
                            retval = WorkCalendar.IsCalWorkDayShiftInDateTime(rampid, DateTime.ParseExact(date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + " " + timefrom + ":00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                            switch (retval)
                            {
                                case 0:
                                    // V dany cas nie je v kalendary rampy pracovna smena (zistena kolizia)
                                    result = -5;
                                    break;
                                case 1:
                                    break;
                                case -1:
                                    // Vynimka v metode IsCalWorkDayShift
                                    result = -6;
                                    break;
                                case -2:
                                    // Chybajuci pracovný kalendar smien rampy
                                    result = -7;
                                    break;
                            }
                            if (result == 0)
                            {
                                // Kontrola konca blokovaneho casu na koliziu s definiciou pracovných smien
                                retval = WorkCalendar.IsCalWorkDayShiftInDateTime(rampid, DateTime.ParseExact(date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + " " + timeto + ":00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).AddSeconds(-1));
                                switch (retval)
                                {
                                    case 0:
                                        // V dany cas nie je v kalendary rampy pracovna smena (zistena kolizia)
                                        result = -8;
                                        break;
                                    case 1:
                                        break;
                                    case -1:
                                        // Vynimka v metode IsCalWorkDayShift
                                        result = -9;
                                        break;
                                    case -2:
                                        // Chybajuci kalendar smien rampy
                                        result = -10;
                                        break;
                                }
                            }
                            break;
                        case -1:
                            // Vynimka v metode IsCalWorkDay
                            result = -11;
                            break;
                    }
                }

                if (result != 0)
                {
                    break; // Opustenie cyklu for
                }

            }
            return result;
        }

        public static int BlockResColision(string sqltable, string rampid, DateTime date, string timefrom, string timeto, int rowid)
        {
            sqltable = sqltable.Trim();
            rampid = rampid.Trim();
            timefrom = timefrom.Trim();
            timeto = timeto.Trim();
            int result = 0;
            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader dr;
            try
            {
                cmd.Connection = conn;
                cmd.Connection.Open();

                cmd.CommandText = "SELECT mon,tue,wed,thu,fri,sat,sun FROM " + sqltable + " WHERE rampid=@Rampid AND rowid<>@Rowid AND validfrom<=@Date AND validto>=@Date ";
                cmd.CommandText = cmd.CommandText + "AND NOT ";
                cmd.CommandText = cmd.CommandText + "( DATEADD( ss, 1, CONVERT( DATETIME, CONVERT( CHAR(10), validfrom, 120 ) + ' ' + timefrom + ':00' ) ) > CONVERT( DATETIME, CONVERT( CHAR(10), validfrom, 120 ) + ' " + timeto + ":00' ) ";
                cmd.CommandText = cmd.CommandText + "OR ";
                cmd.CommandText = cmd.CommandText + "DATEADD( ss, -1, CONVERT( DATETIME, CONVERT( CHAR(10), validfrom, 120 ) + ' ' + timeto + ':00' ) ) < CONVERT(DATETIME, CONVERT(CHAR(10), validfrom, 120) + ' " + timefrom + ":00' ) )";
                cmd.Parameters.Add("@Rampid", SqlDbType.NVarChar).Value = rampid;
                cmd.Parameters.Add("@Rowid", SqlDbType.SmallInt).Value = rowid;
                cmd.Parameters.Add("@Date", SqlDbType.Date).Value = date;

                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        if ( (date.DayOfWeek == DayOfWeek.Monday    && dr.GetValue(0).ToString().Trim() == "1") || (date.DayOfWeek == DayOfWeek.Tuesday  && dr.GetValue(1).ToString().Trim() == "1")  ||
                             (date.DayOfWeek == DayOfWeek.Wednesday && dr.GetValue(2).ToString().Trim() == "1") || (date.DayOfWeek == DayOfWeek.Thursday && dr.GetValue(3).ToString().Trim() == "1")  ||
                             (date.DayOfWeek == DayOfWeek.Friday    && dr.GetValue(4).ToString().Trim() == "1") || (date.DayOfWeek == DayOfWeek.Saturday && dr.GetValue(5).ToString().Trim() == "1")  ||
                             (date.DayOfWeek == DayOfWeek.Sunday    && dr.GetValue(6).ToString().Trim() == "1") )
                        {
                            // Kolizia
                            result = 1;
                            break;
                        }
                    }
                }
                dr.Close();
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

        public static int UnloadColision(string rampid, DateTime datefrom, DateTime dateto, int rowid)
        {
            rampid = rampid.Trim();
            int result = 0;
            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader dr;
            try
            {
                cmd.Connection = conn;
                cmd.Connection.Open();

                cmd.CommandText = "SELECT rampid FROM trucks_unloads WHERE rampid=@Rampid AND rowid<>@Rowid AND NOT ( DATEADD( ss, 1, datefrom )>@Dateto OR DATEADD( ss, -1, dateto )<@Datefrom )";
                cmd.Parameters.Add("@Rampid", SqlDbType.NVarChar).Value = rampid;
                cmd.Parameters.Add("@Rowid", SqlDbType.Int).Value = rowid;
                cmd.Parameters.Add("@Datefrom", SqlDbType.DateTime).Value = datefrom;
                cmd.Parameters.Add("@Dateto", SqlDbType.DateTime).Value = dateto;
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    // Kolizia
                    result = 1;
                }
                dr.Close();
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

        public static List<TimeSlice> GetBlockResSlices(string sqltable, string rampid, string rampdsc, DateTime date, List<TimeSlice> tslices, SqlCommand cmd, SqlDataReader dr, bool isjumpable)
        {
            sqltable = sqltable.Trim();
            rampid = rampid.Trim();
            rampdsc = rampdsc.Trim();
            // Zistenie blokovaných/rezervovaných časov rampy pre obdobie, do ktorého spadá zadaný dátum
            List<TimeSlice> tmptslices = new List<TimeSlice>();
            cmd.CommandText = "SELECT mon,tue,wed,thu,fri,sat,sun,timefrom,timeto" + (isjumpable ? ",jumpable" : "") + " FROM " + sqltable + " WHERE rampid=@Rampid AND validfrom<=@Date AND validto>=@Date ";
            cmd.Parameters.Clear();
            cmd.Parameters.Add("@Rampid", SqlDbType.NVarChar).Value = rampid;
            cmd.Parameters.Add("@Date", SqlDbType.Date).Value = date;
            dr = cmd.ExecuteReader();
            if (dr.HasRows)
            {
                DateTime startdt, enddt;
                while (dr.Read())
                {
                    // Zistenie, či blokovaný čas platí aj pre deň v týždni na ktorý pripadá dátum zadaný užívateľom
                    if ((date.DayOfWeek == DayOfWeek.Monday && dr.GetValue(0).ToString().Trim() == "1") || (date.DayOfWeek == DayOfWeek.Tuesday && dr.GetValue(1).ToString().Trim() == "1") ||
                         (date.DayOfWeek == DayOfWeek.Wednesday && dr.GetValue(2).ToString().Trim() == "1") || (date.DayOfWeek == DayOfWeek.Thursday && dr.GetValue(3).ToString().Trim() == "1") ||
                         (date.DayOfWeek == DayOfWeek.Friday && dr.GetValue(4).ToString().Trim() == "1") || (date.DayOfWeek == DayOfWeek.Saturday && dr.GetValue(5).ToString().Trim() == "1") ||
                         (date.DayOfWeek == DayOfWeek.Sunday && dr.GetValue(6).ToString().Trim() == "1"))
                    {
                        // Vytvorenie časových dielikov (po 10 minútach) pre blokovaný čas
                        startdt = DateTime.ParseExact(date.ToString("yyyy-MM-dd") + " " + dr.GetValue(7).ToString().Trim() + ":00", "yyyy-MM-dd HH:mm:ss", null);
                        enddt = DateTime.ParseExact(date.ToString("yyyy-MM-dd") + " " + dr.GetValue(8).ToString().Trim() + ":00", "yyyy-MM-dd HH:mm:ss", null);
                        int diffminutes = (int)Math.Floor((double)(int)(enddt - startdt).TotalMinutes / Constants.TimeSliceLenghtInMinutes);
                        tmptslices.Clear();
                        tmptslices = WorkCalendar.CreateTimeSlices(rampid, rampdsc, startdt, 1, diffminutes, tmptslices, (isjumpable ? dr.GetValue(9).ToString().Trim() == "1" : false));
                        // Nájdenie každého dieliku blokovaného času v dielikoch pracovných smien danej rampy a ak sa nájde, tak označenie nájdeného dieliku pracovnej smeny príznakom 'obsadený' prípadne o bokovaných časov rampy aj príznakom 'obskočiteľný'
                        tslices = WorkCalendar.CompareTimeSlices(tmptslices, tslices);
                    }
                }
            }
            dr.Close();
            cmd.Dispose();
            return tslices;
        }

        public static List<TimeSlice> GetUnloadSlices(string rampid, string rampdsc, DateTime datefrom, DateTime dateto, List<TimeSlice> tslices, SqlCommand cmd, SqlDataReader dr)
        {
            rampid = rampid.Trim();
            rampdsc = rampdsc.Trim();
            // Zistenie dynamicky rezervovaných časov rampy ktoré spadajú do čaového úseku od-do
            List<TimeSlice> tmptslices = new List<TimeSlice>();
            cmd.CommandText = "SELECT FORMAT(datefrom, 'yyyy-MM-dd HH:mm:ss'), FORMAT(dateto, 'yyyy-MM-dd HH:mm:ss') FROM trucks_unloads WHERE rampid='" + rampid + "' ";
            cmd.CommandText = cmd.CommandText + "AND NOT ( DATEADD( ss, 1, datefrom )>@Dateto OR DATEADD( ss, -1, dateto )<@Datefrom )";
            cmd.Parameters.Clear();
            cmd.Parameters.Add("@Rampid", SqlDbType.NVarChar).Value = rampid;
            cmd.Parameters.Add("@Datefrom", SqlDbType.DateTime).Value = datefrom;
            cmd.Parameters.Add("@Dateto", SqlDbType.DateTime).Value = dateto;
            dr = cmd.ExecuteReader();
            if (dr.HasRows)
            {
                tmptslices.Clear();
                while (dr.Read())
                {
                    DateTime startdt = DateTime.ParseExact(dr.GetValue(0).ToString().Trim(), "yyyy-MM-dd HH:mm:ss", null);
                    DateTime enddt = DateTime.ParseExact(dr.GetValue(1).ToString().Trim(), "yyyy-MM-dd HH:mm:ss", null).AddSeconds(1);
                    int diffminutes = (int)Math.Floor((double)(int)(enddt - startdt).TotalMinutes / Constants.TimeSliceLenghtInMinutes);
                    tmptslices = WorkCalendar.CreateTimeSlices(rampid, rampdsc, startdt, 1, diffminutes, tmptslices, false);
                }
                // Nájdenie každého dieliku dynamicky rezervovaného času v dielikoch pracovných smien danej rampy a ak sa nájde, tak označenie nájdeného dieliku pracovnej smeny príznakom 'obsadený'
                tslices = WorkCalendar.CompareTimeSlices(tmptslices, tslices);
            }
            dr.Close();
            cmd.Dispose();
            return tslices;
        }

        public static bool AllowReservationVendOnRamp(int type, string rampid, string num)
        {
            // zistenie, ci sa dany dodavatel vyklada na danej rampe
            // pre type=1 sa zistuje, ci na danej rampe moze byt vylozena dana firma (pripad, ze firmu, co ma vyhradenu svoju jednu rampu alebo viac ramp, sa pokusame zapisat na inu rampu)
            // pre type=2 sa zistuje, ci na danej rampe moze byt vylozena data firma (pripad, ze firmu, co nema vyhradenu ziadnu svoju rampu, sa pokusame zapisat na vyhradenu rampu inych firiem)
            bool result = true;
            bool found = false;
            string ident1 = (type == 1 ? num : rampid).Trim();
            string ident2 = (type == 1 ? rampid : num).Trim();
            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader dr;
            try
            {
                cmd.Connection = conn;
                cmd.Connection.Open();

                if (type==1)
                {
                    cmd.CommandText = "SELECT rampid FROM sapvendors_ramp WHERE num=@Ident";
                }
                else
                {
                    cmd.CommandText = "SELECT num FROM sapvendors_ramp WHERE rampid=@Ident";
                }
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@Ident", SqlDbType.NVarChar).Value = ident1;
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        if (dr.GetValue(0).ToString().Trim() == ident2)
                        {
                            found = true;
                            break;
                        }
                    }
                    result = found;
                }
                dr.Close();
                cmd.Dispose();
            }
            catch
            {
                result = false;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
            return result;
        }
    }

    public static class WorkCalendar
    {
        public static int IsCalWorkDay(DateTime date)
        {
            // Táto metóda zistí, či je daný deň pracovný
            int result = 0;
            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader dr;
            try
            {
                cmd.Connection = conn;
                cmd.Connection.Open();

                cmd.CommandText = "SELECT date FROM calendar WHERE date=@Date AND flag2=0";
                cmd.Parameters.Add("@Date", SqlDbType.Date).Value = date;
                dr = cmd.ExecuteReader();
                result = (dr.HasRows == true ? 1 : 0);
                dr.Close();
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

        public static int IsCalWorkDayShiftInDateTime(string rampid, DateTime date)
        {
            // Táto metóda zistí, či na danej rampe daný dátum/čas spadá do pracovnej smeny
            rampid = rampid.Trim();
            int result = 0;
            SqlConnection conn = new SqlConnection(ConnString.Value);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader dr;
            try
            {
                cmd.Connection = conn;
                cmd.Connection.Open();
                DateTime Tmpdate = DateTime.ParseExact(date.ToString("yyyy-MM-dd") + " 06:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                cmd.CommandText = "SELECT shift_morn, shift_aftl, shift_night FROM truckramps_cal WHERE rampid=@Rampid AND date=@Date";
                cmd.Parameters.Add("@Rampid", SqlDbType.NVarChar).Value = rampid;
                cmd.Parameters.Add("@Date", SqlDbType.Date).Value = date.AddDays(date < Tmpdate ? -1 : 0);
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    int shift = 0;
                    string time = date.ToString("HH:mm:ss");   // extrakcia casu z datumu/casu
                    if (TimeFuncs.ConvToS(time) < TimeFuncs.ConvToS("06:00:00"))
                    {
                        shift = 3;
                    }
                    else if (TimeFuncs.ConvToS(time) < TimeFuncs.ConvToS("14:00:00"))
                    {
                        shift = 1;
                    }
                    else if (TimeFuncs.ConvToS(time) < TimeFuncs.ConvToS("22:00:00"))
                    {
                        shift = 2;
                    }
                    else
                    {
                        shift = 3;
                    }
                    dr.Read();   // Precitanie prveho zaznamu (mal by byt vzdy iba jeden)
                    // Ak je v danom case v kalendary nastavena pracovna smena, tak funkcia vrati 1, inak 0
                    result = (shift == 1 && dr.GetValue(0).ToString().Trim() == "1") || (shift == 2 && dr.GetValue(1).ToString().Trim() == "1") || (shift == 3 && dr.GetValue(2).ToString().Trim() == "1") ? 1 : 0;
                }
                else
                {
                    // Chybajuci kalendar smien rampy
                    result = -2;
                }
                dr.Close();
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

        public static int IsCalWorkDayShiftInDate(string rampid, DateTime date)
        {
            // Táto metóda zistí, či na danej rampe je v danom dni aspoň jedna smena pracovná
            // Premenná 'date' obsahuje iba dátum, čas má vo formáte 00:00:00, napr. 2018-06-26 00:00:00
            // Ak chcem otestovaťm či je v danom dátum ranná smena, tak k premennej 'date' pridám šesť hodín a dostanem dátum/čas napr. 2018-06-26 06:00:00
            // tento dátum/čas potom otestujem, či spadá do pracovnej smeny danej rampy. Rovnako pre poobednú smenu pripočítam 14 hodín a pre nočnú smenu 22 hodín
            return (WorkCalendar.IsCalWorkDayShiftInDateTime(rampid, date.AddHours(6)) == 1 || WorkCalendar.IsCalWorkDayShiftInDateTime(rampid, date.AddHours(14)) == 1 ||
                WorkCalendar.IsCalWorkDayShiftInDateTime(rampid, date.AddHours(22)) == 1) ? 1 : 0;
        }

        public static List<TimeSlice> CreateTimeSlices(string rampid, string rampdsc, DateTime date, int shift, int numberoftimeslices, List<TimeSlice> tslices, bool jumpable)
        {
            for (int i = 0; i <= (numberoftimeslices - 1); i++)
            {
                tslices.Add(new TimeSlice(rampid, rampdsc, date.AddMinutes(i * Constants.TimeSliceLenghtInMinutes), date.AddMinutes(i * Constants.TimeSliceLenghtInMinutes).AddSeconds(60 * Constants.TimeSliceLenghtInMinutes - 1),
                    shift, false, jumpable));
            }
            return tslices;
        }

        public static List<TimeSlice> CompareTimeSlices(List<TimeSlice> srctslices, List<TimeSlice> trgtslices)
        {
            // Vyhľadanie jednotlivých časových dielikov zo 'srctslices' v zozname dielikov 'trgtslices'. Ak sa dielik v zozname nájde, tak označiť dielik v zozname ako 'obsadený'
            foreach (TimeSlice blockslice in srctslices)
            {
                for (int i = 0; i <= trgtslices.Count - 1; i++)
                {
                    if (blockslice.rampid == trgtslices[i].rampid  &&  blockslice.startdt == trgtslices[i].startdt)
                    {
                        // Konkrétny dielik zo 'srctslices' sa našiel v dielikoch 'trgtslices', poznačíme teda že nájdený dielik v 'trgtslices' je obsadený a zároveň nastavíme príznak 'obskočiteľnosť' v 'trgtslices' na rovnakú hodnotu ako je v 'srctslices'
                        trgtslices[i].loaded = true;
                        trgtslices[i].jumpable = blockslice.jumpable;
                    }
                }

            }
            return trgtslices;
        }

    }

    public static class TimeFuncs
    {
        public static int ConvToS(string time)
        {
            // Prevod času v vo formáte HH:MM:SS na počet sekúnd
            int retval = 0;
            if (time.Trim().Length == 8)
            {
                retval = Int32.Parse( time.Substring(0, 2) ) * 3600;        // hodiny
                retval = retval + Int32.Parse(time.Substring(3, 2)) * 60;   // minuty
                retval = retval + Int32.Parse(time.Substring(6, 2));        // sekundy
            }
            return retval;
        }
    }

    public class TimeSlice
    {
        public string rampid { get; set; }
        public string rampdsc { get; set; }
        public DateTime startdt { get; set; }
        public DateTime enddt { get; set; }
        public int shift { get; set; }
        public bool loaded { get; set; }
        public bool jumpable { get; set; }

        public TimeSlice(string id, string dsc, DateTime start, DateTime end, int sht, bool ld, bool jmpbl)
        {
            rampid = id;
            rampdsc = dsc;
            startdt = start;
            enddt = end;
            shift = sht;
            loaded = ld;
            jumpable = jmpbl;
        }
    }

    public class Constants
    {
        public const int TimeSlicesPerShift = 48;
        public const int TimeSliceLenghtInMinutes = 10;
        public const string ProAlfaTruckManagementEmailAddress = "Protherm Production s.r.o. <proalfatruckmanagement@protherm.sk>";
    }

    public class IdentityServer4Config
    {
        // clients that are allowed to access resources from the Auth server 
        public static IEnumerable<Client> GetClients()
        {
            // client credentials, list of clients
            return new List<Client>
            {
                new Client
                {
                    ClientId = "---",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AccessTokenLifetime = 3600,
 
                    // Client secrets
                    ClientSecrets =
                    {
                        new Secret("---".Sha256())
                    },
                    AllowedScopes = { "---" }
                },
            };
        }

        // API that are allowed to access the Auth server
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("---", "ProAlfa Truck Management API")
            };
        }
    }

    public static class AppCulture
    {
        private static string _urlculturesuffix;
        private static string _name;

        public static string UrlCultureSuffix
        {   get
            {
                return _urlculturesuffix;
            }
            set
            {
                _urlculturesuffix = value;
            }
        }

        public static string Name
        {   get
            {
                return _name;
            }
            set
            {
               _name = value;
                _urlculturesuffix = "?culture=" + _name;
            }
        }
    }

}
