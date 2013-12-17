using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Caliburn.Micro.ReactiveUI;
using ReactiveUI;
using ReactiveUI.Xaml;


namespace Jibbr.ViewModels
{
    public class AddAccountViewModel : AccountViewModel
    {
        private readonly IEventAggregator eventAggregator;
        public AddAccountViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;

            this.WhenAny(x => x.ServerName, x => x.Value).Subscribe(x => raisePropertyChanged("CanAddAccount"));
            this.WhenAny(x => x.UserName, x => x.Value).Subscribe(x => raisePropertyChanged("CanAddAccount"));
            this.WhenAny(x => x.Password, x => x.Value).Subscribe(x => raisePropertyChanged("CanAddAccount"));
        }

        public void AddAccount()
        {
            //accounts.Add(NewAccountViewModel);
            //Send out add account event
            Jibbr.Events.AddAccountEvent addAccountEvent = new Events.AddAccountEvent();
            addAccountEvent.Account = this;
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
                if (String.IsNullOrEmpty(UserName))
                    return false;

                if (String.IsNullOrEmpty(Password))
                    return false;

                if (String.IsNullOrEmpty(ServerName))
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
