﻿using Astralis.Logic;
using Astralis.UserManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Astralis.Views.Animations;
using System.Net.NetworkInformation;

namespace Astralis.Views
{
    /// <summary>
    /// Interaction logic for Lobby.xaml
    /// </summary>
    public partial class Lobby : Page, UserManager.ILobbyManagerCallback
    {
        private const string HOST_CODE = "host";
        private const string ERROR_CODE_LOBBY = "error";
        private const int NO_TEAM = 0;
        private const int TEAM_ONE = 1;
        private const int TEAM_TWO = 2;
        private const int MAX_TEAM_SIZE = 2;

        private bool isHost = false;
        private string gameId;
        private Dictionary<int , bool> freeSpaces;
        private Dictionary<int , LobbyUserCard> userCards = new Dictionary<int, LobbyUserCard>();

        public Lobby()
        {
            InitializeComponent();
            freeSpaces = new Dictionary<int, bool>()
            {
                {0, true },
                {1, true },
                {2, true },
                {3, true },
            };
            btnStartGame.IsEnabled = true; //CAMBIAR A FALSE DESPUES DE PROBAR
        }

        public bool SetLobby(string code) 
        {
            bool gameExist = false;

            InstanceContext context = new InstanceContext(this);
            UserManager.LobbyManagerClient client = new UserManager.LobbyManagerClient(context);

            if (code == HOST_CODE)
            {
                isHost = true;

                User user = new User
                {
                    Nickname = UserSession.Instance().Nickname,
                    ImageId = UserSession.Instance().ImageId
                };

                AddCard(user, NO_TEAM);

                gameId = client.CreateLobby(user);

                if (gameId == ERROR_CODE_LOBBY)
                {
                    MessageBox.Show("msgErrorCreateLobby", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    gameExist = true;
                    lblGameCode.Content = gameId;
                }
            }
            else if(client.GameExist(code))
            {
                User user = new User
                {
                    Nickname = UserSession.Instance().Nickname,
                    ImageId = UserSession.Instance().ImageId
                };

                client.ConnectLobby(user, code);

                gameExist = true;
                gameId = code;
                lblGameCode.Content = gameId;
            }
            else
            {
                MessageBox.Show("msgNoGameFound " , "titleNoGameFound", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            return gameExist;
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
            lobbyUserCard.setCard(user, isHost);
            lobbyUserCard.ChangeTeam(team);
            lobbyUserCard.TeamSelectionChanged += LobbyUserCard_TeamSelectionChanged;
            bool isAdded = false;

            for(int gridRow = 0; gridRow < 4; gridRow++)
            {
                if (freeSpaces[gridRow] == true && isAdded == false)
                {
                    gridUsers.Children.Add(lobbyUserCard);
                    Grid.SetRow(lobbyUserCard, gridRow);
                    freeSpaces[gridRow] = false;
                    userCards.Add(gridRow, lobbyUserCard );
                    isAdded = true;
                }

            }
        }

        private void LobbyUserCard_TeamSelectionChanged(object sender, Tuple<string, int> userTeam)
        {
            InstanceContext context = new InstanceContext(this);
            UserManager.LobbyManagerClient client = new UserManager.LobbyManagerClient(context);

            client.ChangeLobbyUserTeam(userTeam.Item1, userTeam.Item2);
        }

        private void RemoveCard(User user)
        {
            for (int gridRow = 0; gridRow < 4; gridRow++)
            {
                if (userCards.ContainsKey(gridRow))
                {
                    if (userCards[gridRow].UserNickname == user.Nickname)
                    {
                        gridUsers.Children.Remove(userCards[gridRow]);
                        userCards.Remove(gridRow);
                        freeSpaces[gridRow] = true;
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
                if (userCards.ContainsKey(gridRow))
                {
                    if (userCards[gridRow].UserNickname == userNickname)
                    {
                        userCards[gridRow].ChangeTeam(team);
                        break;
                    }
                }

            }

            EnableStartButton();
        }

        public void StartClientGame()
        {
            Game.GameBoard gameBoard = new Game.GameBoard();

            NavigationService.Navigate(gameBoard);
        }

        private void EnableStartButton()
        {
            if (NoFreeSpaces() && TeamsAreComplete() && isHost)
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

            foreach (var space in freeSpaces)
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
                if (userCards.ContainsKey(gridRow))
                {
                    if (userCards[gridRow].Team == TEAM_ONE)
                    {
                        teamOneCounter++;
                    }
                    else if (userCards[gridRow].Team == TEAM_TWO)
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

        private void btnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            string message = UserSession.Instance().Nickname + ": " + txtChat.Text;

            InstanceContext context = new InstanceContext(this);
            UserManager.LobbyManagerClient client = new UserManager.LobbyManagerClient(context);
            
            client.SendMessage(message, gameId);
        }


        private void btnCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            string textToCopy = lblGameCode.Content.ToString();

            Clipboard.SetText(textToCopy);

            MessageBox.Show("Text has been copied to clipboard: " + textToCopy, "Clipboard Copy", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnStartGame_Click(object sender, RoutedEventArgs e)
        {
            InstanceContext context = new InstanceContext(this);
            UserManager.LobbyManagerClient client = new UserManager.LobbyManagerClient(context);

            client.StartGame(gameId);
        }
    }
}
