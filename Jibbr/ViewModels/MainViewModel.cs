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
    public class MainViewModel : 
        ReactiveScreen, 
        IHandle<AccountActivatedEvent>,
        IHandle<AccountDeactivatedEvent>
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
        /// <summary>
        /// Add an account that has been activated(UseThisAccount has been checked)
        /// </summary>
        /// <param name="ev"></param>
        public void Handle(AccountActivatedEvent ev)
        {
            ev.Account.ChatSessionStarted += ChatSessionStarted;
            accounts.Add(ev.Account);
            NotifyOfPropertyChange(() => Accounts);
        }
        /// <summary>
        /// Remove an account that has been deactivated(UseThisAccount has been unchecked)
        /// </summary>
        /// <param name="ev"></param>
        public void Handle(AccountDeactivatedEvent ev)
        {
            ev.Account.ChatSessionStarted -= ChatSessionStarted;
            accounts.Remove(ev.Account);
            NotifyOfPropertyChange(() => Accounts);
        }
        #endregion

        #region Callbacks
        /// <summary>
        /// Called whenever an account creates a new ChatSessionViewModel
        /// </summary>
        /// <param name="chatSessionViewModel"></param>
        void ChatSessionStarted(ChatSessionViewModel chatSessionViewModel)
        {
            chatSessions.Add(chatSessionViewModel);
        }
        #endregion

        #region Properties
        /// <summary>
        /// A list of our active accounts
        /// </summary>
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

        /// <summary>
        /// A list of the active chat sessions
        /// </summary>
        private ObservableCollection<ChatSessionViewModel> chatSessions = new ObservableCollection<ChatSessionViewModel>();
        public ObservableCollection<ChatSessionViewModel> ChatSession { get { return chatSessions; } }

        /// <summary>
        /// The currently selected chat session
        /// </summary>
        private ChatSessionViewModel selectedChatSession = null;
        public ChatSessionViewModel SelectedChatSession
        {
            get { return selectedChatSession; }
            set
            {
                if (value == selectedChatSession)
                    return;

                selectedChatSession = value;
                NotifyOfPropertyChange(() => SelectedChatSession);
            }
        }
        #endregion
    }
}
