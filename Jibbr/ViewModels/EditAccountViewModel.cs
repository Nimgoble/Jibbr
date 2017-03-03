using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Caliburn.Micro.ReactiveUI;
using ReactiveUI;
using ReactiveUI.Xaml;

namespace Jibbr.ViewModels
{
    public class EditAccountViewModel : ReactiveScreen
    {
        private AccountViewModel accountViewModel;
        private readonly IEventAggregator eventAggregator;
        public EditAccountViewModel(IEventAggregator eventAggregator, AccountViewModel account)
        {
            this.eventAggregator = eventAggregator;
            this.accountViewModel = account;

            //Set values of editable fields.
            UserName = accountViewModel.UserName;
            Password = accountViewModel.Password;
            ServerName = accountViewModel.ServerName;
            UseTLS = accountViewModel.UseTLS;
            UseSSL = accountViewModel.UseSSL;
            Port = accountViewModel.Port.ToString();
			this.WhenAny(x => x.ServerName, x => x.Value).Subscribe(x => raisePropertyChanged("CanEditAccount"));
			this.WhenAny(x => x.UserName, x => x.Value).Subscribe(x => raisePropertyChanged("CanEditAccount"));
			this.WhenAny(x => x.Password, x => x.Value).Subscribe(x => raisePropertyChanged("CanEditAccount"));
			this.WhenAny(x => x.CanEditAccount, x => x.Value).Subscribe(x => raisePropertyChanged("CanRegisterAccount"));
        }

        #region Methods
        public void EditAccount()
        {
            //Disconnect to make the changes
            if(accountViewModel.UseThisAccount)
                accountViewModel.SignOut();

            //commit edits.
            accountViewModel.UserName = username;
            accountViewModel.Password = password;
            accountViewModel.ServerName = serverName;
            accountViewModel.UseSSL = useSSL;
            accountViewModel.UseTLS = useTLS;
            //Aaaaaaaand what happens if this fails?  There should be an error message of some sort.
            //Need form validation
            Int32 portValue = 5222;
            if (Int32.TryParse(port, out portValue))
                accountViewModel.Port = portValue;

            //Reconnect if we need to.
            if (accountViewModel.UseThisAccount)
                accountViewModel.SignIn();

            //Send out edit account event
            Jibbr.Events.EditAccountEvent editAccountEvent = new Events.EditAccountEvent() { Account = accountViewModel };
            eventAggregator.Publish(editAccountEvent);
            //Send out close event
            eventAggregator.Publish
            (
                new Jibbr.Events.CloseScreenEvent()
                {
                    ScreenToClose = this
                }
            );
        }

        public bool CanEditAccount
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

        public void CancelEdit()
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

		public void RegisterAccount() {
			accountViewModel.InitializeConnection();
			accountViewModel.SignIn(true);
		}

		public bool CanRegisterAccount { get { return CanEditAccount; } }
        #endregion

        #region Properties
        /// <summary>
        /// XMPP connection setting
        /// </summary>
        private bool useTLS;
        public bool UseTLS
        {
            get { return useTLS; }
            set
            {
                if (value == useTLS)
                    return;

                useTLS = value;
                NotifyOfPropertyChange(() => UseTLS);
            }
        }
        /// <summary>
        /// XMPP connection setting
        /// </summary>
        private bool useSSL;
        public bool UseSSL
        {
            get { return useSSL; }
            set
            {
                if (value == useSSL)
                    return;

                useSSL = value;
                NotifyOfPropertyChange(() => UseSSL);
            }
        }

        /// <summary>
        /// Our username for this server
        /// </summary>
        private String username;
        public String UserName
        {
            get { return username; }
            set
            {
                if (value == username)
                    return;

                username = value;
                NotifyOfPropertyChange(() => UserName);
            }
        }

        /// <summary>
        /// Our password for this server
        /// </summary>
        private String password;
        public String Password
        {
            get { return password; }
            set
            {
                if (value == password)
                    return;

                password = value;
                NotifyOfPropertyChange(() => Password);
            }
        }

        /// <summary>
        /// The server we will be connecting to
        /// </summary>
        private String serverName;
        public String ServerName
        {
            get { return serverName; }
            set
            {
                if (value == serverName)
                    return;

                serverName = value;
                NotifyOfPropertyChange(() => ServerName);
            }
        }

        /// <summary>
        /// Server connection port
        /// </summary>
        private String port;
        public String Port
        {
            get { return port; }
            set
            {
                if (value == port)
                    return;

                port = value;
                NotifyOfPropertyChange(() => Port);
            }
        }
        #endregion
    }
}
