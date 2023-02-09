using PA.SSH.Wpf.ViewModels;
using System;

namespace PA.Ssh
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("working...");
            SshControlViewModel worker = new SshControlViewModel();
            worker.StartAsync();
            Console.WriteLine("Wait For Progress...");
            Console.ReadKey();
        }
    }
}
