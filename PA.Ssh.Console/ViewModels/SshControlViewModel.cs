using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PA.SSH.Wpf.ViewModels
{
    public class SshControlViewModel : BindableBase
    {
        #region Fileds
        private SshConnectionChecker connectionChecker = null;
        private double passCount;
        private bool runAsAnyPortMethod; 
        private bool isRunning;
        private bool isListenToCancel;
        private List<SshConnectionChecker> checkers;
        private const string db = "input.txt";
        #endregion Fileds

        #region Properties
        public bool IsListenToCancel
        {
            get => isListenToCancel;
            set
            {
                SetProperty(ref isListenToCancel, value);
            }
        }
        public bool IsRunning
        {
            get => isRunning;
            set
            {
                SetProperty(ref isRunning, value);
                RaisePropertyChanged("IsIdle");
                RaisePropertyChanged("IsFinished");
            }
        }
        public bool IsIdle { get => !IsRunning; }
        public double PassCount { get => passCount; set => SetProperty(ref passCount, value); }
        public bool RunAsAnyPortMethod { get => runAsAnyPortMethod; set => SetProperty(ref runAsAnyPortMethod, value); }
        public ObservableCollection<SshProfile> SshProfiles { get; private set; }
        public ObservableCollection<SshConnectionStatus> PublicLog { get; private set; }
        public ObservableCollection<SshConnectionStatus> SuccessLog { get; private set; }
        public SshProfile SelectedProfile { get; set; }
        public bool IsFinished
        {
            get
            {
                return PassCount == SshProfiles.Count || IsCanceled;
            }
        }
        public bool IsCanceled { get; private set; }
        #endregion Properties

        public SshControlViewModel()
        {
            IsListenToCancel = false;
            PassCount = 0;
            IsCanceled = false;
            IsRunning = false;
            SshProfiles = new ObservableCollection<SshProfile>();
            PublicLog = new ObservableCollection<SshConnectionStatus>();
            SuccessLog = new ObservableCollection<SshConnectionStatus>();
            InitilizeData();
        }

        #region Private Methods
        public void StartAsync()
        {
            if (SshProfiles.Count == 0)
                return;
            IsListenToCancel = true;
            PassCount = 0;
            PublicLog.Clear();
            SuccessLog.Clear();
            IsCanceled = false;
            IsRunning = true;
            checkers = new List<SshConnectionChecker>();
            foreach(SshProfile prof in SshProfiles)
            {
                if (IsCanceled)
                {
                    IsRunning = false;
                    break;
                }
                connectionChecker = new SshConnectionChecker();
                connectionChecker.AnyPort = RunAsAnyPortMethod;
                connectionChecker.Passed += ConnectionChecker_Passed;
                connectionChecker.LogChanged += ConnectionChecker_LogChanged;
                connectionChecker.Profile = prof;
                connectionChecker.ConnectAsync();
                checkers.Add(connectionChecker);
            }
        }
        private void Stop()
        {
            BroadcastCancelation();
            IsCanceled = true;
            IsListenToCancel = false;
            Console.WriteLine("Process Canceled By User!!!\nMaybe last Process only countinue working...");
        }
        private void BroadcastCancelation()
        {
            foreach (SshConnectionChecker checker in checkers)
                checker.CancelOperation();
        }
        private void ConnectionChecker_Passed(object sender, EventArgs e)
        {
            if (++PassCount == SshProfiles.Count)
            {
                Console.WriteLine("All Profiles Checked!!!");
                IsRunning = false;
                SaveSuccessListByResponse();
                Console.WriteLine("Check 'output.txt' File");
                Console.ReadKey();
            }
        }
        private void ConnectionChecker_LogChanged(object sender, SshConnectionStatus e)
        {
            switch (e.Type)
            {
                case StatusType.Done:
                case StatusType.PingOK:
                    //Console.Current.Dispatcher.Invoke((Action)delegate
                    //{
                    SuccessLog.Add(e);
                    Console.WriteLine(string.Format("{0} - {1}:{2} Ping avg :{3}ms => {4}", e.Time.ToString(), e.Server, e.Port, e.PingAvrage, e.Message));
                    // });
                    break;
                case StatusType.Error:
                case StatusType.Exception:
                case StatusType.Message:
                case StatusType.ReplyButNoAthenticate:
                case StatusType.PingError:
                    //App.Current.Dispatcher.Invoke((Action)delegate
                    //{
                    PublicLog.Add(e);
                    // });
                    break;
            }
        }
        private void SaveSuccessListByResponse()
        {
            var data = SuccessLog.Where(log => log.Type == StatusType.Done).OrderBy(o => o.Duration);
            SaveSuccessList(data);
        }
        private void SaveSuccessList(IOrderedEnumerable<SshConnectionStatus> data)
        {
            StringBuilder sb = new StringBuilder();
            foreach (SshConnectionStatus scs in data)
            {
                string name = SshProfiles.FirstOrDefault(item => item.Server == scs.Server)?.Name;
                sb.AppendLine(string.Format("{0},{1},{2},Ping : {3}, Response : {4}ms", name ?? "", scs.Server, scs.Port,scs.PingAvrage,scs.Duration.TotalMilliseconds));
            }
            string filename = "output.txt";
                File.WriteAllText(filename, sb.ToString());
        }
        private void SaveSuccessListByPing()
        {
            var data = SuccessLog.Where(log => log.Type == StatusType.Done).OrderBy(o => o.PingAvrage);
            SaveSuccessList(data);
        }
        private void SaveProfiles()
        {
            SaveToFile(db);
        }
        private void SaveToFile(string fileName)
        {
            StringBuilder sb = new StringBuilder();
            foreach (SshProfile profile in SshProfiles)
                sb.AppendLine(profile.Serialize());
            File.WriteAllText(fileName, sb.ToString());
        }
        private void Remove()
        {
            if (SelectedProfile == null)
                return;
            SshProfiles.Remove(SelectedProfile);
        }
        private void InitilizeData()
        {
            string[] lines = File.ReadAllLines(db);
            SshProfiles.Clear();
            foreach (string line in lines)
            {
                SshProfile prof = new SshProfile();
                prof.Deserialize(line);
                SshProfiles.Add(prof);
            }
            if (SshProfiles.Count > 0)
                SelectedProfile = SshProfiles[0];
        }
        #endregion Private methods
    }
}
