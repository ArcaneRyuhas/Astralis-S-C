using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageService
{
    public class UserManager : IUserManager
    {
        public User GetUserByNickname(string nickname)
        {
            return new User
            {
                Nickname = "Camelia",
                ImageId = 1,
                Mail = "camelia@hotmail.com",
                Password = "Password"
            };
        }

        public int AddUser(User user)
        {
            Console.WriteLine("Se añadio el usuario...");
            return 1;
        }
    }
}
