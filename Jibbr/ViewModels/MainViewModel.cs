using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using Caliburn.Micro.ReactiveUI;
using ReactiveUI;
using Jibbr.Events;

namespace Jibbr.ViewModels
{
    public class MainViewModel : Screen, IHandle<AddAccountEvent>
    {
        private readonly IEventAggregator eventAggregator;
        public MainViewModel(IEventAggregator eventAggregator)
        {
            accounts = new ObservableCollection<AccountViewModel>();
            DisplayName = "Jibbr";
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
        }

        #region IHandle
        public void Handle(AddAccountEvent ev)
        {
            accounts.Add(ev.Account);
            NotifyOfPropertyChange(() => Accounts);
        }
        #endregion

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
    }
}
