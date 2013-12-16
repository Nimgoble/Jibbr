using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Caliburn.Micro.ReactiveUI;

namespace Jibbr.ViewModels
{
    public class AddAccountViewModel : Caliburn.Micro.ReactiveUI.ReactiveScreen
    {
        private readonly IEventAggregator eventAggregator;
        private AccountViewModel accountViewModel;
        public AddAccountViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            accountViewModel = new AccountViewModel();

            //var canAddAccount = this.
        }

        public ReactiveUI.ReactiveCommand AddAccount { get; protected set; }

        public void fAddAccount()
        {
            //accounts.Add(NewAccountViewModel);
            //Send out add account event
            Jibbr.Events.AddAccountEvent addAccountEvent = new Events.AddAccountEvent();
            addAccountEvent.Account = accountViewModel;
            eventAggregator.Publish(addAccountEvent);
            //Send out close event
            eventAggregator.Publish
            (
                new Jibbr.Events.CloseScreenEvent()
                {
                    ScreenToClose = this
                }
            );
        }

        public bool CanAddAccount
        {
            get
            {

                if (String.IsNullOrEmpty(accountViewModel.UserName))
                    return false;

                if (String.IsNullOrEmpty(accountViewModel.Password))
                    return false;

                if (String.IsNullOrEmpty(accountViewModel.ServerName))
                    return false;

                return true;
            }
        }

        public void CancelAccount()
        {
            //Send out close event
            eventAggregator.Publish
            (
                new Jibbr.Events.CloseScreenEvent()
                {
                    ScreenToClose = this
                }
            );
        }
    }
}
