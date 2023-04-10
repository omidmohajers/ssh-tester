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
        public string Provider { get; set; }
        public int ValidationDays { get; set; }
        public string HostAddress { get; set; }
        public string Location { get; set; }
        public string Server { get; set; }
        public ushort[] Ports { get; private set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string PortsString { get; set; }
        public long PingAvrage { get; set; }
        public string Fullname
        {
            get
            {
                return $"{Provider} {ValidationDays} {HostAddress}({Server}) {Location}";
            }
        }
        #endregion
        public SshProfile()
        {
            this.Provider = "Noname";
            this.ValidationDays = 0;
            this.HostAddress = "www.abc.xyz";
            this.Location = "Earth";
            this.Server = "0.0.0.0";
            this.Ports = new ushort[] { 22, 80, 143, 109, 442, 443, 555, 5555 };
            this.Username = "guest";
            this.Password = "psw";
        }
        #region public methods
        public object Clone()
        {
            SshProfile instance = new SshProfile()
            { 
                Provider = this.Provider,
                ValidationDays = this.ValidationDays,
                HostAddress = this.HostAddress,
                Location = this.Location,
                Ports = this.Ports,
                Username = this.Username,
                Password = this.Password,
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
            this.Provider = instance.Provider;
            this.ValidationDays = instance.ValidationDays;
            this.HostAddress = instance.HostAddress;
            this.Location = instance.Location;
            this.Server = instance.Server;
            this.Ports = instance.Ports;
            this.Username = instance.Username;
            this.Password = instance.Password;
        }
        public void Validate()
        {

        }
        public void Deserialize(string info)
        {
            string[] data = info.Split(',');
            this.Provider = string.IsNullOrWhiteSpace(data[0]) ? "noname" : data[0];
            this.ValidationDays = int.Parse(data[1]);
            this.HostAddress = string.IsNullOrWhiteSpace(data[2]) ? "www.abc.xyz" : data[2];
            this.Location = string.IsNullOrWhiteSpace(data[3]) ? "Earth" : data[3];
            this.Server = string.IsNullOrWhiteSpace(data[4]) ? "0.0.0.0" : data[4];
            this.Username = string.IsNullOrWhiteSpace(data[5]) ? "0.0.0.0" : data[5];
            this.Password = string.IsNullOrWhiteSpace(data[6]) ? "0.0.0.0" : data[6];
            this.PortsString = string.IsNullOrWhiteSpace(data[7]) ? "22:80:109:143:555:5555:442" : data[7];
            ParsePortsString();

        }
        public string Serialize()
        {
            string[] data = new string[10];
            data[0] = Provider;
            data[1] = ValidationDays.ToString();
            data[2] = HostAddress;
            data[3] = Location;
            data[4] = Server;
            data[5] = Username;
            data[6] = Password;
            data[7] = PortsString;
            return string.Join(",", data);
        }
        #endregion
    }
}
