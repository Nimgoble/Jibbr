using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Caliburn.Micro;
using Caliburn.Micro.ReactiveUI;

using Jibbr.Controllers;

namespace Jibbr.ViewModels
{
    public class ShellViewModel : ReactiveConductor<IScreen>.Collection.OneActive
    {
        #region Private Members
        private MainViewModel mainViewModel;
        private AccountsViewModel accountsViewModel;
        private readonly IEventAggregator eventAggregator;
        private LogController logController;
        private Boolean isClosing = false;
        #endregion

        #region Constructor
        public ShellViewModel(IEventAggregator eventAggregator, IWindowManager windowManager)
        {
            this.eventAggregator = eventAggregator;
            ShellViewModel.WindowManager = windowManager;
            logController = new LogController(eventAggregator);
            logController.Initialize();
            mainViewModel = new MainViewModel(eventAggregator);
            accountsViewModel = new AccountsViewModel(eventAggregator);
            ActiveItem = mainViewModel;
            DisplayName = "Jibbr";
        }
        #endregion

        #region Methods
        /// <summary>
        /// Only close the main window(our window), if we've called it from the context menu
        /// </summary>
        /// <param name="callback"></param>
        public override void CanClose(Action<bool> callback)
        {
            //If this wasn't called due to the context menu's CloseJibbr command, then hide the window
            if (!isClosing)
            {
                System.Windows.Window window = this.GetView() as System.Windows.Window;
                if (window != null)
                {
                    window.Hide();
                }
            }

            callback(isClosing); // will cancel close
        } 

        public void ShowMain()
        {
            SetActiveItem(mainViewModel);
        }

        public void ShowAccounts()
        {
            SetActiveItem(accountsViewModel);
        }

        private void SetActiveItem(IScreen item)
        {
            ActiveItem = item;
            NotifyOfPropertyChange(() => AccountsVisibility);
            NotifyOfPropertyChange(() => MainVisibility);
        }
        /// <summary>
        /// If Jibbr's window isn't open, open it.
        /// </summary>
        public void ShowJibbr()
        {
            //If our window isn't visible, show it.
            System.Windows.Window window = this.GetView() as System.Windows.Window;
            if (window != null)
            {
                if (!window.IsVisible)
                    window.Show();
            }
        }
        /// <summary>
        /// Close Jibbr
        /// </summary>
        public void CloseJibbr()
        {
            isClosing = true;
            this.TryClose();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Because I'm tired of passing this thing EVERYWHERE
        /// </summary>
        private static IWindowManager windowManager;
        public static IWindowManager WindowManager
        {
            get { return windowManager; }
            set
            {
                if (value == windowManager)
                    return;

                windowManager = value;
            }
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
        #endregion
    }
}
