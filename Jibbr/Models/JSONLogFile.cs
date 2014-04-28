using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using agsXMPP;
using agsXMPP.protocol.client;

namespace Jibbr.Models
{
    public class JSONLogFile
    {
        #region Private Members
        private List<ChatMessage> chatMessages = null;
        private String fileName = String.Empty;
        private Boolean isInitialized = false;
        #endregion

        #region Constructor
        public JSONLogFile(){/*lol*/}
        #endregion

        #region Methods
        /// <summary>
        /// Open the file, verify the formatting, and load the messages
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool Initialize(String fileName)
        {
            isInitialized = false;
            if (String.IsNullOrEmpty(fileName))
                return isInitialized;

            this.fileName = fileName;

            String fileDirectory = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(fileDirectory))
                Directory.CreateDirectory(fileDirectory);

            if (!File.Exists(fileName))
            {
                FileStream newFile = File.Create(fileName);
                if (newFile != null)
                    newFile.Close();
            }

            try
            {
                using (StreamReader reader = new StreamReader(fileName))
                {
                    String logText = reader.ReadToEnd();

                    if(String.IsNullOrEmpty(logText))
                        logText = "[]";
                    else
                    {
                        Int32 openingBracketIndex = logText.IndexOf('[');
                        Int32 closingBracketIndex = logText.LastIndexOf(']');
                        if (openingBracketIndex != -1)
                        {
                            if (openingBracketIndex != 0)
                            {
                                String textBeforeOpeningBracket = logText.Substring(0, openingBracketIndex);
                                if (!String.IsNullOrWhiteSpace(textBeforeOpeningBracket))
                                {
                                    //What do we do here?  Insert one at the very beginning, or erase the stuff in front of the opening brace?
                                }
                            }
                        }
                        else
                        {
                            logText = (closingBracketIndex != -1) ? logText = "[" + logText : "[" + logText + "]";
                        }
                    }

                    chatMessages = JsonConvert.DeserializeObject<List<ChatMessage>>(logText, new ChatMessageToJsonConverter());
                    isInitialized = true;
                }
            }
            catch (Exception ex)
            {
                isInitialized = false;
            }

            return isInitialized;
        }
        /// <summary>
        /// Add the given chat message to the current log file
        /// </summary>
        /// <param name="chatMessage"></param>
        public bool AddMessageToLog(ChatMessage chatMessage)
        {
            if (!isInitialized)
                return false;

            bool rtn = true;
            try
            {
                using (FileStream stream = new FileStream(fileName, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        chatMessages.Add(chatMessage);
                        String logText = JsonConvert.SerializeObject(chatMessages, new ChatMessageToJsonConverter());
                        writer.Write(logText);
                        writer.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                //WAT DO
                rtn = false;
            }

            return rtn;
        }
        #endregion

        #region Properties
        #endregion
    }
}
