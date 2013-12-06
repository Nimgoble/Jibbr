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
        //[ImportingConstructor]
        public ShellViewModel()
        {
            mainViewModel = new MainViewModel();
            ActiveItem = mainViewModel;
        }

        public void ShowMain()
        {
            ActiveItem = mainViewModel;
        }

        public void ShowAccounts()
        {
        }
    }
}
