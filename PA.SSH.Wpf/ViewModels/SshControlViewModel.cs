using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
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
        public DelegateCommand StopCommand { get; private set; }
        public DelegateCommand LoadCommand { get; private set; }
        public DelegateCommand SaveProfilesCommand { get; private set; }
        public DelegateCommand SaveAsProfilesCommand { get; private set; }
        public DelegateCommand RemoveCommand { get; private set; }
        public DelegateCommand StartCommand { get; private set; }
        public DelegateCommand SaveSuccessListByPingCommand { get; private set; }
        public DelegateCommand SaveSuccessListByResponseCommand { get; private set; }
        public ObservableCollection<SshProfile> SshProfiles { get; private set; }
        public ObservableCollection<SshConnectionStatus> PublicLog { get; private set; }
        public ObservableCollection<SshConnectionStatus> SuccessLog { get; private set; }
        public ObservableCollection<SshConnectionStatus> OutputLog { get; private set; }
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
            OutputLog = new ObservableCollection<SshConnectionStatus>();
            LoadCommand = new DelegateCommand(LoadProfiles, () => true);
            SaveAsProfilesCommand = new DelegateCommand(SaveAsProfiles, () => true);
            RemoveCommand = new DelegateCommand(Remove, () => true);
            SaveProfilesCommand = new DelegateCommand(SaveProfiles, () => true);
            SaveSuccessListByPingCommand = new DelegateCommand(SaveSuccessListByPing, () => true);
            SaveSuccessListByResponseCommand = new DelegateCommand(SaveSuccessListByResponse, () => true);
            StartCommand = new DelegateCommand(StartAsync, () => true);
            StopCommand = new DelegateCommand(Stop, () => true);
            InitilizeData();
        }

        #region Private Methods
        private void SaveAsProfiles()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Text Files|*.txt";
            if (dlg.ShowDialog() ?? false)
                SaveToFile(dlg.FileName);
        }
        private void StartAsync()
        {
            if (SshProfiles.Count == 0)
                return;
            IsListenToCancel = true;
            PassCount = 0;
            PublicLog.Clear();
            SuccessLog.Clear();
            OutputLog.Clear();
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
            MessageBox.Show("Process Canceled By User!!!\nMaybe last Process only countinue working...");
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
                MessageBox.Show("All Profiles Checked!!!");
                IsRunning = false;
            }
        }
        private void ConnectionChecker_LogChanged(object sender, SshConnectionStatus e)
        {
            switch (e.Type)
            {
                case StatusType.Done:
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        OutputLog.Add(e);
                    });
                    break;
                case StatusType.PingOK:
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        SuccessLog.Add(e);
                    });
                    break;
                case StatusType.Error:
                case StatusType.Exception:
                case StatusType.Message:
                case StatusType.ReplyButNoAthenticate:
                case StatusType.PingError:
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        PublicLog.Add(e);
                    });
                    break;
            }
        }
        private void LoadProfiles()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Text Files|*.txt";
            if(dlg.ShowDialog() ?? false)
            {
                string[] lines = File.ReadAllLines(dlg.FileName);
                SshProfiles.Clear();
                foreach(string line in lines)
                {
                    SshProfile prof = new SshProfile();
                    prof.Deserialize(line);
                    SshProfiles.Add(prof);
                }
                if (SshProfiles.Count > 0)
                    SelectedProfile = SshProfiles[0];
            }
        }
        private void SaveSuccessListByResponse()
        {
            //var data = OutputLog.OrderBy(o => o.Duration).ToList();
           // SaveSuccessList(data);
        }
        private void SaveSuccessList()
        {
            string[] lines = new string[OutputLog.Count()];
            int i = 0;
            var MySource = CollectionViewSource.GetDefaultView(SshProfiles);
            foreach (SshConnectionStatus scs in MySource.OfType<SshConnectionStatus>())
            {
                lines[i++] = string.Format("{0},{1},{2},{3},{4},{5},Ping : {6}, Response : {7}ms", scs.Profile?.Provider ?? "", scs.Profile?.ValidationDays ?? 0, scs.Profile?.HostAddress ?? "", scs.Profile?.Location ?? "", scs.Server, scs.Port, scs.PingAvrage, scs.Duration.TotalMilliseconds);
            }
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Text Files|*.txt";
            if (dlg.ShowDialog() ?? false)
                File.WriteAllLines(dlg.FileName, lines);
        }
        private void SaveSuccessListByPing()
        {
           // var data = OutputLog.OrderBy(o => o.PingAvrage).ToList();
            SaveSuccessList();
        }
        private void SaveProfiles()
        {
            SaveToFile(db);
        }
        private void SaveToFile(string fileName)
        {
            var MySource = CollectionViewSource.GetDefaultView(SshProfiles);
            StringBuilder sb = new StringBuilder();
            foreach (SshProfile profile in MySource.OfType<SshProfile>())
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
