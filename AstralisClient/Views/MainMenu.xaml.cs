using Astralis.Logic;
using Astralis.UserManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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
using Astralis.Views.Pages;
using Astralis.Views.Animations;

namespace Astralis.Views
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Page, UserManager.IOnlineUserManagerCallback
    {
        private Dictionary<string, Tuple<bool, int>> friendList = new Dictionary<string, Tuple<bool, int>>();
        public delegate void CloseWindowEventHandler(object sender, EventArgs e);
        public event CloseWindowEventHandler CloseWindowEvent;
        private const bool ONLINE = true;
        private const bool OFFLINE = false;
        private const int IS_PENDING_FRIEND = 2;
        private const int IS_FRIEND = 1;

        public MainMenu()
        {
            InitializeComponent();
            Connect();
        }

        private void btnCreateGame_Click(object sender, RoutedEventArgs e)
        {
            Lobby lobby = new Lobby("host");
            NavigationService.Navigate(lobby);
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Disconnect();
            CloseWindowEvent?.Invoke(this, EventArgs.Empty);
        }

        public void Disconnect()
        {
            InstanceContext context = new InstanceContext(this);
            UserManager.OnlineUserManagerClient client = new UserManager.OnlineUserManagerClient(context);

            client.DisconectUser(UserSession.Instance().Nickname);
        }

        private void btnJoinGame_Click(object sender, RoutedEventArgs e)
        {
            string code = txtJoinCode.Text;

            Lobby lobby = new Lobby(code);
            NavigationService.Navigate(lobby);
        }

        private void btnMyProfile_Click(object sender, RoutedEventArgs e)
        {
            MyProfile myProfile = new MyProfile();
            NavigationService.Navigate(myProfile);
        }


        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings();
            NavigationService.Navigate(settings);
        }

        private void btnFriends_Click(object sender, RoutedEventArgs e)
        {
            var existingFriendWindow = gridFriendsWindow.Children.OfType<FriendWindow>().FirstOrDefault();

            if (existingFriendWindow == null)
            {
                FriendWindow friendWindow = new FriendWindow();
                friendWindow.SendFriendRequestEvent += SendFriendRequest;
                friendWindow.ReplyFriendRequestEvent += ReplyToFriendRequest;


                if (friendList.Count > 0)
                {
                    friendWindow.SetFriendWindow(friendList);
                }
                gridFriendsWindow.Children.Add(friendWindow);
                
            }
            else
            {
                if(existingFriendWindow.IsVisible == true)
                {
                    existingFriendWindow.Visibility = Visibility.Hidden;
                }

                else
                {
                    existingFriendWindow.SetFriendWindow(friendList);
                    existingFriendWindow.Visibility = Visibility.Visible;
                }

            }
        }

        private void Connect()
        {
            InstanceContext context = new InstanceContext(this);
            UserManager.OnlineUserManagerClient client = new UserManager.OnlineUserManagerClient(context);

            client.ConectUser(UserSession.Instance().Nickname);
        }

        public void ShowUserConected(string nickname)
        {
            if (friendList.ContainsKey(nickname))
            {

                if (friendList[nickname].Item2 == IS_FRIEND)
                {
                    Tuple<bool, int> friendTuple = new Tuple<bool, int>(ONLINE, IS_FRIEND);

                    friendList[nickname] = friendTuple;
                    var existingFriendWindow = gridFriendsWindow.Children.OfType<FriendWindow>().FirstOrDefault();

                    if (existingFriendWindow != null)
                    {
                        existingFriendWindow.SetFriendWindow(friendList);
                    }
                }
                else
                {
                    Tuple<bool, int> friendTuple = new Tuple<bool, int>(ONLINE, IS_PENDING_FRIEND);

                    friendList[nickname] = friendTuple;
                    var existingFriendWindow = gridFriendsWindow.Children.OfType<FriendWindow>().FirstOrDefault();

                    if (existingFriendWindow != null)
                    {
                        existingFriendWindow.SetFriendWindow(friendList);
                    }
                }
            }
        }

        public void ShowUserDisconected(string nickname)
        {
            if (friendList.ContainsKey(nickname))
            {
                Tuple<bool, int> friendTuple = new Tuple<bool, int>(OFFLINE, IS_FRIEND);

                friendList[nickname] = friendTuple;
                var existingFriendWindow = gridFriendsWindow.Children.OfType<FriendWindow>().FirstOrDefault();

                if (existingFriendWindow != null)
                {
                    existingFriendWindow.SetFriendWindow(friendList);
                }
            }
        }

        public void ShowOnlineFriends(Dictionary<string, Tuple<bool, int>> onlineFriends)
        {
            friendList = onlineFriends;
        }

        public void ShowFriendRequest (string nickname)
        {
            Tuple <bool, int> friendTuple = new Tuple<bool, int>(OFFLINE, IS_PENDING_FRIEND);

            friendList.Add(nickname, friendTuple);

            var existingFriendWindow = gridFriendsWindow.Children.OfType<FriendWindow>().FirstOrDefault();

            if (existingFriendWindow != null)
            {
                existingFriendWindow.SetFriendWindow(friendList);
            }
        }

        public void ShowFriendAccepted (string nickname)
        {
            Tuple<bool, int> friendTuple = new Tuple<bool, int>(ONLINE, IS_FRIEND);

            friendList.Add(nickname, friendTuple);

            var existingFriendWindow = gridFriendsWindow.Children.OfType<FriendWindow>().FirstOrDefault();

            if (existingFriendWindow != null)
            {
                existingFriendWindow.SetFriendWindow(friendList);
            }
        }

        private void SendFriendRequest(object sender, string friendUsername)
        {
            if (!string.IsNullOrEmpty(friendUsername))
            {
                InstanceContext context = new InstanceContext(this);

                using (OnlineUserManagerClient client = new OnlineUserManagerClient(context))
                {
                    bool requestSent = client.SendFriendRequest(UserSession.Instance().Nickname, friendUsername);

                    if (requestSent)
                    {
                        MessageBox.Show("Solicitud de amistad enviada con éxito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("No se pudo enviar la solicitud de amistad. Verifica que el usuario existe y no haya una solicitud pendiente.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Por favor, ingresa un nombre de usuario.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ReplyToFriendRequest(object sender, string friendUsername)
        {
            InstanceContext context = new InstanceContext(this);

            using (OnlineUserManagerClient client = new OnlineUserManagerClient(context))
            {
                bool requestAccepted = client.ReplyFriendRequest(UserSession.Instance().Nickname, friendUsername, true);

                if (requestAccepted)
                {
                    Tuple<bool, int> friendTuple = new Tuple<bool, int>(friendList[friendUsername].Item1, IS_FRIEND);
                    friendList[friendUsername] = friendTuple;

                    MessageBox.Show($"Has aceptado la solicitud de amistad de {friendUsername}.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    friendList.Remove(friendUsername);

                    MessageBox.Show($"No se pudo aceptar la solicitud de amistad de {friendUsername}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
