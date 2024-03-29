﻿using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using DataAccessProject.Contracts;
using DataAccessProject.DataAccess;
using User = DataAccessProject.Contracts.User;
using System.Data.SqlClient;
using System.Data.Entity.Core;


namespace MessageService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public partial class MessageService : IUserManager
    {
        private const string NICKNAME_ERROR = "ERROR";
        private const int GUEST_IMAGE_ID = 1;
        private const int VALIDATION_FAILURE = 0;
        private const int ERROR = -1;
        private const int VALIDATION_SUCCESS = 1;

        private static readonly ILog log = LogManager.GetLogger(typeof(MessageService));

        public int ConfirmUserCredentials(string nickname, string password)
        {
            int result = VALIDATION_FAILURE;
            UserAccess userAccess = new UserAccess();

            try
            {
                if (userAccess.FindUserByNickname(nickname) == VALIDATION_SUCCESS)
                {
                    result = userAccess.ConfirmUserCredentials(nickname, password);
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
                if (userAccess.FindUserByNickname(user.Nickname) == VALIDATION_FAILURE)
                {
                    result = userAccess.CreateUser(user);
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

        public User AddGuestUser()
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
            UserAccess userAccess = new UserAccess();
            int isFound; 

            try
            {
                isFound = userAccess.FindUserByNickname(nickname);
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
            UserAccess userAccess = new UserAccess();
            User foundUser = new User();
            foundUser.Nickname = NICKNAME_ERROR;

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

        public bool IsUserOnline(string nickname)
        {
            lock (_onlineUsers)
            {
                return _onlineUsers.ContainsKey(nickname);
            }
        }
    }

    public partial class MessageService : ILobbyManager
    {
        private const bool IS_SUCCESFULL = true;
        private const int NO_TEAM = 0;
        private const string USER_NOT_FOUND = "UserNotFound";
        private const string ERROR_STRING = "ERROR";
        private const string VALIDATION_FAILURE_STRING = "FAILURE";
        private const int GAME_FULL = 4;

        private static Dictionary<string, string> _usersInLobby = new Dictionary<string, string>();
        private static Dictionary<string, ILobbyManagerCallback> _usersContext = new Dictionary<string, ILobbyManagerCallback>();
        private static Dictionary<string, int> _usersTeam = new Dictionary<string, int>();

        public bool LobbyExist(string gameId)
        {
            lock(_usersInLobby)
            {
                return _usersInLobby.ContainsValue(gameId);
            }
        }

        public bool LobbyIsNotFull(string gameId)
        {
            int usersInGame = 0;
            bool gameIsNotFull = true;
            var groupedKeys = _usersInLobby.Where(pair => pair.Value == gameId).Select(pair => pair.Key);
            usersInGame = groupedKeys.Count();
            
            if(usersInGame == GAME_FULL)
            {
                gameIsNotFull = false;
            }

            return gameIsNotFull;
        }

        public int CanAccessToLobby(string nickname)
        {
            GameAccess gameAccess = new GameAccess();
            int result;

            try
            {
                result = gameAccess.CanPlay(nickname);

                if (result == VALIDATION_SUCCESS)
                {
                    gameAccess.RemoveBan(nickname);
                }
            }
            catch (EntityException entityException)
            {
                log.Error(entityException);
                result = ERROR;
            }

            return result;
        }

        public void ConnectToLobby(User user, string gameId)
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
                        AddUserToLobbyDictionaries(user, gameId, currentUserCallbackChannel);
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

        private void AddUserToLobbyDictionaries(User user, string gameId, ILobbyManagerCallback currentUserCallbackChannel)
        {
            if (!_usersContext.ContainsKey(user.Nickname))
            {
                _usersContext.Add(user.Nickname, currentUserCallbackChannel);
                _usersInLobby.Add(user.Nickname, gameId);
                _usersTeam.Add(user.Nickname, NO_TEAM);
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

            DisconnectFromLobby(userToDisconnect);
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

                    AddUserToLobbyDictionaries(user, gameId, currentUserCallbackChannel);
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

        public string SendInvitationToLobby(string gameId, string userToSend)
        {
            UserAccess userAccess = new UserAccess();
            string mailString;

            try
            {
                User user = userAccess.GetUserByNickname(userToSend);

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

        public void DisconnectFromLobby(User user)
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
                    ShowDisconnectionOfLobby(userInTheLobby, user);
                }
            }
        }

        private void ShowDisconnectionOfLobby(string userInTheLobby, User user)
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

        public void SendUsersFromLobbyToGame(string gameId)
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
                    catch (SqlException sqlException)
                    {
                        log.Error(sqlException);
                    }
                    catch (EntityException entityException)
                    {
                        log.Error(entityException);
                    }
                }
                TellUsersToGoFromLobbyToGame(result, usersNickname);
            }
        }

        private void TellUsersToGoFromLobbyToGame(int result, List<string> usersNickname)
        {
            if (result > VALIDATION_FAILURE)
            {
                foreach (string userInTheLobby in usersNickname)
                {
                    try
                    {
                        _usersContext[userInTheLobby].SendUserFromLobbyToGame();
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


        public void SendMessage(string message, string nickname)
        {
            if (_usersInLobby.ContainsKey(nickname))
            {
                string gameId = _usersInLobby[nickname];
                List<string> usersNickname = FindKeysByValue(_usersInLobby, gameId);

                if (ContainsBadWords(message))
                {
                    GameAccess gameAccess = new GameAccess();

                    gameAccess.BanUser(nickname);
                    KickUserFromLobby(nickname);
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
            List<string> badwords = new List<string> {"fucker", "Yeison", "fuck" ,"putito", "puto", "marica", "jodido", "negro"};
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

        public void KickUserFromLobby(string userNickname)
        {
            if (_usersInLobby.ContainsKey(userNickname) && _usersContext.ContainsKey(userNickname))
            {
                try
                {
                    _usersContext[userNickname].GetKickedFromLobby();
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

                    DisconnectFromLobby(userToDisconnect);
                }
            }
        }
    }

    public partial class MessageService : IFriendManager
    {
        private static Dictionary<string, IFriendManagerCallback> _onlineUsers = new Dictionary<string, IFriendManagerCallback>();
        private const bool ACCEPTED_FRIEND = true;
        private static readonly Object _lock = new Object();

        [OperationBehavior]
        public void SubscribeToFriendManager(string nickname)
        {
            lock (_onlineUsers)
            {
                IFriendManagerCallback currentUserCallbackChannel = OperationContext.Current.GetCallbackChannel<IFriendManagerCallback>();
                FriendAccess friendAccess = new FriendAccess();

                try
                {
                    currentUserCallbackChannel.ShowFriends(friendAccess.GetFriendList(nickname, _onlineUsers.Keys.ToList()));

                    if (!_onlineUsers.ContainsKey(nickname))
                    {
                        _onlineUsers.Add(nickname, currentUserCallbackChannel);
                        ShowUserSubscribedToFriendManager(nickname);
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
            }
        }

        private void ShowUserSubscribedToFriendManager(string nickname)
        {
            foreach (KeyValuePair<string, IFriendManagerCallback> user in _onlineUsers)
            {
                try
                {
                    if (user.Key != nickname)
                    {
                        user.Value.ShowUserSubscribedToFriendManager(nickname);
                    }
                }
                catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                {
                    log.Error(communicationObjectAbortedException);
                    UnsubscribeToFriendManager(user.Key);
                }
                catch (CommunicationException communicationException)
                {
                    log.Error(communicationException);
                    UnsubscribeToFriendManager(user.Key);
                }
                catch (TimeoutException timeoutException)
                {
                    log.Error(timeoutException);
                    UnsubscribeToFriendManager(user.Key);
                }
            }
        }

        public void UnsubscribeToFriendManager(string nickname)
        {
            lock (_onlineUsers)
            {
                if (_onlineUsers.ContainsKey(nickname))
                {
                    _onlineUsers.Remove(nickname);
                    try
                    {
                        ShowUserUnsubscribedToFriendManager(nickname);
                    }
                    catch (InvalidOperationException invalidOperationException)
                    {
                        log.Error(invalidOperationException);
                    }
                }
            }
        }

        private void ShowUserUnsubscribedToFriendManager(string nickname)
        {
            foreach (KeyValuePair<string, IFriendManagerCallback> user in _onlineUsers)
            {
                try
                {
                    user.Value.ShowUserUnsubscribedToFriendManager(nickname);
                }
                catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                {
                    log.Error(communicationObjectAbortedException);
                    UnsubscribeToFriendManager(user.Key);
                }
                catch (CommunicationException communicationException)
                {
                    log.Error(communicationException);
                    UnsubscribeToFriendManager(user.Key);
                }
                catch (TimeoutException timeoutException)
                {
                    log.Error(timeoutException);
                    UnsubscribeToFriendManager(user.Key);
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
                    int findUserResult = FindUserByNickname(nicknameFriend);

                    if (findUserResult == VALIDATION_SUCCESS)
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
                    else if (findUserResult == ERROR)
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
                    UnsubscribeToFriendManager(nicknameFriend);
                }
                catch (CommunicationException communicationException)
                {
                    log.Error(communicationException);
                    UnsubscribeToFriendManager(nicknameFriend);
                }
                catch (TimeoutException timeoutException)
                {
                    log.Error(timeoutException);
                    UnsubscribeToFriendManager(nicknameFriend);
                }
            }
            return isSucceded;
        }

        public int ReplyFriendRequest(string nickname, string nicknameRequest, bool answer)
        {
            int isSucceded = VALIDATION_FAILURE;

            try
            {
                int findUserResult = FindUserByNickname(nickname);

                if (findUserResult == VALIDATION_SUCCESS)
                {
                    FriendAccess friendAccess = new FriendAccess();

                    if (friendAccess.ReplyFriendRequest(nickname, nicknameRequest, answer) > VALIDATION_FAILURE)
                    {
                        isSucceded = VALIDATION_SUCCESS;

                        if (answer == ACCEPTED_FRIEND && _onlineUsers.ContainsKey(nicknameRequest))
                        {
                            _onlineUsers[nicknameRequest].ShowFriendAccepted(nickname);
                        }
                    }
                }
                else if (findUserResult == ERROR)
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
                UnsubscribeToFriendManager(nicknameRequest);
            }
            catch (CommunicationException communicationException)
            {
                log.Error(communicationException);
                UnsubscribeToFriendManager(nicknameRequest);
            }
            catch (TimeoutException timeoutException)
            {
                log.Error(timeoutException);
                UnsubscribeToFriendManager(nicknameRequest);
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

            ShowFriendRemoved(nickname, nicknamefriendToRemove);

            return isSucceded;
        }

        private void ShowFriendRemoved(string nickname, string nicknamefriendToRemove)
        {
            if (_onlineUsers.ContainsKey(nicknamefriendToRemove))
            {
                try
                {
                    _onlineUsers[nicknamefriendToRemove].ShowFriendDeleted(nickname);
                }
                catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                {
                    log.Error(communicationObjectAbortedException);
                    UnsubscribeToFriendManager(nicknamefriendToRemove);
                }
                catch (CommunicationException communicationException)
                {
                    log.Error(communicationException);
                    UnsubscribeToFriendManager(nicknamefriendToRemove);
                }
                catch (TimeoutException timeoutException)
                {
                    log.Error(timeoutException);
                    UnsubscribeToFriendManager(nicknamefriendToRemove);
                }
            }
        }
    }

    public partial class MessageService : IGameManager
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

        public List<int> DispenseGameCards(string nickname)
        {
            ChangeSingle();

            DeckAccess deckAccess = new DeckAccess();
            List<int> userDeck = new List<int>();

            try
            {
                userDeck = deckAccess.GetDeckByNickname(nickname);
            }
            catch (EntityException entityException)
            {
                log.Error(entityException);
            }

            List<int> shuffledDeck = userDeck.OrderBy(x => _random.Next()).ToList();

            ChangeMultiple();

            return shuffledDeck;
        }

        public void DrawGameCard(string nickname, int [] cardId)
        {
            ChangeSingle();

            Dictionary<string, int> usersInGame = GetUsersPerTeam(nickname);

            foreach (string userInGame in usersInGame.Keys)
            {
                if (userInGame != nickname && usersInGame[nickname] == usersInGame[userInGame])
                {
                    try
                    {
                        _usersInGameContext[userInGame].ShowCardDrawedInGame(nickname, cardId);
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

            ShowUsersGameEnded(usersInGame, winnerTeam);
        }

        private void ShowUsersGameEnded(Dictionary<string, int> usersInGame, int winnerTeam)
        {
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
                if ((usersInGame[userInGame] == usersInGame[nickname]) && (userInGame != nickname))
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
                _usersInGameContext[userInGame].ShowGamePlayerEndedTurn(nickname, boardAfterTurn);
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
                _usersInGameContext[userInGame].ShowGamePlayerEndedTurn(nickname, reversedBoard);
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

        public void SendMessageToGame(string message, string nickname)
        {
            Dictionary <string, int> usersInTeam = GetUsersPerTeam(nickname);

            foreach (string userInGame in usersInTeam.Keys)
            {
                try
                {
                    if (usersInTeam[userInGame] == usersInTeam[nickname])
                    {
                        _usersInGameContext[userInGame].RecieveGameMessage(message);
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

        public void StartFirstGamePhase(string hostNickname)
        {
            Dictionary<string, int> usersInGame = GetUsersPerTeam(hostNickname);
            Tuple<string, string> firstPlayers = CreateFirstPlayerTuple(usersInGame, hostNickname);

            foreach (string userInGame in usersInGame.Keys)
            {
                try
                {
                    _usersInGameContext[userInGame].StartFirstGamePhaseClient(firstPlayers);
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

        private Tuple<string, string> CreateFirstPlayerTuple(Dictionary<string, int> usersInGame, string hostNickname)
        {
            Tuple<string, string> firstPlayers = Tuple.Create("", "");

            foreach (string userInGame in usersInGame.Keys)
            {
                if (usersInGame[userInGame] != usersInGame[hostNickname])
                {
                    firstPlayers = new Tuple<string, string>(userInGame, hostNickname);
                    break;
                }
            }

            return firstPlayers;
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

    public partial class MessageService : IEndGame
    {
        public void GetEndGameUsers(string nickname)
        {
            Dictionary<string, int> usersInGame = GetUsersPerTeam(nickname);
            List<UserWithTeam> usersWithTeam = new List<UserWithTeam>();
            IEndGameCallback currentUserCallbackChannel = OperationContext.Current.GetCallbackChannel<IEndGameCallback>();

            foreach (string userNickname in usersInGame.Keys)
            {
                usersWithTeam.Add(AddUserWithTeam(userNickname, usersInGame));
            }

            try
            {
                currentUserCallbackChannel.ShowEndGameUsers(usersWithTeam);
            }
            catch (CommunicationObjectAbortedException communicationObjectAbortedException)
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

        private UserWithTeam AddUserWithTeam(string userNickname, Dictionary<string, int> usersInGame)
        {
            UserWithTeam userWithTeam = new UserWithTeam();
            User user = GetUserByNickname(userNickname);
            userWithTeam.Nickname = userNickname;
            userWithTeam.Mail = user.Mail;
            userWithTeam.ImageId = user.ImageId;
            userWithTeam.Team = usersInGame[userNickname];

            return userWithTeam;
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

    public partial class MessageService : ILeaderboardManager
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
                GamesWonInfo gamesWonInfoError = new GamesWonInfo();
                gamesWonInfoError.GamesWonCount = -1;
                gamesWonInfoError.Username = ERROR_STRING;

                listOfGamers.Add(gamesWonInfoError);
                log.Error(entityException);
            }

            return listOfGamers;
        }
    }

}