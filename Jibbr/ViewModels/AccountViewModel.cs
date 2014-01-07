using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Security.Cryptography;
using Caliburn.Micro;
using Caliburn.Micro.ReactiveUI;
using agsXMPP;
using agsXMPP.protocol.client;

namespace Jibbr.ViewModels
{
    public class AccountViewModel : ReactiveScreen
    {
        private agsXMPP.XmppClientConnection clientConnection;
        private PresenceManager presenceManager;
        public AccountViewModel()
        {
            username = serverName = password = String.Empty;
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
            if (useThisAccount)
                SignIn();
        }

        #region Functions
        /// <summary>
        /// Sign in with this account
        /// </summary>
        public void SignIn()
        {
            if (String.IsNullOrEmpty(serverName) || String.IsNullOrEmpty(username) || String.IsNullOrEmpty(password))
                return;

            if (clientConnection != null)
            {
                clientConnection.Close();
                clientConnection = null;
                presenceManager = null;
            }

            clientConnection = new XmppClientConnection()
            {
                Server = serverName,
                ConnectServer = null,
                //ConnectServer = "bagmakers.local",
                //ConnectServer = String.Format("http://{0}", serverName),
                Username = username,
                Password = password,
                Port = 5222,
                SocketConnectionType = agsXMPP.net.SocketConnectionType.Direct,
                AutoResolveConnectServer = true,
                //AutoResolveConnectServer = false,
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
            clientConnection = null;
        }

        /// <summary>
        /// Send a message to a particular Jid
        /// </summary>
        /// <param name="target"></param>
        /// <param name="message"></param>
        public void SendMessage(Jid target, String message)
        {
            //Only allow messages when the session is active
            if (connectionState != XmppConnectionState.SessionStarted)
                return;

            agsXMPP.protocol.client.Message sendMessage = new agsXMPP.protocol.client.Message(target, message);
            clientConnection.Send(sendMessage);
        }
        /// <summary>
        /// Open up a new chat session with the target
        /// </summary>
        /// <param name="target"></param>
        public void StartNewChatSession(Jid target)
        {
            lock (chatSessionsMutext)
            {
                //Do we already have a chat session open with this target?
                ChatSessionViewModel chatSession = chatSessions.SingleOrDefault(x => x.Target == target);
                if (chatSession != null)
                    return;

                //Create and add.
                chatSession = new ChatSessionViewModel(this, target);
                chatSessions.Add(chatSession);
                NotifyChatSessionStarted(chatSession);
            }
        }
        #endregion

        #region Events, etc.
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
                UseThisAccount = this.UseThisAccount
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
        }

        void clientConnection_OnClose(object sender)
        {
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
            clientConnection.SendMyPresence();
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
                ChatSessionViewModel chatSession = chatSessions.SingleOrDefault(x => x.Target == msg.From);
                if (chatSession == null)
                {
                    //Nope.  Create a new one.
                    chatSession = new ChatSessionViewModel(this, msg.From);
                    chatSessions.Add(chatSession);
                    NotifyChatSessionStarted(chatSession);
                }

                //Add the message.
                chatSession.OnMessage
                (
                    new Models.ChatMessage() 
                    { 
                        To = AccountJid.ToString(), 
                        From = msg.From.ToString(), 
                        Date = DateTime.Now, 
                        Message = msg.Body 
                    }
                );
            }
        }

        private void OnRosterStart(object sender)
        {
        }

        private void OnRosterItem(object sender, agsXMPP.protocol.iq.roster.RosterItem item)
        {
            friends.Add(item.Jid);
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
        /// Our Jid for this account
        /// </summary>
        public Jid AccountJid
        {
            get 
            {
                return new Jid(String.Format("{0}@{1}", UserName, ServerName)); 
            }
        }

        /// <summary>
        /// A list of our friends
        /// </summary>
        private ObservableCollection<Jid> friends = new ObservableCollection<Jid>();
        public ObservableCollection<Jid> Friends
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
