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

namespace PA.SSH.Wpf.ViewModels
{
    public class SshControlViewModel : BindableBase
    {
        private SshConnectionChecker connectionChecker = null;
        public DelegateCommand LoadCommand { get; private set; }
        public DelegateCommand SaveProfilesCommand { get; private set; }
        public DelegateCommand AddCommand { get; private set; }
        public DelegateCommand EditCommand { get; private set; }
        public DelegateCommand RemoveCommand { get; private set; }
        public DelegateCommand StartCommand { get; private set; }
        public DelegateCommand SaveSuccessListCommand { get; private set; }
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
            SaveSuccessListCommand = new DelegateCommand(SaveSuccessList, () => true);
            StartCommand = new DelegateCommand(StartAsync, () => true);
            InitilizeData();
        }

        private void StartAsync()
        {
            if (SshProfiles.Count == 0)
                return;
            PublicLog.Clear();
            SuccessLog.Clear();
            foreach(SshProfile prof in SshProfiles)
            {
                connectionChecker = new SshConnectionChecker();
                connectionChecker.LogChanged += ConnectionChecker_LogChanged;
                connectionChecker.Profile = prof;
                connectionChecker.ConnectAsync();
               // connectionChecker.LogChanged -= ConnectionChecker_LogChanged;
            }
        }

        private void ConnectionChecker_LogChanged(object sender, SshConnectionStatus e)
        {
            switch (e.Type)
            {
                case StatusType.Done:
                case StatusType.ReplyButNoAthenticate:
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        SuccessLog.Add(e);
                    });
                    break;
                case StatusType.Error:
                case StatusType.Exception:
                case StatusType.Message:
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

        private void SaveSuccessList()
        {
            throw new NotImplementedException();
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
