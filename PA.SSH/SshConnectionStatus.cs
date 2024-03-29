﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PA.SSH
{
    public class SshConnectionStatusEventArgs : EventArgs
    {
        public string Server { get; set; }
        public ushort Port { get; set; }
        public DateTime Time { get; set; }
        public string Message { get; set; }
        public byte[] Fingerprint { get; set; }
        public double Duration { get; set; }
        public StatusType Type { get; set; }
        public long PingAvrage { get; set; }
        public SshProfile Profile { get; set; }

        public SshConnectionStatusEventArgs(SshProfile profile, string srvr,ushort port, DateTime time, string message, byte[] fingerprint, double duration,StatusType sType,long pingAvrg)
        {
            Profile = profile;
            Server = srvr;
            Port = port;
            Time = time;
            Message = message;
            Fingerprint = fingerprint;
            Duration = duration;
            Type = sType;
            PingAvrage = pingAvrg;
        }
    }
}
