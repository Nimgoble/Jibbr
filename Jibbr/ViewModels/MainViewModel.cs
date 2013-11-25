using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using ReactiveUI;

namespace Jibbr.ViewModels
{
    public class MainViewModel : Screen
    {
        //private ObservableCollection
        public MainViewModel()
        {
            DisplayName = "Jibbr";
            //AddAccount = 
        }

        //public ICommand AddAccount { get; protected set; }
        public void AddAccount()
        {
        }
    }
}
