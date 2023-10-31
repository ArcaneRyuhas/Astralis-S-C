using Astralis.Logic;
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

namespace Astralis.Views
{
    /// <summary>
    /// Interaction logic for Lobby.xaml
    /// </summary>
    public partial class Lobby : Page, UserManager.ILobbyManagerCallback
    {
        private const string HOST_CODE = "host";
        private const string ERROR_CODE_LOBBY = "error";
        private int gridRow = 0;
        private string gameId;

        public Lobby(string code)
        {
            InitializeComponent();
            SetLobby(code);
        }

        private void SetLobby(string code) 
        {
            if (code == HOST_CODE)
            {
                User user = new User();
                user.Nickname = UserSession.Instance().Nickname;
                user.ImageId = UserSession.Instance().ImageId;

                AddCard(user);

                InstanceContext context = new InstanceContext(this);
                UserManager.LobbyManagerClient client = new UserManager.LobbyManagerClient(context);
                gameId = client.CreateLobby(user);

                if (gameId == ERROR_CODE_LOBBY)
                {
                    MessageBox.Show("msgErrorCreateLobby", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    lblGameCode.Content = gameId;
                }
            }
            else
            {
                User user = new User();
                user.Nickname = UserSession.Instance().Nickname;
                user.ImageId = UserSession.Instance().ImageId;


                InstanceContext context = new InstanceContext(this);
                UserManager.LobbyManagerClient client = new UserManager.LobbyManagerClient(context);

                client.ConnectLobby(user, code);

                gameId = code;
                lblGameCode.Content = gameId;
            }
        }

        public void AddCard (User user)
        {
            LobbyUserCard lobbyUserCard = new LobbyUserCard();
            lobbyUserCard.setCard(user);

            gridUsers.Children.Add(lobbyUserCard);
            Grid.SetRow(lobbyUserCard, gridRow);
            gridRow++;
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
            AddCard(user);
        }

        public void ShowDisconnectionInLobby(string nickname)
        {
            throw new NotImplementedException();
        }

        public void ShowUsersInLobby(User[] userList)
        {
            for (int i = 0; i < userList.Length; i++)
            {
                AddCard(userList[i]);
            }

            User user = new User();
            user.Nickname = UserSession.Instance().Nickname;

            AddCard(user);
        }

        public void UpdateLobbyUserTeam(User user, int team)
        {
            throw new NotImplementedException();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void btnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            string message = UserSession.Instance().Nickname + ": " + txtChat.Text;

            InstanceContext context = new InstanceContext(this);
            UserManager.LobbyManagerClient client = new UserManager.LobbyManagerClient(context);
            
            client.SendMessage(message, gameId);
        }

    }
}
