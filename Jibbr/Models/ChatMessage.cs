using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using agsXMPP;
using agsXMPP.protocol.client;

using Newtonsoft.Json;

namespace Jibbr.Models
{
    //This class represents a single chat message.
    public class ChatMessage
    {
        public ChatMessage()
        {
            Date = DateTime.Now;
            MessageType = agsXMPP.protocol.client.MessageType.normal;
        }
        public DateTime Date { get; set; }
        public String Message { get; set; }
        public Jid To { get; set; }
        public Jid From { get; set; }
        public MessageType MessageType { get; set; }
    }

    public class ChatMessageToJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ChatMessage);
        }

        public override bool CanRead { get { return false; } }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            ChatMessage chatMessage = value as ChatMessage;
            if (chatMessage == null)
                return;

            var properties = chatMessage.GetType().GetProperties();
            writer.WriteStartObject();
            foreach (PropertyInfo propertyInfo in properties)
            {
                writer.WritePropertyName(propertyInfo.Name);
                writer.WriteValue(propertyInfo.GetValue(chatMessage).ToString());
                //if (propertyInfo.PropertyType == typeof(Jid))
                //{
                //    Jid jid = propertyInfo.GetValue(chatMessage) as Jid;
                //    if (jid != null)
                //        writer.WriteValue(jid.User);
                //    else
                //        writer.WriteUndefined();
                //}
                //else
                //{
                //    writer.WriteValue(propertyInfo.GetValue(chatMessage).ToString());
                //}
            }
            writer.WriteEndObject();
        }
    }
}
