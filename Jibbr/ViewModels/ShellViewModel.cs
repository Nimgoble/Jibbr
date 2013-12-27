using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Caliburn.Micro.ReactiveUI;

namespace Jibbr.ViewModels
{
    //[Export(typeof(IShell))]
    public class ShellViewModel : Conductor<Screen>.Collection.OneActive
    {
        private MainViewModel mainViewModel;
        private AccountsViewModel accountsViewModel;
        private readonly IEventAggregator eventAggregator;
        //[ImportingConstructor]
        public ShellViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            mainViewModel = new MainViewModel(eventAggregator);
            accountsViewModel = new AccountsViewModel(eventAggregator);
            ActiveItem = mainViewModel;
            DisplayName = "Jibbr";
        }

        public void ShowMain()
        {
            SetActiveItem(mainViewModel);
        }

        public void ShowAccounts()
        {
            SetActiveItem(accountsViewModel);
        }

        private void SetActiveItem(Screen item)
        {
            ActiveItem = item;
            NotifyOfPropertyChange(() => AccountsVisibility);
            NotifyOfPropertyChange(() => MainVisibility);
        }

        public System.Windows.Visibility AccountsVisibility
        {
            get
            {
                return (ActiveItem == mainViewModel) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        }

        public System.Windows.Visibility MainVisibility
        {
            get
            {
                return (ActiveItem == accountsViewModel) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        }
    }
}
