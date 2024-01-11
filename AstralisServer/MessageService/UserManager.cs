using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using DataAccessProject.Contracts;
using DataAccessProject.DataAccess;
using User = DataAccessProject.Contracts.User;
using System.Data.SqlClient;
using System.Data.Entity.Core;
using DataAccessProject;
using System.Runtime.CompilerServices;
using System.Data.Entity.Infrastructure;
using System.ServiceModel.Channels;

namespace MessageService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]

    public partial class UserManager : IUserManager
    {
        private const string NICKNAME_ERROR = "ERROR";
        private const int GUEST_IMAGE_ID = 1;
        private const int VALIDATION_FAILURE = 0;
        private const int ERROR = -1;
        private const int VALIDATION_SUCCESS = 1;

        private static readonly ILog log = LogManager.GetLogger(typeof(UserManager));

        public int ConfirmUser(string nickname, string password)
        {
            int result = VALIDATION_FAILURE;
            UserAccess userAccess = new UserAccess();

            try
            {
                if (userAccess.FindUserByNickname(nickname) == VALIDATION_SUCCESS)
                {
                    result = userAccess.ConfirmUser(nickname, password);
                }
                if (result > VALIDATION_FAILURE)
                {
                    result = VALIDATION_SUCCESS;
                }
            }
            catch (EntityException entityException)
            {
                log.Error(entityException);
                result = ERROR;
            }
            catch (SqlException sqlException)
            {
                log.Error(sqlException);
                result = ERROR;
            }
            return result;
        }

        public int AddUser(User user)
        {
            UserAccess userAccess = new UserAccess();
            int result = VALIDATION_FAILURE;

            try
            {
                if (FindUserByNickname(user.Nickname) != VALIDATION_SUCCESS)
                {
                    result = userAccess.CreateUser(user);

                    if (result > VALIDATION_FAILURE)
                    {
                        result = VALIDATION_SUCCESS;
                    }
                }
            }
            catch (SqlException sqlException)
            {
                log.Error(sqlException);
                result = ERROR;
            }
            catch (EntityException entityException)
            {
                log.Error(entityException);
                result = ERROR;
            }

            return result;
        }

        public User AddGuest()
        {
            User guestUser = new User();
            try
            {
                UserAccess userAccess = new UserAccess();
                int maxGuestNumber = userAccess.GetHigherGuests();
                int nextGuestNumber = maxGuestNumber + 1;

                string guestNickname = $"Guest{nextGuestNumber}";

                guestUser.Nickname = guestNickname;
                guestUser.Password = guestNickname;
                guestUser.ImageId = GUEST_IMAGE_ID;
                guestUser.Mail = $"{guestNickname.ToLower()}@guest.com";

                userAccess.CreateGuest(guestUser);
            }
            catch (SqlException sqlException)
            {
                log.Error(sqlException);
                guestUser.Nickname = NICKNAME_ERROR;
            }
            catch (EntityException entityException)
            {
                log.Error(entityException);
                guestUser.Nickname = NICKNAME_ERROR;
            }

            return guestUser;
        }

        public int FindUserByNickname(string nickname)
        {
            int isFound = VALIDATION_FAILURE;

            UserAccess userAccess = new UserAccess();
            try
            {
                isFound = userAccess.FindUserByNickname(nickname);

                if (isFound > VALIDATION_FAILURE)
                {
                    isFound = VALIDATION_SUCCESS;
                }
            }
            catch (EntityException entityException)
            {
                log.Error(entityException);
                isFound = ERROR;
            }
            catch (SqlException sqlException)
            {
                log.Error(sqlException);
                isFound = ERROR;
            }

            return isFound;
        }

        public User GetUserByNickname(string nickname)
        {
            User foundUser = new User();
            foundUser.Nickname = NICKNAME_ERROR;

            UserAccess userAccess = new UserAccess();
            try
            {
                foundUser = userAccess.GetUserByNickname(nickname);
            }
            catch (SqlException sqlException)
            {
                log.Error(sqlException);
                foundUser.Nickname = NICKNAME_ERROR;
            }
            catch (EntityException entityException)
            {
                log.Error(entityException);
                foundUser.Nickname = NICKNAME_ERROR;
            }

            return foundUser;
        }

        public int UpdateUser(User user)
        {
            int result = VALIDATION_FAILURE;

            UserAccess userAccess = new UserAccess();
            try
            {
                if (userAccess.FindUserByNickname(user.Nickname) == VALIDATION_SUCCESS)
                {
                    result = userAccess.UpdateUser(user);

                    if (result > VALIDATION_FAILURE)
                    {
                        result = VALIDATION_SUCCESS;
                    }
                }
            }
            catch (SqlException sqlException)
            {
                log.Error(sqlException);
                result = ERROR;
            }
            catch (EntityException entityException)
            {
                log.Error(entityException);
                result = ERROR;
            }

            return result;

        }

        public bool UserOnline(string nickname)
        {
            lock (_onlineUsers)
            {
                return _onlineUsers.ContainsKey(nickname);
            }
        }
    }

    public partial class UserManager : ILobbyManager
    {
        private const bool IS_SUCCESFULL = true;
        private const int NO_TEAM = 0;
        private const string USER_NOT_FOUND = "UserNotFound";
        private const string ERROR_STRING = "ERROR";
        private const string VALIDATION_FAILURE_STRING = "FAILURE";

        private static Dictionary<string, string> _usersInLobby = new Dictionary<string, string>();
        private static Dictionary<string, ILobbyManagerCallback> _usersContext = new Dictionary<string, ILobbyManagerCallback>();
        private static Dictionary<string, int> _usersTeam = new Dictionary<string, int>();

        public bool GameExist(string gameId)
        {
            lock(_usersInLobby)
            {
                return _usersInLobby.ContainsValue(gameId);
            }
        }

        public bool GameIsNotFull(string gameId)
        {
            int usersInGame = 0;
            bool gameIsNotFull = true;

            var groupedKeys = _usersInLobby.Where(pair => pair.Value == gameId).Select(pair => pair.Key);
            usersInGame = groupedKeys.Count();
            
            if(usersInGame > 3)
            {
                gameIsNotFull = false;
            }

            return gameIsNotFull;
        }

        public int CanPlay(string nickname)
        {
            int result = VALIDATION_FAILURE;

            GameAccess gameAccess = new GameAccess();
            try
            {
                result = gameAccess.CanPlay(nickname);
               
                if (result > VALIDATION_FAILURE) 
                {
                    result = VALIDATION_SUCCESS;
                }

            }
            catch (EntityException entityException)
            {
                log.Error(entityException);
                result = ERROR;
            }

            return result;
        }

        public void ConnectLobby(User user, string gameId)
        {
            lock (_usersTeam)
            {
                if (_usersInLobby.ContainsValue(gameId))
                {
                    List<string> usersNickname = FindKeysByValue(_usersInLobby, gameId);
                    List<Tuple<User, int>> users = new List<Tuple<User, int>>();

                    foreach (string nickname in usersNickname)
                    {
                        users.Add(new Tuple<User, int>(GetUserByNickname(nickname), _usersTeam[nickname]));
                    }

                    ILobbyManagerCallback currentUserCallbackChannel = OperationContext.Current.GetCallbackChannel<ILobbyManagerCallback>();

                    try
                    {
                        currentUserCallbackChannel.ShowUsersInLobby(users);

                        if (!_usersContext.ContainsKey(user.Nickname))
                        {
                            _usersContext.Add(user.Nickname, currentUserCallbackChannel);
                            _usersInLobby.Add(user.Nickname, gameId);
                            _usersTeam.Add(user.Nickname, NO_TEAM);
                        }
                    }
                    catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                    {
                        log.Error(communicationObjectAbortedException);
                    }
                    catch (CommunicationException communicationException)
                    {
                        log.Error(communicationException);
                    }
                    catch (TimeoutException timeoutException)
                    {
                        log.Error(timeoutException);
                    }

                    ShowConnectionInLobby(usersNickname, user);
                }
            }
        }

        private void ShowConnectionInLobby(List<string> usersNickname, User user)
        {
            foreach (string userInTheLobby in usersNickname)
            {
                try
                {
                    _usersContext[userInTheLobby].ShowConnectionInLobby(user);
                }
                catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                {
                    log.Error(communicationObjectAbortedException);
                    HandleLobbyException(userInTheLobby);
                }
                catch (CommunicationException communicationException)
                {
                    log.Error(communicationException);
                    HandleLobbyException(userInTheLobby);
                }
                catch (TimeoutException timeoutException)
                {
                    log.Error(timeoutException);
                    HandleLobbyException(userInTheLobby);
                }
            }
        }

        private void HandleLobbyException(string userInTheLobby)
        {
            User userToDisconnect = new User();
            userToDisconnect.Nickname = userInTheLobby;
            DisconnectLobby(userToDisconnect);
        }

        public string CreateLobby(User user)
        {
            string gameId = GenerateGameId();
            GameAccess gameAccess = new GameAccess();
            try
            {
                while (gameAccess.GameIdIsRepeated(gameId))
                {
                    gameId = GenerateGameId();
                }
            
                if (gameAccess.CreateGame(gameId) == IS_SUCCESFULL)
                {
                    ILobbyManagerCallback currentUserCallbackChannel = OperationContext.Current.GetCallbackChannel<ILobbyManagerCallback>();
                    _usersContext.Add(user.Nickname, currentUserCallbackChannel);
                    _usersInLobby.Add(user.Nickname, gameId);
                    _usersTeam.Add(user.Nickname, NO_TEAM);
                }
                else
                {
                    gameId = VALIDATION_FAILURE_STRING;
                }
            }
            catch(SqlException sqlException)
            {
                log.Error(sqlException);
                gameId = ERROR_STRING;
            }
            catch(EntityException entityException)
            {
                log.Error(entityException);
                gameId = ERROR_STRING;
            }
            return gameId;
        }

        public string SendFriendInvitation(string gameId, string userToSend)
        {
            UserAccess userAccess = new UserAccess();
            User user = new User();
            string mailString;

            try
            {
                user = userAccess.GetUserByNickname(userToSend);

                if (user.Nickname != USER_NOT_FOUND)
                {
                    mailString = Mail.Mail.Instance().SendInvitationMail(user.Mail, gameId);
                }
                else
                {
                    mailString = Mail.Mail.Instance().SendInvitationMail(userToSend, gameId);
                }
            }
            catch(SqlException sqlException)
            {
                log.Error(sqlException);
                mailString = ERROR_STRING;
            }
            catch(EntityException entityException)
            {
                log.Error(entityException);
                mailString = ERROR_STRING;
            }

            return mailString;
        }

        public void DisconnectLobby(User user)
        {
            if (_usersInLobby.ContainsKey(user.Nickname))
            {
                string gameId = _usersInLobby[user.Nickname];
                List<string> usersNickname = FindKeysByValue(_usersInLobby, gameId);

                _usersInLobby.Remove(user.Nickname);
                _usersContext.Remove(user.Nickname);
                _usersTeam.Remove(user.Nickname);

                foreach (string userInTheLobby in usersNickname)
                {
                    ShowDisconnection(userInTheLobby, user);
                }
            }
        }

        private void ShowDisconnection(string userInTheLobby, User user)
        {
            try
            {
                if (_usersContext.ContainsKey(userInTheLobby))
                {
                    _usersContext[userInTheLobby].ShowDisconnectionInLobby(user);
                }
            }
            catch (CommunicationObjectAbortedException communicationObjectAbortedException)
            {
                log.Error(communicationObjectAbortedException);
                HandleLobbyException(userInTheLobby);
            }
            catch (CommunicationException communicationException)
            {
                log.Error(communicationException);
                HandleLobbyException(userInTheLobby);
            }
            catch (TimeoutException timeoutException)
            {
                log.Error(timeoutException);
                HandleLobbyException(userInTheLobby);
            }
        }

        public void ChangeLobbyUserTeam(string userNickname, int team)
        {
            if (_usersTeam.ContainsKey(userNickname))
            {
                _usersTeam[userNickname] = team;
                string gameId = _usersInLobby[userNickname];
                List<string> usersNickname = FindKeysByValue(_usersInLobby, gameId);

                foreach (var userInTheLobby in usersNickname)
                {
                    UpdateTeamToUsers(userNickname, team, userInTheLobby);
                }
            }
        }

        private void UpdateTeamToUsers(string userNickname, int team, string userInTheLobby)
        {

            if (userInTheLobby != userNickname)
            {
                try
                {
                    _usersContext[userInTheLobby].UpdateLobbyUserTeam(userNickname, team);
                }
                catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                {
                    log.Error(communicationObjectAbortedException);
                    HandleLobbyException(userInTheLobby);
                }
                catch (CommunicationException communicationException)
                {
                    log.Error(communicationException);
                    HandleLobbyException(userInTheLobby);
                }
                catch (TimeoutException timeoutException)
                {
                    log.Error(timeoutException);
                    HandleLobbyException(userInTheLobby);
                }
            }
        }

        public void StartGame(string gameId)
        {

            List<string> usersNickname = FindKeysByValue(_usersInLobby, gameId);
            int result = VALIDATION_FAILURE;

            if (_usersInLobby.ContainsValue(gameId))
            {
                GameAccess gameAccess = new GameAccess();

                foreach (var user in usersNickname)
                {
                    try
                    {
                        result += gameAccess.CreatePlaysRelation(user, gameId, _usersTeam[user]);
                    }
                    catch(SqlException sqlException)
                    {
                        log.Error(sqlException);
                    }
                    catch (EntityException entityException)
                    {
                        log.Error(entityException);
                    }
                }
                GameValidationResult(result, usersNickname);
            }
        }

        private void GameValidationResult(int result, List<string> usersNickname)
        {

            if (result > VALIDATION_FAILURE)
            {
                foreach (string userInTheLobby in usersNickname)
                {
                    try
                    {
                        _usersContext[userInTheLobby].StartClientGame();
                        _usersContext.Remove(userInTheLobby);
                    }
                    catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                    {
                        log.Error(communicationObjectAbortedException);
                        HandleLobbyException(userInTheLobby);
                    }
                    catch (CommunicationException communicationException)
                    {
                        log.Error(communicationException);
                        HandleLobbyException(userInTheLobby);
                    }
                    catch (TimeoutException timeoutException)
                    {
                        log.Error(timeoutException);
                        HandleLobbyException(userInTheLobby);
                    }
                }
            }
        }


        public void SendMessage(string message, string gameId)
        {
            int indexToCut = message.IndexOf(':');
            string nickname = message.Substring(0, indexToCut);

            if (_usersInLobby.ContainsValue(gameId))
            {
                List<string> usersNickname = FindKeysByValue(_usersInLobby, gameId);

                if (ContainsBadWords(message))
                {
                    GameAccess gameAccess = new GameAccess();
                    gameAccess.BanUser(nickname);
                    KickUser(nickname);
                }
                else
                {
                    SendMessageToUsers(message, usersNickname);
                }
            }
        }

        private void SendMessageToUsers(string message, List<string> usersNickname)
        {
            foreach (string userInTheLobby in usersNickname)
            {
                try
                {
                    _usersContext[userInTheLobby].ReceiveMessage(message);
                }
                catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                {
                    log.Error(communicationObjectAbortedException);
                    HandleLobbyException(userInTheLobby);
                }
                catch (CommunicationException communicationException)
                {
                    log.Error(communicationException);
                    HandleLobbyException(userInTheLobby);
                }
                catch (TimeoutException timeoutException)
                {
                    log.Error(timeoutException);
                    HandleLobbyException(userInTheLobby);
                }
            }
        }

        static bool ContainsBadWords(string text)
        {
            List<string> badwords = new List<string> { "fucker","fuck" ,"putito", "puto", "marica", "jodido", "revo", "negro"};

            text = text.ToLower();

            return badwords.Exists(groseria => text.Contains(groseria.ToLower()));
        }


        private string GenerateGameId()
        {
            Guid guid = Guid.NewGuid();

            string base64Guid = Convert.ToBase64String(guid.ToByteArray());
            string uniqueID = base64Guid.Replace("=", "").Substring(0, 6);

            return uniqueID;
        }

        private List<string> FindKeysByValue(Dictionary<string, string> dictionary, string value)
        {
            return dictionary
                .Where(pair => pair.Value == value)
                .Select(pair => pair.Key)
                .ToList();
        }

        public void KickUser(string userNickname)
        {
            if (_usersInLobby.ContainsKey(userNickname) && _usersContext.ContainsKey(userNickname))
            {
                try
                {
                    _usersContext[userNickname].GetKicked();
                }
                catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                {
                    log.Error(communicationObjectAbortedException);
                }
                catch (CommunicationException communicationException)
                {
                    log.Error(communicationException);
                }
                catch (TimeoutException timeoutException)
                {
                    log.Error(timeoutException);
                }
                finally
                {
                    User userToDisconnect = new User();
                    userToDisconnect.Nickname = userNickname;
                    DisconnectLobby(userToDisconnect);
                }
            }
        }
    }

    public partial class UserManager : IOnlineUserManager
    {
        private static Dictionary<string, IOnlineUserManagerCallback> _onlineUsers = new Dictionary<string, IOnlineUserManagerCallback>();
        private const int IS_SUCCEDED = 0;
        private const bool ACCEPTED_FRIEND = true;
        private static Object _lock = new Object();

        [OperationBehavior]
        public void ConectUser(string nickname)
        {
            lock (_onlineUsers)
            {
                IOnlineUserManagerCallback currentUserCallbackChannel = OperationContext.Current.GetCallbackChannel<IOnlineUserManagerCallback>();
                FriendAccess friendAccess = new FriendAccess();

                try
                {
                    currentUserCallbackChannel.ShowOnlineFriends(friendAccess.GetFriendList(nickname, _onlineUsers.Keys.ToList()));
                    if (!_onlineUsers.ContainsKey(nickname))
                    {
                        _onlineUsers.Add(nickname, currentUserCallbackChannel);
                        ShowUserConnected(nickname);
                    }
                    else
                    {
                        _onlineUsers[nickname] = currentUserCallbackChannel;
                    }
                }
                catch (EntityException entityException)
                {
                    log.Error(entityException);
                }
                catch (InvalidOperationException invalidOperationException)
                {
                    log.Error(invalidOperationException);
                }
            }
        }

        private void ShowUserConnected(string nickname)
        {
            foreach (var user in _onlineUsers)
            {
                try
                {
                    if (user.Key != nickname)
                    {
                        user.Value.ShowUserConected(nickname);
                    }
                }
                catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                {
                    log.Error(communicationObjectAbortedException);
                    DisconectUser(user.Key);
                }
                catch (CommunicationException communicationException)
                {
                    log.Error(communicationException);
                    DisconectUser(user.Key);
                }
                catch (TimeoutException timeoutException)
                {
                    log.Error(timeoutException);
                    DisconectUser(user.Key);
                }
            }
        }

        public void DisconectUser(string nickname)
        {
            lock (_onlineUsers)
            {
                if (_onlineUsers.ContainsKey(nickname))
                {
                    _onlineUsers.Remove(nickname);
                    try
                    {
                        foreach (var user in _onlineUsers)
                        {
                            try
                            {
                                user.Value.ShowUserDisconected(nickname);
                            }
                            catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                            {
                                log.Error(communicationObjectAbortedException);
                                DisconectUser(user.Key);
                            }
                            catch (CommunicationException communicationException)
                            {
                                log.Error(communicationException);
                                DisconectUser(user.Key);
                            }
                            catch (TimeoutException timeoutException)
                            {
                                log.Error(timeoutException);
                                DisconectUser(user.Key);
                            }
                        }
                    }
                    catch (InvalidOperationException invalidOperationException)
                    {
                        log.Error(invalidOperationException);
                    }
                }
            }
        }

        public int SendFriendRequest(string nickname, string nicknameFriend)
        {
            int isSucceded = VALIDATION_FAILURE;

            lock (_lock)
            {
                try
                {
                    int findUserAnswer = FindUserByNickname(nicknameFriend);

                    if (findUserAnswer == VALIDATION_SUCCESS)
                    {
                        FriendAccess friendAccess = new FriendAccess();
                        if (!friendAccess.FriendRequestExists(nickname, nicknameFriend) && friendAccess.SendFriendRequest(nickname, nicknameFriend))
                        {
                            isSucceded = VALIDATION_SUCCESS;
                            if (_onlineUsers.ContainsKey(nicknameFriend))
                            {
                                _onlineUsers[nicknameFriend].ShowFriendRequest(nickname);
                            }
                        }
                    }
                    else if (findUserAnswer == ERROR)
                    {
                        isSucceded = ERROR;
                    }
                }
                catch (EntityException entityException)
                {
                    log.Error(entityException);
                    isSucceded = ERROR;
                }
                catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                {
                    log.Error(communicationObjectAbortedException);
                    DisconectUser(nicknameFriend);
                }
                catch (CommunicationException communicationException)
                {
                    log.Error(communicationException);
                    DisconectUser(nicknameFriend);
                }
                catch (TimeoutException timeoutException)
                {
                    log.Error(timeoutException);
                    DisconectUser(nicknameFriend);
                }
            }
            return isSucceded;
        }

        public int ReplyFriendRequest(string nickname, string nicknameRequest, bool answer)
        {
            int isSucceded = VALIDATION_FAILURE;

            try
            {
                int findUserAnswer = FindUserByNickname(nickname);

                if (findUserAnswer == VALIDATION_SUCCESS)
                {
                    FriendAccess friendAccess = new FriendAccess();

                    if (friendAccess.ReplyFriendRequest(nickname, nicknameRequest, answer) > IS_SUCCEDED)
                    {
                        isSucceded = VALIDATION_SUCCESS;

                        if (answer == ACCEPTED_FRIEND && _onlineUsers.ContainsKey(nicknameRequest))
                        {
                            _onlineUsers[nicknameRequest].ShowFriendAccepted(nickname);
                        }
                    }
                }
                else if (findUserAnswer == ERROR)
                {
                    isSucceded = ERROR;
                }
            }
            catch (EntityException entityException)
            {
                log.Error(entityException);
                isSucceded = ERROR;
            }
            catch (CommunicationObjectAbortedException communicationObjectAbortedException)
            {
                log.Error(communicationObjectAbortedException);
                DisconectUser(nicknameRequest);
            }
            catch (CommunicationException communicationException)
            {
                log.Error(communicationException);
                DisconectUser(nicknameRequest);
            }
            catch (TimeoutException timeoutException)
            {
                log.Error(timeoutException);
                DisconectUser(nicknameRequest);
            }

            return isSucceded;
        }

        public int RemoveFriend(string nickname, string nicknamefriendToRemove)
        {
            int isSucceded = VALIDATION_FAILURE;

            FriendAccess friendAccess = new FriendAccess();
            try
            {
                if(friendAccess.RemoveFriend(nickname, nicknamefriendToRemove))
                {
                    isSucceded = VALIDATION_SUCCESS;
                }
            }
            catch (EntityException entityException)
            {
                log.Error(entityException);
                isSucceded = ERROR;
            }

            if (_onlineUsers.ContainsKey(nicknamefriendToRemove))
            {
                try
                {
                    _onlineUsers[nicknamefriendToRemove].FriendDeleted(nickname);
                }
                catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                {
                    log.Error(communicationObjectAbortedException);
                    DisconectUser(nicknamefriendToRemove);
                }
                catch (CommunicationException communicationException)
                {
                    log.Error(communicationException);
                    DisconectUser(nicknamefriendToRemove);
                }
                catch (TimeoutException timeoutException)
                {
                    log.Error(timeoutException);
                    DisconectUser(nicknamefriendToRemove);
                }
            }

            return isSucceded;
        }
    }

    public partial class UserManager : IGameManager
    {

        private readonly Random _random = new Random();
        private static Dictionary<string, IGameManagerCallback> _usersInGameContext = new Dictionary<string, IGameManagerCallback>();
        private const int GAME_ABORTED = 0;

        public void ConnectGame(string nickname)
        {
            if (_usersInLobby.ContainsValue(_usersInLobby[nickname]))
            {
                IGameManagerCallback currentUserCallbackChannel = OperationContext.Current.GetCallbackChannel<IGameManagerCallback>();

                if (!_usersInGameContext.ContainsKey(nickname))
                {
                    _usersInGameContext.Add(nickname, currentUserCallbackChannel);
                }

                Dictionary<string, int> usersInGame = GetUsersPerTeam(nickname);

                try
                {
                    currentUserCallbackChannel.ShowUsersInGame(usersInGame);
                }
                catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                {
                    GameAccess gameAccess = new GameAccess();
                    gameAccess.BanUser(nickname);
                    log.Error(communicationObjectAbortedException);
                    EndGame(GAME_ABORTED, nickname);
                }
                catch (CommunicationException communicationException)
                {
                    GameAccess gameAccess = new GameAccess();
                    gameAccess.BanUser(nickname);
                    log.Error(communicationException);
                    EndGame(GAME_ABORTED, nickname);
                }
                catch (TimeoutException timeoutException)
                {
                    GameAccess gameAccess = new GameAccess();
                    gameAccess.BanUser(nickname);
                    log.Error(timeoutException);
                    EndGame(GAME_ABORTED, nickname);
                }

                ShowUsersConnectedGame(usersInGame, nickname);
            }
        }

        private void ShowUsersConnectedGame(Dictionary<string, int> usersInGame, string nickname)
        {
            foreach (string userInGame in usersInGame.Keys)
            {
                if (userInGame != nickname)
                {
                    try
                    {
                        _usersInGameContext[userInGame].ShowUserConnectedGame(nickname, _usersTeam[nickname]);
                    }
                    catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                    {
                        CommunicationEndedException(nickname, userInGame);
                        log.Error(communicationObjectAbortedException);
                    }
                    catch (CommunicationException communicationException)
                    {
                        CommunicationEndedException(nickname, userInGame);
                        log.Error(communicationException);
                    }
                    catch (TimeoutException timeoutException)
                    {
                        CommunicationEndedException(nickname, userInGame);
                        log.Error(timeoutException);
                    }
                }
            }
        }

        public List<int> DispenseCards(string nickname)
        {
            ChangeSingle();

            DeckAccess deckAccess = new DeckAccess();
            List<int> userDeck = new List<int>();

            try
            {
                userDeck = deckAccess.GetDeckByNickname(nickname);
            }
            catch(EntityException entityException)
            {
                log.Error(entityException);
            }

            List<int> shuffledDeck = userDeck.OrderBy(x => _random.Next()).ToList();

            ChangeMultiple();

            return shuffledDeck;
        }

        public void DrawCard(string nickname, int [] cardId)
        {
            ChangeSingle();

            Dictionary<string, int> usersInGame = GetUsersPerTeam(nickname);

            foreach (string userInGame in usersInGame.Keys)
            {
                if (userInGame != nickname && usersInGame[nickname] == usersInGame[userInGame])
                {
                    try
                    {
                        _usersInGameContext[userInGame].DrawCardClient(nickname, cardId);
                    }
                    catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                    {
                        CommunicationEndedException(nickname, userInGame);
                        log.Error(communicationObjectAbortedException);
                    }
                    catch (CommunicationException communicationException)
                    {
                        CommunicationEndedException(nickname, userInGame);
                        log.Error(communicationException);
                    }
                    catch (TimeoutException timeoutException)
                    {
                        CommunicationEndedException(nickname, userInGame);
                        log.Error(timeoutException);
                    }
                }
            }

            ChangeMultiple();
        }

        public void EndGame(int winnerTeam, string nickname)
        {
            Dictionary<string, int> usersInGame = GetUsersPerTeam(nickname);
            try
            {
                GameAccess gameAccess = new GameAccess();
                gameAccess.EndGame(winnerTeam, _usersInLobby[nickname]);
            }
            catch(EntityException entityException)
            {
                log.Error(entityException);
                HandleEntityExceptionInGame(usersInGame);
            }

            foreach (string userInGame in usersInGame.Keys)
            {
                try
                {
                    if (_usersInGameContext.ContainsKey(userInGame))
                    {
                        _usersInGameContext[userInGame].EndGameClient(winnerTeam);
                    }
                }
                catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                {
                    RemoveUser(userInGame);
                    log.Error(communicationObjectAbortedException.Message);
                }
                catch (CommunicationException communicationException)
                {
                    RemoveUser(userInGame);
                    log.Error(communicationException.Message);
                }
                catch (TimeoutException timeoutException)
                {
                    RemoveUser(userInGame);
                    log.Error(timeoutException.Message);
                }
                catch (ObjectDisposedException objectDisposedException)
                {
                    log.Error(objectDisposedException);
                }

            } 
            
        }

        private void HandleEntityExceptionInGame(Dictionary<string, int> usersInGame)
        {
            foreach(string userInGame in usersInGame.Keys)
            {
                RemoveUser(userInGame);
            }
        }

        public void EndGameTurn(string nickname, Dictionary<int, int> boardAfterTurn)
        {
            Dictionary<string, int> usersInGame = GetUsersPerTeam(nickname);         

            foreach (string userInGame in usersInGame.Keys)
            {
                if((usersInGame[userInGame] == usersInGame[nickname]) && (userInGame != nickname))
                {
                   AllyEndedTurn(nickname, boardAfterTurn, userInGame);
                }
                else
                {
                    EnemyEndedTurn(nickname, boardAfterTurn, userInGame);
                }
            }
        }

        private void AllyEndedTurn(string nickname, Dictionary<int, int> boardAfterTurn, string userInGame)
        {
            try
            {
                _usersInGameContext[userInGame].PlayerEndedTurn(nickname, boardAfterTurn);
            }
            catch (CommunicationObjectAbortedException communicationObjectAbortedException)
            {
                CommunicationEndedException(nickname, userInGame);
                log.Error(communicationObjectAbortedException.Message);
            }
            catch (CommunicationException communicationException)
            {
                CommunicationEndedException(nickname, userInGame);
                log.Error(communicationException.Message);
            }
            catch (TimeoutException timeoutException)
            {
                CommunicationEndedException(nickname, userInGame);
                log.Error(timeoutException.Message);
            }
        }

        private void EnemyEndedTurn(string nickname, Dictionary<int, int> boardAfterTurn, string userInGame)
        {
            Dictionary<int, int> reversedBoard = ReverseBoard(boardAfterTurn);
            try
            {
                _usersInGameContext[userInGame].PlayerEndedTurn(nickname, reversedBoard);
            }
            catch (CommunicationObjectAbortedException communicationObjectAbortedException)
            {
                CommunicationEndedException(nickname, userInGame);
                log.Error(communicationObjectAbortedException);
            }
            catch(CommunicationException communicationException)
            {
                CommunicationEndedException(nickname, userInGame);
                log.Error(communicationException);
            }
            catch(TimeoutException timeoutException)
            {
                CommunicationEndedException(nickname, userInGame);
                log.Error(timeoutException);
            }
        }

        private void CommunicationEndedException(string nickname, string userInGame)
        {
            GameAccess gameAccess = new GameAccess();
            gameAccess.BanUser(userInGame);
            RemoveUser(userInGame);
            EndGame(GAME_ABORTED, nickname);
        }

        private Dictionary<int, int> ReverseBoard(Dictionary<int, int> originalBoard)
        {
            Dictionary<int, int> reversedBoard = new Dictionary<int, int>();

            for (int i = 0; i < originalBoard.Count; i++)
            {
                reversedBoard.Add(originalBoard.Count - i, originalBoard[i + 1]);
            }

            return reversedBoard;
        }

        public void SendMessageGame(string message, string nickname)
        {
            Dictionary <string, int> usersInTeam = GetUsersPerTeam(nickname);

            foreach (string userInGame in usersInTeam.Keys)
            {
                try
                {
                    if (usersInTeam[userInGame] == usersInTeam[nickname])
                    {
                        _usersInGameContext[userInGame].ReceiveMessageGame(message);
                    }
                }
                catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                {
                    CommunicationEndedException(nickname, userInGame);
                    log.Error(communicationObjectAbortedException);
                }
                catch (CommunicationException communicationException)
                {
                    CommunicationEndedException(nickname, userInGame);
                    log.Error(communicationException);
                }
                catch (TimeoutException timeoutException)
                {
                    CommunicationEndedException(nickname, userInGame);
                    log.Error(timeoutException);
                }
            }
        }


        private Dictionary<string, int> GetUsersPerTeam(string nickname)
        {
            Dictionary<string, int> usersInGame = new Dictionary<string, int>();

            if (_usersInLobby.ContainsKey(nickname))
            {
                List<string> usersNickname = FindKeysByValue(_usersInLobby, _usersInLobby[nickname]);

                foreach (string userNickname in usersNickname)
                {
                    if (_usersInGameContext.ContainsKey(userNickname))
                    {
                        usersInGame.Add(userNickname, _usersTeam[userNickname]);
                    }
                }
            }            

            return usersInGame;
        }

        public void StartFirstPhase(string hostNickname)
        {
            Dictionary<string, int> usersInGame = GetUsersPerTeam(hostNickname);
            Tuple<string, string> firstPlayers = new Tuple<string, string>("","");

            foreach(string userInGame in usersInGame.Keys)
            {
                if (usersInGame[userInGame] != usersInGame[hostNickname])
                {
                    firstPlayers = new Tuple<string, string>(userInGame, hostNickname);
                    break;
                }
            }

            foreach (string userInGame in usersInGame.Keys)
            {
                try
                {
                    _usersInGameContext[userInGame].StartFirstPhaseClient(firstPlayers);
                }
                catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                {
                    CommunicationEndedException(hostNickname, userInGame);
                    log.Error(communicationObjectAbortedException);
                }
                catch (CommunicationException communicationException)
                {
                    CommunicationEndedException(hostNickname, userInGame);
                    log.Error(communicationException);
                }
                catch (TimeoutException timeoutException)
                {
                    CommunicationEndedException(hostNickname, userInGame);
                    log.Error(timeoutException);
                }

            }
        }

        private void ChangeSingle()
        {
            var hostService = (ServiceHost) OperationContext.Current.Host;
            var behaviour = hostService.Description.Behaviors.Find<ServiceBehaviorAttribute>();
            behaviour.ConcurrencyMode = ConcurrencyMode.Single;
        }

        private void ChangeMultiple()
        {
            var hostService = (ServiceHost)OperationContext.Current.Host;
            var behaviour = hostService.Description.Behaviors.Find<ServiceBehaviorAttribute>();
            behaviour.ConcurrencyMode = ConcurrencyMode.Multiple;
        }
    }

    public partial class UserManager : IEndGame
    {
        public void GetUsersWithTeam(string nickname)
        {
            Dictionary<string, int> usersInGame = GetUsersPerTeam(nickname);
            List<UserWithTeam> usersWithTeam = new List<UserWithTeam>();
            IEndGameCallback currentUserCallbackChannel = OperationContext.Current.GetCallbackChannel<IEndGameCallback>();

            foreach (string userNickname in usersInGame.Keys)
            {
                UserWithTeam userWithTeam = new UserWithTeam();
                User user = GetUserByNickname(userNickname);

                userWithTeam.Nickname = userNickname;
                userWithTeam.Mail = user.Mail;
                userWithTeam.ImageId = user.ImageId;
                userWithTeam.Team = usersInGame[userNickname];

                usersWithTeam.Add(userWithTeam);
            }
            try
            {
                currentUserCallbackChannel.SetUsers(usersWithTeam);
            }
            catch(CommunicationObjectAbortedException communicationObjectAbortedException)
            {
                log.Error(communicationObjectAbortedException);
                RemoveUser(nickname);
            }
            catch (CommunicationException communicationException)
            {
                log.Error(communicationException);
                RemoveUser(nickname);
            }
            catch (TimeoutException timeoutException)
            {
                log.Error(timeoutException);
                RemoveUser(nickname);
            }
        }

        public void GameEnded(string nickname)
        {
            RemoveUser(nickname);
        }

        private void RemoveUser(string nickname)
        {
            if(_usersInGameContext.ContainsKey(nickname))
            {
                _usersInGameContext.Remove(nickname);
            }

            if (_usersInLobby.ContainsKey(nickname))
            {
                _usersInLobby.Remove(nickname);
            }

            if (_usersTeam.ContainsKey(nickname))
            {
                _usersTeam.Remove(nickname);
            }
        }
    }

    public partial class UserManager : ILeaderboardManager
    {
        public List<GamesWonInfo> GetLeaderboardInfo()
        {
            List<GamesWonInfo> listOfGamers = new List<GamesWonInfo>();

            GameAccess gameAccess = new GameAccess();

            try
            {
                listOfGamers = gameAccess.GetTopGamesWon();
            }
            catch (EntityException entityException)
            {
                listOfGamers = null;
                log.Error(entityException);
            }

            return listOfGamers;
        }
    }

}