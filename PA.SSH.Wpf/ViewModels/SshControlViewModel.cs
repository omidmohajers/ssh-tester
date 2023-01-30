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

namespace PA.SSH.Wpf.ViewModels
{
    public class SshControlViewModel : BindableBase
    {
        private SshConnectionChecker connectionChecker = null;
        private double passCount;
        private bool runAsAnyPortMethod;
        public double PassCount { get => passCount; set => SetProperty(ref passCount, value); }
        public bool RunAsAnyPortMethod { get => runAsAnyPortMethod; set => SetProperty(ref runAsAnyPortMethod, value); }
        public DelegateCommand LoadCommand { get; private set; }
        public DelegateCommand SaveProfilesCommand { get; private set; }
        public DelegateCommand AddCommand { get; private set; }
        public DelegateCommand EditCommand { get; private set; }
        public DelegateCommand RemoveCommand { get; private set; }
        public DelegateCommand StartCommand { get; private set; }
        public DelegateCommand SaveSuccessListByPingCommand { get; private set; }
        public DelegateCommand SaveSuccessListByResponseCommand { get; private set; }
        
        public ObservableCollection<SshProfile> SshProfiles { get; private set; }
        public ObservableCollection<SshConnectionStatus> PublicLog { get; private set; }
        public ObservableCollection<SshConnectionStatus> SuccessLog { get; private set; }
        public SshProfile SelectedProfile { get; set; }
        public SshControlViewModel()
        {
            SshProfiles = new ObservableCollection<SshProfile>();
            PublicLog = new ObservableCollection<SshConnectionStatus>();
            SuccessLog = new ObservableCollection<SshConnectionStatus>();
            LoadCommand = new DelegateCommand(LoadProfiles, () => true);
            AddCommand = new DelegateCommand(Add, () => true);
            EditCommand = new DelegateCommand(Edit, () => true);
            RemoveCommand = new DelegateCommand(Remove, () => true);
            SaveProfilesCommand = new DelegateCommand(SaveProfiles, () => true);
            SaveSuccessListByPingCommand = new DelegateCommand(SaveSuccessListByPing, () => true);
            SaveSuccessListByResponseCommand = new DelegateCommand(SaveSuccessListByResponse, () => true);
            StartCommand = new DelegateCommand(StartAsync, () => true);
            InitilizeData();
        }
        private void StartAsync()
        {
            if (SshProfiles.Count == 0)
                return;
            PassCount = 0;
            PublicLog.Clear();
            SuccessLog.Clear();
            foreach(SshProfile prof in SshProfiles)
            {
                connectionChecker = new SshConnectionChecker();
                connectionChecker.AnyPort = RunAsAnyPortMethod;
                connectionChecker.Passed += ConnectionChecker_Passed;
                connectionChecker.LogChanged += ConnectionChecker_LogChanged;
                connectionChecker.Profile = prof;
                connectionChecker.ConnectAsync();
            }
        }

        private void ConnectionChecker_Passed(object sender, EventArgs e)
        {
            if (++PassCount == SshProfiles.Count)
                MessageBox.Show("All Profiles Checked!!!");
        }

        private void ConnectionChecker_LogChanged(object sender, SshConnectionStatus e)
        {
            switch (e.Type)
            {
                case StatusType.Done:
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
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Text Files|*.txt";
            if (dlg.ShowDialog() ?? false)
                File.WriteAllText(dlg.FileName, sb.ToString());
        }

        private void SaveSuccessListByPing()
        {
            var data = SuccessLog.Where(log => log.Type == StatusType.Done).OrderBy(o => o.PingAvrage);
            SaveSuccessList(data);
        }

        private void SaveProfiles()
        {
            throw new NotImplementedException();
        }

        private void Remove()
        {
            throw new NotImplementedException();
        }

        private void Edit()
        {
            throw new NotImplementedException();
        }

        private void Add()
        {
            throw new NotImplementedException();
        }

        private void InitilizeData()
        {

        }
    }
}
