using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
namespace Jibbr.ViewModels
{
    class AccountViewModel : Screen
    {
        public AccountViewModel()
        {
        }

        #region Properties

        private String username;
        public String UserName
        {
            get { return username; }
            set
            {
                if (value == username)
                    return;

                username = value;
                NotifyOfPropertyChange(() => UserName);
            }
        }

        private String password;
        public String Password
        {
            get { return password; }
            set
            {
                if (value == password)
                    return;

                password = value;
                NotifyOfPropertyChange(() => Password);
            }
        }

        public ICommand SignIn { get; protected set; }

        #endregion
    }
}
