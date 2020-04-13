using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1.Global
{
    [Serializable]
    public class OfficeUser
    {
        private string email;
        public string Email
        {
            get { return email; }
            set { email = value; }
        }

        private string password;
        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        private string lastdate;
        public string Lastdate
        {
            get { return lastdate; }
            set { lastdate = value; }
        }

    }
}
