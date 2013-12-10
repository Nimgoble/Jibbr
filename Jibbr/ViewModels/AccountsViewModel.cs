using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Jibbr.ViewModels
{
    public class AccountsViewModel : Screen
    {
        public AccountsViewModel()
        {
            accounts = new ObservableCollection<AccountViewModel>();
            AddAccountVisibility = System.Windows.Visibility.Collapsed;
        }

        public void NewAccount()
        {
            NewAccountViewModel = new AccountViewModel();
            AddAccountVisibility = System.Windows.Visibility.Visible;
        }

        public void AddAccount()
        {
            if (String.IsNullOrEmpty(NewAccountViewModel.UserName))
                return;

            if (String.IsNullOrEmpty(NewAccountViewModel.Password))
                return;

            if (String.IsNullOrEmpty(NewAccountViewModel.ServerName))
                return;

            Accounts.Add(NewAccountViewModel);
            NotifyOfPropertyChange(() => Accounts);

            AddAccountVisibility = System.Windows.Visibility.Collapsed;
        }

        public void CancelAccount()
        {
            AddAccountVisibility = System.Windows.Visibility.Collapsed;
        }

        private ObservableCollection<AccountViewModel> accounts;
        public ObservableCollection<AccountViewModel> Accounts
        {
            get
            {
                return accounts;
            }
            set
            {
                if (value == accounts)
                    return;

                accounts = value;
                NotifyOfPropertyChange(() => Accounts);
            }
        }

        private AccountViewModel newAccountViewModel;
        public AccountViewModel NewAccountViewModel
        {
            get { return newAccountViewModel; }
            set
            {
                if (value == newAccountViewModel)
                    return;

                newAccountViewModel = value;
                NotifyOfPropertyChange(() => NewAccountViewModel);
            }
        }

        private System.Windows.Visibility addAccountVisibility;
        public System.Windows.Visibility AddAccountVisibility
        {
            get { return addAccountVisibility; }
            set
            {
                if (value == addAccountVisibility)
                    return;

                addAccountVisibility = value;
                NotifyOfPropertyChange(() => AddAccountVisibility);
            }
        }
    }
}
