using Astralis.UserManager;

namespace Astralis.Logic
{
    public class UserSession
    {
        private static UserSession _instance;
        private readonly string _nickname = "";
        private readonly string _mail = "";
        private readonly int _imageId = 1;

        private UserSession(User user)
        {
            _nickname = user.Nickname;
            _mail = user.Mail;
            _imageId = user.ImageId;
        }

        public static UserSession Instance (User user)
        {
            if (_instance == null)
            {
                _instance = new UserSession (user);
            }

            return _instance;
        }

        public static UserSession Instance()
        {
             return _instance;
        }

        public string Nickname { get { return _nickname; } }
        public string Mail { get { return _mail; } }
        public int ImageId { get {  return _imageId; } }
    }
}
