using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PA.SSH
{
    public class SshConnectionChecker
    {
        private Stopwatch watcher;
        private bool finished = false;
        public SshProfile Profile { get; set; }
        public List<SshConnectionStatus> Log { get; private set; }
        public bool AnyPort { get; set; }
        public string LastMessage { get; private set; }
        public bool IsCanceled { get; private set; }

        public event EventHandler<SshConnectionStatus> LogChanged = null;
        public event EventHandler<EventArgs> Passed = null;
        public SshConnectionChecker()
        {
            Log = new List<SshConnectionStatus>();
        }
        public void AddLog(SshConnectionStatus e)
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
                SshClient client = new SshClient(server, port, username, password);
                client.ErrorOccurred += Client_ErrorOccurred;
                client.HostKeyReceived += Client_HostKeyReceived;
                watcher = new Stopwatch();
                try
                {
                    SshConnectionStatus state = new SshConnectionStatus(
                        client.ConnectionInfo.Host,
                        (ushort)client.ConnectionInfo.Port,
                        DateTime.Now,
                        "Connecting...",
                        null,
                        TimeSpan.Zero,
                         StatusType.Message,
                         0
                        );
                    AddLog(state);
                    if (IsCanceled)
                        return;
                    watcher.Start();
                    client.SayHello();
                    watcher.Stop();
                    state = new SshConnectionStatus(
                        client.ConnectionInfo.Host,
                        (ushort)client.ConnectionInfo.Port,
                        DateTime.Now,
                        "Connected!",
                        null,
                        watcher.Elapsed,
                         StatusType.Done,
                         Profile.PingAvrage
                        );
                    AddLog(state);
                }
                catch (System.ObjectDisposedException)
                {
                    watcher.Stop();
                    SshConnectionStatus state = new SshConnectionStatus(
                        client.ConnectionInfo.Host,
                        (ushort)client.ConnectionInfo.Port,
                        DateTime.Now,
                        "The method was called after the client was disposed.",
                        null,
                        watcher.Elapsed,
                         StatusType.Exception,
                         0
                        );
                    AddLog(state);
                }
                catch (System.InvalidOperationException)
                {
                    watcher.Stop();
                    SshConnectionStatus state = new SshConnectionStatus(
                        client.ConnectionInfo.Host,
                        (ushort)client.ConnectionInfo.Port,
                        DateTime.Now,
                        "The client is already connected.",
                        null,
                        watcher.Elapsed,
                         StatusType.Exception,
                         0
                        );
                    AddLog(state);
                }
                catch (System.Net.Sockets.SocketException)
                {
                    watcher.Stop();
                    SshConnectionStatus state = new SshConnectionStatus(
                        client.ConnectionInfo.Host,
                        (ushort)client.ConnectionInfo.Port,
                        DateTime.Now,
                        "Socket connection to the SSH server or proxy server could not be established, or an error occurred while resolving the hostname.",
                        null,
                        watcher.Elapsed,
                         StatusType.Exception,
                         0
                        );
                    AddLog(state);
                }
                catch (Renci.SshNet.Common.SshConnectionException)
                {
                    watcher.Stop();
                    SshConnectionStatus state = new SshConnectionStatus(
                        client.ConnectionInfo.Host,
                        (ushort)client.ConnectionInfo.Port,
                        DateTime.Now,
                        "SSH session could not be established.",
                        null,
                        watcher.Elapsed,
                         StatusType.Exception,
                         0
                        );
                    AddLog(state);
                }
                catch (Renci.SshNet.Common.SshAuthenticationException)
                {
                    watcher.Stop();
                    SshConnectionStatus state = new SshConnectionStatus(
                        client.ConnectionInfo.Host,
                        (ushort)client.ConnectionInfo.Port,
                        DateTime.Now,
                        "Authentication of SSH session failed.",
                        null,
                        watcher.Elapsed,
                         StatusType.ReplyButNoAthenticate,
                         0
                        );
                    AddLog(state);
                }
                catch (Renci.SshNet.Common.ProxyException)
                {
                    watcher.Stop();
                    SshConnectionStatus state = new SshConnectionStatus(
                        client.ConnectionInfo.Host,
                        (ushort)client.ConnectionInfo.Port,
                        DateTime.Now,
                        "Failed to establish proxy connection.",
                        null,
                        watcher.Elapsed,
                         StatusType.Exception,
                         0
                        );
                    AddLog(state);
                }
                catch (Exception ex)
                {
                    watcher.Stop();
                    SshConnectionStatus state = new SshConnectionStatus(
                        client.ConnectionInfo.Host,
                        (ushort)client.ConnectionInfo.Port,
                        DateTime.Now,
                        ex.Message,
                        null,
                        watcher.Elapsed,
                         StatusType.Exception,
                         0
                        );
                    AddLog(state);
                }
                finally
                {
                    watcher = null;
                    if (client.IsConnected)
                        client.Disconnect();
                }
            });

        }

        public void CancelOperation()
        {
            IsCanceled = true;
        }

        private void Client_HostKeyReceived(object sender, HostKeyEventArgs e)
        {
            watcher.Stop();
            SshClient client = sender as SshClient;
            SshConnectionStatus state = new SshConnectionStatus(
                client.ConnectionInfo.Host,
                (ushort)client.ConnectionInfo.Port,
                DateTime.Now,
                string.Format("{0}\nHost Key Received : \nFingerPrint: {1} \nHostKey : {2} \nHostKey Name : {3}"
                                , Profile.Name
                                , string.Join(",", e.FingerPrint)
                                , string.Join(",", e.HostKey)
                                , e.HostKeyName),
                e.FingerPrint,
                watcher.Elapsed,
                 StatusType.Done,
                 Profile.PingAvrage
                );
            AddLog(state);
            finished = AnyPort;
        }

        private void Client_ErrorOccurred(object sender, ExceptionEventArgs e)
        {
            SshClient client = sender as SshClient;
            SshConnectionStatus state = new SshConnectionStatus(
                client.ConnectionInfo.Host,
                (ushort)client.ConnectionInfo.Port,
                DateTime.Now,
                string.Format("Error Received : {0}", e.Exception.Message),
                null,
                TimeSpan.Zero,
                StatusType.Error,
                0
            );
            AddLog(state);
        }

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
                StringBuilder sb = new StringBuilder().AppendLine(Profile.Name);
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
                    SshConnectionStatus state = new SshConnectionStatus(
                        address,
                        (ushort)0,
                        DateTime.Now,
                        sb.AppendLine("Ping result is OK :)").ToString(),
                        null,
                        TimeSpan.Zero,
                        StatusType.PingOK,
                        Profile.PingAvrage
                        );

                    AddLog(state);
                    return true;
                }
                else
                {
                    SshConnectionStatus state = new SshConnectionStatus(
                        address,
                        (ushort)0,
                        DateTime.Now,
                        sb.AppendLine("Ping result is not OK :(").ToString(),
                        null,
                        TimeSpan.Zero,
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
