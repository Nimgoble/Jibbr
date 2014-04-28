using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using System.Security.Cryptography;
using System.Xml;

using Jibbr.Models;

using Caliburn.Micro;
using Caliburn.Micro.ReactiveUI;
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.protocol.extensions.filetransfer;

namespace Jibbr.ViewModels
{
    public class AccountViewModel : ReactiveScreen
    {
        #region Private Members
        private agsXMPP.XmppClientConnection clientConnection;
        private PresenceManager presenceManager;
        private Timer reconnectTimer = null;
        private bool reconnectOnDisconnect = false;
        #endregion

        #region Constructors/Destructor
        public AccountViewModel()
        {
            username = serverName = password = String.Empty;
            port = 5222;
            ConnectionState = XmppConnectionState.Disconnected;
        }

        public AccountViewModel(Jibbr.Models.Account account)
        {
            username = account.Username;
            serverName = account.ServerName;
            password = account.Password;
            useTLS = account.UseTLS;
            useSSL = account.UseSSL;
            useThisAccount = account.UseThisAccount;
            ConnectionState = XmppConnectionState.Disconnected;
            connectServer = account.ConnectServerName;
            autoResolveConnectServer = account.AutoResolveConnectServer;
            port = account.Port;
            if (InitializeConnection())
            {
                if (useThisAccount)
                    SignIn();
            }
        }

        ~AccountViewModel()
        {
            if (connectionState != XmppConnectionState.Disconnected)
                SignOut();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initialize the connection for this account
        /// </summary>
        /// <returns></returns>
        private Boolean InitializeConnection()
        {
            if (String.IsNullOrEmpty(serverName) || String.IsNullOrEmpty(username) || String.IsNullOrEmpty(password))
                return false;

            if (clientConnection != null)
            {
                clientConnection.Close();
                clientConnection = null;
                presenceManager = null;
            }

            //TODO: If the server's jid winds up not being the same as our ServerName, disconnect the connection, set the ConnectServer = ServerName, set Server = jid, set AutoResolveConnectServer = false
            //and then try to reconnect.
            clientConnection = new XmppClientConnection()
            {
                Server = serverName,
                AutoResolveConnectServer = autoResolveConnectServer,
                ConnectServer = connectServer,
                Username = username,
                Password = password,
                Port = port,
                SocketConnectionType = agsXMPP.net.SocketConnectionType.Direct,
                KeepAlive = true,
                UseSSL = useSSL,
                UseStartTLS = useTLS,
                Resource = "Jabber/XMPP",
                UseCompression = true,
                AutoAgents = true,
                AutoPresence = true,
                AutoRoster = true
            };

            clientConnection.OnXmppConnectionStateChanged += OnXmppConnectionStateChanged;
            clientConnection.OnLogin += OnLogin;
            clientConnection.OnError += OnError;
            clientConnection.OnMessage += OnMessage;
            clientConnection.OnRosterStart += OnRosterStart;
            clientConnection.OnRosterItem += OnRosterItem;
            clientConnection.OnRosterEnd += OnRosterEnd;
            clientConnection.OnAuthError += OnAuthError;
            clientConnection.OnPresence += OnPresence;

            clientConnection.OnAgentStart += clientConnection_OnAgentStart;
            clientConnection.OnAgentItem += clientConnection_OnAgentItem;
            clientConnection.OnAgentEnd += clientConnection_OnAgentEnd;
            clientConnection.OnBinded += clientConnection_OnBinded;
            clientConnection.OnClose += clientConnection_OnClose;
            clientConnection.OnIq += clientConnection_OnIq;
            clientConnection.OnSaslStart += clientConnection_OnSaslStart;
            clientConnection.OnSaslEnd += clientConnection_OnSaslEnd;
            clientConnection.OnSocketError += clientConnection_OnSocketError;
            clientConnection.OnStreamError += clientConnection_OnStreamError;
            clientConnection.OnReadSocketData += clientConnection_OnReadSocketData;
            clientConnection.OnReadXml += clientConnection_OnReadXml;
            clientConnection.OnWriteSocketData += clientConnection_OnWriteSocketData;
            clientConnection.OnWriteXml += clientConnection_OnWriteXml;
            clientConnection.ClientSocket.OnValidateCertificate += ClientSocket_OnValidateCertificate;

            presenceManager = new PresenceManager(clientConnection);

            return true;
        }
        /// <summary>
        /// Sign in with this account
        /// </summary>
        public void SignIn()
        {
            if (this.clientConnection == null)
                return;

            //Make sure the client connection's configuration is up to date
            clientConnection.UseSSL = useSSL;
            clientConnection.UseStartTLS = useTLS;
            clientConnection.ConnectServer = connectServer;
            clientConnection.AutoResolveConnectServer = autoResolveConnectServer;
            clientConnection.Server = serverName;
            clientConnection.Username = username;
            clientConnection.Password = password;
            clientConnection.Port = port;
            //Open the connection
            clientConnection.Open();
        }
        /// <summary>
        /// Sign out
        /// </summary>
        public void SignOut()
        {
            if (this.clientConnection == null)
                return;

            clientConnection.Close();

            lock (chatSessionsMutext)
            {
                chatSessions.Clear();
            }
        }
        /// <summary>
        /// Schedule a call to SignIn in 2.5 seconds
        /// </summary>
        /// <param name="interval"></param>
        private void Reconnect(Int32 interval = 2500)
        {
            if (reconnectTimer != null)
                return;

            reconnectTimer = new Timer(){Interval = interval};
            reconnectTimer.Elapsed += DoReconnect;
            reconnectTimer.Start();
        }
        /// <summary>
        /// Update connection configuration and call SignIn
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoReconnect(object sender, ElapsedEventArgs e)
        {
            //Get rid of the timer
            reconnectTimer.Stop();
            reconnectTimer = null;

            //sign in
            SignIn();
        }
        /// <summary>
        /// Send a message to a particular Jid
        /// </summary>
        /// <param name="target"></param>
        /// <param name="message"></param>
        public void SendMessage(ChatSessionViewModel chatSessionVM, ChatMessage chatMessage)
        {
            //Only allow messages when the session is active
            if (connectionState != XmppConnectionState.SessionStarted)
                return;

            agsXMPP.protocol.client.Message sendMessage = new agsXMPP.protocol.client.Message(chatSessionVM.Target, chatMessage.MessageType, chatMessage.Message);
            clientConnection.Send(sendMessage);
            NotifyChatChatMessage(chatSessionVM, chatMessage);
        }

        public void TryStartNewChatSession(KeyEventArgs eventArgs, JIDViewModel target)
        {
            if (eventArgs.IsDown && eventArgs.Key == Key.Enter)
                StartNewChatSession(target);
        }
        /// <summary>
        /// Open up a new chat session with the target
        /// </summary>
        /// <param name="target"></param>
        public void StartNewChatSession(JIDViewModel target)
        {
            if (connectionState != XmppConnectionState.SessionStarted)
                return;

            lock (chatSessionsMutext)
            {
                //Do we already have a chat session open with this target?
                ChatSessionViewModel chatSession = chatSessions.SingleOrDefault(x => x.Target == target);
                if (chatSession == null)
                {
                    //Create and add.
                    chatSession = new ChatSessionViewModel(this, target);
                    chatSessions.Add(chatSession);
                }

                //Always notify
                NotifyChatSessionInitiatedByUser(chatSession);
            }
        }
        /// <summary>
        /// Gets the timestamp from a message
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private DateTime GetTimestamp(agsXMPP.protocol.client.Message msg)
        {
            try
            {
                DateTime timeStamp;
                agsXMPP.protocol.x.Delay delay = msg.XDelay;
                if (delay != null)
                    timeStamp = delay.Stamp;
                else
                    timeStamp = DateTime.Now;

                return timeStamp;
            }
            catch (Exception ex)
            {
                return DateTime.Now;
            }
        }
        #endregion

        #region Events, etc.
        /// <summary>
        /// Used to notify the MainViewModel that a chat session has started
        /// </summary>
        /// <param name="chatSessionViewModel"></param>
        public delegate void ChatSessionInitiatedByUserHandler(ChatSessionViewModel chatSessionViewModel);
        public event ChatSessionInitiatedByUserHandler ChatSessionInitiatedByUser;
        private void NotifyChatSessionInitiatedByUser(ChatSessionViewModel chatSessionViewModel)
        {
            if (ChatSessionInitiatedByUser != null)
                ChatSessionInitiatedByUser(chatSessionViewModel);
        }

        /// <summary>
        /// Used to notify the MainViewModel that a chat session has started
        /// </summary>
        /// <param name="chatSessionViewModel"></param>
        public delegate void ChatSessionStartedHandler(ChatSessionViewModel chatSessionViewModel);
        public event ChatSessionStartedHandler ChatSessionStarted;
        private void NotifyChatSessionStarted(ChatSessionViewModel chatSessionViewModel)
        {
            if (ChatSessionStarted != null)
                ChatSessionStarted(chatSessionViewModel);
        }
        /// <summary>
        /// Notify any listeners that we've received a chat message.  Used for listeners that are late to the party, etc.
        /// </summary>
        /// <param name="chatSessionViewModel"></param>
        public delegate void ChatMessageHandler(ChatSessionViewModel chatSessionViewModel, ChatMessage chatMessage);
        public event ChatMessageHandler OnChatMessage;
        private void NotifyChatChatMessage(ChatSessionViewModel chatSessionViewModel, ChatMessage chatMessage)
        {
            if (OnChatMessage != null)
                OnChatMessage(chatSessionViewModel, chatMessage);
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Converts this to an Account model, for Xml serialization
        /// </summary>
        /// <returns></returns>
        public Jibbr.Models.Account ToAccount()
        {
            return new Models.Account()
            {
                Username = this.UserName,
                Password = this.Password,
                ServerName = this.ServerName,
                UseSSL = this.UseSSL,
                UseTLS = this.useTLS,
                UseThisAccount = this.UseThisAccount,
                Port = this.port,
                AutoResolveConnectServer = autoResolveConnectServer,
                ConnectServerName = connectServer
            };
        }
        #endregion

        #region Superfulous XMPP Stuff

        bool ClientSocket_OnValidateCertificate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        void clientConnection_OnWriteXml(object sender, string xml)
        {
        }

        void clientConnection_OnWriteSocketData(object sender, byte[] data, int count)
        {
        }

        void clientConnection_OnReadXml(object sender, string xml)
        {
            if (connectionState == XmppConnectionState.Connected)
            {
                try
                {
                    agsXMPP.Xml.Dom.Document document = new agsXMPP.Xml.Dom.Document();
                    document.LoadXml(xml);
                    if (document.ChildNodes.Count > 0)
                    {
                        foreach (agsXMPP.Xml.Dom.Node node in document.ChildNodes)
                        {
                            if (node is agsXMPP.protocol.Base.Stream)
                            {
                                agsXMPP.protocol.Base.Stream stream = node as agsXMPP.protocol.Base.Stream;
                                if (stream.From != serverName && stream.From != connectServer)
                                {
                                    //Do reset here.
                                    SignOut();
                                    AutoResolveConnectServer = false;
                                    ConnectServer = serverName;
                                    ServerName = stream.From;
                                    reconnectOnDisconnect = true;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

        void clientConnection_OnReadSocketData(object sender, byte[] data, int count)
        {
        }

        void clientConnection_OnStreamError(object sender, agsXMPP.Xml.Dom.Element e)
        {
        }

        void clientConnection_OnSocketError(object sender, Exception ex)
        {
        }

        void clientConnection_OnSaslEnd(object sender)
        {
        }

        void clientConnection_OnSaslStart(object sender, agsXMPP.sasl.SaslEventArgs args)
        {
        }

        void clientConnection_OnIq(object sender, agsXMPP.protocol.client.IQ iq)
        {
            if (iq.Type == IqType.error)
            {
                //TODO: Notify of error here
            }
        }

        void clientConnection_OnClose(object sender)
        {
            //Did we manually disconnect the session due to incorrect server name settings?
            if (reconnectOnDisconnect)
            {
                reconnectOnDisconnect = false;
                Reconnect();
            }
        }

        void clientConnection_OnBinded(object sender)
        {
        }

        void clientConnection_OnAgentEnd(object sender)
        {
        }

        void clientConnection_OnAgentItem(object sender, agsXMPP.protocol.iq.agent.Agent agent)
        {
        }

        void clientConnection_OnAgentStart(object sender)
        {
        }
        #endregion

        #region XMPP Callbacks

        void OnXmppConnectionStateChanged(object sender, XmppConnectionState state)
        {
            ConnectionState = state;
        }

        private void OnLogin(object sender)
        {
        }

        private void OnPresence(object sender, agsXMPP.protocol.client.Presence pres)
        {
            string debugme = String.Empty;
        }

        private void OnError(object sender, Exception ex)
        {
        }

        private void OnAuthError(object sender, agsXMPP.Xml.Dom.Element e)
        {
        }

        private void OnMessage(object sender, agsXMPP.protocol.client.Message msg)
        {
            lock (chatSessionsMutext)
            {
                //Do we have a chat session with the sender of this message?
                ChatSessionViewModel chatSession = chatSessions.SingleOrDefault(x => x.Target.Bare == msg.From.Bare);
                if (chatSession == null)
                {
                    JIDViewModel friend = friends.SingleOrDefault(x => x.Bare == msg.From.Bare);
                    if (friend == null)
                    {
                        friend = new JIDViewModel(msg.From, this);
                        friends.Add(friend);
                        //No way to get groups here. :\
                    }
                    //Nope.  Create a new one.
                    chatSession = new ChatSessionViewModel(this, friend);
                    chatSessions.Add(chatSession);
                    NotifyChatSessionStarted(chatSession);
                }

                if (!String.IsNullOrEmpty(msg.Body))
                {
                    ChatMessage newMsg = new ChatMessage()
                        {
                            To = msg.To,
                            From = msg.From,
                            Date = GetTimestamp(msg),
                            Message = msg.Body,
                            MessageType = msg.Type
                        };
                    //Add the message.
                    chatSession.OnMessage(newMsg);

                    NotifyChatChatMessage(chatSession, newMsg);
                }
                else
                    chatSession.OnChatState(msg.Chatstate);
            }
        }

        private void OnRosterStart(object sender)
        {
        }

        private void OnRosterItem(object sender, agsXMPP.protocol.iq.roster.RosterItem item)
        {
            Execute.BeginOnUIThread
            (
                new System.Action
                (
                    () => 
                    {
                        //Create the new JIDViewModel
                        JIDViewModel jidvm = new JIDViewModel(item.Jid, this);
                        //Add it to each group.  Create the group when needed
                        agsXMPP.Xml.Dom.ElementList list = item.GetGroups();
                        foreach (agsXMPP.Xml.Dom.Element element in list)
                        {
                            agsXMPP.protocol.Base.Group group = element as agsXMPP.protocol.Base.Group;
                            RosterGroupViewModel rosterGroupVM = this.groups.SingleOrDefault(x => x.GroupName == group.Name);
                            if (rosterGroupVM == null)
                            {
                                rosterGroupVM = new RosterGroupViewModel(group, this);
                                this.groups.Add(rosterGroupVM);
                                //Shitty, but this is the only way to re-order an observable collection
                                groups = new ObservableCollection<RosterGroupViewModel>(groups.OrderBy(x => x.GroupName));
                                NotifyOfPropertyChange(() => Groups);
                            }
                            rosterGroupVM.Members.Add(jidvm);
                        }
                        //Add to our friends
                        friends.Add(jidvm); 
                    }
                )
            );
        }

        private void OnRosterEnd(object sender)
        {
        }
        #endregion

        #region Properties

        /// <summary>
        /// Whether or not we should sign in with this account
        /// </summary>
        private bool useThisAccount;
        public bool UseThisAccount
        {
            get { return useThisAccount; }
            set
            {
                if (value == useThisAccount)
                    return;

                useThisAccount = value;
                NotifyOfPropertyChange(() => UseThisAccount);

                if (useThisAccount)
                    SignIn();
                else
                    SignOut();
            }
        }
        
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
                NotifyOfPropertyChange(() => AccountJid);
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
                NotifyOfPropertyChange(() => AccountJid);
            }
        }

        /// <summary>
        /// Server connection port
        /// </summary>
        private Int32 port;
        public Int32 Port
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
        /// <summary>
        /// Should we automatically resolve the connect server?
        /// </summary>
        private Boolean autoResolveConnectServer = true;
        public Boolean AutoResolveConnectServer
        {
            get { return autoResolveConnectServer; }
            set
            {
                if (value == autoResolveConnectServer)
                    return;

                autoResolveConnectServer = value;
                NotifyOfPropertyChange(() => AutoResolveConnectServer);
            }
        }
        /// <summary>
        /// Our Connect Server
        /// </summary>
        private String connectServer = String.Empty;
        public String ConnectServer
        {
            get { return connectServer; }
            set
            {
                if (value == connectServer)
                    return;

                connectServer = value;
                NotifyOfPropertyChange(() => ConnectServer);
            }
        }

        /// <summary>
        /// Our Jid for this account
        /// </summary>
        public Jid AccountJid { get { return new Jid(String.Format("{0}@{1}", UserName, ServerName)); } }

        /// <summary>
        /// A list of our friends
        /// </summary>
        private ObservableCollection<JIDViewModel> friends = new ObservableCollection<JIDViewModel>();
        public ObservableCollection<JIDViewModel> Friends
        {
            get { return friends; }
            set
            {
                if (value == friends)
                    return;

                friends = value;
                NotifyOfPropertyChange(() => Friends);
            }
        }

        private ObservableCollection<RosterGroupViewModel> groups = new ObservableCollection<RosterGroupViewModel>();
        public ObservableCollection<RosterGroupViewModel> Groups { get { return groups; } }

        /// <summary>
        /// The connection state of this account
        /// </summary>
        private XmppConnectionState connectionState = XmppConnectionState.Disconnected;
        public XmppConnectionState ConnectionState
        {
            get { return connectionState; }
            set
            {
                if (value == connectionState)
                    return;

                connectionState = value;
                NotifyOfPropertyChange(() => ConnectionState);
                NotifyOfPropertyChange(() => AccountStatus);
            }
        }
        /// <summary>
        /// A display-friendly version of connectionState.
        /// </summary>
        public String AccountStatus
        {
            get
            {
                switch (connectionState)
                {
                    case XmppConnectionState.Authenticated:
                        return "Authenticated";
                    case XmppConnectionState.Authenticating:
                        return "Authenticating";
                    case XmppConnectionState.Binded:
                        return "Bound";
                    case XmppConnectionState.Binding:
                        return "Binding";
                    case XmppConnectionState.Compressed:
                        return "Compressed";
                    case XmppConnectionState.Connected:
                        return "Connected";
                    case XmppConnectionState.Connecting:
                        return "Connecting";
                    case XmppConnectionState.Disconnected:
                        return "Disconnected";
                    case XmppConnectionState.Registered:
                        return "Registered";
                    case XmppConnectionState.Registering:
                        return "Registering";
                    case XmppConnectionState.Securing:
                        return "Securing";
                    case XmppConnectionState.SessionStarted:
                        return "Session Started";
                    case XmppConnectionState.StartCompression:
                        return "Starting Compression";
                    case XmppConnectionState.StartSession:
                        return "Session Started";
                };

                return String.Empty;
            }
        }

        /// <summary>
        /// Our active chat sessions
        /// </summary>
        private object chatSessionsMutext = new object();
        private ObservableCollection<ChatSessionViewModel> chatSessions = new ObservableCollection<ChatSessionViewModel>();
        public ObservableCollection<ChatSessionViewModel> ChatSessions
        {
            get { return chatSessions; }
        }
        #endregion
    }
}
