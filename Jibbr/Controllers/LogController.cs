using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Caliburn.Micro;
using Newtonsoft.Json;
using agsXMPP;
using agsXMPP.protocol.client;

using Jibbr.Events;
using Jibbr.Models;
using Jibbr.ViewModels;

namespace Jibbr.Controllers
{
    public class LogController : IHandle<AccountActivatedEvent>, IHandle<AccountDeactivatedEvent>
    {
        private class LogMessageContainer
        {
            public String FileName { get; set; }
            public ChatMessage ChatMessage { get; set; }
        }

        #region Private Members
        private readonly IEventAggregator eventAggregator;
        private ConcurrentQueue<LogMessageContainer> logMessageQueue = new ConcurrentQueue<LogMessageContainer>();
        private BackgroundWorker logWorker = new BackgroundWorker();
        #endregion

        #region Constructor 
        public LogController(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.eventAggregator.Subscribe(this);
            logWorker.DoWork += WriteLogsBackground;
        }
        #endregion

        #region Methods
        public bool Initialize()
        {
            bool rtn = true;
            try
            {
                if (!Directory.Exists(LogsDirectory))
                    Directory.CreateDirectory(LogsDirectory);
            }
            catch (Exception ex)
            {
                rtn = false;
            }
            return rtn;
        }

        private String AccountLogDirectory(AccountViewModel account)
        {
            return Path.Combine(LogsDirectory, account.AccountJid.User);
        }

        private String JIDLogFile(JIDViewModel jidVM)
        {
            return String.Format(@"{0}\{1}.log", AccountLogDirectory(jidVM.Account), jidVM.User);
        }

        void Account_OnChatMessage(ChatSessionViewModel chatSessionViewModel, ChatMessage chatMessage)
        {
            LogMessageContainer container = new LogMessageContainer() { FileName = JIDLogFile(chatSessionViewModel.Target), ChatMessage = chatMessage };
            logMessageQueue.Enqueue(container);
            if (!logWorker.IsBusy)
                logWorker.RunWorkerAsync();
        }

        void WriteLogsBackground(object sender, DoWorkEventArgs e)
        {
            while (logMessageQueue.Count > 0)
            {
                LogMessageContainer container = null;
                if (logMessageQueue.TryDequeue(out container))
                {
                    JSONLogFile logFile = new JSONLogFile();
                    if (logFile.Initialize(container.FileName))
                        logFile.AddMessageToLog(container.ChatMessage);

                }
            }
        }
        private void CreateLogFile(String fileName)
        {

        }
        #endregion

        #region IHandle
        public void Handle(AccountActivatedEvent ev)
        {
            ev.Account.OnChatMessage += Account_OnChatMessage;
        }

        public void Handle(AccountDeactivatedEvent ev)
        {
            ev.Account.OnChatMessage -= Account_OnChatMessage;
        }
        #endregion

        #region Properties
        private static String ApplicationDirectory
        {
            get { return System.AppDomain.CurrentDomain.BaseDirectory; }
        }

        private static String LogsDirectory
        {
            get { return Path.Combine(ApplicationDirectory, "Logs"); }
        }
        #endregion
    }
}
