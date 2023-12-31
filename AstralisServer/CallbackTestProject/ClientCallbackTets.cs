﻿using CallbackTestProject.AstralisServer;
using DataAccessProject.DataAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace CallbackTestProject
{
    [TestClass]
    public class CallbackLobbyTests
    {
        private static LobbyManagerClient _firstClient;
        private static LobbyManagerClient _secondClient;
        private static LobbyManagerClient _thirdClient;
        private static LobbyManagerClient _fourthClient;
        private static LobbyCallbackImplementation _firstCallback;
        private static LobbyCallbackImplementation _secondCallback;
        private static LobbyCallbackImplementation _thirdCallback;
        private static LobbyCallbackImplementation _fourthCallback;

        private static string _gameId = string.Empty;
        private static string _otherGameId = string.Empty;

        private static User FIRST_USER = new User()
        {
            Nickname = "FirstTester",
            ImageId = 1
        };

        private static User LAST_USER = new User()
        {
            Nickname = "Last_Tester",
            ImageId = 2
        };

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            _firstCallback = new LobbyCallbackImplementation();
            _firstClient = new LobbyManagerClient(new InstanceContext(_firstCallback));

            _gameId = _firstClient.CreateLobby(FIRST_USER);

            _secondCallback = new LobbyCallbackImplementation();
            _secondClient = new LobbyManagerClient(new InstanceContext(_secondCallback));

            _thirdCallback = new LobbyCallbackImplementation();
            _thirdClient = new LobbyManagerClient(new InstanceContext(_thirdCallback));

            _fourthCallback = new LobbyCallbackImplementation();
            _fourthClient = new LobbyManagerClient(new InstanceContext(_fourthCallback));

            _otherGameId = _fourthClient.CreateLobby(LAST_USER);
        }

        [TestMethod]
        public async Task ConnectToServerSuccesful()
        {
            User secondUser = new User()
            {
                Nickname = "SecondUser",
                ImageId = 1
            };

            if (_secondClient.GameExist(_gameId))
            {
                _secondClient.ConnectLobby(secondUser, _gameId);
            }

            await Task.Delay(5000);
            _secondClient.DisconnectLobby(secondUser);
            Assert.AreEqual(secondUser.Nickname, _firstCallback.ConnectionInLobby);
        }

        [TestMethod]
        public async Task ShowUsersInLobbySuccessful()
        {
            User secondUser = new User()
            {
                Nickname = "SecondUser",
                ImageId = 1
            };

            if (_secondClient.GameExist(_gameId))
            {
                _secondClient.ConnectLobby(secondUser, _gameId);
            }

            await Task.Delay(2000);
            _secondClient.DisconnectLobby(secondUser);
            Assert.IsNotNull(_secondCallback.ConnectedToLobby);
        }

        [TestMethod]
        public async Task DisconnectionSuccesful()
        {
            User secondUser = new User()
            {
                Nickname = "SecondUser",
                ImageId = 1
            };

            if (_secondClient.GameExist(_gameId))
            {
                _secondClient.ConnectLobby(secondUser, _gameId);
            }

            await Task.Delay(5000);
            _secondClient.DisconnectLobby(secondUser);

            await Task.Delay(2000);
            Assert.AreEqual(secondUser.Nickname, _firstCallback.DisconnectionInLobby);
        }

        [TestMethod]
        public async Task UpdateLobbyUserTeam()
        {
            User secondUser = new User()
            {
                Nickname = "SecondUser",
                ImageId = 1
            };

            if (_secondClient.GameExist(_gameId))
            {
                _secondClient.ConnectLobby(secondUser, _gameId);
            }

            await Task.Delay(2000);

            _secondClient.ChangeLobbyUserTeam(secondUser.Nickname, 1);

            await Task.Delay(2000);
            Assert.AreEqual(secondUser.Nickname, _firstCallback.NicknameTeamChange);
            Assert.AreEqual(1, _firstCallback.TeamChanged);
        }

        [TestMethod]
        public async Task SendMessageLobbySuccesfull()
        {
            User secondUser = new User()
            {
                Nickname = "SecondUser",
                ImageId = 1
            };

            if (_secondClient.GameExist(_gameId))
            {
                _secondClient.ConnectLobby(secondUser, _gameId);
            }

            string message = "Message";

            await Task.Delay(2000);

            _secondClient.SendMessage(message, _gameId);

            await Task.Delay(2000);
            Assert.AreEqual(message, _firstCallback.Message);
        }

        [TestMethod]
        public async Task KickedSuccesfull()
        {
            User secondUser = new User()
            {
                Nickname = "SecondUser",
                ImageId = 1
            };

            if (_secondClient.GameExist(_gameId))
            {
                _secondClient.ConnectLobby(secondUser, _gameId);
            }

            await Task.Delay(2000);

            _firstClient.KickUser(secondUser.Nickname);

            await Task.Delay(2000);
            Assert.IsTrue(_secondCallback.BeenKicked);
        }

        [TestMethod]
        public async Task StartGameSuccesfull()
        {
            User secondUser = new User()
            {
                Nickname = "SecondUser",
                ImageId = 1
            };

            if (_secondClient.GameExist(_gameId))
            {
                _secondClient.ConnectLobby(secondUser, _gameId);
            }

            await Task.Delay(2000);
            _firstClient.ChangeLobbyUserTeam(FIRST_USER.Nickname, 1);
            await Task.Delay(2000);
            _secondClient.ChangeLobbyUserTeam(secondUser.Nickname, 2);
            await Task.Delay(2000);
            _firstClient.StartGame(FIRST_USER.Nickname);
            await Task.Delay(2000);
            Assert.IsTrue(_firstCallback.GameStarted);
        }

        [TestMethod]
        public async Task ConnectToServerUnsuccesful()
        {
            User secondUser = new User()
            {
                Nickname = "SecondUser",
                ImageId = 1
            };

            if (_secondClient.GameExist(_gameId))
            {
                _secondClient.ConnectLobby(secondUser, _gameId);
            }

            await Task.Delay(5000);
            _secondClient.DisconnectLobby(secondUser);
            Assert.AreNotEqual(secondUser.Nickname, _fourthCallback.ConnectionInLobby);
        }

        [TestMethod]
        public async Task DisconnectionUnsuccesful()
        {
            User thirdUser = new User()
            {
                Nickname = "ThirdUser",
                ImageId = 1
            };

            if (_thirdClient.GameExist(_otherGameId))
            {
                _thirdClient.ConnectLobby(thirdUser, _otherGameId);
            }

            await Task.Delay(5000);
            _thirdClient.DisconnectLobby(thirdUser);

            await Task.Delay(2000);
            Assert.AreNotEqual(thirdUser.Nickname, _firstCallback.DisconnectionInLobby);
        }

        [TestMethod]
        public async Task UpdateLobbyUserTeamUnsuccesful()
        {
            User thirdUser = new User()
            {
                Nickname = "ThirdUser",
                ImageId = 1
            };

            if (_thirdClient.GameExist(_otherGameId))
            {
                _thirdClient.ConnectLobby(thirdUser, _otherGameId);
            }
            await Task.Delay(2000);

            _thirdClient.ChangeLobbyUserTeam(thirdUser.Nickname, 2);
            await Task.Delay(2000);

            _thirdClient.DisconnectLobby(thirdUser);
            await Task.Delay(2000);

            Assert.AreNotEqual(thirdUser.Nickname, _firstCallback.NicknameTeamChange);
            Assert.AreNotEqual(2, _firstCallback.TeamChanged);

            
        }

        [TestMethod]
        public async Task SendMessageLobbyUnsuccesfull()
        {
            User thirdUser = new User()
            {
                Nickname = "ThirdUser",
                ImageId = 1
            };

            if (_thirdClient.GameExist(_otherGameId))
            {
                _thirdClient.ConnectLobby(thirdUser, _otherGameId);
            }

            string message = "MessageError";

            await Task.Delay(2000);

            _thirdClient.SendMessage(message, _otherGameId);

            await Task.Delay(2000);
            _thirdClient.DisconnectLobby(thirdUser);

            await Task.Delay(2000);
            Assert.AreNotEqual(message, _firstCallback.Message);
        }

        [TestMethod]
        public async Task KickedUnsuccesfull()
        {
            User thirdUser = new User()
            {
                Nickname = "ThirdUser",
                ImageId = 1
            };

            _firstClient.KickUser(thirdUser.Nickname);

            await Task.Delay(2000);
            Assert.IsFalse(_thirdCallback.BeenKicked);
        }


        [ClassCleanup]
        public static void Cleanup()
        {
            _firstClient.DisconnectLobby(FIRST_USER);
            _fourthClient.DisconnectLobby(LAST_USER);
        }
    }

    public class LobbyCallbackImplementation : ILobbyManagerCallback
    {
        public bool BeenKicked { get; set; }

        public Tuple<User, int>[] ConnectedToLobby { get; set; }

        public string ConnectionInLobby { get; set; }

        public string DisconnectionInLobby { get; set; }

        public string NicknameTeamChange { get; set; }

        public int TeamChanged { get; set; }

        public string Message { get; set; }

        public bool GameStarted { get; set; }

        public LobbyCallbackImplementation()
        {
            BeenKicked = false;
            ConnectedToLobby = null;
            NicknameTeamChange = string.Empty;
            ConnectionInLobby = string.Empty;
            DisconnectionInLobby = string.Empty;
            TeamChanged = 0;
            Message = string.Empty;
            GameStarted = false;
        }

        public void ShowConnectionInLobby(User user)
        {
            ConnectionInLobby = user.Nickname;
        }

        public void ShowUsersInLobby(Tuple<User, int>[] users)
        {
            ConnectedToLobby = users;
        }

        public void ShowDisconnectionInLobby(User user)
        {
            DisconnectionInLobby = user.Nickname;
        }

        public void UpdateLobbyUserTeam(string userNickname, int team)
        {
            NicknameTeamChange = userNickname;
            TeamChanged = team;
        }

        public void ReceiveMessage(string message)
        {
            Message = message;
        }

        public void StartClientGame()
        {
            GameStarted = true;
        }

        public void GetKicked()
        {
            BeenKicked = true;
        }
    }

    [TestClass]
    public class CallbackOnlineUsersTests
    {
        private static OnlineUserManagerClient _firstClient;
        private static OnlineUserManagerClient _secondClient;
        private static OnlineUserManagerClient _thirdClient;
        private static OnlineUsersCallbackImplementation _firstCallback;
        private static OnlineUsersCallbackImplementation _secondCallback;
        private static OnlineUsersCallbackImplementation _thirdCallback;
        private static UserAccess userAccess = new UserAccess();

        private readonly static string FIRST_USER = "FirstUser";
        private const string SECOND_USER = "SecondUser";
        private const string THIRD_USER = "ThirdUser";
        private const int ERROR = 0;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            DataAccessProject.Contracts.User firstUser = new DataAccessProject.Contracts.User()
            {
                Nickname = FIRST_USER,
                ImageId = 1,
                Password = "password",
                Mail = "m@a.com"
            };

            DataAccessProject.Contracts.User secondUser = new DataAccessProject.Contracts.User()
            {
                Nickname = SECOND_USER,
                ImageId = 1,
                Password = "password",
                Mail = "A@a.com"
            };

            userAccess.CreateUser(firstUser);

            userAccess.CreateUser(secondUser);

            _firstCallback = new OnlineUsersCallbackImplementation();
            _firstClient = new OnlineUserManagerClient(new InstanceContext(_firstCallback));

            _firstClient.ConectUser(FIRST_USER);

            _secondCallback = new OnlineUsersCallbackImplementation();
            _secondClient = new OnlineUserManagerClient(new InstanceContext(_secondCallback));

            _thirdCallback = new OnlineUsersCallbackImplementation();
            _thirdClient = new OnlineUserManagerClient(new InstanceContext(_thirdCallback));
        }

        [TestMethod]
        public async Task ShowConnectionToGameSuccesful()
        {
            _secondClient.ConectUser(SECOND_USER);
            await Task.Delay(2000);

            _secondClient.DisconectUser(SECOND_USER);
            Assert.AreEqual(SECOND_USER, _firstCallback.UserConnected);
        }

        [TestMethod]
        public async Task ShowDisconnectionInGameSuccesful()
        {
            _secondClient.ConectUser(SECOND_USER);
            await Task.Delay(2000);
            _secondClient.DisconectUser(SECOND_USER);
            await Task.Delay(2000);
            Assert.AreEqual(SECOND_USER, _firstCallback.UserDisconnected);
        }

        [TestMethod]
        public async Task ShowFriendRequestSuccesful()
        {
            _secondClient.ConectUser(SECOND_USER);
            _secondClient.SendFriendRequest(SECOND_USER, FIRST_USER);

            await Task.Delay(2000);

            Assert.AreEqual(SECOND_USER, _firstCallback.FriendRequest);
        }


        [TestMethod]
        public async Task ShowFriendRequestUnsuccesful()
        {
            _secondClient.ConectUser(SECOND_USER);

            await Task.Delay(2000);

            Assert.AreEqual(ERROR, _secondClient.SendFriendRequest(SECOND_USER, THIRD_USER));
            Assert.IsNull(_thirdCallback.FriendRequest);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            _firstClient.DisconectUser(FIRST_USER);

            userAccess.DeleteUser(FIRST_USER);
            userAccess.DeleteUser(SECOND_USER);
        }
    }

    public class OnlineUsersCallbackImplementation : IOnlineUserManagerCallback
    {
        public string FriendRemoved { get; set; }
        public string FriendAccepted { get; set; }
        public string FriendRequest { get; set; }
        public Dictionary<string, Tuple<bool, int>> Friends { get; set; }
        public string UserConnected { get; set; }
        public string UserDisconnected { get; set; }

        public OnlineUsersCallbackImplementation()
        {
            FriendRemoved = string.Empty;
            FriendAccepted = string.Empty;
            FriendRequest = null;
            Friends = null;
            UserConnected = string.Empty;
            UserDisconnected = string.Empty;
        }

        public void FriendDeleted(string nickname)
        {
            FriendRemoved = nickname;
        }

        public void ShowFriendAccepted(string nickname)
        {
            FriendAccepted = nickname;
        }

        public void ShowFriendRequest(string nickname)
        {
            FriendRequest = nickname;
        }

        public void ShowOnlineFriends(Dictionary<string, Tuple<bool, int>> onlineFriends)
        {
            Friends = onlineFriends;
        }

        public void ShowUserConected(string nickname)
        {
            UserConnected = nickname;
        }

        public void ShowUserDisconected(string nickname)
        {
            UserDisconnected = nickname;
        }
    }
}
