using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using agsXMPP;
using agsXMPP.protocol;

namespace Jibbr.ViewModels
{
    public class RosterGroupViewModel
    {
        #region Constructor
        public RosterGroupViewModel(agsXMPP.protocol.Base.Group group, AccountViewModel account)
        {
            this.groupName = group.Name;
            this.account = account;
        }
        #endregion

        #region Properties
        private AccountViewModel account = null;
        public AccountViewModel Account { get { return account; } }

        private ObservableCollection<JIDViewModel> members = new ObservableCollection<JIDViewModel>();
        public ObservableCollection<JIDViewModel> Members { get { return members; } }

        private String groupName = String.Empty;
        public String GroupName { get { return groupName; } }
        #endregion

        #region Overridden
        public override bool Equals(object obj)
        {
            if(obj is RosterGroupViewModel)
            {
                RosterGroupViewModel other = obj as RosterGroupViewModel;
                return other.GroupName == this.GroupName;
            }

            return false;
        }
        public override int GetHashCode()
        {
            return this.groupName.GetHashCode();
        }
        public override string ToString()
        {
            return groupName;
        }
        #endregion
    }
}
