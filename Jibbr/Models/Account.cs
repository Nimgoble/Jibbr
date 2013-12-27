using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Jibbr.Models
{
    public class Account
    {
        public Account()
        {
            //Init to String.Empty
            Username = Password = ServerName = String.Empty;
            //Init to false
            UseTSL = UseSSL = UseThisAccount = false;
        }

        [XmlAttribute]
        public String Username { get; set; }
        [XmlAttribute]
        public String Password { get; set; }
        [XmlAttribute]
        public String ServerName { get; set; }
        [XmlAttribute]
        public Boolean UseTSL { get; set; }
        [XmlAttribute]
        public Boolean UseSSL { get; set; }
        [XmlAttribute]
        public Boolean UseThisAccount { get; set; }
    }
}
