using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public partial class Users
    {
        public string EncryptedUserId { get; set; }
        public bool UserAuthenticated { get; set; }
        public bool LinkExpired { get; set; }
        public string UserFeedback { get; set; }
    }
}
