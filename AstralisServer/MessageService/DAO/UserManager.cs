using MessageService.Contracts;
using MessageService.Database;
using System;
using System.Collections;
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

        public int AddUser(Contracts.User user)
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

        public Contracts.User GetUserByNickname(string nickname)
        {
            Contracts.User foundUser = new Contracts.User();
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

        private static Dictionary<string, string> usersInLobby = new Dictionary<string, string>();
        private static Dictionary<string, ILobbyManagerCallback> usersContext = new Dictionary<string, ILobbyManagerCallback>();

        public void ConnectLobby(Contracts.User user, string gameId)
        {
            if (usersInLobby.ContainsValue(gameId))
            {
                List<string> users = FindKeysByValue(usersInLobby, gameId);

                ILobbyManagerCallback currentUserCallbackChannel = OperationContext.Current.GetCallbackChannel<ILobbyManagerCallback>();
                usersContext.Add(user.Nickname, currentUserCallbackChannel);
                currentUserCallbackChannel.ShowUsersInLobby(users);
                usersInLobby.Add(user.Nickname, gameId);

                foreach (string userInLobby in users)
                {
                    usersContext[userInLobby].ShowConnectionInLobby(user.Nickname);
                }

            }
        }

        public int CreateLobby(Contracts.   User user)
        {
            int result = 0;
            string gameId = generateGameId();

            while (gameIdIsRepeated(gameId))
            {
                gameId = generateGameId();
            }

            using (var context = new AstralisDBEntities())
            {
                context.Database.Log = Console.WriteLine;

                var newSession = context.Game.Add(new Game() { gameId = gameId });

                result = context.SaveChanges();
            };

            if(result > 0)
            {
                ILobbyManagerCallback currentUserCallbackChannel = OperationContext.Current.GetCallbackChannel<ILobbyManagerCallback>();
                usersContext.Add(user.Nickname, currentUserCallbackChannel);
                usersInLobby.Add(user.Nickname, gameId);
            }

            return result;
        }

        public void DisconnectLobby(Contracts.User user)
        {
            throw new NotImplementedException();
        }

        public void ChangeLobbyUserTeam(Contracts.User user, int team)
        {
            throw new NotImplementedException();
        }

        private string generateGameId() 
        {
            Guid guid = Guid.NewGuid();

            string base64Guid = Convert.ToBase64String(guid.ToByteArray());
            string uniqueID = base64Guid.Replace("=", "").Substring(0, 6);

            return uniqueID;
        }

        private bool gameIdIsRepeated (string gameId)
        {
            bool isRepeated = false;

            //TODO

            return isRepeated;
        }

        private List<string> FindKeysByValue(Dictionary<string, string> dictionary, string value)
        {
            return dictionary
                .Where(pair => pair.Value == value)
                .Select(pair => pair.Key)
                .ToList();
        }

    }

    public partial class UserManager: IOnlineUserManager
    {
        private static Dictionary<string, IOnlineUserManagerCallback> onlineUsers = new Dictionary<string, IOnlineUserManagerCallback>();
        
        public void ConectUser(string nickname)
        {
            if(!onlineUsers.ContainsKey(nickname))
            {
                IOnlineUserManagerCallback currentUserCallbackChannel = OperationContext.Current.GetCallbackChannel<IOnlineUserManagerCallback>();
                List<string> onlineNicknames = onlineUsers.Keys.ToList();
                currentUserCallbackChannel.ShowUsersOnline(onlineNicknames);

                foreach (var user in onlineUsers)
                {
                    user.Value.ShowUserConected(nickname);
                }

                onlineUsers.Add(nickname, currentUserCallbackChannel);
                
            }
        }
        public void DisconectUser(string nickname)
        {
            if (onlineUsers.ContainsKey(nickname))
            {
                onlineUsers.Remove(nickname);
                foreach (var user in onlineUsers)
                {
                    user.Value.ShowUserDisconected(nickname);
                }
            }
            
        }
      
    }
}
