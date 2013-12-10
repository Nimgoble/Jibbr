using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Jibbr.ViewModels
{
    //[Export(typeof(IShell))]
    public class ShellViewModel : Conductor<Screen>.Collection.OneActive/*, IShell*/
    {
        private MainViewModel mainViewModel;
        private AccountsViewModel accountsViewModel;
        //[ImportingConstructor]
        public ShellViewModel()
        {
            mainViewModel = new MainViewModel();
            accountsViewModel = new AccountsViewModel();
            ActiveItem = mainViewModel;
            DisplayName = "Jibbr";
        }

        public void ShowMain()
        {
            ActiveItem = mainViewModel;
        }

        public void ShowAccounts()
        {
            ActiveItem = accountsViewModel;
        }
    }
}
