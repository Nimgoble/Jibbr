using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Security.Cryptography;
using Caliburn.Micro;
using Caliburn.Micro.ReactiveUI;
using agsXMPP;

namespace Jibbr.ViewModels
{
    public class AccountViewModel : ReactiveScreen
    {
        private agsXMPP.XmppClientConnection clientConnection;
        public AccountViewModel()
        {
            username = serverName = password = String.Empty;
            ConnectionState = XmppConnectionState.Disconnected;
        }

        public void SignIn()
        {
            if (String.IsNullOrEmpty(serverName) || String.IsNullOrEmpty(username) || String.IsNullOrEmpty(password))
                return;

            if (clientConnection != null)
            {
                clientConnection.Close();
                clientConnection = null;
            }

            clientConnection = new XmppClientConnection()
            {
                Server = serverName,
                ConnectServer = null,
                //ConnectServer = String.Format("http://{0}", serverName),
                Username = username,
                Password = password,
                Port = 5222,
                SocketConnectionType = agsXMPP.net.SocketConnectionType.Direct,
                AutoResolveConnectServer = true,
                KeepAlive = true,
                UseSSL = useSSL,
                UseStartTLS = useTSL,
                Resource = "Jabber/XMPP",
                UseCompression = true,
                AutoAgents = true,
                AutoPresence = true,
                AutoRoster = true
            };

            /*clientConnection.AutoResolveConnectServer = true;
            clientConnection.Resource = "Jabber/XMPP";
            clientConnection.Port = 5222;
            clientConnection.UseSSL = false;
            //clientConnection.SocketConnectionType = agsXMPP.net.SocketConnectionType.Direct;
            clientConnection.UseStartTLS = true;
            clientConnection.EnableCapabilities = true;*/

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
            clientConnection.Open();
        }

        public void SignOut()
        {
            if (this.clientConnection == null)
                return;

            clientConnection.Close();
            clientConnection = null;
        }

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

        #region Superfulous XMPP Stuff
        

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
        }

        private void OnPresence(object sender, agsXMPP.protocol.client.Presence pres)
        {
        }

        private void OnError(object sender, Exception ex)
        {
        }

        private void OnAuthError(object sender, agsXMPP.Xml.Dom.Element e)
        {
        }

        private void OnMessage(object sender, agsXMPP.protocol.client.Message msg)
        {
        }

        private void OnRosterStart(object sender)
        {
        }

        private void OnRosterItem(object sender, agsXMPP.protocol.iq.roster.RosterItem item)
        {
        }

        private void OnRosterEnd(object sender)
        {
        }
        #endregion

        #region Properties

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

        private bool useTSL;
        public bool UseTSL
        {
            get { return useTSL; }
            set
            {
                if (value == useTSL)
                    return;

                useTSL = value;
                NotifyOfPropertyChange(() => UseTSL);
            }
        }

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

        public Jid AccountJid
        {
            get 
            {
                return new Jid(String.Format("{0}@{1}", UserName, ServerName)); 
            }
        }

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
        #endregion
    }
}
