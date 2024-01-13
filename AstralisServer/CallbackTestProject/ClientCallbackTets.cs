using CallbackTestProject.AstralisService;
using DataAccessProject.Contracts;
using DataAccessProject.DataAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel;
using System.Threading.Tasks;
using IFriendManagerCallback = CallbackTestProject.AstralisService.IFriendManagerCallback;
using ILobbyManagerCallback = CallbackTestProject.AstralisService.ILobbyManagerCallback;

namespace CallbackTestProject
{

    [TestClass]
    public class CallbackLobbyNotFullTests
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

        private static User SECOND_USER = new User()
        {
            Nickname = "SecondTester",
            ImageId = 2
        };

        private static User THIRD_USER = new User()
        {
            Nickname = "ThirdTester",
            ImageId = 3
        };

        private static User FOURTH_USER = new User()
        {
            Nickname = "FourthTester",
            ImageId = 4
        };

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            GetConnectionString();
        }

        public static void GetConnectionString()
        {
            string connectionString = Environment.GetEnvironmentVariable("ASTRALIS");
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var connectionStringSection = config.ConnectionStrings.ConnectionStrings["AstralisDBEntities"];

            if (connectionStringSection != null)
            {
                connectionStringSection.ConnectionString = connectionString;

                config.Save(ConfigurationSaveMode.Modified);

                ConfigurationManager.RefreshSection("connectionStrings");

            }
        }

        [TestInitialize]
        public async Task Initialize()
        {
            _firstCallback = new LobbyCallbackImplementation();
            _firstClient = new LobbyManagerClient(new InstanceContext(_firstCallback));

            _gameId = _firstClient.CreateLobby(FIRST_USER);

            _secondCallback = new LobbyCallbackImplementation();
            _secondClient = new LobbyManagerClient(new InstanceContext(_secondCallback));

            await Task.Delay(1000);
            _secondClient.ConnectToLobby(SECOND_USER, _gameId);

            _thirdCallback = new LobbyCallbackImplementation();
            _thirdClient = new LobbyManagerClient(new InstanceContext(_thirdCallback));

            await Task.Delay(1000);
            _otherGameId = _thirdClient.CreateLobby(THIRD_USER);

            _fourthCallback = new LobbyCallbackImplementation();
            _fourthClient = new LobbyManagerClient(new InstanceContext(_fourthCallback));
        }

        [TestCleanup]
        public void TestCleaunp()
        {
            _firstClient.DisconnectFromLobby(FIRST_USER);
            _secondClient.DisconnectFromLobby(SECOND_USER);
            _thirdClient.DisconnectFromLobby(THIRD_USER);
            _fourthClient.DisconnectFromLobby(FOURTH_USER);

            GameAccess gameAccess = new GameAccess();

            gameAccess.CleanupGame(_gameId);
            gameAccess.CleanupGame(_otherGameId);
        }

        [TestMethod]
        public async Task ShowConnectionToLobbySuccesful()
        {
            _fourthClient.ConnectToLobby(FOURTH_USER, _gameId);
            await Task.Delay(5000);
            Assert.AreEqual(FOURTH_USER.Nickname, _firstCallback.ConnectionInLobby);
            Assert.AreEqual(FOURTH_USER.Nickname, _secondCallback.ConnectionInLobby);
        }

        [TestMethod]
        public async Task ConnectToLobbyUnsuccesful()
        {
            string gameNotCreated = "gameNotCreated";

            _fourthClient.ConnectToLobby(FOURTH_USER, gameNotCreated);
            await Task.Delay(5000);
            Assert.AreNotEqual(FOURTH_USER.Nickname, _firstCallback.ConnectionInLobby);
            Assert.AreNotEqual(FOURTH_USER.Nickname, _secondCallback.ConnectionInLobby);
        }

        [TestMethod]
        public async Task NoShowOfConnectionInDifferentLobby()
        {
            _fourthClient.ConnectToLobby(FOURTH_USER, _gameId);
            await Task.Delay(5000);
            Assert.AreNotEqual(FOURTH_USER.Nickname, _thirdCallback.ConnectionInLobby);
        }

        [TestMethod]
        public async Task ShowUsersInLobbySuccessful()
        {
            _fourthClient.ConnectToLobby(FOURTH_USER, _gameId);
            await Task.Delay(2000);
            Assert.IsNotNull(_fourthCallback.ConnectedToLobby);
        }

        [TestMethod]
        public async Task ShowUsersInLobbyInNewLobby()
        {
            _fourthClient.CreateLobby(FOURTH_USER);
            await Task.Delay(2000);
            Assert.IsNull(_fourthCallback.ConnectedToLobby);
        }

        [TestMethod]
        public async Task ShowDisconnectionFromLobbySuccesful()
        {
            _secondClient.DisconnectFromLobby(SECOND_USER);
            await Task.Delay(2000);
            Assert.AreEqual(SECOND_USER.Nickname, _firstCallback.DisconnectionInLobby);
        }

        [TestMethod]
        public async Task DisconnectionFromLobbyUnsuccesful()
        {
            _fourthClient.DisconnectFromLobby(FOURTH_USER);
            await Task.Delay(2000);
            Assert.AreNotEqual(FOURTH_USER.Nickname, _firstCallback.DisconnectionInLobby);
            Assert.AreNotEqual(FOURTH_USER.Nickname, _secondCallback.DisconnectionInLobby);
        }

        [TestMethod]
        public async Task NoShowDisconnectionFromOtherLobby()
        {
            _thirdClient.DisconnectFromLobby(THIRD_USER);

            await Task.Delay(2000);
            Assert.AreNotEqual(THIRD_USER.Nickname, _firstCallback.DisconnectionInLobby);
            Assert.AreNotEqual(THIRD_USER.Nickname, _secondCallback.DisconnectionInLobby);
        }


        [TestMethod]
        public async Task KickedSuccesfullFromLobby()
        {
            _firstClient.KickUserFromLobby(SECOND_USER.Nickname);
            await Task.Delay(2000);
            Assert.IsTrue(_secondCallback.BeenKicked);
            Assert.AreEqual(SECOND_USER.Nickname, _firstCallback.DisconnectionInLobby);
        }


        [TestMethod]
        public async Task KickedUnsuccesfullFromLobby()
        {

            _firstClient.KickUserFromLobby(FOURTH_USER.Nickname);
            await Task.Delay(2000);
            Assert.IsFalse(_fourthCallback.BeenKicked);
        }


        [TestMethod]
        public async Task SendUsersFromGameToLobbyUnsuccesful()
        {
            _firstClient.SendUsersFromLobbyToGame(FIRST_USER.Nickname);
            await Task.Delay(2000);
            Assert.IsFalse(_firstCallback.GameStarted);
        }

    }

    [TestClass]
    public class CallbackFullLobbyTests
    {
        private static LobbyManagerClient _firstClient;
        private static LobbyManagerClient _secondClient;
        private static LobbyManagerClient _thirdClient;
        private static LobbyManagerClient _fourthClient;
        private static LobbyManagerClient _fifthClient;
        private static LobbyCallbackImplementation _firstCallback;
        private static LobbyCallbackImplementation _secondCallback;
        private static LobbyCallbackImplementation _thirdCallback;
        private static LobbyCallbackImplementation _fourthCallback;
        private static LobbyCallbackImplementation _fifthCallback;

        private const int FIRST_TEAM = 1;

        private static string _gameId = string.Empty;
        private static string _otherGameId = string.Empty;

        private static User FIRST_USER = new User()
        {
            Nickname = "FirstTester",
            ImageId = 1
        };

        private static User SECOND_USER = new User()
        {
            Nickname = "SecondTester",
            ImageId = 2
        };

        private static User THIRD_USER = new User()
        {
            Nickname = "ThirdTester",
            ImageId = 3
        };

        private static User FOURTH_USER = new User()
        {
            Nickname = "FourthTester",
            ImageId = 4
        };

        private static User FIFTH_USER = new User()
        {
            Nickname = "FifthTester",
            ImageId = 2
        };

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            GetConnectionString();
        }

        public static void GetConnectionString()
        {
            string connectionString = Environment.GetEnvironmentVariable("ASTRALIS");
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var connectionStringSection = config.ConnectionStrings.ConnectionStrings["AstralisDBEntities"];

            if (connectionStringSection != null)
            {
                connectionStringSection.ConnectionString = connectionString;

                config.Save(ConfigurationSaveMode.Modified);

                ConfigurationManager.RefreshSection("connectionStrings");

            }
        }

        [TestInitialize]
        public async Task Initialize()
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

            _fifthCallback = new LobbyCallbackImplementation();
            _fifthClient = new LobbyManagerClient(new InstanceContext(_fourthCallback));

            await Task.Delay(2000);
            _secondClient.ConnectToLobby(SECOND_USER, _gameId);
            await Task.Delay(2000);
            _thirdClient.ConnectToLobby(THIRD_USER, _gameId);
            await Task.Delay(2000);
            _fourthClient.ConnectToLobby(FOURTH_USER, _gameId);
            await Task.Delay(2000);

            _otherGameId = _fourthClient.CreateLobby(FIFTH_USER);
        }

        [TestMethod]
        public async Task ShowUpdateLobbyUserTeam()
        {
            _secondClient.ChangeLobbyUserTeam(SECOND_USER.Nickname, FIRST_TEAM);
            await Task.Delay(10000);
            Assert.AreEqual(SECOND_USER.Nickname, _firstCallback.NicknameTeamChange);
            Assert.AreEqual(FIRST_TEAM, _firstCallback.TeamChanged);
            Assert.AreEqual(SECOND_USER.Nickname, _thirdCallback.NicknameTeamChange);
            Assert.AreEqual(FIRST_TEAM, _thirdCallback.TeamChanged);
            Assert.AreEqual(SECOND_USER.Nickname, _fourthCallback.NicknameTeamChange);
            Assert.AreEqual(FIRST_TEAM, _fourthCallback.TeamChanged);
        }

        [TestMethod]
        public async Task NoShowUpdateLobbyUserTeamFromOtherGame()
        {
            await Task.Delay(2000);
            _secondClient.ChangeLobbyUserTeam(SECOND_USER.Nickname, FIRST_TEAM);
            await Task.Delay(4000);
            Assert.AreNotEqual(SECOND_USER.Nickname, _fifthCallback.NicknameTeamChange);
            Assert.AreNotEqual(FIRST_TEAM, _fifthCallback.TeamChanged);
        }

        [TestMethod]
        public async Task ShowSendMessageToLobbySuccesfull()
        {
            string message = "SecondTester: Message";

            _secondClient.SendMessage(message, SECOND_USER.Nickname);
            await Task.Delay(5000);
            Assert.AreEqual(message, _firstCallback.Message);
            Assert.AreEqual(message, _thirdCallback.Message);
            Assert.AreEqual(message, _fourthCallback.Message);
        }

        [TestMethod]
        public async Task NoShowSendMessageToOtherLobby()
        {
            string message = "SecondTester: OtherLobbyMessage";

            _secondClient.SendMessage(message, SECOND_USER.Nickname);
            await Task.Delay(4000);
            Assert.AreNotEqual(message, _fifthCallback.Message);
        }


        [TestCleanup]
        public void TestCleaunp()
        {
            _firstClient.DisconnectFromLobby(FIRST_USER);
            _secondClient.DisconnectFromLobby(SECOND_USER);
            _thirdClient.DisconnectFromLobby(THIRD_USER);
            _fourthClient.DisconnectFromLobby(FOURTH_USER);
            _fifthClient.DisconnectFromLobby(FIFTH_USER);

            GameAccess gameAccess = new GameAccess();
            gameAccess.CleanupGame(_gameId);
            gameAccess.CleanupGame(_otherGameId);
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

        public void SendUserFromLobbyToGame()
        {
            GameStarted = true;
        }

        public void GetKickedFromLobby()
        {
            BeenKicked = true;
        }

        public void ShowUsersInLobby(Tuple<User, int>[] users)
        {
            ConnectedToLobby = users;
        }
    }

    [TestClass]
    public class CallbackOnlineUsersTests
    {
        private static FriendManagerClient _firstClient;
        private static FriendManagerClient _secondClient;
        private static FriendManagerClient _thirdClient;
        private static FriendManagerClient _fourthClient;

        private static FriendManagerCallbackImplementation _firstCallback;
        private static FriendManagerCallbackImplementation _secondCallback;
        private static FriendManagerCallbackImplementation _thirdCallback;
        private static FriendManagerCallbackImplementation _fourthCallback;

        private static UserAccess userAccess = new UserAccess();

        private const int ERROR = 0;

        private static User FIRST_USER = new User()
        {
            Nickname = "FirstUser",
            ImageId = 1,
            Password = "password",
            Mail = "m@a.com"
        };

        private static User SECOND_USER = new User()
        {
            Nickname = "SecondUser",
            ImageId = 1,
            Password = "password",
            Mail = "A@a.com"
        };

        private static User THIRD_USER = new User()
        {
            Nickname = "ThirdUser",
            ImageId = 1,
            Password = "password",
            Mail = "A@a.com"
        };

        private static User FOURTH_USER = new User()
        {
            Nickname = "FourthUser",
            ImageId = 1,
            Password = "password",
            Mail = "A@a.com"
        };

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            GetConnectionString();
        }

        public static void GetConnectionString()
        {
            string connectionString = Environment.GetEnvironmentVariable("ASTRALIS");
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var connectionStringSection = config.ConnectionStrings.ConnectionStrings["AstralisDBEntities"];

            if (connectionStringSection != null)
            {
                connectionStringSection.ConnectionString = connectionString;

                config.Save(ConfigurationSaveMode.Modified);

                ConfigurationManager.RefreshSection("connectionStrings");

            }
        }

        [TestInitialize]
        public async Task Initialize()
        {
            userAccess.CreateUser(FIRST_USER);
            userAccess.CreateUser(SECOND_USER);
            userAccess.CreateUser(FOURTH_USER);

            _firstCallback = new FriendManagerCallbackImplementation();
            _firstClient = new FriendManagerClient(new InstanceContext(_firstCallback));

            await Task.Delay(1000);
            _firstClient.SubscribeToFriendManager(FIRST_USER.Nickname);

            _secondCallback = new FriendManagerCallbackImplementation();
            _secondClient = new FriendManagerClient(new InstanceContext(_secondCallback));

            await Task.Delay(1000);
            _secondClient.SubscribeToFriendManager(SECOND_USER.Nickname);

            _thirdCallback = new FriendManagerCallbackImplementation();
            _thirdClient = new FriendManagerClient(new InstanceContext(_thirdCallback));

            _fourthCallback = new FriendManagerCallbackImplementation();
            _fourthClient = new FriendManagerClient(new InstanceContext(_fourthCallback));

            await Task.Delay(1000);
            _fourthClient.SubscribeToFriendManager(FOURTH_USER.Nickname);
            await Task.Delay(1000);
            _fourthClient.SendFriendRequest(FOURTH_USER.Nickname, SECOND_USER.Nickname);
            await Task.Delay(1000);
            _fourthClient.SendFriendRequest(FOURTH_USER.Nickname, FIRST_USER.Nickname);
            await Task.Delay(1000);
            _firstClient.ReplyFriendRequest(FIRST_USER.Nickname, FOURTH_USER.Nickname, true);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _firstClient.UnsubscribeToFriendManager(FIRST_USER.Nickname);
            _secondClient.UnsubscribeToFriendManager(SECOND_USER.Nickname);
            _thirdClient.UnsubscribeToFriendManager(THIRD_USER.Nickname);
            _fourthClient.UnsubscribeToFriendManager(FOURTH_USER.Nickname);
            userAccess.DeleteUser(FIRST_USER.Nickname);
            userAccess.DeleteUser(SECOND_USER.Nickname);
            userAccess.DeleteUser(FOURTH_USER.Nickname);
        }

        [TestMethod]
        public async Task ShowSubscriptionToFriendManagerSuccesful()
        {
            _thirdClient.SubscribeToFriendManager(THIRD_USER.Nickname);
            await Task.Delay(2000);
            Assert.AreEqual(THIRD_USER.Nickname, _firstCallback.UserConnected);
        }

        [TestMethod]
        public async Task ShowUnsubscriptionToFriendManagerSuccesful()
        {
            _secondClient.UnsubscribeToFriendManager(SECOND_USER.Nickname);
            await Task.Delay(2000);
            Assert.AreEqual(SECOND_USER.Nickname, _firstCallback.UserDisconnected);
        }

        [TestMethod]
        public async Task ShowFriendRequestSuccesful()
        {
            _secondClient.SendFriendRequest(SECOND_USER.Nickname, FIRST_USER.Nickname);
            await Task.Delay(2000);
            Assert.AreEqual(SECOND_USER.Nickname, _firstCallback.FriendRequest);
        }

        [TestMethod]
        public async Task NoShowFriendRequestToOtherUser()
        {
            _secondClient.SendFriendRequest(SECOND_USER.Nickname, FIRST_USER.Nickname);
            await Task.Delay(2000);
            Assert.AreNotEqual(SECOND_USER.Nickname, _fourthCallback.FriendRequest);
        }

        [TestMethod]
        public async Task ShowFriendRequestUnsuccesful()
        {
            await Task.Delay(1000);
            Assert.AreEqual(ERROR, _secondClient.SendFriendRequest(SECOND_USER.Nickname, THIRD_USER.Nickname));
            Assert.IsNull(_thirdCallback.FriendRequest);
        }

        [TestMethod]
        public async Task ShowAcceptFriendRequestSuccesful()
        {
            _secondClient.ReplyFriendRequest(SECOND_USER.Nickname, FOURTH_USER.Nickname, true);
            await Task.Delay(6000);
            Assert.AreEqual(SECOND_USER.Nickname, _fourthCallback.FriendAccepted);
        }

        [TestMethod]
        public async Task NoShowAcceptFriendRequestToOtherUser()
        {
            _secondClient.ReplyFriendRequest(SECOND_USER.Nickname, FOURTH_USER.Nickname, true);
            await Task.Delay(6000);
            Assert.AreNotEqual(SECOND_USER.Nickname, _firstCallback.FriendAccepted);
        }

        [TestMethod]
        public async Task DenyFriendRequestSuccesful()
        {
            _secondClient.ReplyFriendRequest(SECOND_USER.Nickname, FOURTH_USER.Nickname, false);
            await Task.Delay(2000);
            Assert.AreNotEqual(SECOND_USER.Nickname, _fourthCallback.FriendRequest);
        }

        [TestMethod]
        public async Task ShowRemoveFriendSuccesful()
        {
            _firstClient.RemoveFriend(FIRST_USER.Nickname, FOURTH_USER.Nickname);
            await Task.Delay(2000);
            Assert.AreEqual(FIRST_USER.Nickname, _fourthCallback.FriendRemoved);
        }

        [TestMethod]
        public async Task NoShowElsesRemoveFriend()
        {
            _firstClient.RemoveFriend(FIRST_USER.Nickname, FOURTH_USER.Nickname);
            await Task.Delay(2000);
            Assert.AreNotEqual(FIRST_USER.Nickname, _secondCallback.FriendRemoved);
        }

        [TestMethod]
        public async Task ShowFriends()
        {
            _thirdClient.SubscribeToFriendManager(SECOND_USER.Nickname);
            await Task.Delay(2000);

            Assert.IsNotNull(_secondCallback.Friends);
        }

    }

    public class FriendManagerCallbackImplementation : IFriendManagerCallback
    {
        public string FriendRemoved { get; set; }

        public string FriendAccepted { get; set; }

        public string FriendRequest { get; set; }

        public Dictionary<string, Tuple<bool, int>> Friends { get; set; }

        public string UserConnected { get; set; }

        public string UserDisconnected { get; set; }

        public FriendManagerCallbackImplementation()
        {
            FriendRemoved = string.Empty;
            FriendAccepted = string.Empty;
            FriendRequest = null;
            Friends = null;
            UserConnected = string.Empty;
            UserDisconnected = string.Empty;
        }

        public void ShowFriendAccepted(string nickname)
        {
            FriendAccepted = nickname;
        }

        public void ShowFriendRequest(string nickname)
        {
            FriendRequest = nickname;
        }

        public void ShowUserSubscribedToFriendManager(string nickname)
        {
            UserConnected = nickname;
        }

        public void ShowUserUnsubscribedToFriendManager(string nickname)
        {
            UserDisconnected = nickname;
        }

        public void ShowFriends(Dictionary<string, Tuple<bool, int>> onlineFriends)
        {
            Friends = onlineFriends;
        }

        public void ShowFriendDeleted(string nickname)
        {
            FriendRemoved = nickname;
        }
    }

    [TestClass]
    public class UserManagerTest
    {
        private const int INT_VALIDATION_SUCCESS = 1;
        private const int INT_VALIDATION_FAILURE = 0;
        private const string NICKNAME_ERROR = "ERROR";

        private static UserManagerClient _clientUserManager = new UserManagerClient();
        private static UserAccess userAccess = new UserAccess();


        public static void GetConnectionString()
        {
            string connectionString = Environment.GetEnvironmentVariable("ASTRALIS");
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var connectionStringSection = config.ConnectionStrings.ConnectionStrings["AstralisDBEntities"];

            if (connectionStringSection != null)
            {
                connectionStringSection.ConnectionString = connectionString;

                config.Save(ConfigurationSaveMode.Modified);

                ConfigurationManager.RefreshSection("connectionStrings");

            }
        }

        [ClassInitialize]
        public static void InitializeClass(TestContext context)
        {
            GetConnectionString();

            User userToConfirm = new User()
            {
                Nickname = "ConfirmUserTest",
                ImageId = 1,
                Mail = "ConfirmUserTest@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(userToConfirm);

            User userToConfirmWrongPassword = new User()
            {
                Nickname = "ConfirmUserTestIncorrect",
                ImageId = 1,
                Mail = "ConfirmUserTest@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(userToConfirmWrongPassword);

            User userToAddTest = new User()
            {
                Nickname = "UserToAddTest",
                ImageId = 1,
                Mail = "UserToAddTest@hotmail.com",
                Password = "password"
            };

            _clientUserManager.AddUser(userToAddTest);

            User userToFind = new User()
            {
                Nickname = "UserToFindTest",
                ImageId = 1,
                Mail = "UserToFindTest@hotmail.com",
                Password = "password"
            };

            _clientUserManager.AddUser(userToFind);

            User userToGet = new User()
            {
                Nickname = "UserToGetTest",
                ImageId = 1,
                Mail = "UserToGetTest@hotmail.com",
                Password = "password"
            };

            _clientUserManager.AddUser(userToGet);

            User userToUpdate = new User()
            {
                Nickname = "UserToUpdate",
                ImageId = 1,
                Mail = "UserToUpdate@hotmail.com",
                Password = "password"
            };

            _clientUserManager.AddUser(userToUpdate);
        }

        [TestMethod]
        public void SuccessfullyConfirmUserMessageService()
        {
            string nickname = "ConfirmUserTest";
            string password = "password";

            Assert.IsTrue(_clientUserManager.ConfirmUserCredentials(nickname, password) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnSuccessfullyConfirmUserMessageService()
        {
            string nickname = "ConfirmUserTestUnsuccess";
            string password = "password";

            Assert.IsTrue(_clientUserManager.ConfirmUserCredentials(nickname, password) == INT_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void WrongPasswordConfirmUserMessageService()
        {
            string nickname = "ConfirmUserTestIncorrect";
            string password = "incorrectPassword";

            Assert.IsTrue(_clientUserManager.ConfirmUserCredentials(nickname, password) == INT_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void UnSuccessfullyAddUserMessageService()
        {
            User userToAddTest = new User()
            {
                Nickname = "UserToAddTest",
                ImageId = 1,
                Mail = "UserToAddTest@hotmail.com",
                Password = "password"
            };

            Assert.IsTrue(_clientUserManager.AddUser(userToAddTest) == INT_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccessfullyAddUserMessageService()
        {
            User userToAdd = new User()
            {
                Nickname = "UserToAddTestSuccesful",
                ImageId = 1,
                Mail = "UserToAddTest@hotmail.com",
                Password = "password"
            };

            Assert.IsTrue(_clientUserManager.AddUser(userToAdd) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void SuccessfullyAddGuestMessageService()
        {
            Assert.IsTrue(_clientUserManager.AddGuestUser().Nickname != NICKNAME_ERROR);
        }

        [TestMethod]
        public void SuccessfullyFindUserByNicknameMessageService()
        {
            string nickname = "UserToFindTest";

            Assert.IsTrue(_clientUserManager.FindUserByNickname(nickname) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnSuccessfullyFindUserByNicknameMessageService()
        {
            string nickname = "UserToFindTestUnsuccess";

            Assert.IsTrue(_clientUserManager.FindUserByNickname(nickname) == INT_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccessfullyGetUserByNicknameMessageService()
        {
            string nickname = "UserToGetTest";

            Assert.IsTrue(_clientUserManager.GetUserByNickname(nickname).Nickname != NICKNAME_ERROR);
        }

        [TestMethod]
        public void UnsuccessfullyGetUserByNicknameMessageService()
        {
            string nickname = "UserToGetTestUnsuccesful";

            Assert.IsTrue(_clientUserManager.GetUserByNickname(nickname).Nickname != NICKNAME_ERROR);
        }

        [TestMethod]
        public void UnSuccessfullyUpdateUserMessageService()
        {
            User userToUpdate = new User()
            {
                Nickname = "UserToUpdateUnsuccesful",
                ImageId = 1,
                Mail = "UserToUpdate@hotmail.com",
                Password = "password"
            };

            Assert.IsTrue(_clientUserManager.UpdateUser(userToUpdate) == INT_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccessfullyUpdateUserMessageService()
        {
            User userUpdated = new User()
            {
                Nickname = "UserToUpdate",
                ImageId = 3,
                Mail = "UserToUpdate@gmail.com",
                Password = "password"
            };

            int result = _clientUserManager.UpdateUser(userUpdated);

            Assert.IsTrue(result == INT_VALIDATION_SUCCESS);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            userAccess.DeleteUser("ConfirmUserTest");
            userAccess.DeleteUser("UserToAddTest");
            userAccess.DeleteUser("UserToFindTest");
            userAccess.DeleteUser("UserToUpdate");
            userAccess.DeleteUser("UserToGetTest");
            userAccess.DeleteUser("NicknameUpdated");
            userAccess.DeleteUser("UserCanPlay");
            userAccess.DeleteUser("ConfirmUserTestUnsuccess");
            userAccess.DeleteUser("ConfirmUserTestIncorrect");
            userAccess.DeleteUser("UserToAddTestSuccesful");
        }
    }

    [TestClass]
    public class UserManagerTestExceptionErrors
    {
        private const int ERROR = -1;
        private const string NICKNAME_ERROR = "ERROR";

        private static UserManagerClient client = new UserManagerClient();

        [TestMethod]
        public void ErrorConfirmUserMessageService()
        {
            string nickname = "UserToConfirmEntity";
            string password = "password";

            Assert.IsTrue(client.ConfirmUserCredentials(nickname, password) == ERROR);
        }

        [TestMethod]
        public void ErrorAddUserMessageService()
        {
            User userToAdd = new User()
            {
                Nickname = "UserToAddTest",
                ImageId = 1,
                Mail = "UserToAddTest@hotmail.com",
                Password = "password"
            };

            Assert.IsTrue(client.AddUser(userToAdd) == ERROR);
        }

        [TestMethod]
        public void ErrorAddGuestMessageService()
        {
            Assert.IsTrue(client.AddGuestUser().Nickname == NICKNAME_ERROR);
        }

        [TestMethod]
        public void ErrorFindUserByNicknameMessageService()
        {
            string nickname = "UserToFindTestERROR";

            Assert.IsTrue(client.FindUserByNickname(nickname) == ERROR);
        }

        [TestMethod]
        public void ErrorGetUserByNicknameMessageService()
        {
            string nickname = "UserToGetTestError";

            Assert.IsTrue(client.GetUserByNickname(nickname).Nickname == NICKNAME_ERROR);
        }

        [TestMethod]
        public void ErrorUpdateUserMessageService()
        {
            User userToUpdate = new User()
            {
                Nickname = "UserToUpdate",
                ImageId = 1,
                Mail = "UserToUpdate@hotmail.com",
                Password = "password"
            };

            Assert.IsTrue(client.UpdateUser(userToUpdate) == ERROR);
        }
    }
}