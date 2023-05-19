using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;

namespace PA.SSH
{
    public class SshClient
    {
        public event EventHandler ProcessSucceed = null;
        public event EventHandler ProcessFailed = null; 
        private Stopwatch watcher= new Stopwatch();
        private Process process;
        private int timeOut = 0;
        private string confermText = string.Empty;
        private bool done = false;
        public SshClient(string server, int port, string username, string password)
        {
            Server = server;
            Port = port;    
            Username = username;
            Password = password;
        }

        public string Server { get; private set; }
        public int Port { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public long ResponseTime { get; private set; }
        public string Data { get; private set; }

        public void SayHello()
        {
            process = new Process();
            process.StartInfo.WorkingDirectory = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
            process.StartInfo.FileName = "ssh.exe";// ,"ssh.exe");
            process.StartInfo.Arguments = $"{Username}@{Server} -p {Port} -o BatchMode=yes  -o StrictHostKeyChecking=no -o ConnectTimeout=5";
            confermText = $"permission denied (publickey,password)".ToLower().Trim();
            process.EnableRaisingEvents = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.UseShellExecute = false;

            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;

            var stdOutput = new StringBuilder();

            // Use AppendLine rather than Append since args.Data is one line of output, not including the newline character.
            process.OutputDataReceived += (o, args) =>
            {
                string data = args?.Data ?? string.Empty;
                ConfermOutput(data);
            };
            process.ErrorDataReceived += Process_ErrorDataReceived;
            process.Exited += Process_Exited;

            try
            {
                watcher = Stopwatch.StartNew();

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
               // ResponseTime = (long)process?.TotalProcessorTime.TotalMilliseconds;
                //if (watcher != null)
                //{
                //    ResponseTime = watcher.ElapsedMilliseconds;
                //    watcher.Stop();
                //    watcher = null;
                //}
                if (!done)
                    ProcessFailed?.Invoke(this, EventArgs.Empty);
            }
            catch
            {
                ProcessFailed?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            if (watcher != null)
            {
                watcher.Stop();
                ResponseTime = watcher.ElapsedMilliseconds;
                watcher = null;
            }
            //Process p = sender as Process;
            //ResponseTime = (long)p.UserProcessorTime.TotalMilliseconds;
        }

        private void ConfermOutput(string data)
        {
            //if(process != null)
            //{
            //    ResponseTime = (long)process.PrivilegedProcessorTime.TotalMilliseconds;
            //}
            if (data.Length > 0)
            {
                if (data.ToLower().Trim().Contains(confermText))
                {
                    done = true;
                    //if (watcher != null)
                    //{
                    //    ResponseTime = watcher.ElapsedMilliseconds;
                    //    watcher.Stop();
                    //    //watcher.Reset();
                    //    watcher = null;
                    //}
                    ProcessSucceed?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            string data = e?.Data ?? string.Empty;
            ConfermOutput(data);
        }
    }
}
