using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jibbr.ViewModels;

namespace Jibbr.Events
{
    public class EditAccountEvent
    {
        public AccountViewModel Account { get; set; }
    }
}
