using CallbackTestProject.AstralisService;
using DataAccessProject.Contracts;
using DataAccessProject.DataAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
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
            Nickname = "SecondtTester",
            ImageId = 2
        };

        private static User THIRD_USER = new User()
        {
            Nickname = "LastTester",
            ImageId = 3
        };

        private static User FOURTH_USER = new User()
        {
            Nickname = "FourthTester",
            ImageId = 4
        };

        [TestInitialize]
        public void Initialize()
        {
            _firstCallback = new LobbyCallbackImplementation();
            _firstClient = new LobbyManagerClient(new InstanceContext(_firstCallback));

            _gameId = _firstClient.CreateLobby(FIRST_USER);

            _secondCallback = new LobbyCallbackImplementation();
            _secondClient = new LobbyManagerClient(new InstanceContext(_secondCallback));

            _secondClient.ConnectToLobby(SECOND_USER, _gameId);

            _thirdCallback = new LobbyCallbackImplementation();
            _thirdClient = new LobbyManagerClient(new InstanceContext(_thirdCallback));

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
        public async Task ConnectToLobbySuccesful()
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
        public async Task ConnectToDifferentServer()
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
        public async Task DisconnectionFromLobbySuccesful()
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
        public async Task DiscconnectionFromOtherLobby()
        {
            _thirdClient.DisconnectFromLobby(THIRD_USER);

            await Task.Delay(2000);
            Assert.AreNotEqual(THIRD_USER.Nickname, _firstCallback.DisconnectionInLobby);
            Assert.AreNotEqual(THIRD_USER.Nickname, _secondCallback.DisconnectionInLobby);
        }


        [TestMethod]
        public async Task KickedSuccesfull()
        {
            _firstClient.KickUserFromLobby(SECOND_USER.Nickname);

            await Task.Delay(2000);
            Assert.IsTrue(_secondCallback.BeenKicked);
            Assert.AreEqual(SECOND_USER.Nickname, _firstCallback.DisconnectionInLobby);
        }


        [TestMethod]
        public async Task KickedUnsuccesfull()
        {

            _firstClient.KickUserFromLobby(FOURTH_USER.Nickname);

            await Task.Delay(2000);
            Assert.IsFalse(_fourthCallback.BeenKicked);
        }

        [TestMethod]
        public async Task KickedFromOtherLobby()
        {
            _firstClient.KickUserFromLobby(SECOND_USER.Nickname);

            await Task.Delay(2000);
            Assert.IsTrue(_secondCallback.BeenKicked);
            Assert.AreEqual(SECOND_USER.Nickname, _firstCallback.DisconnectionInLobby);
        }


        [TestMethod]
        public async Task DisconnectionUnsuccesful()
        {
            _fourthClient.DisconnectFromLobby(FOURTH_USER);

            await Task.Delay(2000);
            Assert.AreNotEqual(FOURTH_USER.Nickname, _firstCallback.DisconnectionInLobby);
        }

        [TestMethod]
        public async Task DisconnectionFromDifferentLobby()
        {
            _secondClient.DisconnectFromLobby(SECOND_USER);

            await Task.Delay(2000);
            Assert.AreNotEqual(SECOND_USER.Nickname, _thirdCallback.DisconnectionInLobby);
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
        private const int SECOND_TEAM = 2;

        private static string _gameId = string.Empty;
        private static string _otherGameId = string.Empty;

        private static User FIRST_USER = new User()
        {
            Nickname = "FirstTester",
            ImageId = 1
        };

        private static User SECOND_USER = new User()
        {
            Nickname = "SecondtTester",
            ImageId = 2
        };

        private static User THIRD_USER = new User()
        {
            Nickname = "LastTester",
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
            _secondClient.ConnectToLobby(SECOND_USER ,_gameId);
            await Task.Delay(2000);
            _thirdClient.ConnectToLobby(THIRD_USER, _gameId);
            await Task.Delay(2000);
            _fourthClient.ConnectToLobby(FOURTH_USER, _gameId);
            await Task.Delay(2000);

            _otherGameId = _fourthClient.CreateLobby(FIFTH_USER);
        }

        [TestMethod]
        public async Task UpdateLobbyUserTeam()
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
        public async Task UpdateLobbyUserTeamFromOtherGame()
        {
            await Task.Delay(2000);

            _secondClient.ChangeLobbyUserTeam(SECOND_USER.Nickname, FIRST_TEAM);

            await Task.Delay(4000);
            Assert.AreNotEqual(SECOND_USER.Nickname, _fifthCallback.NicknameTeamChange);
            Assert.AreNotEqual(FIRST_TEAM, _fifthCallback.TeamChanged);
        }

        [TestMethod]
        public async Task SendMessageLobbySuccesfull()
        {
            string message = "SecondUser: Message";

            _secondClient.SendMessage(message, SECOND_USER.Nickname);

            await Task.Delay(5000);
            Assert.AreEqual(message, _firstCallback.Message);
            Assert.AreEqual(message, _thirdCallback.Message);
            Assert.AreEqual(message, _fourthCallback.Message);
        }

        [TestMethod]
        public async Task SendMessageToOtherLobby()
        {
            string message = "SecondUser: OtherLobbyMessage";

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

        private static FriendManagerCallbackImplementation _firstCallback;
        private static FriendManagerCallbackImplementation _secondCallback;
        private static FriendManagerCallbackImplementation _thirdCallback;
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

        [TestInitialize]
        public static void Initialize(TestContext context)
        {
            userAccess.CreateUser(FIRST_USER);
            userAccess.CreateUser(SECOND_USER);

            _firstCallback = new FriendManagerCallbackImplementation();
            _firstClient = new FriendManagerClient (new InstanceContext(_firstCallback));

            _secondCallback = new FriendManagerCallbackImplementation();
            _secondClient = new FriendManagerClient(new InstanceContext(_secondCallback));

            _thirdCallback = new FriendManagerCallbackImplementation();
            _thirdClient = new FriendManagerClient(new InstanceContext(_thirdCallback));
        }

        [TestMethod]
        public async Task ShowConnectionToGameSuccesful()
        {
            _secondClient.SubscribeToFriendManager(SECOND_USER);
            await Task.Delay(2000);

            _secondClient.UnsubscribeToFriendManager(SECOND_USER);
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
            await Task.Delay(2000);

            _secondClient.SendFriendRequest(SECOND_USER, FIRST_USER);
            await Task.Delay(2000);

            _firstClient.ReplyFriendRequest(FIRST_USER, SECOND_USER, false);
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

        [TestMethod]
        public async Task AcceptFriendRequestSuccesful()
        {
            _secondClient.ConectUser(SECOND_USER);
            _secondClient.SendFriendRequest(SECOND_USER, FIRST_USER);
            await Task.Delay(2000);

            _firstClient.ReplyFriendRequest(FIRST_USER, SECOND_USER, true);
            await Task.Delay(2000);

            _firstClient.RemoveFriend(FIRST_USER, SECOND_USER);
            Assert.AreEqual(FIRST_USER, _secondCallback.FriendAccepted);
        }

        [TestMethod]
        public async Task DenyFriendRequestSuccesful()
        {
            _firstClient.ConectUser(FIRST_USER);
            _firstClient.SendFriendRequest(FIRST_USER, SECOND_USER);

            await Task.Delay(2000);

            _secondClient.ReplyFriendRequest(SECOND_USER, FIRST_USER, false);

            Assert.AreNotEqual(SECOND_USER, _firstCallback.FriendRequest);
        }

        [TestMethod]
        public async Task RemoveFriendSuccesful()
        {
            _secondClient.ConectUser(SECOND_USER);
            await Task.Delay(2000);

            _secondClient.SendFriendRequest(SECOND_USER, FIRST_USER);
            await Task.Delay(2000);

            _firstClient.ReplyFriendRequest(FIRST_USER, SECOND_USER, false);
            Assert.AreEqual(SECOND_USER, _firstCallback.FriendRequest);
        }

        [TestMethod]
        public async Task ShowOnlineFriends()
        {
            _secondClient.ConectUser(SECOND_USER);
            await Task.Delay(2000);

            Assert.IsNotNull(_secondCallback.Friends);
        }


        [ClassCleanup]
        public static void Cleanup()
        {
            _firstClient.DisconectUser(FIRST_USER);

            userAccess.DeleteUser(FIRST_USER);
            userAccess.DeleteUser(SECOND_USER);
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

        public void ShowUserSubscribedToFriendManager(string nickname)
        {
            throw new NotImplementedException();
        }

        public void ShowUserUnsubscribedToFriendManager(string nickname)
        {
            throw new NotImplementedException();
        }

        public void ShowFriends(Dictionary<string, Tuple<bool, int>> onlineFriends)
        {
            throw new NotImplementedException();
        }

        public void ShowFriendDeleted(string nickname)
        {
            throw new NotImplementedException();
        }
    }

    [TestClass]
    public class UserManagerTest
    {

        private const int INT_VALIDATION_SUCCESS = 1;
        private const int INT_VALIDATION_FAILURE = 0;
        private const string NICKNAME_ERROR = "ERROR";
        private const int ERROR = -1;
        private const string USER_NOT_FOUND = "UserNotFound";

        private static UserManagerClient clientUserManager = new UserManagerClient();
        private static UserAccess userAccess = new UserAccess();

        [TestMethod]
        public void SuccessfullyConfirmUserUM()
        {
            DataAccessProject.Contracts.User userToConfirm = new DataAccessProject.Contracts.User()
            {
                Nickname = "ConfirmUserTest",
                ImageId = 1,
                Mail = "ConfirmUserTest@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(userToConfirm);
            Assert.IsTrue(clientUserManager.ConfirmUser(userToConfirm.Nickname, userToConfirm.Password) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnSuccessfullyConfirmUserUM()
        {
            DataAccessProject.Contracts.User userToConfirm = new DataAccessProject.Contracts.User()
            {
                Nickname = "ConfirmUserTestUnsuccess",
                ImageId = 1,
                Mail = "ConfirmUserTest@hotmail.com",
                Password = "password"
            };

            Assert.IsTrue(clientUserManager.ConfirmUser(userToConfirm.Nickname, userToConfirm.Password) == INT_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void WrongPasswordConfirmUserUM()
        {
            DataAccessProject.Contracts.User userToConfirm = new DataAccessProject.Contracts.User()
            {
                Nickname = "ConfirmUserTestUnsuccess",
                ImageId = 1,
                Mail = "ConfirmUserTest@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(userToConfirm);
            Assert.IsTrue(clientUserManager.ConfirmUser(userToConfirm.Nickname, "Incorrect") == INT_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void UnSuccessfullyAddUserUM()
        {
            User userToAdd = new User()
            {
                Nickname = "UserToAddTest",
                ImageId = 1,
                Mail = "UserToAddTest@hotmail.com",
                Password = "password"
            };

            clientUserManager.AddUser(userToAdd);

            Assert.IsTrue(clientUserManager.AddUser(userToAdd) == INT_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccessfullyAddUserUM()
        {
            User userToAdd = new User()
            {
                Nickname = "UserToAddTest",
                ImageId = 1,
                Mail = "UserToAddTest@hotmail.com",
                Password = "password"
            };

            Assert.IsTrue(clientUserManager.AddUser(userToAdd) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void SuccessfullyAddGuestUM()
        {
            Assert.IsTrue(clientUserManager.AddGuestUser().Nickname != NICKNAME_ERROR);
        }

        [TestMethod]
        public void SuccessfullyFindUserByNicknameUM()
        {
            User userToFind = new User()
            {
                Nickname = "UserToFindTest",
                ImageId = 1,
                Mail = "UserToFindTest@hotmail.com",
                Password = "password"
            };

            clientUserManager.AddUser(userToFind);

            Assert.IsTrue(clientUserManager.FindUserByNickname(userToFind.Nickname) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnSuccessfullyFindUserByNicknameUM()
        {
            User userToFind = new User()
            {
                Nickname = "UserToFindTestUnsuccess",
                ImageId = 1,
                Mail = "UserToFindTest@hotmail.com",
                Password = "password"
            };

            Assert.IsTrue(clientUserManager.FindUserByNickname(userToFind.Nickname) == INT_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccessfullyGetUserByNicknameUM()
        {
            User userToGet = new User()
            {
                Nickname = "UserToGetTest",
                ImageId = 1,
                Mail = "UserToGetTest@hotmail.com",
                Password = "password"
            };

            clientUserManager.AddUser(userToGet);

            Assert.IsTrue(clientUserManager.GetUserByNickname(userToGet.Nickname).Nickname != NICKNAME_ERROR);
        }

        [TestMethod]
        public void UnsuccessfullyGetUserByNicknameUM()
        {
            User userToGet = new User()
            {
                Nickname = "UserToGetTestUnsuccesful",
                ImageId = 1,
                Mail = "UserToGetTest@hotmail.com",
                Password = "password"
            };

            Assert.IsTrue(clientUserManager.GetUserByNickname(userToGet.Nickname).Nickname != NICKNAME_ERROR);
        }

        [TestMethod]
        public void UnSuccessfullyUpdateUserUM()
        {
            User userToUpdate = new User()
            {
                Nickname = "UserToUpdate",
                ImageId = 1,
                Mail = "UserToUpdate@hotmail.com",
                Password = "password"
            };

            Assert.IsTrue(clientUserManager.UpdateUser(userToUpdate) == INT_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccessfullyUpdateUserUM()
        {
            User userToUpdate = new User()
            {
                Nickname = "UserToUpdate",
                ImageId = 1,
                Mail = "UserToUpdate@hotmail.com",
                Password = "password"
            };

            User userUpdated = new User()
            {
                Nickname = "UserToUpdate",
                ImageId = 3,
                Mail = "UserToUpdate@gmail.com",
                Password = "password"
            };

            clientUserManager.AddUser(userToUpdate);

            int result = clientUserManager.UpdateUser(userUpdated);

            userAccess.DeleteUser("UserToUpdate");

            Assert.IsTrue(result == INT_VALIDATION_SUCCESS);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            userAccess.DeleteUser("ConfirmUserTest");
            userAccess.DeleteUser("UserToAddTest");
            userAccess.DeleteUser("UserToFindTest");
            userAccess.DeleteUser("UserToGetTest");
            userAccess.DeleteUser("NicknameUpdated");
            userAccess.DeleteUser("UserCanPlay");
            userAccess.DeleteUser("ConfirmUserTestUnsuccess");
        }
    }

    [TestClass]
    public class UserManagerTestExceptionErrors
    {
        private const int INT_VALIDATION_SUCCESS = 1;
        private const int INT_VALIDATION_FAILURE = 0;
        private const int ERROR = -1;
        private const string NICKNAME_ERROR = "ERROR";

        private static UserManagerClient client = new UserManagerClient();

        [TestMethod]
        public void ErrorConfirmUserUM()
        {
            DataAccessProject.Contracts.User userToConfirm = new DataAccessProject.Contracts.User()
            {
                Nickname = "ConfirmUserTest",
                ImageId = 1,
                Mail = "ConfirmUserTest@hotmail.com",
                Password = "password"
            };

            Assert.IsTrue(client.ConfirmUser(userToConfirm.Nickname, userToConfirm.Password) == ERROR);
        }

        [TestMethod]
        public void ErrorAddUserUM()
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
        public void ErrorAddGuestUM()
        {
            Assert.IsTrue(client.AddGuestUser().Nickname == NICKNAME_ERROR);
        }

        [TestMethod]
        public void ErrorFindUserByNicknameUM()
        {
            User userToFind = new User()
            {
                Nickname = "UserToFindTestERROR",
                ImageId = 1,
                Mail = "UserToFindTest@hotmail.com",
                Password = "password"
            };

            Assert.IsTrue(client.FindUserByNickname(userToFind.Nickname) == ERROR);
        }

        [TestMethod]
        public void ErrorGetUserByNicknameUM()
        {
            User userToGet = new User()
            {
                Nickname = "UserToGetTest",
                ImageId = 1,
                Mail = "UserToGetTest@hotmail.com",
                Password = "password"
            };

            Assert.IsTrue(client.GetUserByNickname(userToGet.Nickname).Nickname == NICKNAME_ERROR);
        }

        [TestMethod]
        public void ErrorUpdateUserUM()
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