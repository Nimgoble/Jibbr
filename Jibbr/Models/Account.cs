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
            Username = Password = ServerName = ConnectServerName = String.Empty;
            //Init to false
            UseTLS = UseSSL = UseThisAccount = false;
            AutoResolveConnectServer = true;
        }

        [XmlAttribute]
        public String Username { get; set; }
        [XmlAttribute]
        public String Password { get; set; }
        [XmlAttribute]
        public String ServerName { get; set; }
        [XmlAttribute]
        public String ConnectServerName { get; set; }
        [XmlAttribute]
        public Boolean UseTLS { get; set; }
        [XmlAttribute]
        public Boolean UseSSL { get; set; }
        [XmlAttribute]
        public Boolean UseThisAccount { get; set; }
        [XmlAttribute]
        public Boolean AutoResolveConnectServer { get; set; }
    }
}
