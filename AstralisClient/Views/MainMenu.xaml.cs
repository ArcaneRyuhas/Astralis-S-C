using Astralis.Logic;
using Astralis.Views.Animations;
using Astralis.Views.Pages;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Astralis.Views
{
    public partial class MainMenu : Page
    {
       
        public delegate void CloseWindowEventHandler(object sender, EventArgs e);
        public event CloseWindowEventHandler CloseWindowEvent;

        private const string IS_HOST = "host";
        private const string MAIN_MENU_WINDOW = "MAIN_MENU";

        private FriendWindow friendWindow;
        private GameWindow _gameWindow;

        public MainMenu(GameWindow gameWindow)
        {
            _gameWindow = gameWindow;
            InitializeComponent();
            friendWindow = new FriendWindow(MAIN_MENU_WINDOW);
            friendWindow.SetFriendWindow();
            gridFriendsWindow.Children.Add(friendWindow);
            btnMyProfile.Content = UserSession.Instance().Nickname;

        }

        private void btnCreateGame_Click(object sender, RoutedEventArgs e)
        {
            Lobby lobby = new Lobby(_gameWindow);


            if (lobby.SetLobby(IS_HOST))
            {
                NavigationService.Navigate(lobby);
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            friendWindow.Disconnect();
            CloseWindowEvent?.Invoke(this, EventArgs.Empty);
        }

      

        private void btnJoinGame_Click(object sender, RoutedEventArgs e)
        {
            string code = txtJoinCode.Text;

            Lobby lobby = new Lobby(_gameWindow);
            if (lobby.GameIsNotFull(code)  && lobby.SetLobby(code))
            {
                NavigationService.Navigate(lobby);
            }
            else
            {
                MessageBox.Show("msgGameIsFullOrLobbyDoesntExist", "titleLobbyDoesntExist", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnMyProfile_Click(object sender, RoutedEventArgs e)
        {
            MyProfile myProfile = new MyProfile(friendWindow);
            NavigationService.Navigate(myProfile);
        }


        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings();
            NavigationService.Navigate(settings);
        }

        private void btnFriends_Click(object sender, RoutedEventArgs e)
        {
            if (friendWindow.IsVisible == true)
            {
                friendWindow.Visibility = Visibility.Hidden;
            }

            else
            {
                friendWindow.SetFriendWindow();
                friendWindow.Visibility = Visibility.Visible;
            }
        }

        private void BtnLeaderboardClick(object sender, RoutedEventArgs e)
        {
            LeaderBoard leaderboard = new LeaderBoard();
            NavigationService.Navigate(leaderboard);
        }
    }
}
