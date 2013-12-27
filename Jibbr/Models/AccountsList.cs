using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
namespace Jibbr.Models
{
    [XmlRoot("Accounts")]
    public class AccountsList
    {
        public AccountsList()
        {
            Accounts = new List<Account>();
        }

        [XmlElement("Account")]
        public List<Account> Accounts { get; set; }
    }
}
