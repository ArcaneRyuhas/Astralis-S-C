using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using DataAccessProject.Contracts;
using DataAccessProject.DataAccess;
using User = DataAccessProject.Contracts.User;
using DataAccessProject;

namespace MessageService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]

    public partial class UserManager : IUserManager
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(UserManager));

        public int ConfirmUser(string nickname, string password)
        {
            int result = 0;

            if (FindUserByNickname(nickname))
            {
                UserAccess userAccess = new UserAccess();
                result = userAccess.ConfirmUser(nickname, password);
            }

            return result;
        }

        public int AddUser(User user)
        {
            int result = 0;

            UserAccess userAccess = new UserAccess();
            result = userAccess.CreateUser(user);

            return result;
        }

        public bool FindUserByNickname(string nickname)
        {
            bool isFound = false;

            UserAccess userAccess = new UserAccess();
            isFound = userAccess.FindUserByNickname(nickname);

            return isFound;
        }

        public User GetUserByNickname(string nickname)
        {
            User foundUser = new User();
            
            UserAccess userAccess = new UserAccess();
            foundUser = userAccess.GetUserByNickname(nickname);

            return foundUser;
        }

        public int UpdateUser(User user)
        {
            int result = 0;

            UserAccess userAccess = new UserAccess();
            result = userAccess.UpdateUser(user);

            return result;

        }
    }

    public partial class UserManager : ILobbyManager
    {
        private const bool IS_SUCCESFULL = true;
        private const string ERROR_CODE_LOBBY = "error";
        private const int NO_TEAM = 0;

        private static Dictionary<string, string> usersInLobby = new Dictionary<string, string>();
        private static Dictionary<string, ILobbyManagerCallback> usersContext = new Dictionary<string, ILobbyManagerCallback>();
        private static Dictionary<string, int> usersTeam = new Dictionary<string, int>();

        public bool GameExist(string gameId)
        {
            return usersInLobby.ContainsValue(gameId);
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

        public void ConnectLobby(User user, string gameId)
        {
            if (usersInLobby.ContainsValue(gameId))
            {
                List<string> usersNickname = FindKeysByValue(usersInLobby, gameId);
                List<Tuple<User, int>> users = new List<Tuple<User, int>>();

                foreach (string nickname in usersNickname)
                {
                    users.Add(new Tuple <User, int> (GetUserByNickname(nickname), usersTeam[nickname]));
                }

                ILobbyManagerCallback currentUserCallbackChannel = OperationContext.Current.GetCallbackChannel<ILobbyManagerCallback>();
                currentUserCallbackChannel.ShowUsersInLobby(users);

                if (!usersContext.ContainsKey(user.Nickname))
                {
                    usersContext.Add(user.Nickname, currentUserCallbackChannel);
                    usersInLobby.Add(user.Nickname, gameId);
                    usersTeam.Add(user.Nickname, NO_TEAM);
                }

                foreach (string userInTheLobby in usersNickname)
                {
                    usersContext[userInTheLobby].ShowConnectionInLobby(user);
                }
            }
        }

        public string CreateLobby(User user)
        {
            string gameId = GenerateGameId();
            GameAccess gameAccess = new GameAccess();

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
                gameId = ERROR_CODE_LOBBY;
            }

            return gameId;
        }

        public void DisconnectLobby(User user)
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
                usersTeam.Remove(user.Nickname);
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
                        usersContext[userInTheLobby].UpdateLobbyUserTeam(userNickname, team);
                    }
                }
            }
        }

        public void StartGame(string gameId)
        {

            List<string> usersNickname = FindKeysByValue(usersInLobby, gameId);
            int result = 0;

            if (usersInLobby.ContainsValue(gameId))
            {
                GameAccess gameAccess = new GameAccess();

                foreach (var user in usersNickname)
                {
                    result =+ gameAccess.CreatePlaysRelation(user, gameId, usersTeam[user]);

                }

                if (result > 0)
                {
                    foreach (string userInTheLobby in usersNickname)
                    {
                        usersContext[userInTheLobby].StartClientGame();
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
                    usersContext[userInTheLobby].ReceiveMessage(message);
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

    }

    public partial class UserManager : IOnlineUserManager
    {
        private static Dictionary<string, IOnlineUserManagerCallback> onlineUsers = new Dictionary<string, IOnlineUserManagerCallback>();
        private const int IS_FRIEND = 1;
        private const int IS_PENDING_FRIEND = 2;
        private const bool ONLINE = true;
        private const bool OFFLINE = false;
        private const int IS_SUCCEDED = 0;
        private const bool ACCEPTED_FRIEND = true;

        [OperationBehavior]
        public void ConectUser(string nickname)
        {
            if (!onlineUsers.ContainsKey(nickname))
            {
                IOnlineUserManagerCallback currentUserCallbackChannel = OperationContext.Current.GetCallbackChannel<IOnlineUserManagerCallback>();
                List<string> onlineNicknames = onlineUsers.Keys.ToList();
                FriendAccess friendAccess = new FriendAccess();

                currentUserCallbackChannel.ShowOnlineFriends(friendAccess.GetFriendList(nickname, onlineUsers.Keys.ToList()));

                try
                {
                    foreach (var user in onlineUsers)
                    {
                        user.Value.ShowUserConected(nickname);
                    }
                }
                catch (CommunicationObjectAbortedException exception)
                {
                    log.Error(exception);
                }

                onlineUsers.Add(nickname, currentUserCallbackChannel);

            }
        }

        public void DisconectUser(string nickname)
        {
            if (onlineUsers.ContainsKey(nickname))
            {
                onlineUsers.Remove(nickname);

                try
                {
                    foreach (var user in onlineUsers)
                    {
                        user.Value.ShowUserDisconected(nickname);
                    }
                }
                catch(CommunicationObjectAbortedException exception) 
                {
                    log.Error("Error in DisconnectUser method ", exception);
                }
            }
        }

        public bool SendFriendRequest(string nicknameSender, string nicknameReciever) //Se puede cambiar el retorno a un int para saber 0.- NO EXISITE EL USUARIO 1.- Exitoso 2.-Ya existe la solicitud o son amigos
        {
            bool isSucceded = false;

            if (FindUserByNickname(nicknameSender) && FindUserByNickname(nicknameReciever))
            {
                FriendAccess friendAccess = new FriendAccess();
                if (friendAccess.SendFriendRequest(nicknameSender, nicknameReciever))
                {
                    isSucceded = true;
                    if (onlineUsers.ContainsKey(nicknameReciever))
                    {
                        onlineUsers[nicknameReciever].ShowFriendRequest(nicknameSender);
                    }
                }
            }
            return isSucceded;
        }

        public bool ReplyFriendRequest(string nicknameReciever, string nicknameSender, bool answer)
        {
            bool IsSucceded = false;

            if (FindUserByNickname(nicknameReciever) && FindUserByNickname(nicknameSender))
            {
                FriendAccess friendAccess = new FriendAccess();

                if (friendAccess.ReplyFriendRequest(nicknameReciever, nicknameSender, answer) > IS_SUCCEDED)
                {
                    IsSucceded = true;

                    if (answer == ACCEPTED_FRIEND)
                    {
                        if (onlineUsers.ContainsKey(nicknameSender))
                        {
                            onlineUsers[nicknameSender].ShowFriendAccepted(nicknameReciever);
                        }
                    }
                }
            }
            
            return IsSucceded;
        }

        public bool RemoveFriend(string nickname, string nicknamefriendToRemove)
        {
            bool isSucceded = false;

            FriendAccess friendAccess = new FriendAccess();
            isSucceded = friendAccess.RemoveFriend(nickname, nicknamefriendToRemove);

            return isSucceded;
        }

    }

    public partial class UserManager : IGameManager
    {

        private static Dictionary<string, IGameManagerCallback> usersInGameContext = new Dictionary<string, IGameManagerCallback>();

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
                currentUserCallbackChannel.ShowUsersInGame(usersInGame);

                foreach (string userInGame in usersInGame.Keys)
                {
                    if (userInGame != nickname)
                    {
                        usersInGameContext[userInGame].ShowUserConnectedGame(nickname, usersTeam[nickname]);
                    }
                }
            }
        }

        public List <int> DispenseCards(string nickname)
        {
            DeckAccess deckAccess = new DeckAccess();
            List <int> userDeck = deckAccess.GetDeckByNickname(nickname);
            Random random = new Random();

            List<int> shuffledDeck = userDeck.OrderBy(x => random.Next()).ToList();

            return shuffledDeck;
        }

        public void DrawCard(string nickname, int cardId)
        {
            Dictionary<string, int> usersInGame = GetUsersPerTeam(nickname);

            foreach (string userInGame in usersInGame.Keys)
            {
                if(userInGame != nickname && usersInGame[nickname] == usersInGame[userInGame])
                {
                  usersInGameContext[userInGame].DrawCardClient(nickname, cardId);
                }
            }
        }

        public void EndGame(int winnerTeam)
        {
            throw new NotImplementedException();
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
                        usersInGameContext[userInGame].PlayerEndedTurn(nickname, boardAfterTurn);
                    }
                }
                else
                {
                    boardAfterTurn = ReverseBoard(boardAfterTurn);//REGRESAR AL ELSE
                    usersInGameContext[userInGame].PlayerEndedTurn(nickname, boardAfterTurn);
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



        private Dictionary<string, int> GetUsersPerTeam(string nickname)
        {
            List<string> usersNickname = FindKeysByValue(usersInLobby, usersInLobby[nickname]);
            Dictionary<string, int> usersInGame = new Dictionary<string, int>();

            foreach (string userNickname in usersNickname)
            {
                if (usersInGameContext.ContainsKey(userNickname))
                {
                    usersInGame.Add(userNickname, usersTeam[userNickname]);
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
                usersInGameContext[userInGame].StartFirstPhaseClient(firstPlayers);
            }
        }
    }
}