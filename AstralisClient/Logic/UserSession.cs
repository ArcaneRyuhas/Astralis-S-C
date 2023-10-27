using Astralis.UserManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astralis.Logic
{
    public class UserSession
    {
        private static UserSession instance;
        private string nickname = "";
        private string mail = "";
        private int imageId = 1;

        private UserSession(User user)
        {
            nickname = user.Nickname;
            mail = user.Mail;
            imageId = user.ImageId;
        }

        public static UserSession Instance (User user)
        {
            if (instance == null)
            {
                instance = new UserSession (user);
            }

            return instance;
        }

        public static UserSession Instance()
        {
             return instance;
        }
        
        public string Nickname { get { return nickname; } }
        public string Mail { get { return mail; } }
        public int ImageId { get {  return imageId; } }
    }
}
