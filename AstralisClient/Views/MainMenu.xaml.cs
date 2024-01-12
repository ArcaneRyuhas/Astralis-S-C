using Astralis.Logic;
using Astralis.Views.Cards;
using Astralis.Views.Pages;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Astralis.Views
{
    public partial class MainMenu : Page
    {
        public delegate void CloseWindowEventHandler(object sender, EventArgs e);
        public event CloseWindowEventHandler CloseWindowEvent;

        private const string IS_HOST = "host";
        private const string MAIN_MENU_WINDOW = "MAIN_MENU";
        private const int MAX_FIELDS_LENGHT = 10;

        private FriendWindow _friendWindow;
        private readonly GameWindow _gameWindow;

        public MainMenu(GameWindow gameWindow)
        {
            InitializeComponent();
            AddFriendWindow();

            _gameWindow = gameWindow;
            btnMyProfile.Content = UserSession.Instance().Nickname;

        }

        private void AddFriendWindow()
        {
            _friendWindow = new FriendWindow(MAIN_MENU_WINDOW)
            {
                Visibility = Visibility.Hidden
            };

            _friendWindow.SetFriendWindow();
            gridFriendsWindow.Children.Add(_friendWindow);
        }

        private void BtnCreateGameClick(object sender, RoutedEventArgs e)
        {
            Lobby lobby = new Lobby(_gameWindow);
            int canPlay = lobby.CanAccessToLobby();

            if (canPlay == Constants.VALIDATION_SUCCESS)
            {
                if (lobby.SetLobby(IS_HOST))
                {
                    NavigationService.Navigate(lobby);
                }
            }
            else if (canPlay == Constants.VALIDATION_FAILURE)
            {
                MessageBox.Show(Properties.Resources.msgBanned, "Astralis", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(Properties.Resources.msgUnableToAnswer, "Astralis", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExitClick(object sender, RoutedEventArgs e)
        {
            _friendWindow.UnsubscribeFromFriendManager();
            CloseWindowEvent?.Invoke(this, EventArgs.Empty);
        }

        private void BtnJoinGameClick(object sender, RoutedEventArgs e)
        {
            string code = txtJoinCode.Text;
            Lobby lobby = new Lobby(_gameWindow);
            int canPlay = lobby.CanAccessToLobby();

            if (lobby.CanAccessToLobby() == Constants.VALIDATION_SUCCESS)
            {
                if (lobby.GameIsNotFull(code))
                {
                    if (lobby.SetLobby(code))
                    {
                        NavigationService.Navigate(lobby);
                    }
                }
                else
                {
                    MessageBox.Show(Properties.Resources.msgGameIsFull, "Astralis", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (canPlay == Constants.VALIDATION_FAILURE)
            {
                MessageBox.Show(Properties.Resources.msgBanned, "Astralis", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(Properties.Resources.msgUnableToAnswer, "Astralis", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnMyProfileClick(object sender, RoutedEventArgs e)
        {
            MyProfile myProfile = new MyProfile(_friendWindow);

            NavigationService.Navigate(myProfile);
        }

        private void BtnSettingsClick(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings();

            NavigationService.Navigate(settings);
        }

        private void BtnFriendsClick(object sender, RoutedEventArgs e)
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

        private void BtnLeaderboardClick(object sender, RoutedEventArgs e)
        {
            LeaderBoard leaderboard = new LeaderBoard();
            NavigationService.Navigate(leaderboard);
        }

        private void TextLimeterForCode(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Length >= MAX_FIELDS_LENGHT)
            {
                e.Handled = true;
            }
        }
    }
}
