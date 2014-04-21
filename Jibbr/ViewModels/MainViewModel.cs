using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Jibbr.Events;
using Jibbr.Models;

using Caliburn.Micro;
using Caliburn.Micro.ReactiveUI;
using ReactiveUI;

namespace Jibbr.ViewModels
{
    public class MainViewModel : 
        ReactiveScreen, 
        IHandle<AccountActivatedEvent>,
        IHandle<AccountDeactivatedEvent>
    {
        #region Private Members
        private readonly IEventAggregator eventAggregator;
        private DelayedSearchHelper<JIDViewModel> searchHelper;
        #endregion

        #region Constructor
        public MainViewModel(IEventAggregator eventAggregator)
        {
            accounts = new ObservableCollection<AccountViewModel>();
            DisplayName = "Jibbr";
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);

            //Weeeeeeeeee lambdas
            searchHelper = new DelayedSearchHelper<JIDViewModel>
            (
                500, //Interval
                (tempSearchText) => //Search Action
                {
                    IEnumerable<JIDViewModel> allFriends = accounts.SelectMany(x => x.Friends).OrderBy(x => x.Bare);
                    IEnumerable<JIDViewModel> tempSearchResults = null;
                    if (String.IsNullOrEmpty(tempSearchText))
                        tempSearchResults = allFriends;
                    else
                    {
                        String lowerSearchText = tempSearchText.ToLower();
                        tempSearchResults = from friend
                                                in allFriends
                                            where
                                                friend.Bare.ToLower().Contains(lowerSearchText)
                                            select friend;
                    }
                    return tempSearchResults;
                },
                (tempSearchResults) => //Search Done Action
                {
                    searchResults.Clear();
                    foreach (JIDViewModel friend in tempSearchResults)
                        searchResults.Add(friend);
                }
            );
        }
        #endregion

        #region Methods
        /// <summary>
        /// Starts a new chat message for the given JID
        /// </summary>
        /// <param name="target"></param>
        public void StartNewChatMessage(JIDViewModel target)
        {
            target.Account.StartNewChatSession(target);//This is idiotic.
            ClearSearch();
        }
        /// <summary>
        /// Determines whether the given key should start a new chat message(enter)
        /// </summary>
        /// <param name="eventArgs"></param>
        /// <param name="target"></param>
        public void TryStartNewChatMessage(KeyEventArgs eventArgs, JIDViewModel target)
        {
            if (eventArgs.IsDown && eventArgs.Key == Key.Enter)
                StartNewChatMessage(target);
        }
        /// <summary>
        /// Remove the specified chat session
        /// </summary>
        /// <param name="chatSession"></param>
        public void CloseChatSession(ChatSessionViewModel chatSession)
        {
            chatSessions.Remove(chatSession);
        }

        public void ClearSearch()
        {
            SearchText = String.Empty;
            searchHelper.ForceRefresh();
            SearchResults.Clear();
            SearchResultsVisible = false;
        }
        #endregion

        #region IHandle
        /// <summary>
        /// Add an account that has been activated(UseThisAccount has been checked)
        /// </summary>
        /// <param name="ev"></param>
        public void Handle(AccountActivatedEvent ev)
        {
            ev.Account.ChatSessionStarted += ChatSessionStarted;
            ev.Account.ChatSessionInitiatedByUser += ChatSessionInitiatedByUser;
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
            foreach (ChatSessionViewModel chatSession in ev.Account.ChatSessions)
                chatSessions.Remove(chatSession);

            //This SUCKS, but FullJidComparer is throwing an exception when we do this.  And it doesn't make sense.
            //I think a null value is getting passed to it.
            try
            {
                accounts.Remove(ev.Account);
            }
            catch (Exception ex){}

            NotifyOfPropertyChange(() => Accounts);
        }
        #endregion

        #region Callbacks
        /// <summary>
        /// Called whenever an account creates a new ChatSessionViewModel upon receiving a message from a JID that we did not previously have a chat session with
        /// </summary>
        /// <param name="chatSessionViewModel"></param>
        private void ChatSessionStarted(ChatSessionViewModel chatSessionViewModel)
        {
            Execute.OnUIThread(new System.Action(() => { chatSessions.Add(chatSessionViewModel); }));
        }
        /// <summary>
        /// Called when we have successfully initiated a new chat session
        /// </summary>
        /// <param name="chatSessionViewModel"></param>
        void ChatSessionInitiatedByUser(ChatSessionViewModel chatSessionViewModel)
        {
            Execute.OnUIThread(new System.Action(() => { chatSessions.Add(chatSessionViewModel); SelectedChatSession = chatSessionViewModel; }));
        }
        /// <summary>
        /// We may have closed this chat session, then gotten another message from the target.  Re-open it.
        /// </summary>
        /// <param name="chatSessionViewModel"></param>
        private void OnChatMessage(ChatSessionViewModel chatSessionViewModel)
        {
            Execute.OnUIThread
            (
                new System.Action
                (
                    () => 
                    { 
                        if(!chatSessions.Contains(chatSessionViewModel))
                            chatSessions.Add(chatSessionViewModel); 
                    }
                )
            );
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
        public ObservableCollection<ChatSessionViewModel> ChatSessions { get { return chatSessions; } }

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

        private ObservableCollection<JIDViewModel> searchResults = new ObservableCollection<JIDViewModel>();
        public ObservableCollection<JIDViewModel> SearchResults { get { return searchResults; } }

        private String searchText = String.Empty;
        public String SearchText
        {
            get { return searchText; }
            set
            {
                if (value == searchText)
                    return;

                searchText = value;
                NotifyOfPropertyChange(() => SearchText);
                searchHelper.SearchText = value;
                SearchResultsVisible = !String.IsNullOrEmpty(searchText);
            }
        }

        private Boolean searchResultsVisible = false;
        public Boolean SearchResultsVisible
        {
            get { return searchResultsVisible; }
            set
            {
                if (value == searchResultsVisible)
                    return;

                searchResultsVisible = value;
                NotifyOfPropertyChange(() => SearchResultsVisible);
                NotifyOfPropertyChange(() => SearchResultsVisibility);
            }
        }

        public System.Windows.Visibility SearchResultsVisibility { get { return (searchResultsVisible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed); } }
        #endregion
    }
}
