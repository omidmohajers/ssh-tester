using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PA.SSH
{
    public class SshConnectionChecker
    {
        public SshProfile Profile { get; set; }
        public List<SshConnectionStatus> Log { get; private set; }
        public string LastMessage { get; private set; }

        public event EventHandler<EventArgs> Connectting = null;
        public event EventHandler<EventArgs> Connected = null;
        public event EventHandler<HostKeyEventArgs> HostKeyRecieved = null;
        public event EventHandler<UnhandledExceptionEventArgs> ExceptionReceived = null;

        public SshConnectionChecker()
        {
            Log = new List<SshConnectionStatus>();
        }
        public async Task<List<SshConnectionStatus>> ConnectAsync(string server ,int port,string username,string password)
        {
            SshClient client = new SshClient(server, port, username, password);
            client.ErrorOccurred += Client_ErrorOccurred;
            client.HostKeyReceived += Client_HostKeyReceived;
            Stopwatch watcher = new Stopwatch();
            try
            {
                Connectting?.Invoke(this, EventArgs.Empty);
                watcher.Start();
                client.Connect();
                watcher.Stop();
                Connected?.Invoke(this, EventArgs.Empty);
                SshConnectionStatus state = new SshConnectionStatus(
                    client.ConnectionInfo.Host,
                    (ushort)client.ConnectionInfo.Port,
                    DateTime.Now,
                    "Connected!",
                    null,
                    watcher.Elapsed,
                     StatusType.Done
                    );
                Log.Add(state);
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
                Log.Add(state);
                await Task.Delay(5000);
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
                Log.Add(state);
                await Task.Delay(5000);
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
                Log.Add(state);
                await Task.Delay(5000);
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
                Log.Add(state);
                await Task.Delay(5000);
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
                Log.Add(state);
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
                Log.Add(state);
                await Task.Delay(5000);
            }
            catch(Exception ex)
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
                Log.Add(state);
                await Task.Delay(5000);
            }
            finally
            {
                watcher = null;
                if (client.IsConnected)
                    client.Disconnect();
            }
            return await Task.FromResult(Log.Where(l => l.Port == port).ToList());
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
            Log.Add(state);
            HostKeyRecieved?.Invoke(this, e);
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
            Log.Add(state);
            ExceptionReceived?.Invoke(this, new UnhandledExceptionEventArgs(e.Exception, false));
        }

        public async Task<List<SshConnectionStatus>> ConnectAsync(SshProfile profile)
        {
            var getConnectionTasks = new List<Task<List<SshConnectionStatus>>>();
            foreach (ushort port in profile.Ports)
            {
                getConnectionTasks.Add(ConnectAsync(profile.Server, port, profile.Username, profile.Password));
            }
            return await Task.WhenAny(getConnectionTasks).Result;
        }
    }
}
