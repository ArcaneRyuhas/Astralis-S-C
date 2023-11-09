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
            if (FindUserByNickname(nickname))
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

                string cleanedPassword = user.Password.Trim();

                var newSession = context.UserSession.Add(new UserSession() { password = cleanedPassword });

                Database.User databaseUser = new Database.User();
                databaseUser.nickName = user.Nickname;
                databaseUser.mail = user.Mail;
                databaseUser.imageId = (short)user.ImageId;
                databaseUser.userSessionFk = newSession.userSessionId;
                databaseUser.UserSession = newSession;

                var newUser = context.User.Add(databaseUser);

                result = context.SaveChanges();

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

                if (databaseUser != null)
                {
                    foundUser.Nickname = databaseUser.nickName;
                    foundUser.Mail = databaseUser.mail;
                    foundUser.ImageId = databaseUser.imageId;
                }
                else
                {
                    foundUser.Nickname = "UserNotFound";
                }

            }

            return foundUser;
        }

        public int UpdateUser(Contracts.User user)
        {
            int result = 0;

            using (var context = new AstralisDBEntities())
            {
                context.Database.Log = Console.WriteLine;
                var databaseUser = context.User.Find(user.Nickname);

                if (databaseUser != null)
                {
                    databaseUser.mail = user.Mail;
                    databaseUser.imageId = (short)user.ImageId;

                    var databaseUserSession = context.UserSession.Find(databaseUser.userSessionFk);

                    if (databaseUserSession != null)
                    {
                        if (!string.IsNullOrEmpty(user.Password))
                        {
                            databaseUserSession.password = user.Password;
                        }
                    }

                    result = context.SaveChanges();
                }
            }

            return result;

        }
    }

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public partial class UserManager : ILobbyManager
    {
        private const string ERROR_CODE_LOBBY = "error";

        private static Dictionary<string, string> usersInLobby = new Dictionary<string, string>();
        private static Dictionary<string, ILobbyManagerCallback> usersContext = new Dictionary<string, ILobbyManagerCallback>();

        public void ConnectLobby(Contracts.User user, string gameId)
        {
            if (usersInLobby.ContainsValue(gameId))
            {
                List<string> usersNickname = FindKeysByValue(usersInLobby, gameId);
                List<Contracts.User> users = new List<Contracts.User>();


                foreach (string nickname in usersNickname)
                {
                    users.Add(GetUserByNickname(nickname));
                }

                ILobbyManagerCallback currentUserCallbackChannel = OperationContext.Current.GetCallbackChannel<ILobbyManagerCallback>();
                usersContext.Add(user.Nickname, currentUserCallbackChannel);
                currentUserCallbackChannel.ShowUsersInLobby(users);
                usersInLobby.Add(user.Nickname, gameId);

                foreach (string userInTheLobby in usersNickname)
                {
                    usersContext[userInTheLobby].ShowConnectionInLobby(user);
                }

            }
        }

        public string CreateLobby(Contracts.User user)
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

            if (result > 0)
            {
                ILobbyManagerCallback currentUserCallbackChannel = OperationContext.Current.GetCallbackChannel<ILobbyManagerCallback>();
                usersContext.Add(user.Nickname, currentUserCallbackChannel);
                usersInLobby.Add(user.Nickname, gameId);
            }
            else
            {
                gameId = ERROR_CODE_LOBBY;
            }

            return gameId;
        }

        public void DisconnectLobby(Contracts.User user)
        {
            if (usersInLobby.ContainsKey(user.Nickname))
            {
                string gameId = usersInLobby[user.Nickname];

                List<string> usersNickname = FindKeysByValue(usersInLobby, gameId);

                foreach (var userInTheLobby in usersNickname)
                {
                    if (userInTheLobby != user.Nickname)
                    {
                        usersContext[userInTheLobby].ShowDisconnectionInLobby(user);
                    }
                }
                usersInLobby.Remove(user.Nickname);
                usersContext.Remove(user.Nickname);
            }
        }

        public void ChangeLobbyUserTeam(Contracts.User user, int team)
        {
            throw new NotImplementedException();
        }

        public void SendMessage(string message, string gameId)
        {
            if (usersInLobby.ContainsValue(gameId))
            {
                List<string> usersNickname = FindKeysByValue(usersInLobby, gameId);

                foreach (string userInTheLobby in usersNickname)
                {
                    usersContext[userInTheLobby].ReceiveMessage(message);
                }
            }
        }

        private string generateGameId()
        {
            Guid guid = Guid.NewGuid();

            string base64Guid = Convert.ToBase64String(guid.ToByteArray());
            string uniqueID = base64Guid.Replace("=", "").Substring(0, 6);

            return uniqueID;
        }

        private bool gameIdIsRepeated(string gameId)
        {
            bool isRepeated = false;

            using (var context = new AstralisDBEntities())
            {
                context.Database.Log = Console.WriteLine;

                var databaseGameId = context.User.Find(gameId);

                if (databaseGameId != null)
                {
                    isRepeated = true;
                }
            }

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

    public partial class UserManager : IOnlineUserManager
    {
        private static Dictionary<string, IOnlineUserManagerCallback> onlineUsers = new Dictionary<string, IOnlineUserManagerCallback>();
        private const int IS_FRIEND = 1;
        private const int IS_PENDING_FRIEND = 2;

        public void ConectUser(string nickname)
        {
            if (!onlineUsers.ContainsKey(nickname))
            {
                IOnlineUserManagerCallback currentUserCallbackChannel = OperationContext.Current.GetCallbackChannel<IOnlineUserManagerCallback>();
                List<string> onlineNicknames = onlineUsers.Keys.ToList();
                currentUserCallbackChannel.ShowOnlineFriends(GetFriendList(nickname));

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

        public bool SendFriendRequest(string nicknameSender, string nicknameReciever) //Se puede cambiar el retorno a un int para saber 0.- NO EXISITE EL USUARIO 1.- Exitoso 2.-Ya existe la solicitud o son amigos
        {
            bool IsSucceded = false;

            if (FindUserByNickname(nicknameSender) && FindUserByNickname(nicknameReciever))
            {
                using (var context = new AstralisDBEntities())
                {
                    context.Database.Log = Console.WriteLine;

                    var existingRequest = context.UserFriend
                        .FirstOrDefault(f =>
                            (f.Nickname1 == nicknameSender && f.Nickname2 == nicknameReciever) ||
                            (f.Nickname1 == nicknameReciever && f.Nickname2 == nicknameSender) &&
                            f.FriendStatusId == IS_PENDING_FRIEND);

                    if (existingRequest == null)
                    {
                        var newFriendRequest = new UserFriend
                        {
                            Nickname1 = nicknameSender,
                            Nickname2 = nicknameReciever,
                            FriendStatusId = IS_PENDING_FRIEND
                        };

                        context.UserFriend.Add(newFriendRequest);
                        context.SaveChanges();

                        if (onlineUsers.ContainsKey(nicknameReciever))
                        {
                            onlineUsers[nicknameReciever].ShowFriendRequest(nicknameSender);
                        }

                        IsSucceded = true;
                    }
                }
            }

            return IsSucceded;
        }

        public bool ReplyFriendRequest(string nicknameReciever, string nicknameSender, bool answer)
        {
            bool IsSucceded = false;

            if (FindUserByNickname(nicknameReciever) && FindUserByNickname(nicknameSender))
            {
                using (var context = new AstralisDBEntities())
                {
                    context.Database.Log = Console.WriteLine;

                    var existingRequest = context.UserFriend
                        .FirstOrDefault(f =>
                            (f.Nickname1 == nicknameReciever && f.Nickname2 == nicknameSender) ||
                            (f.Nickname1 == nicknameSender && f.Nickname2 == nicknameReciever) &&
                            f.FriendStatusId == IS_PENDING_FRIEND);

                    if (existingRequest != null)
                    {
                        if (answer)
                        {
                            existingRequest.FriendStatusId = IS_FRIEND;
                        }
                        else
                        {
                            context.UserFriend.Remove(existingRequest);
                        }

                        int result = context.SaveChanges();
                        
                        if(result > 0)
                        {
                            IsSucceded = true;

                            if (answer)
                            {
                                if (onlineUsers.ContainsKey(nicknameReciever))
                                {
                                    onlineUsers[nicknameReciever].ShowFriendAccepted(nicknameSender);
                                }
                            }
                        }
                    }
                }
            }
            return IsSucceded;
        }

        public bool RemoveFriend(string nickname, string nicknamefriendToRemove)
        {
            bool IsSucceded = false;

            using (var context = new AstralisDBEntities())
            {
                context.Database.Log = Console.WriteLine;

                var friendRelationship = context.UserFriend
                    .FirstOrDefault(f =>
                        (f.Nickname1 == nickname && f.Nickname2 == nicknamefriendToRemove) ||
                        (f.Nickname1 == nicknamefriendToRemove && f.Nickname2 == nickname) &&
                        f.FriendStatusId == IS_FRIEND);

                if (friendRelationship != null)
                {
                    context.UserFriend.Remove(friendRelationship);
                    context.SaveChanges();
                    IsSucceded = true;
                }
            }

            return IsSucceded;
        }

        private Dictionary<string, bool> GetFriendList(string nickname)
        {
            Dictionary<string, bool> friendList = new Dictionary<string, bool>();

            using (var context = new AstralisDBEntities())
            {
                context.Database.Log = Console.WriteLine;
                var databaseFriends = context.UserFriend.Where(databaseFriend => (databaseFriend.Nickname1 == nickname || databaseFriend.Nickname2 == nickname) && databaseFriend.FriendStatusId == IS_FRIEND).ToList();

                foreach (var friend in databaseFriends)
                {
                    if (friend.Nickname1 != nickname)
                    {
                        if (onlineUsers.ContainsKey(friend.Nickname1))
                        {
                            friendList.Add(friend.Nickname1, true);
                        }
                        else
                        {
                            friendList.Add(friend.Nickname1, false);
                        }

                    }
                    else
                    {
                        if (onlineUsers.ContainsKey(friend.Nickname2))
                        {
                            friendList.Add(friend.Nickname2, true);
                        }
                        else
                        {
                            friendList.Add(friend.Nickname2, false);
                        }
                    }

                     var pendingRequests = context.UserFriend.Where(f => (f.Nickname2 == nickname) && f.FriendStatusId == IS_PENDING_FRIEND).ToList();

                    foreach (var request in pendingRequests)
                    {
                        if (!friendList.ContainsKey(request.Nickname1))
                        {
                            friendList.Add(request.Nickname1, false);
                        }
                    }
                }
            }
            return friendList;
        }
    }
}