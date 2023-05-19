using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace testcommad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Stopwatch watcher;
        private Process p;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //   var Server = "45.90.59.242";
            //   var Port = "143";
            //   p = new Process();
            //   p.OutputDataReceived += P_OutputDataReceived;
            //   p.StartInfo.FileName = "ssh.exe";
            //   p.StartInfo.Arguments = $" ssh {Server} -p {Port} -o \"BatchMode=yes\" -o \"ConnectTimeOut=5\"";
            // p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //   p.StartInfo.UseShellExecute = false;
            //   p.StartInfo.RedirectStandardOutput = true;
            //  // p.StartInfo.RedirectStandardInput = true;
            //   p.StartInfo.CreateNoWindow = true;
            //   p.EnableRaisingEvents = true;
            //   p.Start();
            //   var reader = p.StandardOutput;
            //  // p.BeginOutputReadLine();
            //   while (!reader.EndOfStream) ;
            //   string text = reader.ReadLine();
            //   MessageBox.Show(text);
            //////  p.
            ////   //  watcher = Stopwatch.StartNew();
            ////   bool d = p.Start();
            ////   p.Exited += P_Exited;
            ////  // p.BeginOutputReadLine();
            ////   MessageBox.Show(d.ToString());

            ////      p.BeginOutputReadLine();
            ////      //p.WaitForExit();
            ////   //while (!p.HasExited)
            ////   //{
            ////   //    string line = p.StandardOutput.ReadToEnd();
            ////   //    MessageBox.Show(line);
            ////   //}
            ///

            var Server = "141.95.19.230";
            var Port = "22";
            var process = new Process();
            process.StartInfo.WorkingDirectory = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
            process.StartInfo.FileName = "ssh.exe";// ,"ssh.exe");
            process.StartInfo.Arguments = $"guest@{Server} -p {Port} -o \"StrictHostKeyChecking no\" -o \"PasswordAuthentication no\"";

            process.EnableRaisingEvents = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.UseShellExecute = false;

            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;

            var stdOutput = new StringBuilder();

            // Use AppendLine rather than Append since args.Data is one line of output, not including the newline character.
            process.OutputDataReceived += (o , args) =>
            {
                // Console.WriteLine(args.Data);
               // MessageBox.Show(args?.Data ?? "");
                stdOutput.AppendLine(args?.Data ?? "");
            };
            process.ErrorDataReceived += Process_ErrorDataReceived;

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                throw new Exception("OS error while executing ", ex);
            }

            //if (process.ExitCode == 0)
            //{
            //    MessageBox.Show(stdOutput.ToString());
            //}
            //else
            //{

            //    throw new Exception("finished with exit code = " + process.ExitCode);
            //}
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            MessageBox.Show(e.Data);
        }

        private void P_Exited(object sender, EventArgs e)
        {
         //   string line = p.StandardOutput.ReadToEnd();
          //  MessageBox.Show(line);
            // throw new NotImplementedException();
        }

        private void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            MessageBox.Show(p.StandardOutput.ReadToEnd());
            watcher.Stop();
            //ResponseTime = watcher.ElapsedMilliseconds;
            //Data = e.Data;
            //DataReceived?.Invoke(this, e);
            //  p.Close();
            p.Kill();
            p = null;
           
        }
    }
}
