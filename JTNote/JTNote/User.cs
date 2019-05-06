using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTNote
{
    public class User
    {
        public User() { }
        public User(int id, string userName, string email, string password)
        {
            Id = id;
            UserName = userName;
            Email = email;
            Password = password;
        }

        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
