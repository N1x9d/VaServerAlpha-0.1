using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaServerAlpha_0._1
{
    public class KeywordReqType 
    {
        public string Word { get; set; }
        public string Type { get; set; }

        public KeywordReqType(string keyWord, string type)
        {
            Word = keyWord;
            Type = type;
        }

    }

    public class UserDeviceInfo
    {
        public UserDeviceInfo(string login)
        {
            Login = login;
        }

        public UserDeviceInfo(string ip, byte[] mac)
        {
            MACAdress = mac;
            this.IP = ip;
        }

        public UserDeviceInfo(string iP, byte[] mac,  string sault)
        {
            MACAdress = mac;
            IP = iP;
            this.sault = sault;
        }

        public string Login { get; set; }
        public byte[] MACAdress { get; set; }
        public string IP { get; set; }
        public string sault { get; set; }
        public bool IsOnline { get; set; }
        public int DevType{ get; set; }
        

        public bool CheckMac(byte[] mac)
        {
            if (mac.Length != MACAdress.Length)
                return false;
            for (int i = 0; i < mac.Length; i++)
                if (mac[i] != MACAdress[i])
                    return false;
            return true;
        }
    }
}
