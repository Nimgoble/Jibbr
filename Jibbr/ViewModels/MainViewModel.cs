using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using ReactiveUI;

namespace Jibbr.ViewModels
{
    public class MainViewModel : Screen
    {
        
        public MainViewModel()
        {
            DisplayName = "Jibbr";
            accounts = new ObservableCollection<AccountViewModel>();
            //AddAccount = 
            CurrentAccount = new AccountViewModel();
        }

        //public ICommand AddAccount { get; protected set; }
        public void AddAccount()
        {
            AccountViewModel viewModel = new AccountViewModel();
            viewModel.ActivateWith(this);
            accounts.Add(viewModel);
            NotifyOfPropertyChange(() => Accounts);
        }

        private ObservableCollection<AccountViewModel> accounts;
        public ObservableCollection<AccountViewModel> Accounts
        {
            get { return accounts; }
            set
            {
                accounts = value;
                NotifyOfPropertyChange(() => Accounts);
            }
        }

        public AccountViewModel CurrentAccount { get; set; }
    }
}
