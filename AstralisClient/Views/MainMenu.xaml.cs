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
        private Dictionary<string, bool> friendList = new Dictionary<string, bool>();
        public delegate void CloseWindowEventHandler(object sender, EventArgs e);
        public event CloseWindowEventHandler CloseWindowEvent;
        private const bool IS_ONLINE = true;
        private const bool IS_OFFLINE = false;

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
            var existingFriendWindow = gridFirendsWindow.Children.OfType<FriendWindow>().FirstOrDefault();

            if (existingFriendWindow == null)
            {
                FriendWindow friendWindow = new FriendWindow();
                if(friendList.Count > 0)
                {
                    friendWindow.SetFriendWindow(friendList);
                }
                gridFirendsWindow.Children.Add(friendWindow);
                
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
                friendList[nickname] = IS_ONLINE;
                var existingFriendWindow = gridFirendsWindow.Children.OfType<FriendWindow>().FirstOrDefault();
                
                if(existingFriendWindow != null)
                {
                    existingFriendWindow.SetFriendWindow(friendList);
                }
            }
        }

        public void ShowUserDisconected(string nickname)
        {
            if (friendList.ContainsKey(nickname))
            {
                friendList[nickname] = IS_OFFLINE;
                var existingFriendWindow = gridFirendsWindow.Children.OfType<FriendWindow>().FirstOrDefault();

                if (existingFriendWindow != null)
                {
                    existingFriendWindow.SetFriendWindow(friendList);
                }
            }
        }

        public void ShowOnlineFriends(Dictionary<string, bool> onlineFriends)
        {
            friendList = onlineFriends;
        }
    }
}
