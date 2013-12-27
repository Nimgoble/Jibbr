using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Caliburn.Micro;
using Caliburn.Micro.ReactiveUI;
using Jibbr.Events;

namespace Jibbr.ViewModels
{
    public class AccountsListViewModel : ReactiveScreen, IHandle<AddAccountEvent>
    {
        private readonly IEventAggregator eventAggregator;
        public AccountsListViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            accounts = new ObservableCollection<AccountViewModel>();
            LoadAccounts();
        }

        #region Functions
        /// <summary>
        /// Loads up the accounts saved in the Accounts.xml file
        /// </summary>
        private void LoadAccounts()
        {
            if (!AccountsFileExists())
                CreateAccountsFile();
            else
            {
                Jibbr.Models.AccountsList accountsList = GetSavedAccounts();
                foreach (Jibbr.Models.Account account in accountsList.Accounts)
                    this.accounts.Add(new AccountViewModel(account));
            }
        }

        /// <summary>
        /// Gets an AccountsList from the Accounts.xml file.  AccountsList is exactly what it sounds like
        /// </summary>
        /// <returns></returns>
        private Jibbr.Models.AccountsList GetSavedAccounts()
        {
            Jibbr.Models.AccountsList accountsList;
            using (FileStream accountsFile = new FileStream("Accounts.xml", FileMode.Open))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Jibbr.Models.AccountsList));
                accountsList = (Jibbr.Models.AccountsList)serializer.Deserialize(accountsFile);
            }

            return accountsList;
        }

        /// <summary>
        /// Is this necessary?
        /// </summary>
        /// <returns></returns>
        private Boolean AccountsFileExists()
        {
            return File.Exists("Accounts.xml");
        }

        /// <summary>
        /// Creates and initializes the Accounts.xml file, if it doesn't exist.
        /// </summary>
        private void CreateAccountsFile()
        {
            using (FileStream accountsFile = new FileStream("Accounts.xml", FileMode.OpenOrCreate))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Jibbr.Models.AccountsList));
                Jibbr.Models.AccountsList accountsList = new Models.AccountsList();
                serializer.Serialize(accountsFile, accountsList);
            }
        }
        #endregion

        #region IHandle
        public void Handle(AddAccountEvent ev)
        {
            accounts.Add(ev.Account);

            //Booooo.  Loads up the entire file again, adds the new account to the list, then re-writes the file. :\
            Jibbr.Models.AccountsList accountsList = GetSavedAccounts();
            accountsList.Accounts.Add(ev.Account.ToAccount());
            using (FileStream accountsFile = new FileStream("Accounts.xml", FileMode.Truncate))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Jibbr.Models.AccountsList));
                serializer.Serialize(accountsFile, accountsList);
            }
        }
        #endregion

        #region Properties
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

        private AccountViewModel selectedAccount;
        public AccountViewModel SelectedAccount
        {
            get { return selectedAccount; }
            set
            {
                if (value == selectedAccount)
                    return;

                selectedAccount = value;
                NotifyOfPropertyChange(() => SelectedAccount);
            }
        }
        #endregion
    }
}
