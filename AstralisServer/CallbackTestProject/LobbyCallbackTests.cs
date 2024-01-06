using CallbackTestProject.AstralisServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace CallbackTestProject
{
    [TestClass]
    public class LobbyCallbackTests
    {
        private static LobbyManagerClient _firstClient;
        private static LobbyManagerClient _secondClient;
        private static LobbyManagerClient _thirdClient;
        private static LobbyCallbackImplementation _firstCallback;
        private static LobbyCallbackImplementation _secondCallback;
        private static LobbyCallbackImplementation _thirdCallback;

        private static string _gameId = string.Empty;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            _firstCallback = new LobbyCallbackImplementation();
            _firstClient = new LobbyManagerClient(new InstanceContext(_firstCallback));
            User firstUser = new User()
            {
                Nickname = "FirstTester",
                ImageId = 1
            };
            _gameId = _firstClient.CreateLobby(firstUser);
        }

        [TestMethod]
        public async Task ConnectToServerSuccesful()
        {
            _secondCallback = new LobbyCallbackImplementation();
            _secondClient = new LobbyManagerClient(new InstanceContext(_secondCallback));
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
            Assert.AreEqual(_firstCallback.ConnectionInLobby, secondUser.Nickname);
        }

        [TestMethod]
        public async Task ShowUsersInLobbySuccessful()
        {
            _thirdCallback = new LobbyCallbackImplementation();
            _thirdClient = new LobbyManagerClient(new InstanceContext(_thirdCallback));
            User thirdUser = new User()
            {
                Nickname = "ThirdUser",
                ImageId = 1
            };

            if (_thirdClient.GameExist(_gameId))
            {
                _thirdClient.ConnectLobby(thirdUser, _gameId);
            }

            await Task.Delay(2000);
            Assert.IsNotNull(_thirdCallback.ConnectedToLobby);
        }
    }

    public class LobbyCallbackImplementation: ILobbyManagerCallback
    {
        public bool BeenKicked { get; set; }

        public Tuple<User, int>[] ConnectedToLobby { get; set; }

        public string ConnectionInLobby { get; set; }

        public string DisconnectionInLobby { get; set; }

        public string NicknameTeamChange { get; set;}

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
}
