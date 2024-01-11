using Astralis.Logic;
using Astralis.UserManager;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Astralis.Views.Cards;
using System.Windows.Input;
using System.Text.RegularExpressions;

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
        private const int MAX_MAIL_LENGHT = 30;
        private const int MAX_CHAT_LENGHT = 100;
        private const string MAIL_REGEX = @"^.+@[^\.].*\.[a-z]{2,}$";


        private bool _isHost = false;
        private string _gameId;
        private bool _gameExist = false;
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
            InitializeFriendWindow();
        }

        public int CanAccessToLobby()
        {
            string nickname = UserSession.Instance().Nickname;
            int canPlayResult = _client.CanAccessToLobby(nickname);

            return canPlayResult;
        }

        private void InitializeLobby(GameWindow gameWindow)
        {
            InstanceContext context = new InstanceContext(this);
            _client = new LobbyManagerClient(context);
            btnStartGame.IsEnabled = false;
            _gameWindow = gameWindow;
        }

        private void InitializeFriendWindow()
        {
            if (!UserSession.Instance().Nickname.StartsWith(GUEST_NAME))
            {
                _friendWindow = new FriendWindow(LOBBY_WINDOW);

                _friendWindow.SetFriendWindow();

                _friendWindow.SendGameInvitation += SendInvitationToLobbyEvent;

                gridFriendsWindow.Children.Add(_friendWindow);
            }
        }

        private void SendInvitationToLobbyEvent(object sender, string friendUsername)
        {
            try
            {
                string mailString = _client.SendInvitationToLobby(_gameId, friendUsername);

                ShowValidationMessage(mailString);

            }
            catch (CommunicationObjectFaultedException)
            {
                MessageBox.Show(Properties.Resources.msgPreviousConnectioLost, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
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

        public bool SetLobby(string code) 
        {
            try
            {
                if (code == HOST_CODE)
                {
                    SetHostLobby();
                }
                else if (_client.LobbyExist(code))
                {
                    ConnectToLobby(code);
                    _gameExist = true;
                }
                else
                {
                    MessageBox.Show(Properties.Resources.msgNoGameFound, Properties.Resources.titleNoGameFound, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (CommunicationObjectFaultedException)
            {
                MessageBox.Show(Properties.Resources.msgPreviousConnectioLost, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
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

            return _gameExist;
        }

        private void SetHostLobby()
        {
            _isHost = true;
            User user = CreateUser();
            _gameId = _client.CreateLobby(user);

            CreateCard(user, NO_TEAM);
            IsLobbyCreatedSuccesfully();
        }

        private void IsLobbyCreatedSuccesfully()
        {
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
                _gameExist = true;
                lblGameCode.Content = _gameId;
            }

        }

        private User CreateUser()
        {
            User user = new User
            {
                Nickname = UserSession.Instance().Nickname,
                ImageId = UserSession.Instance().ImageId
            };

            return user;
        }

        private void ConnectToLobby(string code)
        {
            try
            {
                User user = CreateUser();
                _client.ConnectToLobby(user, code);

                _gameId = code;
                lblGameCode.Content = _gameId;
            }
            catch (CommunicationObjectFaultedException)
            {
                MessageBox.Show(Properties.Resources.msgPreviousConnectioLost, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
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

        public bool GameIsNotFull(string gameId)
        {
            bool lobbyIsNotFull = false;

            try
            {
                lobbyIsNotFull = _client.LobbyIsNotFull(gameId);
            }
            catch (CommunicationObjectFaultedException)
            {
                MessageBox.Show(Properties.Resources.msgPreviousConnectioLost, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
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

            return lobbyIsNotFull;
        }

        private void CreateCard (User user, int team)
        {
            LobbyUserCard lobbyUserCard = new LobbyUserCard();

            lobbyUserCard.SetCard(user, _isHost);
            lobbyUserCard.ChangeTeam(team);
            lobbyUserCard.TeamSelectionChanged += LobbyUserCardTeamSelectionChanged;
            lobbyUserCard.UserKicked += LobbyUserCardUserKicked;
            bool isAdded = false;

            AddCard(lobbyUserCard, isAdded);
        }

        private void AddCard(LobbyUserCard lobbyUserCard, bool isAdded)
        {
            for (int gridRow = 0; gridRow < 4; gridRow++)
            {
                if (_freeSpaces[gridRow] && !isAdded)
                {
                    gridUsers.Children.Add(lobbyUserCard);
                    Grid.SetRow(lobbyUserCard, gridRow);
                    _freeSpaces[gridRow] = false;
                    _userCards.Add(gridRow, lobbyUserCard);
                    isAdded = true;
                }
            }
        }

        private void LobbyUserCardTeamSelectionChanged(object sender, Tuple<string, int> userTeam)
        {
            try
            {
                _client.ChangeLobbyUserTeam(userTeam.Item1, userTeam.Item2);
                EnableStartButton();
            }
            catch (CommunicationObjectFaultedException)
            {
                MessageBox.Show(Properties.Resources.msgPreviousConnectioLost, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
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

        public void ReceiveMessage(string message)
        {
            tbChat.Text = tbChat.Text + "\n" + message;
        }

        public void ShowConnectionInLobby(User user)
        {
            CreateCard(user, NO_TEAM);
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
                CreateCard(users[i].Item1, users[i].Item2);
            }

            User user = CreateUser();
            CreateCard(user, NO_TEAM);
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

        public void SendUserFromLobbyToGame()
        {
            Game.GameBoard gameBoard = new Game.GameBoard
            {
                IsHost = _isHost
            };

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

            foreach (KeyValuePair <int, bool> space in _freeSpaces)
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
            User user = CreateUser();

            try
            {
                _client.DisconnectFromLobby(user);
            }
            catch (CommunicationObjectFaultedException)
            {
                MessageBox.Show(Properties.Resources.msgPreviousConnectioLost, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
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
            string nickname = UserSession.Instance().Nickname;
            string message = nickname + ": " + txtChat.Text;
            txtChat.Text = Properties.Resources.txtChat;

            try
            {
                _client.SendMessage(message, nickname);
            }
            catch (CommunicationObjectFaultedException)
            {
                MessageBox.Show(Properties.Resources.msgPreviousConnectioLost, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
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
            try
            {
                _client.SendUsersFromLobbyToGame(_gameId);
            }
            catch (CommunicationObjectFaultedException)
            {
                MessageBox.Show(Properties.Resources.msgPreviousConnectioLost, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
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
            GameWindow windowParent = (GameWindow)this.Parent;

            if (windowParent != null)
            {
                windowParent.Close();
            }
        }

        private void BtnSendInvitationClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string toSendMail = txtFriendMail.Text;
                txtFriendMail.Text = string.Empty;
                lblFriendMail.Visibility = Visibility.Visible;

                if (ValidMail(toSendMail))
                {
                    string mailString = _client.SendInvitationToLobby(_gameId, toSendMail);
                    ShowValidationMessage(mailString);
                }
                
            }
            catch (CommunicationObjectFaultedException)
            {
                MessageBox.Show(Properties.Resources.msgPreviousConnectioLost, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
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

        private bool ValidMail(string toSendMail)
        {
            bool validMail = true;

            if (!Regex.IsMatch(toSendMail, MAIL_REGEX))
            {
                MessageBox.Show(Properties.Resources.msgMailIncorrect, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                validMail = false;
            }

            return validMail;
        }

        private void ShowValidationMessage(string mailString)
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
                _client.KickUserFromLobby(userNickname);
            }
            catch (CommunicationObjectFaultedException)
            {
                MessageBox.Show(Properties.Resources.msgPreviousConnectioLost, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
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

        public void GetKickedFromLobby()
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

        private void TextLimiterForMail(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Length >= MAX_MAIL_LENGHT)
            {
                e.Handled = true;
            }

            lblFriendMail.Visibility = Visibility.Collapsed;
        }

        private void TextLimiterForChat(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Length >= MAX_CHAT_LENGHT)
            {
                e.Handled = true;
            }
        }
    }
}
