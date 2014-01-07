using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Caliburn.Micro.ReactiveUI;
using ReactiveUI;
using ReactiveUI.Xaml;

using Jibbr.Events;

namespace Jibbr.ViewModels
{
    public class AccountsViewModel :
        ReactiveConductor<ReactiveScreen>.Collection.OneActive, 
        IHandle<CloseScreenEvent>
    {
        private AccountsListViewModel accountsListViewModel;
        private readonly IEventAggregator eventAggregator;
        public AccountsViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            accountsListViewModel = new AccountsListViewModel(eventAggregator);
            this.ActivateItem(accountsListViewModel);
            Action<AccountViewModel> onCanEdit = e => { NotifyOfPropertyChange(() => CanEditAccount); };
            this.WhenAny(x => x.accountsListViewModel.SelectedAccount, x => x.Value).Subscribe(onCanEdit);
        }

        #region Commands
        public void NewAccount()
        {
            AddAccountViewModel accountViewModel = new AddAccountViewModel(eventAggregator);
            this.ActivateItem(accountViewModel);
        }

        public void EditAccount()
        {
        }

        public bool CanEditAccount
        {
            get
            {
                return (accountsListViewModel.SelectedAccount != null);
            }
        }

        #endregion

        #region IHandle
        public void Handle(CloseScreenEvent ev)
        {
            if (ev.ScreenToClose == this.ActiveItem)
                this.ActiveItem.TryClose();

            this.ActivateItem(accountsListViewModel);
        }
        #endregion

        #region Properties
        public System.Windows.Visibility NewAccountVisibility
        {
            get
            {
                return (this.ActiveItem == accountsListViewModel) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        }
        #endregion

        protected override void OnActivationProcessed(ReactiveScreen item, bool success)
        {
            NotifyOfPropertyChange(() => NewAccountVisibility);
            base.OnActivationProcessed(item, success);
        }
    }
}
