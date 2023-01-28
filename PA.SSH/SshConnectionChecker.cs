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
        public SshProfile Profile { get; set; }
        public List<SshConnectionStatus> Log { get; private set; }
        public string LastMessage { get; private set; }

        public event EventHandler<SshConnectionStatus> LogChanged = null;

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
        public async Task ConnectAsync(string server ,int port,string username,string password)
        {
            await Task.Run(() =>
            {
                SshClient client = new SshClient(server, port, username, password);
                client.ErrorOccurred += Client_ErrorOccurred;
                client.HostKeyReceived += Client_HostKeyReceived;
                Stopwatch watcher = new Stopwatch();
                try
                {
                    SshConnectionStatus state = new SshConnectionStatus(
                        client.ConnectionInfo.Host,
                        (ushort)client.ConnectionInfo.Port,
                        DateTime.Now,
                        "Connecting...",
                        null,
                        TimeSpan.Zero,
                         StatusType.Message
                        );
                    AddLog(state);
                    watcher.Start();
                    client.Connect();
                    watcher.Stop();
                    state = new SshConnectionStatus(
                        client.ConnectionInfo.Host,
                        (ushort)client.ConnectionInfo.Port,
                        DateTime.Now,
                        "Connected!",
                        null,
                        watcher.Elapsed,
                         StatusType.Done
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
                         StatusType.Exception
                        );
                    AddLog(state);
                    Task.Delay(5000);
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
                         StatusType.Exception
                        );
                    AddLog(state);
                    Task.Delay(5000);
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
                         StatusType.Exception
                        );
                    AddLog(state);
                    Task.Delay(5000);
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
                         StatusType.Exception
                        );
                    AddLog(state);
                    Task.Delay(5000);
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
                         StatusType.ReplyButNoAthenticate
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
                         StatusType.Exception
                        );
                    AddLog(state);
                    Task.Delay(5000);
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
                         StatusType.Exception
                        );
                    AddLog(state);
                    Task.Delay(5000);
                }
                finally
                {
                    watcher = null;
                    if (client.IsConnected)
                        client.Disconnect();
                }
            });
            
        }

        private void Client_HostKeyReceived(object sender, HostKeyEventArgs e)
        {
            SshClient client = sender as SshClient;
            SshConnectionStatus state = new SshConnectionStatus(
                client.ConnectionInfo.Host,
                (ushort)client.ConnectionInfo.Port,
                DateTime.Now,
                string.Format("Host Key Received : FingerPrint: {0} \n HostKey : {1} \n Host Key Name : {2}"
                                , string.Join(",", e.FingerPrint)
                                , string.Join(",", e.HostKey)
                                , e.HostKeyName),
                e.FingerPrint,
                TimeSpan.Zero,
                 StatusType.Message
                );
            AddLog(state);
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
                 StatusType.Error
                );
            AddLog(state);
        }

        public async void ConnectAsync()
        {
            bool hasPing = await GetPing(Profile.Server);
            if (hasPing)
            {
                var getConnectionTasks = new List<Task<List<SshConnectionStatus>>>();
                foreach (ushort port in Profile.Ports)
                {
                    await ConnectAsync(Profile.Server, port, Profile.Username, Profile.Password);
                }
            }
        }

        public async Task<bool> GetPing(string address)
        {
            return await Task.Run(() =>
            {
                Ping ping = new Ping();
                int done = 0;
                for (int i = 0; i < 4; i++)
                {
                    PingReply reply = ping.Send(address);

                    SshConnectionStatus state = new SshConnectionStatus(
                        address,
                        (ushort)0,
                        DateTime.Now,
                        string.Format("Ping : {0} {1} Time :{2} TTL : {3}", address, reply.Status.ToString(), reply.RoundtripTime, reply.Options?.Ttl),
                        null,
                        new TimeSpan(0, 0, 0, 0, (int)reply.RoundtripTime),
                         StatusType.Message
                        );
                    if (reply.Status == 0)
                        done++;
                    AddLog(state);
                }
                return done >= 3;
            });
        }
    }
}
