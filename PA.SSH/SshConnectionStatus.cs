using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PA.SSH
{
    public class SshConnectionStatus
    {
        public string Server { get; set; }
        public ushort Port { get; set; }
        public DateTime Time { get; set; }
        public string Message { get; set; }
        public byte[] Fingerprint { get; set; }
        public TimeSpan Duration { get; set; }
        public StatusType Type { get; set; }

        public SshConnectionStatus(string srvr,ushort port, DateTime time, string message, byte[] fingerprint, TimeSpan duration,StatusType sType)
        {
            Server = srvr;
            Port = port;
            Time = time;
            Message = message;
            Fingerprint = fingerprint;
            Duration = duration;
            Type = sType;
        }
    }
}
