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

            try
            {
                if (FindUserByNickname(nickname) == VALIDATION_SUCCESS)
                {
                    UserAccess userAccess = new UserAccess();
                    result = userAccess.ConfirmUser(nickname, password);
                }
                if (result > VALIDATION_FAILURE)
                {
                    result = VALIDATION_SUCCESS;
                }
            }
            catch(EntityException entityException)
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

                int result = userAccess.CreateGuest(guestUser);
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
            catch(EntityException entityException)
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

            return foundUser;
        }

        public int UpdateUser(User user)
        {
            int result = VALIDATION_FAILURE;

            UserAccess userAccess = new UserAccess();
            try
            {
                if (FindUserByNickname(user.Nickname) == VALIDATION_SUCCESS)
                {
                    result = userAccess.UpdateUser(user);

                    if (result > VALIDATION_FAILURE)
                    {
                        result = VALIDATION_SUCCESS;
                    }
                }
            }
            catch(SqlException sqlException)
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
            lock (onlineUsers)
            {
                return onlineUsers.ContainsKey(nickname);
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

        private static Dictionary<string, string> usersInLobby = new Dictionary<string, string>();
        private static Dictionary<string, ILobbyManagerCallback> usersContext = new Dictionary<string, ILobbyManagerCallback>();
        private static Dictionary<string, int> usersTeam = new Dictionary<string, int>();

        public bool GameExist(string gameId)
        {
            lock(usersInLobby)
            {
                return usersInLobby.ContainsValue(gameId);
            }
        }

        public bool GameIsNotFull(string gameId)
        {
            int usersInGame = 0;
            bool gameIsNotFull = true;

            var groupedKeys = usersInLobby.Where(pair => pair.Value == gameId).Select(pair => pair.Key);
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
            lock (usersTeam)
            {
                if (usersInLobby.ContainsValue(gameId))
                {
                    List<string> usersNickname = FindKeysByValue(usersInLobby, gameId);
                    List<Tuple<User, int>> users = new List<Tuple<User, int>>();

                    foreach (string nickname in usersNickname)
                    {
                        users.Add(new Tuple<User, int>(GetUserByNickname(nickname), usersTeam[nickname]));
                    }

                    ILobbyManagerCallback currentUserCallbackChannel = OperationContext.Current.GetCallbackChannel<ILobbyManagerCallback>();

                    try
                    {
                        currentUserCallbackChannel.ShowUsersInLobby(users);

                        if (!usersContext.ContainsKey(user.Nickname))
                        {
                            usersContext.Add(user.Nickname, currentUserCallbackChannel);
                            usersInLobby.Add(user.Nickname, gameId);
                            usersTeam.Add(user.Nickname, NO_TEAM);
                        }
                    }
                    catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                    {
                        log.Error(communicationObjectAbortedException);
                    }

                    foreach (string userInTheLobby in usersNickname)
                    {
                        try
                        {
                            usersContext[userInTheLobby].ShowConnectionInLobby(user);
                        }
                        catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                        {
                            log.Error(communicationObjectAbortedException);
                            User userToDisconnect = new User();
                            userToDisconnect.Nickname = userInTheLobby;
                            DisconnectLobby(userToDisconnect);
                        }
                    }
                }
            }
            
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
                    usersContext.Add(user.Nickname, currentUserCallbackChannel);
                    usersInLobby.Add(user.Nickname, gameId);
                    usersTeam.Add(user.Nickname, NO_TEAM);
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
                    mailString = Mail.Mail.Instance().sendInvitationMail(user.Mail, gameId);
                }
                else
                {
                    mailString = Mail.Mail.Instance().sendInvitationMail(userToSend, gameId);
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
            if (usersInLobby.ContainsKey(user.Nickname))
            {
                string gameId = usersInLobby[user.Nickname];
                List<string> usersNickname = FindKeysByValue(usersInLobby, gameId);

                usersInLobby.Remove(user.Nickname);
                usersContext.Remove(user.Nickname);
                usersTeam.Remove(user.Nickname);

                foreach (var userInTheLobby in usersNickname)
                {
                    try
                    {
                        if (usersContext.ContainsKey(userInTheLobby))
                        {
                            usersContext[userInTheLobby].ShowDisconnectionInLobby(user);
                        }
                    }
                    catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                    {
                        log.Error(communicationObjectAbortedException);
                        User userToDisconnect = new User();
                        userToDisconnect.Nickname = userInTheLobby;
                        DisconnectLobby(userToDisconnect);
                    }
                    catch (CommunicationException communicationException)
                    {
                        log.Error("User disconnected: " + userInTheLobby + "/n"+ communicationException);
                        User userToDisconnect = new User();
                        userToDisconnect.Nickname = userInTheLobby;
                        DisconnectLobby(userToDisconnect);
                    }
                }
            }
        }

        public void ChangeLobbyUserTeam(string userNickname, int team)
        {
            if (usersTeam.ContainsKey(userNickname))
            {
                string gameId = usersInLobby[userNickname];
                usersTeam[userNickname] = team;
                List<string> usersNickname = FindKeysByValue(usersInLobby, gameId);

                foreach (var userInTheLobby in usersNickname)
                {
                    if (userInTheLobby != userNickname)
                    {
                        try
                        {
                            usersContext[userInTheLobby].UpdateLobbyUserTeam(userNickname, team);
                        }
                        catch(CommunicationObjectAbortedException communicationObjectAbortedException)
                        {
                            log.Error(communicationObjectAbortedException);
                            User userToDisconnect = new User();
                            userToDisconnect.Nickname = userInTheLobby;
                            DisconnectLobby(userToDisconnect);
                        }
                    }
                }
            }
        }

        public void StartGame(string gameId)
        {

            List<string> usersNickname = FindKeysByValue(usersInLobby, gameId);
            int result = VALIDATION_FAILURE;

            if (usersInLobby.ContainsValue(gameId))
            {
                GameAccess gameAccess = new GameAccess();

                foreach (var user in usersNickname)
                {
                    try
                    {
                        result =+ gameAccess.CreatePlaysRelation(user, gameId, usersTeam[user]);
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

                if (result > VALIDATION_FAILURE)
                {
                    foreach (string userInTheLobby in usersNickname)
                    {
                        try
                        {
                            usersContext[userInTheLobby].StartClientGame();
                            usersContext.Remove(userInTheLobby);
                        }
                        catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                        {
                            log.Error(communicationObjectAbortedException);
                            User userToDisconnect = new User();
                            userToDisconnect.Nickname = userInTheLobby;
                            DisconnectLobby(userToDisconnect);
                        }
                    }
                }
            }
        }

        public void SendMessage(string message, string gameId)
        {
            if (usersInLobby.ContainsValue(gameId))
            {
                List<string> usersNickname = FindKeysByValue(usersInLobby, gameId);

                foreach (string userInTheLobby in usersNickname)
                {
                    try
                    {
                        usersContext[userInTheLobby].ReceiveMessage(message);
                    }
                    catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                    {
                        log.Error(communicationObjectAbortedException);
                        User userToDisconnect = new User();
                        userToDisconnect.Nickname = userInTheLobby;
                        DisconnectLobby(userToDisconnect);
                    }
                }
            }
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
            if (usersInLobby.ContainsKey(userNickname))
            {
                string gameId = usersInLobby[userNickname];

                if (usersContext.ContainsKey(userNickname))
                {
                    try
                    {
                        usersContext[userNickname].GetKicked();
                    }
                    catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                    {
                        log.Error(communicationObjectAbortedException);
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

        public bool IsBanned(string nickname)
        {
            GameAccess gameAccess = new GameAccess();
            bool banned = false;

            if (gameAccess.CanPlay(nickname) == VALIDATION_FAILURE)
            {
                 banned = true;
            }

            return banned;
        }
    }

    public partial class UserManager : IOnlineUserManager
    {
        private static Dictionary<string, IOnlineUserManagerCallback> onlineUsers = new Dictionary<string, IOnlineUserManagerCallback>();
        private const int IS_SUCCEDED = 0;
        private const bool ACCEPTED_FRIEND = true;

        [OperationBehavior]
        public void ConectUser(string nickname)
        {
            lock (onlineUsers)
            {
                IOnlineUserManagerCallback currentUserCallbackChannel = OperationContext.Current.GetCallbackChannel<IOnlineUserManagerCallback>();
                List<string> onlineNicknames = onlineUsers.Keys.ToList();
                FriendAccess friendAccess = new FriendAccess();

                try
                {
                    currentUserCallbackChannel.ShowOnlineFriends(friendAccess.GetFriendList(nickname, onlineUsers.Keys.ToList()));
                    if (!onlineUsers.ContainsKey(nickname))
                    {
                        onlineUsers.Add(nickname, currentUserCallbackChannel);

                        try
                        {
                            foreach (var user in onlineUsers)
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
                                    DisconectUser(user.Key);
                                    log.Error("Error in ConnectUser IOnlineUserManager\n" + communicationObjectAbortedException);
                                }
                            }
                        }
                        catch (InvalidOperationException invalidOperationException) 
                        {
                            log.Error(invalidOperationException);
                        }
                                             
                    }
                    else
                    {
                        onlineUsers[nickname] = currentUserCallbackChannel;
                    }
                }
                catch (SqlException sqlException)
                {
                    log.Error(sqlException);
                }
            }
        }

        public void DisconectUser(string nickname)
        {
            lock (onlineUsers)
            {
                if (onlineUsers.ContainsKey(nickname))
                {
                    onlineUsers.Remove(nickname);
                    try
                    {
                        foreach (var user in onlineUsers)
                        {
                            try
                            {
                                user.Value.ShowUserDisconected(nickname);
                            }
                            catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                            {
                                onlineUsers.Remove(user.Key);
                                log.Error("Error in DisconnectUser method ", communicationObjectAbortedException);
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

        public int SendFriendRequest(string nicknameSender, string nicknameReciever)
        {
            int isSucceded = VALIDATION_FAILURE;

            try
            {
                int findUserAnswer = FindUserByNickname(nicknameReciever);

                if (findUserAnswer == VALIDATION_SUCCESS)
                {
                    FriendAccess friendAccess = new FriendAccess();
                    if (friendAccess.SendFriendRequest(nicknameSender, nicknameReciever))
                    {
                        isSucceded = VALIDATION_SUCCESS;
                        if (onlineUsers.ContainsKey(nicknameReciever))
                        {
                            onlineUsers[nicknameReciever].ShowFriendRequest(nicknameSender);
                        }
                    }
                }
                else if(findUserAnswer == ERROR)
                {
                    isSucceded = ERROR;
                }
            }
            catch (SqlException sqlException)
            {
                log.Error(sqlException);
                isSucceded = ERROR;
            }
            catch (EntityException entityException)
            {
                log.Error(entityException);
                isSucceded = ERROR;
            }
            catch (CommunicationObjectAbortedException communicationObjectAbortedException)
            {
                log.Error(communicationObjectAbortedException);
                DisconectUser(nicknameReciever);
            }
            
            return isSucceded;
        }

        public int ReplyFriendRequest(string nicknameReciever, string nicknameSender, bool answer)
        {
            int isSucceded = VALIDATION_FAILURE;

            try
            {
                int findUserAnswer = FindUserByNickname(nicknameReciever);

                if (findUserAnswer == VALIDATION_SUCCESS)
                {
                    FriendAccess friendAccess = new FriendAccess();

                    if (friendAccess.ReplyFriendRequest(nicknameReciever, nicknameSender, answer) > IS_SUCCEDED)
                    {
                        isSucceded = VALIDATION_SUCCESS;

                        if (answer == ACCEPTED_FRIEND)
                        {
                            if (onlineUsers.ContainsKey(nicknameSender))
                            {
                                onlineUsers[nicknameSender].ShowFriendAccepted(nicknameReciever);
                            }
                        }
                    }
                }
                else if (findUserAnswer == ERROR)
                {
                    isSucceded = ERROR;
                }
            }
            catch (SqlException sqlException)
            {
                log.Error(sqlException);
                isSucceded = ERROR;
            }
            catch (EntityException entityException)
            {
                log.Error(entityException);
                isSucceded = ERROR;
            }
            catch (CommunicationObjectAbortedException communicationObjectAbortedException)
            {
                log.Error(communicationObjectAbortedException);
                DisconectUser(nicknameReciever);
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
            catch (SqlException sqlException)
            {
                log.Error(sqlException);
                isSucceded = ERROR;
            }
            catch (EntityException entityException)
            {
                log.Error(entityException);
                isSucceded = ERROR;
            }

            if (onlineUsers.ContainsKey(nicknamefriendToRemove))
            {
                try
                {
                    onlineUsers[nicknamefriendToRemove].FriendDeleted(nickname);
                }
                catch(CommunicationObjectAbortedException communicationObjectAbortedException) 
                {
                    log.Error(communicationObjectAbortedException);
                    onlineUsers.Remove(nicknamefriendToRemove);
                }
                
            }
            return isSucceded;
        }
    }

    public partial class UserManager : IGameManager
    {

        private readonly Random random = new Random();
        private static Dictionary<string, IGameManagerCallback> usersInGameContext = new Dictionary<string, IGameManagerCallback>();
        private const int GAME_ABORTED = 0;

        public void ConnectGame(string nickname)
        {
            if (usersInLobby.ContainsValue(usersInLobby[nickname]))
            {
                IGameManagerCallback currentUserCallbackChannel = OperationContext.Current.GetCallbackChannel<IGameManagerCallback>();

                if (!usersInGameContext.ContainsKey(nickname))
                {
                    usersInGameContext.Add(nickname, currentUserCallbackChannel);
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
                
                foreach (string userInGame in usersInGame.Keys)
                {
                    if (userInGame != nickname)
                    {
                        try
                        {
                            usersInGameContext[userInGame].ShowUserConnectedGame(nickname, usersTeam[nickname]);
                        }
                        catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                        {
                            GameAccess gameAccess = new GameAccess();
                            gameAccess.BanUser(userInGame);
                            log.Error("Error in ConnectGame " + communicationObjectAbortedException);
                            EndGame(GAME_ABORTED, nickname);
                        }
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
            catch(SqlException sqlException)
            {
                log.Error(sqlException);
            }

            List<int> shuffledDeck = userDeck.OrderBy(x => random.Next()).ToList();

            ChangeMultiple();

            return shuffledDeck;
        }

        public void DrawCard(string nickname, int [] cardsId)
        {
            ChangeSingle();

            Dictionary<string, int> usersInGame = GetUsersPerTeam(nickname);

            foreach (string userInGame in usersInGame.Keys)
            {
                if (userInGame != nickname && usersInGame[nickname] == usersInGame[userInGame])
                {
                    try
                    {
                        usersInGameContext[userInGame].DrawCardClient(nickname, cardsId);
                    }
                    catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                    {
                        GameAccess gameAccess = new GameAccess();
                        gameAccess.BanUser(userInGame);
                        log.Error("Error in DrawCard " + communicationObjectAbortedException);
                        EndGame(GAME_ABORTED, nickname);
                    }
                }
            }

            ChangeMultiple();
        }

        public void EndGame(int winnerTeam, string nickname)
        {
            Dictionary<string, int> usersInGame = GetUsersPerTeam(nickname);

            GameAccess gameAccess = new GameAccess();
            gameAccess.EndGame(winnerTeam, usersInLobby[nickname]);

            foreach (string userInGame in usersInGame.Keys)
            {
                try
                {
                    if (usersInGameContext.ContainsKey(userInGame))
                    {
                        usersInGameContext[userInGame].EndGameClient(winnerTeam);
                    }
                }
                catch (FaultException faultException)
                {
                    if (usersInGameContext.ContainsKey(userInGame))
                    {
                        usersInGameContext.Remove(userInGame);
                    }
                    log.Error(faultException.Message);
                }
                catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                {
                    GameAccess gameAccess = new GameAccess();
                    gameAccess.BanUser(userInGame);
                    log.Error(communicationObjectAbortedException);
                    usersInGameContext.Remove(userInGame);
                }
                catch (ObjectDisposedException objectDisposedException)
                {
                    log.Error(objectDisposedException);
                }
            } 
            
        }

        public void EndGameTurn(string nickname, Dictionary<int, int> boardAfterTurn)
        {
            Dictionary<string, int> usersInGame = GetUsersPerTeam(nickname);         

            foreach (string userInGame in usersInGame.Keys)
            {
                if(usersInGame[userInGame] == usersInGame[nickname])
                {
                    if(userInGame != nickname)
                    {
                        try
                        {
                            usersInGameContext[userInGame].PlayerEndedTurn(nickname, boardAfterTurn);
                        }
                        catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                        {
                            GameAccess gameAccess = new GameAccess();
                            gameAccess.BanUser(userInGame);
                            log.Error(communicationObjectAbortedException);
                            EndGame(GAME_ABORTED, nickname);
                        }
                    }
                }
                else
                {
                    Dictionary<int, int> reversedBoard = ReverseBoard(boardAfterTurn);
                    try
                    {
                        usersInGameContext[userInGame].PlayerEndedTurn(nickname, reversedBoard);
                    }
                    catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                    {
                        GameAccess gameAccess = new GameAccess();
                        gameAccess.BanUser(userInGame);
                        log.Error(communicationObjectAbortedException);
                        EndGame(GAME_ABORTED, nickname);
                    }
                    catch (FaultException faultException)
                    {
                        log.Error(faultException);
                        EndGame(GAME_ABORTED, nickname);
                    }
                }
            }
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
                        usersInGameContext[userInGame].ReceiveMessageGame(message);
                    }
                }
                catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                {
                    GameAccess gameAccess = new GameAccess();
                    gameAccess.BanUser(userInGame);
                    log.Error(communicationObjectAbortedException);
                    EndGame(GAME_ABORTED, nickname);
                }
            }
        }


        private Dictionary<string, int> GetUsersPerTeam(string nickname)
        {
            Dictionary<string, int> usersInGame = new Dictionary<string, int>();

            if (usersInLobby.ContainsKey(nickname))
            {
                List<string> usersNickname = FindKeysByValue(usersInLobby, usersInLobby[nickname]);

                foreach (string userNickname in usersNickname)
                {
                    if (usersInGameContext.ContainsKey(userNickname))
                    {
                        usersInGame.Add(userNickname, usersTeam[userNickname]);
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
                    usersInGameContext[userInGame].StartFirstPhaseClient(firstPlayers);
                }
                catch (CommunicationObjectAbortedException communicationObjectAbortedException)
                {
                    GameAccess gameAccess = new GameAccess();
                    gameAccess.BanUser(userInGame);
                    log.Error(communicationObjectAbortedException);
                    EndGame(GAME_ABORTED, userInGame);
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
            UserAccess userAccess = new UserAccess();
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
            }
        }

        public void GameEnded(string nickname)
        {
            RemoveUser(nickname);
        }

        private void RemoveUser(string nickname)
        {
            if(usersInGameContext.ContainsKey(nickname))
            {
                usersInGameContext.Remove(nickname);
            }

            if (usersInLobby.ContainsKey(nickname))
            {
                usersInLobby.Remove(nickname);
            }

            if (usersTeam.ContainsKey(nickname))
            {
                usersTeam.Remove(nickname);
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
            catch (SqlException sqlException)
            {
                log.Error(sqlException);
            }
            catch (EntityException entityException)
            {
                log.Error(entityException);
            }

            return listOfGamers;
        }
    }

}