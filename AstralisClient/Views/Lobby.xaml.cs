using Astralis.Logic;
using Astralis.UserManager;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Astralis.Views.Cards;

namespace Astralis.Views
{
    public partial class Lobby : Page, ILobbyManagerCallback
    {
        private const string HOST_CODE = "host";
        private const int NO_TEAM = 0;
        private const int TEAM_ONE = 1;
        private const int TEAM_TWO = 2;
        private const int MAX_TEAM_SIZE = 2;
        private const string GUEST_NAME = "Guest";
        private const string LOBBY_WINDOW = "LOBBY";

        private bool _isHost = false;
        private string _gameId;
        private Dictionary<int , bool> _freeSpaces;
        private Dictionary<int , LobbyUserCard> _userCards = new Dictionary<int, LobbyUserCard>();
        private LobbyManagerClient _client;
        private FriendWindow _friendWindow;
        private GameWindow _gameWindow;


        public Lobby(GameWindow gameWindow)
        {
            InitializeComponent();
            _freeSpaces = new Dictionary<int, bool>()
            {
                {0, true },
                {1, true },
                {2, true },
                {3, true },
            };

            InitializeLobby(gameWindow);
            
        }

        public bool CanPlay()
        {
            InstanceContext context = new InstanceContext(this);
            _client = new LobbyManagerClient(context);
            bool canPlay = true;

            string nickname = UserSession.Instance().Nickname;

            if (_client.IsBanned(nickname))
            {
                canPlay = false;
            }

            return canPlay;
        }

        private void InitializeLobby(GameWindow gameWindow)
        {
            InstanceContext context = new InstanceContext(this);
            _client = new LobbyManagerClient(context);

            btnStartGame.IsEnabled = false;

            if(!UserSession.Instance().Nickname.StartsWith(GUEST_NAME))
            {
                _friendWindow = new FriendWindow(LOBBY_WINDOW);

                _friendWindow.SetFriendWindow();

                _friendWindow.SendGameInvitation += SendGameInvitationEvent;

                gridFriendsWindow.Children.Add(_friendWindow);
            }

            _gameWindow = gameWindow;
        }

        private void SendGameInvitationEvent(object sender, string friendUsername)
        {
            try
            {
                string mailString = _client.SendFriendInvitation(_gameId, friendUsername);

                MailValidations(mailString);

            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool SetLobby(string code) 
        {
            bool gameExist = false;
            try
            {
                if (code == HOST_CODE)
                {
                    _isHost = true;

                    User user = new User
                    {
                        Nickname = UserSession.Instance().Nickname,
                        ImageId = UserSession.Instance().ImageId
                    };

                    AddCard(user, NO_TEAM);

                    _gameId = _client.CreateLobby(user);

                    if (_gameId == Constants.VALIDATION_FAILURE_STRING)
                    {
                        MessageBox.Show(Properties.Resources.msgErrorCreateLobby, Properties.Resources.titleError, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (_gameId == Constants.ERROR_STRING)
                    {
                        MessageBox.Show(Properties.Resources.msgConnectionError, Properties.Resources.titleNoGameFound, MessageBoxButton.OK, MessageBoxImage.Information);
                        _friendWindow.Disconnect();
                        App.RestartApplication();
                    }
                    else
                    {
                        gameExist = true;
                        lblGameCode.Content = _gameId;
                    }
                }
                else if (_client.GameExist(code))
                {
                    ConnectToGame(code);
                    gameExist = true;
                }
                else
                {
                    MessageBox.Show(Properties.Resources.msgNoGameFound, Properties.Resources.titleNoGameFound, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, Properties.Resources.titleNoGameFound, MessageBoxButton.OK, MessageBoxImage.Information);
                App.RestartApplication();
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, Properties.Resources.titleNoGameFound, MessageBoxButton.OK, MessageBoxImage.Information);
                App.RestartApplication();
            }

            return gameExist;
        }

        private void ConnectToGame(string code)
        {
            User user = new User
            {
                Nickname = UserSession.Instance().Nickname,
                ImageId = UserSession.Instance().ImageId
            };

            try
            {
                _client.ConnectLobby(user, code);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
            
            _gameId = code;
            lblGameCode.Content = _gameId;
        }

        public bool GameIsNotFull(string gameId)
        {
            InstanceContext context = new InstanceContext(this);
            UserManager.LobbyManagerClient client = new UserManager.LobbyManagerClient(context);
            bool gameIsNotFull = false;

            try
            {
                gameIsNotFull = client.GameIsNotFull(gameId);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
            return gameIsNotFull;
        }

        private void AddCard (User user, int team)
        {
            LobbyUserCard lobbyUserCard = new LobbyUserCard();

            lobbyUserCard.SetCard(user, _isHost);
            lobbyUserCard.ChangeTeam(team);
            lobbyUserCard.TeamSelectionChanged += LobbyUserCardTeamSelectionChanged;
            lobbyUserCard.UserKicked += LobbyUserCardUserKicked;
            bool isAdded = false;

            for(int gridRow = 0; gridRow < 4; gridRow++)
            {
                if (_freeSpaces[gridRow] && !isAdded)
                {
                    gridUsers.Children.Add(lobbyUserCard);
                    Grid.SetRow(lobbyUserCard, gridRow);
                    _freeSpaces[gridRow] = false;
                    _userCards.Add(gridRow, lobbyUserCard );
                    isAdded = true;
                }

            }
        }

        private void LobbyUserCardTeamSelectionChanged(object sender, Tuple<string, int> userTeam)
        {

            try
            {
                _client.ChangeLobbyUserTeam(userTeam.Item1, userTeam.Item2);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }

            EnableStartButton();
        }

        private void RemoveCard(User user)
        {
            for (int gridRow = 0; gridRow < 4; gridRow++)
            {
                if (_userCards.ContainsKey(gridRow) && _userCards[gridRow].UserNickname == user.Nickname)
                {
                    gridUsers.Children.Remove(_userCards[gridRow]);
                    _userCards.Remove(gridRow);
                    _freeSpaces[gridRow] = true;
                }  

            }
        }

        public void GiveLobbyId(string gameId)
        {
            lblGameCode.Content = gameId;
        }

        public void ReceiveMessage(string message)
        {
            tbChat.Text = tbChat.Text + "\n" + message;
        }

        public void ShowConnectionInLobby(User user)
        {
            AddCard(user, NO_TEAM);
        }

        public void ShowDisconnectionInLobby(User user)
        {
            btnStartGame.IsEnabled = false;

            RemoveCard(user);
        }

        public void ShowUsersInLobby(Tuple<User,int>[] users)
        {
            for (int i = 0; i < users.Length; i++)
            {
                AddCard(users[i].Item1, users[i].Item2);
            }

            User user = new User();
            user.Nickname = UserSession.Instance().Nickname;
            user.ImageId = UserSession.Instance().ImageId;

            AddCard(user, NO_TEAM);
        }

        public void UpdateLobbyUserTeam(string userNickname, int team)
        {
            for (int gridRow = 0; gridRow < 4; gridRow++)
            {
                if (_userCards.ContainsKey(gridRow) && _userCards[gridRow].UserNickname == userNickname)
                {
                    _userCards[gridRow].ChangeTeam(team);
                    break;
                }

            }
            EnableStartButton();
        }

        public void StartClientGame()
        {
            Game.GameBoard gameBoard = new Game.GameBoard();
            
            gameBoard.IsHost = _isHost;
            gameBoard.Show();
            _gameWindow.Close();
        }

        private void EnableStartButton()
        {
            if (NoFreeSpaces() && TeamsAreComplete() && _isHost)
            {
                btnStartGame.IsEnabled = true;
            }
            else
            {
                btnStartGame.IsEnabled = false;
            }

        }

        private bool NoFreeSpaces()
        {
            bool noFreeSpaces = true;

            foreach (var space in _freeSpaces)
            {
                if (space.Value)
                {
                    noFreeSpaces = false;
                    break;
                }
            }

            return noFreeSpaces;
        }

        private bool TeamsAreComplete()
        {
            int teamOneCounter = 0;
            int teamTwoCounter = 0;

            for (int gridRow = 0; gridRow < 4; gridRow++)
            {
                if (_userCards.ContainsKey(gridRow))
                {
                    if (_userCards[gridRow].Team == TEAM_ONE)
                    {
                        teamOneCounter++;
                    }
                    else if (_userCards[gridRow].Team == TEAM_TWO)
                    {
                        teamTwoCounter++;
                    }
                }
            }

            return (teamOneCounter == MAX_TEAM_SIZE && teamTwoCounter == MAX_TEAM_SIZE);
        }

        private void BtnExitClick(object sender, RoutedEventArgs e)
        {
            InstanceContext context = new InstanceContext(this);
            LobbyManagerClient client = new LobbyManagerClient(context);

            User user = new User();
            user.Nickname = UserSession.Instance().Nickname;

            try
            {
                client.DisconnectLobby(user);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }

            if (!user.Nickname.StartsWith(GUEST_NAME))
            {
                NavigationService.GoBack();
            }
            else
            {
                LogIn logIn = new LogIn();

                logIn.Show();
                _gameWindow.Close();
            }
        }

        private void BtnSendMessageClick(object sender, RoutedEventArgs e)
        {
            string message = UserSession.Instance().Nickname + ": " + txtChat.Text;
            txtChat.Text = Properties.Resources.txtChat;
            try
            {
                _client.SendMessage(message, _gameId);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
        }


        private void BtnCopyToClipboardClick(object sender, RoutedEventArgs e)
        {
            string textToCopy = lblGameCode.Content.ToString();

            Clipboard.SetText(textToCopy);

            MessageBox.Show(Properties.Resources.msgCopyToClipboard + textToCopy, Properties.Resources.titleCopyToClipboard, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnStartGameClick(object sender, RoutedEventArgs e)
        {

            GameWindow windowParent = (GameWindow)this.Parent; 
            if(windowParent != null)
            {
                windowParent.Visibility = Visibility.Collapsed;
            }

            try
            {
                _client.StartGame(_gameId);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
        }

        private void BtnSendInvitationClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string toSendMail = txtFriendMail.Text;
                string mailString = _client.SendFriendInvitation(_gameId, toSendMail);

                MailValidations(mailString);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MailValidations(string mailString)
        {

            if (mailString == Constants.USER_NOT_FOUND)
            {
                MessageBox.Show(Properties.Resources.msgUserNotFound, Properties.Resources.titleMail, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (mailString == Constants.ERROR_STRING)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                _friendWindow.Disconnect();
            }
            else
            {
                MessageBox.Show(mailString, Properties.Resources.titleMail, MessageBoxButton.OK, MessageBoxImage.Information);

            }
        }

        private void LobbyUserCardUserKicked(object sender, string userNickname)
        {
            try
            {
                _client.KickUser(userNickname);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
           
        }

        public void GetKicked()
        {           
            MessageBox.Show(Properties.Resources.msgKickedOut, Properties.Resources.titleKickedOut, MessageBoxButton.OK, MessageBoxImage.Information);
            if (!UserSession.Instance().Nickname.StartsWith(GUEST_NAME))
            {
                NavigationService.GoBack();
            }
            else
            {
                LogIn logIn = new LogIn();

                logIn.Show();
                _gameWindow.Close();
            }
        }

        private void BtnFriendWindowClick(object sender, RoutedEventArgs e)
        {
            if(_friendWindow != null)
            {
                if (_friendWindow.IsVisible)
                {
                    _friendWindow.Visibility = Visibility.Hidden;
                }
                else
                {
                    _friendWindow.SetFriendWindow();
                    _friendWindow.Visibility = Visibility.Visible;
                }
            }
            
        }
    }
}
