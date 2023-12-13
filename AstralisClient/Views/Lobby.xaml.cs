using Astralis.Logic;
using Astralis.UserManager;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Astralis.Views.Animations;

namespace Astralis.Views
{
    public partial class Lobby : Page, UserManager.ILobbyManagerCallback
    {
        private const string HOST_CODE = "host";
        private const string ERROR_CODE_LOBBY = "error";
        private const int NO_TEAM = 0;
        private const int TEAM_ONE = 1;
        private const int TEAM_TWO = 2;
        private const int MAX_TEAM_SIZE = 2;

        private bool _isHost = false;
        private string _gameId;
        private Dictionary<int , bool> _freeSpaces;
        private Dictionary<int , LobbyUserCard> _userCards = new Dictionary<int, LobbyUserCard>();
        private LobbyManagerClient _client;

        public Lobby()
        {
            InitializeComponent();
            _freeSpaces = new Dictionary<int, bool>()
            {
                {0, true },
                {1, true },
                {2, true },
                {3, true },
            };
            btnStartGame.IsEnabled = false;
        }

        public bool SetLobby(string code) 
        {
            bool gameExist = false;

            InstanceContext context = new InstanceContext(this);
            _client = new LobbyManagerClient(context);

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

                if (_gameId == ERROR_CODE_LOBBY)
                {
                    MessageBox.Show("msgErrorCreateLobby", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    gameExist = true;
                    lblGameCode.Content = _gameId;
                }
            }
            else if(_client.GameExist(code))
            {
                ConnectToGame(code);
                gameExist = true;
            }
            else
            {
                MessageBox.Show("msgNoGameFound " , "titleNoGameFound", MessageBoxButton.OK, MessageBoxImage.Information);
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

            _client.ConnectLobby(user, code);
            
            _gameId = code;
            lblGameCode.Content = _gameId;
        }

        public bool GameIsNotFull(string gameId)
        {
            InstanceContext context = new InstanceContext(this);
            UserManager.LobbyManagerClient client = new UserManager.LobbyManagerClient(context);

            bool gameIsNotFull = client.GameIsNotFull(gameId);

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
                if (_freeSpaces[gridRow] == true && isAdded == false)
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

            _client.ChangeLobbyUserTeam(userTeam.Item1, userTeam.Item2);

            EnableStartButton();
        }

        private void RemoveCard(User user)
        {
            for (int gridRow = 0; gridRow < 4; gridRow++)
            {
                if (_userCards.ContainsKey(gridRow))
                {
                    if (_userCards[gridRow].UserNickname == user.Nickname)
                    {
                        gridUsers.Children.Remove(_userCards[gridRow]);
                        _userCards.Remove(gridRow);
                        _freeSpaces[gridRow] = true;
                    }
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

        public void ShowUsersInLobby(Tuple<User,int>[] userList)
        {
            for (int i = 0; i < userList.Length; i++)
            {
                AddCard(userList[i].Item1, userList[i].Item2);
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
                if (_userCards.ContainsKey(gridRow))
                {
                    if (_userCards[gridRow].UserNickname == userNickname)
                    {
                        _userCards[gridRow].ChangeTeam(team);
                        break;
                    }
                }

            }
            EnableStartButton();
        }

        public void StartClientGame()
        {
            GameWindow windowParent = (GameWindow)this.Parent;

            if(windowParent != null)
            {
                windowParent.Visibility = Visibility.Collapsed;
            }

            Game.GameBoard gameBoard = new Game.GameBoard(windowParent);
            
            gameBoard.IsHost = _isHost;
            gameBoard.Show();
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
                if (space.Value == true)
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

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            InstanceContext context = new InstanceContext(this);
            UserManager.LobbyManagerClient client = new UserManager.LobbyManagerClient(context);

            User user = new User();
            user.Nickname = UserSession.Instance().Nickname;

            client.DisconnectLobby(user);

            NavigationService.GoBack();
        }

        private void BtnSendMessageClick(object sender, RoutedEventArgs e)
        {
            string message = UserSession.Instance().Nickname + ": " + txtChat.Text;

            InstanceContext context = new InstanceContext(this);
            UserManager.LobbyManagerClient client = new UserManager.LobbyManagerClient(context);
            
            client.SendMessage(message, _gameId);
        }


        private void BtnCopyToClipboardClick(object sender, RoutedEventArgs e)
        {
            string textToCopy = lblGameCode.Content.ToString();

            Clipboard.SetText(textToCopy);

            MessageBox.Show("Text has been copied to clipboard: " + textToCopy, "Clipboard Copy", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnStartGameClick(object sender, RoutedEventArgs e)
        {

            GameWindow windowParent = (GameWindow)this.Parent; 
            if(windowParent != null)
            {
                windowParent.Visibility = Visibility.Collapsed;
            }
    
            _client.StartGame(_gameId);
        }

        private void BtnSendInvitationClick(object sender, RoutedEventArgs e)
        {
            string toSendMail = txtFriendMail.Text;

            string mailString = _client.SendFriendInvitation(_gameId, toSendMail);
            MessageBox.Show(mailString, "titleMail", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LobbyUserCardUserKicked(object sender, string userNickname)
        {
            // Llama al método del servidor para expulsar al usuario
            _client.KickUser(userNickname);
        }

        public void GetKicked()
        {           
            MessageBox.Show("Has sido expulsado del lobby.", "Expulsado", MessageBoxButton.OK, MessageBoxImage.Information);
            NavigationService.GoBack();
        }
    }
}
