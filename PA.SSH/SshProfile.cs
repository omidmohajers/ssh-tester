using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PA.SSH
{
    public class SshProfile : ICloneable
    {
        #region properties
        public string Server { get; set; }
        public ushort[] Ports { get; private set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string PortsString { get; set; }
        public string Name { get; set; }
        public long PingAvrage { get; set; }
        #endregion
        public SshProfile()
        {
            this.Server = "0.0.0.0";
            this.Ports = new ushort[] { 22, 80, 143, 109, 442, 443, 555, 5555 };
            this.Username = "guest";
            this.Password = "psw";
            this.Name = "ssh profile";
        }
        #region public methods
        public object Clone()
        {
            SshProfile instance = new SshProfile()
            {
                Server = this.Server,
                Ports = this.Ports,
                Username = this.Username,
                Password = this.Password,
                Name = this.Name
            };
            return instance;
        }
        public void ParsePortsString()
        {
            string[] prts = PortsString.Split(':');
            Ports = new ushort[prts.Length];
            if (prts != null && prts.Length > 0)
                for (int i = 0; i < prts.Length; i++)
                {
                    Ports[i] = Convert.ToUInt16(prts[i]);
                }
        }
        public void Copy(SshProfile instance)
        {
            this.Server = instance.Server;
            this.Ports = instance.Ports;
            this.Username = instance.Username;
            this.Password = instance.Password;
            this.Name = instance.Name;
        }
        public void Validate()
        {

        }
        public void Deserialize(string info)
        {
            string[] data = info.Split(',');
            this.Name = string.IsNullOrWhiteSpace(data[0]) ? "noname" : data[0];
            this.Server = string.IsNullOrWhiteSpace(data[0]) ? "0.0.0.0" : data[1];
            this.Username = string.IsNullOrWhiteSpace(data[2]) ? "0.0.0.0" : data[2];
            this.Password = string.IsNullOrWhiteSpace(data[3]) ? "0.0.0.0" : data[3];
            this.PortsString = string.IsNullOrWhiteSpace(data[4]) ? "22,80,109,143,555,5555,442" : data[4];
            ParsePortsString();
            //this.Win1 = string.IsNullOrWhiteSpace(data[5]) ? "0.0.0.0" : data[5];
            //this.Win2 = string.IsNullOrWhiteSpace(data[6]) ? "0.0.0.0" : data[6];
            //this.DHCPEnabled = string.IsNullOrWhiteSpace(data[7]) ? false : bool.Parse(data[7]);
            //this.NIC = string.IsNullOrWhiteSpace(data[8]) ? "" : data[8];

        }
        public string Serialize()
        {
            string[] data = new string[10];
            data[0] = Name;
            data[1] = Server;
            data[2] = Username;
            data[3] = Password;
            data[4] = PortsString;
            //data[5] = Win1;
            //data[6] = Win2;
            //data[7] = DHCPEnabled.ToString();
            //data[8] = NIC;
            //data[9] = Name;
            return string.Join(",", data);
        }
        #endregion
    }
}
