using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Jibbr.Models;

using agsXMPP;
using Caliburn.Micro;

namespace Jibbr.ViewModels
{
    public class JIDViewModel : Screen
    {
        #region Private Members
        private Jid jabbrID;
        #endregion

        #region Constructor
        public JIDViewModel(Jid model, AccountViewModel account)
        {
            jabbrID = model;
            this.account = account;
        }
        #endregion

        #region Properties
        private AccountViewModel account = null;
        public AccountViewModel Account { get { return account; } }

        private ObservableCollection<RosterGroupViewModel> groups = new ObservableCollection<RosterGroupViewModel>();
        public ObservableCollection<RosterGroupViewModel> Groups { get { return groups; } }

        public String Bare { get { return jabbrID.Bare; } }
        public String Resource { get { return jabbrID.Resource; } }
        public String Server { get { return jabbrID.Server; } }
        public String User { get { return jabbrID.User; } }
        #endregion

        public static implicit operator Jid(JIDViewModel vm) { return vm.jabbrID; }
    }
}
