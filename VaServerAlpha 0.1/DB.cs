using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaServerAlpha_0._1
{

    public class DB
    {
        public static List<UserDeviceInfo> users { get; set; }
        private static DB dB = new DB();
        private static readonly MySqlConnection conn;

        static DB()
        {
            conn = new MySqlConnection("server=localhost;port=3306;user=root;database=mydb;password=root");
            users = new List<UserDeviceInfo>();
            var adapter = new MySqlDataAdapter();
            var table = new DataTable();
            openConnection();
            var command = new MySqlCommand("Select UserName From userbase", conn);
            adapter.SelectCommand = command;
            adapter.Fill(table);
            var a = table.Rows.Cast<DataRow>().Select(c => new[] { c[0] }).ToArray();
            foreach (var b in a)
            {
                users.Add(new UserDeviceInfo(b[0].ToString()));
            }

            closeConnection();
        }


        private static void openConnection()
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();
        }

        public static void AddClientInfo(string Ip, byte[] MAC)
        {
            if (!users.Where(c => c.CheckMac(MAC)).Any() || users.Count == 0)
            {
                try
                {
                    openConnection();
                    var command = new MySqlCommand(
                        "Insert into clientspanel (ClientType_id, DeviceMAC)" +
                        "values (@inv,@mm)", conn);
                    command.Parameters.Add("@inv", MySqlDbType.Int32).Value = 2;
                    command.Parameters.Add("@mm", MySqlDbType.VarChar).Value = Encoding.ASCII.GetString(MAC);
                    command.ExecuteNonQuery();
                    closeConnection();
                    users.Add(new UserDeviceInfo(Ip, MAC));
                }
                catch (Exception e)
                {
                }
            }
        }

        private static void closeConnection()
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }

        public static bool? IsUserWasOnline(string ip)
        {
            try
            {
                var a = users.Where(c => c.IP.Contains(ip)).Any();
                return a;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static bool? ISUserOnline(byte[] mac)
        {
            try
            {
                List<bool> res = new List<bool>();
                foreach (var user in users)
                {
                    if (user.CheckMac(mac))
                        res.Add(user.CheckMac(mac));
                }

                if (res.Count == 0)
                    return false;
                else
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        //public static void analize(string mac, string[] words)
        //{
        //    string req = "";
        //    for (int i = 1; i < words.Length; i++)
        //    {
        //        req += words + " ";
        //    }
        //    var cl = new Client(req);

        //}

        private static void ReqLog(string mac, string reqestType)
        {

            var login = users.First(c => c.CheckMac(Encoding.ASCII.GetBytes(mac))).Login;
            MySqlCommand command;
            if (reqestType != null)
            {
                openConnection();
                command = new MySqlCommand("call UpdateReqlog2(@id,@mac,@reqType)", conn);
                command.Parameters.Add("@id", MySqlDbType.Int32).Value = getUId(login);
                command.Parameters.Add("@mac", MySqlDbType.VarChar).Value = mac;

                command.Parameters.Add("@reqType", MySqlDbType.VarChar).Value = getRId(reqestType);
                command.ExecuteNonQuery();
            }
            else
            {
                openConnection();
                command = new MySqlCommand("call UpdateReqlog(@id,@mac)", conn);
                command.Parameters.Add("@id", MySqlDbType.Int32).Value = getUId(login);
                command.Parameters.Add("@mac", MySqlDbType.VarChar).Value = mac;
                command.ExecuteNonQuery();
            }

            closeConnection();
        }

        private static int getUId(string login)
        {
            var adapter = new MySqlDataAdapter();
            var table = new DataTable();
            openConnection();
            var command =
                new MySqlCommand("Select * From userbase where UserName=@login ",
                    conn);
            command.Parameters.Add("@login", MySqlDbType.VarChar).Value = login;
            adapter.SelectCommand = command;
            adapter.Fill(table);
            closeConnection();
            var a = table.Rows.Cast<DataRow>().Select(c => new[] { c[0] }).ToArray();
            var b = a[0][0];
            return (int)b;
        }

        private static int getRId(string Reqest)
        {
            if (Reqest != null)
            {
                var adapter = new MySqlDataAdapter();
                var table = new DataTable();
                openConnection();
                var command =
                    new MySqlCommand("Select * From requesttype where RequestType=@login ",
                        conn);
                command.Parameters.Add("@login", MySqlDbType.VarChar).Value = Reqest;
                adapter.SelectCommand = command;
                adapter.Fill(table);
                closeConnection();
                var a = table.Rows.Cast<DataRow>().Select(c => new[] { c[0] }).ToArray();
                var b = a[0][0];
                return (int)b;
            }

            return -1;
        }

        public static bool AutorizeUser(string login, string password, string mac)
        {
            var istrue = true;
            try
            {
                var adapter = new MySqlDataAdapter();
                var table = new DataTable();
                openConnection();
                var command =
                    new MySqlCommand("Select * From userbase where UserName=@login and UserPasswordHeshSumm=@passwd",
                        conn);
                command.Parameters.Add("@login", MySqlDbType.VarChar).Value = login;
                command.Parameters.Add("@passwd", MySqlDbType.VarChar).Value = password;
                adapter.SelectCommand = command;
                adapter.Fill(table);
                closeConnection();
                if (table.Rows.Count == 0)
                    istrue = false;
                else
                {
                    var a = table.Rows.Cast<DataRow>().Select(c => new[] { c[0] }).ToArray();
                    var b = a[0][0];
                    openConnection();
                    mac = mac.ToLower();
                    command = new MySqlCommand("call UpdateAutodizelog(@id,@mac)", conn);
                    command.Parameters.Add("@id", MySqlDbType.Int32).Value = b;
                    command.Parameters.Add("@mac", MySqlDbType.VarChar).Value = mac;
                    command.ExecuteNonQuery();
                    closeConnection();
                }
            }
            catch (Exception e)
            {
                istrue = false;
            }
            if (istrue)
            {
                UserDeviceInfo d;
                d = users.Where(c => c.Login == login).First();
                mac = mac.ToUpper();
                d.MACAdress = Encoding.ASCII.GetBytes(mac);
                d.IsOnline = true;
            }

            return istrue;
        }

        public static void AddPreRegestredInfo(byte[] mac, string ip, byte[] sault)
        {
            if (!users.Select(c => c.CheckMac(mac)).ToArray().Any())
            {
                users.Add(new UserDeviceInfo(ip, mac, Encoding.ASCII.GetString(sault)));
            }
            else
            {
                var b = users.Where(c => c.CheckMac(mac)).First();
                b.sault = Encoding.ASCII.GetString(sault);
                b.IP = ip;
            }
        }

        public static bool RegistrateUser(string login, string password, string role, byte[] mac)
        {
            var istrue = true;
            try
            {
                var adapter = new MySqlDataAdapter();
                var table = new DataTable();
                var sault = users.Where(c => c.CheckMac(mac)).First().sault;
                openConnection();
                var command =
                    new MySqlCommand(
                        "insert into userbase ( UserName, UserPasswordHeshSumm,AcsessType,Sault) values (@login,@passwd,@role,@sault)",
                        conn);
                command.Parameters.Add("@login", MySqlDbType.VarChar).Value = login;
                command.Parameters.Add("@passwd", MySqlDbType.VarChar).Value = password;
                command.Parameters.Add("@passwd", MySqlDbType.VarChar).Value = role;
                command.Parameters.Add("@passwd", MySqlDbType.VarChar).Value = sault;
                command.ExecuteNonQuery();
                closeConnection();
                openConnection();
                command = new MySqlCommand(
                    "Select * From userbase where UserName=@login and UserPasswordHeshSumm=@passwd", conn);
                command.Parameters.Add("@login", MySqlDbType.VarChar).Value = login;
                command.Parameters.Add("@passwd", MySqlDbType.VarChar).Value = password;
                adapter.SelectCommand = command;
                adapter.Fill(table);
                closeConnection();
                if (table.Rows.Count == 0)
                    istrue = false;
                else
                {
                    var a = table.Rows.Cast<DataRow>().Select(c => new[] { c[0] }).ToArray();
                    var b = a[0][0];
                    openConnection();
                    command = new MySqlCommand("call UpdateAutodizelog(@id,@mac)", conn);
                    command.Parameters.Add("@id", MySqlDbType.Int32).Value = b;
                    command.Parameters.Add("@mac", MySqlDbType.VarChar).Value = mac;
                    command.ExecuteNonQuery();
                    closeConnection();
                }
            }
            catch (Exception)
            {
                istrue = false;
            }

            if (istrue)
            {
                UserDeviceInfo d;
                d = users.Where(c => c.Login == login).First();
                d.IsOnline = true;
            }

            return istrue;
        }

        public static DataSet GetDataTable(int reportType, string property = null)
        {
            var adapter = new MySqlDataAdapter();
            var table = new DataSet();
            openConnection();

            MySqlCommand command;
            switch (reportType)
            {
                case 1:
                    command = new MySqlCommand("call ReqestChart()", conn);
                    adapter.SelectCommand = command;
                    break;
                case 2:
                    command = new MySqlCommand("call UsersList()", conn);
                    adapter.SelectCommand = command;
                    break;
                case 3:
                    command = new MySqlCommand("call KeyWordsForType(@a)", conn);
                    command.Parameters.Add("@a", MySqlDbType.VarChar).Value = property;
                    adapter.SelectCommand = command;
                    break;
                case 4:
                    command = new MySqlCommand("call ReqestHistoryA(@a)", conn);
                    command.Parameters.Add("@a", MySqlDbType.VarChar).Value = property;
                    adapter.SelectCommand = command;
                    break;
                case 5:
                    command = new MySqlCommand("call UnReqognizedReqests()", conn);
                    adapter.SelectCommand = command;

                    break;
            }

            try
            {
                adapter.Fill(table);
            }
            catch (Exception a)
            {
                return null;
            }

            closeConnection();
            return table;
        }

        public static byte[] GetSault(string login)
        {
            try
            {
                var adapter = new MySqlDataAdapter();
                var table = new DataTable();
                openConnection();
                var command = new MySqlCommand("Select Sault From userbase where UserName=@login", conn);
                command.Parameters.Add("@login", MySqlDbType.VarChar).Value = login;
                adapter.SelectCommand = command;
                adapter.Fill(table);
                closeConnection();


                var a = table.Rows.Cast<DataRow>().Select(c => new[] { c[0] }).ToArray();
                var b = a[0][0].ToString();

                return Encoding.ASCII.GetBytes(b);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static List<KeywordReqType> KB = new List<KeywordReqType>();

        public static void getKeyWords()
        {
            var adapter = new MySqlDataAdapter();
            var table = new DataTable();
            openConnection();
            var command =
                new MySqlCommand(
                    "call GetReqType()",
                    conn);
            adapter.SelectCommand = command;
            adapter.Fill(table);
            closeConnection();


            var a = table.Rows.Cast<DataRow>().Select(c => new[] { c[0], c[1] }).ToArray();
            foreach (var row in a)
            {
                KB.Add(new KeywordReqType(row[0].ToString(), row[1].ToString()));
            }
        }
    }

}
