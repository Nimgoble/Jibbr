using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Jibbr.Events;
using Jibbr.Models;

using agsXMPP;
using Caliburn.Micro;
using Caliburn.Micro.ReactiveUI;
using ReactiveUI;

namespace Jibbr.ViewModels
{
    public class ChatSessionViewModel : ReactiveScreen
    {
        public ChatSessionViewModel(AccountViewModel account, JIDViewModel target)
        {
            this.account = account;
            this.target = target;
            this.WhenAny(x => x.account, x => x.Value).Subscribe(x => raisePropertyChanged("CanSendMessage"));
        }

        #region Functions
        /// <summary>
        /// Called when our account receives a message for us.  Locks our messages in case we try to send one at the
        /// same time.
        /// </summary>
        /// <param name="message">The messages received from target</param>
        public void OnMessage(ChatMessage message)
        {
            lock (messagesLock)
            {
                //do it on the UI thread.
                Execute.OnUIThread
                (
                    new System.Action
                    (
                        () =>
                        {
                            messages.Add(message);
                            NotifyOfPropertyChange(() => Messages);
                        }
                    )
                );
            }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Used for 'Enter' keypress.  Can't use SendMessage, because CanSendMessage returns false, disabling the textbox, and locking us out.
        /// </summary>
        public void TrySendMessage(KeyEventArgs keyArgs)
        {
            if (keyArgs == null)
                return;

            if (!CanSendMessage)
                return;

            if (keyArgs.Key == Key.Enter && keyArgs.IsDown && (keyArgs.KeyboardDevice.Modifiers != ModifierKeys.Shift))
                SendMessage();
        }
        /// <summary>
        /// Send a message to our target
        /// </summary>
        public void SendMessage()
        {
            //No need to lock this.  Doing so would cause a dead-lock
            account.SendMessage(target, sendText);

            lock (messagesLock)
            {
                messages.Add(new ChatMessage() { To = target.ToString(), From = account.AccountJid, Date = DateTime.Now, Message = sendText });
            }
            //Reset the text
            SendText = String.Empty;
        }
        /// <summary>
        /// Only allows us to send a message if our text is not empty or null
        /// </summary>
        /// <returns></returns>
        public bool CanSendMessage
        {
            get
            {
                bool hasText = !String.IsNullOrEmpty(sendText);
                bool isConnected = (account.ConnectionState == XmppConnectionState.SessionStarted);
                return ( hasText && isConnected );
            }
        }
        #endregion

        #region Properties

        /// <summary>
        /// All of the messages between this account and the target Jid
        /// </summary>
        private object messagesLock = new object();
        private ObservableCollection<ChatMessage> messages = new ObservableCollection<ChatMessage>();
        public ObservableCollection<ChatMessage> Messages { get { return messages; } }

        /// <summary>
        /// The person that this account is chatting with
        /// </summary>
        private JIDViewModel target;
        public JIDViewModel Target { get { return target; } }

        /// <summary>
        /// Whichever account we are using to chat with the target
        /// </summary>
        private AccountViewModel account;
        public AccountViewModel Account { get { return account; } }

        /// <summary>
        /// The text that we'll be sending to target
        /// </summary>
        private String sendText = String.Empty;
        public String SendText
        {
            get { return sendText; }
            set
            {
                if (value == sendText)
                    return;

                sendText = value;
                NotifyOfPropertyChange(() => SendText);
                NotifyOfPropertyChange(() => CanSendMessage);
            }
        }

        #endregion

    }
}
