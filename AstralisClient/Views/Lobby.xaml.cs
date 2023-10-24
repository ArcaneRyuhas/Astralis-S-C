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
        private int gridRow = 0;

        public Lobby(string code)
        {
            InitializeComponent();
            
            if(code == "host") 
            {
                User user = new User();
                user.Nickname = UserSession.Instance().Nickname;

                AddCard(user);

                InstanceContext context = new InstanceContext(this);
                UserManager.LobbyManagerClient client = new UserManager.LobbyManagerClient(context);
                if (!(client.CreateLobby(user) > 0))
                {
                    MessageBox.Show("msgErrorCreateLobby", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                User user = new User();
                user.Nickname = UserSession.Instance().Nickname;

                InstanceContext context = new InstanceContext(this);
                UserManager.LobbyManagerClient client = new UserManager.LobbyManagerClient(context);
                client.ConnectLobby(user, code);
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

        public void ShowConnectionInLobby(string user)
        {
            User newUser = new User();
            newUser.Nickname = user;

            AddCard(newUser);
        }

        public void ShowDisconnectionInLobby(string nickname)
        {
            throw new NotImplementedException();
        }

        public void ShowUsersInLobby(string[] userList)
        {
            for (int i = 0; i < userList.Length; i++)
            {
                User newUser = new User();
                newUser.Nickname = userList[i];

                AddCard(newUser);
            }
        }

        public void UpdateLobbyUserTeam(User user, int team)
        {
            throw new NotImplementedException();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

    }
}
