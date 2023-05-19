using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace PA.SSH
{
    public class SshConnectionChecker
    {
        private bool finished = false;
        public SshProfile Profile { get; set; }
        public List<SshConnectionStatusEventArgs> Log { get; private set; }
        public bool AnyPort { get; set; }
        public string LastMessage { get; private set; }
        public bool IsCanceled { get; private set; }

        public event EventHandler<SshConnectionStatusEventArgs> LogChanged = null;
        public event EventHandler<EventArgs> Passed = null;
        public SshConnectionChecker()
        {
            Log = new List<SshConnectionStatusEventArgs>();
        }
        public void AddLog(SshConnectionStatusEventArgs e)
        {
            Log.Add(e);
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                LogChanged?.Invoke(this, e);
            });

        }
        public async Task ConnectAsync(string server, int port, string username, string password)
        {
            await Task.Run(() =>
            {
                SshClient client = new SshClient(
                    server,
                    port,
                    username,
                    password);
                client.ProcessSucceed += Client_DataReceived;
                client.ProcessFailed += Client_ProcessFailed;
               // client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(5);
              //  client.ErrorOccurred += Client_ErrorOccurred;
              //  client.HostKeyReceived += Client_HostKeyReceived;
                try
                {
                    SshConnectionStatusEventArgs state = new SshConnectionStatusEventArgs(
                        Profile,
                        client.Server,
                        (ushort)client.Port,
                        DateTime.Now,
                        "Connecting...",
                        null,
                        0,
                         StatusType.Message,
                         0
                        );
                    AddLog(state);
                    if (IsCanceled)
                        return;
                    client.SayHello();
                   //
                    //state = new SshConnectionStatusEventArgs(
                    //    Profile,
                    //    client.Server,
                    //    (ushort)client.Port,
                    //    DateTime.Now,
                    //    "Connected!",
                    //    null,
                    //    0,
                    //     StatusType.Done,
                    //     Profile.PingAvrage
                    //    );
                    //AddLog(state);
                }
                catch (System.ObjectDisposedException)
                {
                    SshConnectionStatusEventArgs state = new SshConnectionStatusEventArgs(Profile,
                        client.Server,
                        (ushort)client.Port,
                        DateTime.Now,
                        "The method was called after the client was disposed.",
                        null,
                        0,
                         StatusType.Exception,
                         0
                        );
                    AddLog(state);
                }
                catch (System.InvalidOperationException)
                {
                    SshConnectionStatusEventArgs state = new SshConnectionStatusEventArgs(
                        Profile,
                        client.Server, (ushort)client.Port,
                        DateTime.Now,
                        "The client is already connected.",
                        null,
                        0,
                         StatusType.Exception,
                         0
                        );
                    AddLog(state);
                }
                catch (System.Net.Sockets.SocketException)
                {
                    SshConnectionStatusEventArgs state = new SshConnectionStatusEventArgs(Profile,
                        client.Server, (ushort)client.Port,
                        DateTime.Now,
                        "Socket connection to the SSH server or proxy server could not be established, or an error occurred while resolving the hostname.",
                        null,
                        0,
                         StatusType.Exception,
                         0
                        );
                    AddLog(state);
                }
                finally
                {
                    //if (client.IsConnected)
                    //    client.Disconnect();
                }
            });

        }

        private void Client_ProcessFailed(object sender, EventArgs e)
        {
            SshClient client = sender as SshClient;
            SshConnectionStatusEventArgs state = new SshConnectionStatusEventArgs(
                Profile,
                client.Server,
                (ushort)client.Port,
                DateTime.Now,
                client.Data,
                null,
                client.ResponseTime,
                 StatusType.Error,
                 Profile.PingAvrage
                );
            AddLog(state);
            finished = AnyPort;
        }

        private void Client_DataReceived(object sender, EventArgs e)
        {
            SshClient client = sender as SshClient;
            SshConnectionStatusEventArgs state = new SshConnectionStatusEventArgs(
                Profile,
                client.Server,
                (ushort)client.Port,
                DateTime.Now,
                client.Data,
                null,
                client.ResponseTime,
                 StatusType.Done,
                 Profile.PingAvrage
                );
            AddLog(state);
            finished = AnyPort;
        }

        public void CancelOperation()
        {
            IsCanceled = true;
        }

        //private void Client_ErrorOccurred(object sender, ExceptionEventArgs e)
        //{
        //    SshClient client = sender as SshClient;
        //    SshConnectionStatusEventArgs state = new SshConnectionStatus(Profile,
        //        client.ConnectionInfo.Host,
        //        (ushort)client.ConnectionInfo.Port,
        //        DateTime.Now,
        //        string.Format("Error Received : {0}", e.Exception.Message),
        //        null,
        //        0,
        //        StatusType.Error,
        //        0
        //    );
        //    AddLog(state);
        //}

        public async void ConnectAsync()
        {
            if (IsCanceled)
                return;
            finished = false;
            bool hasPing = await GetPing(Profile.Server);
            if (hasPing && !IsCanceled)
            {
                foreach (ushort port in Profile.Ports)
                {
                    if (finished || IsCanceled)
                    {
                        finished = true;
                        break;
                    }
                    await ConnectAsync(Profile.Server, port, Profile.Username, Profile.Password);
                }
            }
            Passed?.Invoke(this, EventArgs.Empty);
        }

        public async Task<bool> GetPing(string address)
        {
            return await Task.Run(() =>
            {
                Ping ping = new Ping();
                int done = 0;
                long pingAvrg = 0;

                StringBuilder sb = new StringBuilder().AppendLine(Profile.Fullname);
                for (int i = 0; i < 4; i++)
                {
                    PingReply reply = ping.Send(address);
                    sb.AppendLine(string.Format("Ping : {0} {1} Time :{2} TTL : {3}", address, reply.Status.ToString(), reply.RoundtripTime, reply.Options?.Ttl));

                    if (reply.Status == IPStatus.Success)
                    {
                        done++;
                        pingAvrg += reply.RoundtripTime;
                    }
                }
                if (done >= 3)
                {
                    pingAvrg /= done;
                    Profile.PingAvrage = pingAvrg;
                    SshConnectionStatusEventArgs state = new SshConnectionStatusEventArgs(
                        Profile,
                        address,
                        (ushort)0,
                        DateTime.Now,
                        sb.AppendLine("Ping result is OK :)").ToString(),
                        null,
                        0,
                        StatusType.PingOK,
                        Profile.PingAvrage
                        );

                    AddLog(state);
                    return true;
                }
                else
                {
                    SshConnectionStatusEventArgs state = new SshConnectionStatusEventArgs(
                        Profile,
                        address,
                        (ushort)0,
                        DateTime.Now,
                        sb.AppendLine("Ping result is not OK :(").ToString(),
                        null,
                        0,
                        StatusType.PingError,
                        0
                        );

                    AddLog(state);
                    return false;
                }
            });
        }
    }
}
