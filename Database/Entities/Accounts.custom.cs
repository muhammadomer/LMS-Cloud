using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public partial class Accounts
    {
        public string EncryptedAccountId { get; set; }
        public string Name { get; set; }
        public int UserLimit { get; set; }
        public int ID { get; set; }
        public int UserAvailable { get; set; }
        public List<int> CourseIDs { get; set; }
        public UserAddModel UserAdd { get; set; }
    }
    public class UserAddModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string ConfirmPassword { get; set; }
        public string Password { get; set; }
    }
}
