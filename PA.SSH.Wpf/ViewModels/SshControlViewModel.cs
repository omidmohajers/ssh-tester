using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PA.SSH.Wpf.ViewModels
{
    public class SshControlViewModel : BindableBase
    {
        public DelegateCommand LoadCommand { get; private set; }
        public DelegateCommand SaveProfilesCommand { get; private set; }
        public DelegateCommand AddCommand { get; private set; }
        public DelegateCommand EditCommand { get; private set; }
        public DelegateCommand RemoveCommand { get; private set; }
        public DelegateCommand SaveSuccessListCommand { get; private set; }
        public ObservableCollection<SshProfile> SshProfiles { get; private set; }
        public ObservableCollection<SshConnectionStatus> PublicLog { get; private set; }
        public ObservableCollection<SshConnectionStatus> SuccessLog { get; private set; }
        public SshProfile SelectedProfile { get; set; }
        public SshControlViewModel()
        {
            SshProfiles = new ObservableCollection<SshProfile>();
            AddCommand = new DelegateCommand(Add, () => true);
            EditCommand = new DelegateCommand(Edit, () => true);
            RemoveCommand = new DelegateCommand(Remove, () => true);
            SaveProfilesCommand = new DelegateCommand(SaveProfiles, () => true);
            SaveSuccessListCommand = new DelegateCommand(SaveSuccessList, () => true);
            InitilizeData();
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
