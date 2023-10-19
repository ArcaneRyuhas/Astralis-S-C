using MessageService.Contracts;
using MessageService.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MessageService
{
    public partial class UserManager : IUserManager

    {
        public int ConfirmUser(string nickname, string password)
        {
            int result = 0;
            if(FindUserByNickname(nickname))
            {
                using (var context = new AstralisDBEntities())
                {
                    context.Database.Log = Console.WriteLine;

                    var databaseUser = context.User.Find(nickname);

                    var databaseUsersession = context.UserSession.Find(databaseUser.userSessionFk);

                    if (databaseUsersession != null && databaseUsersession.password == password)
                    {
                        result = 1;
                    }
                    else
                    {
                        result = 0;
                    }
                }
            }
            
            return result;
        }

        public int AddUser(User user)
        {

            int result = 0;

            using (var context = new AstralisDBEntities()) 
            {
                context.Database.Log = Console.WriteLine;

                var newSession = context.UserSession.Add(new UserSession() { password = user.Password });

                Database.User databaseUser = new Database.User();
                databaseUser.nickName = user.Nickname;
                databaseUser.mail = user.Mail;
                databaseUser.imageId = (short)user.ImageId;
                databaseUser.userSessionFk = newSession.userSessionId;
                databaseUser.UserSession = newSession;

                var newUser = context.User.Add(databaseUser);

                result = context.SaveChanges();
                
                Console.WriteLine(result);
            };

            return result;
        }

        public bool FindUserByNickname(String nickname)
        {
            bool isFound = false;

            using (var context = new AstralisDBEntities())
            {
                context.Database.Log = Console.WriteLine;

                var databaseUser = context.User.Find(nickname);

                if (databaseUser != null)
                {
                    isFound = true;
                }
            }

            return isFound;
        }

        public User GetUserByNickname(string nickname)
        {
            User foundUser = new User();
            using (var context = new AstralisDBEntities())
            {
                context.Database.Log = Console.WriteLine;
                var databaseUser = context.User.Find(nickname);

                if(databaseUser != null)
                {
                    foundUser.Nickname = databaseUser.nickName;
                    foundUser.Mail = databaseUser.mail;
                    foundUser.ImageId = databaseUser.imageId;
                }
                else
                {
                    foundUser.Nickname = "NotFound";
                }
                
            }

            return foundUser;
        }
    }

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public partial class UserManager : ILobbyManager
    {
        public void CreateLobby(Contracts.User user)
        {
            throw new NotImplementedException();
        }

        public void JoinLobby(Contracts.User user)
        {
            OperationContext.Current.GetCallbackChannel<ILobbyManagerCallback>().UpdateLobby(user);
        }

        public void ChangeLobbyUserTeam(Contracts.User user, int team)
        {
            throw new NotImplementedException();
        }
    }
}
