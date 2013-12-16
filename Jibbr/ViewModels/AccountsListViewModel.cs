using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        }

        #region IHandle
        public void Handle(AddAccountEvent ev)
        {
            accounts.Add(ev.Account);
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
        #endregion
    }
}
