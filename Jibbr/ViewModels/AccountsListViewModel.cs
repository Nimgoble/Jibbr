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
using ReactiveUI;
using Jibbr.Events;

namespace Jibbr.ViewModels
{
    public class AccountsListViewModel : 
        ReactiveScreen, 
        IHandle<AddAccountEvent>,
        IHandle<EditAccountEvent>
    {
        private readonly IEventAggregator eventAggregator;
        public AccountsListViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            accounts = new ObservableCollection<AccountViewModel>();

            reactiveAccounts = new ReactiveUI.ReactiveList<AccountViewModel>();
            reactiveAccounts.ChangeTrackingEnabled = true;
            System.Action<IObservedChange<AccountViewModel, object>> writeAccounts = (e) => { this.OnItemChanged(e); };
            this.WhenAnyObservable(x => x.reactiveAccounts.ItemChanged).Subscribe(writeAccounts);

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
                {
                    AccountViewModel accountViewModel = new AccountViewModel(account);
                    this.accounts.Add(accountViewModel);
                    this.reactiveAccounts.Add(accountViewModel);

                    if (accountViewModel.UseThisAccount == true)
                    {
                        object ev = new Events.AccountActivatedEvent() { Account = accountViewModel };
                        eventAggregator.Publish(ev);
                    }
                }
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

        private void WriteAccountsToFile()
        {
            //Booooo.  Loads up the entire file again, adds the new account to the list, then re-writes the file. :\
            Jibbr.Models.AccountsList accountsList = new Models.AccountsList();
            var convertedAccounts = from accountvm in this.accounts select accountvm.ToAccount();
            accountsList.Accounts.AddRange( convertedAccounts );
            using (FileStream accountsFile = new FileStream("Accounts.xml", FileMode.Truncate))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Jibbr.Models.AccountsList));
                serializer.Serialize(accountsFile, accountsList);
            }
        }

        private void OnItemChanged(IObservedChange<AccountViewModel, object> change)
        {
            if (change.PropertyName == "UseThisAccount")
            {
                WriteAccountsToFile();

                //Let everyone know
                object ev = null;
                if (change.Sender.UseThisAccount == true)
                    ev = new Events.AccountActivatedEvent() { Account = change.Sender };
                else
                    ev = new Events.AccountDeactivatedEvent() { Account = change.Sender };

                eventAggregator.Publish(ev);
            }
        }

        #endregion

        #region IHandle
        public void Handle(AddAccountEvent ev)
        {
            accounts.Add(ev.Account);
            reactiveAccounts.Add(ev.Account);
            WriteAccountsToFile();
        }

        public void Handle(EditAccountEvent ev)
        {
            //This will have to do until we have a better way to replace a single account in the accounts.xml file.
            WriteAccountsToFile();
        }
       
        #endregion

        #region Properties
        private ReactiveUI.ReactiveList<AccountViewModel> reactiveAccounts;
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
