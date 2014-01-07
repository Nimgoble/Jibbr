using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jibbr.Models
{
    //This class represents a single chat message.
    public class ChatMessage
    {
        public DateTime Date { get; set; }
        public String Message { get; set; }
        public String To { get; set; }
        public String From { get; set; }
    }
}
