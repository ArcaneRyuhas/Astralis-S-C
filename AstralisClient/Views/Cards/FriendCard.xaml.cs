using System;
using System.Windows;
using System.Windows.Controls;


namespace Astralis.Views.Cards
{
    /// <summary>
    /// Interaction logic for FriendCard.xaml
    /// </summary>
    public partial class FriendCard : UserControl
    {
        public event EventHandler<Tuple<string, bool>> ReplyToFriendRequestEvent;
        public event EventHandler<string> RemoveFriendEvent;
        public event EventHandler<string> SendGameInvitation;

        private bool _onlineStatus = false;

        private const int IS_FRIEND = 1;
        private const bool ACCEPTED_FRIEND = true;
        private const bool DENIED_FRIEND = false;

        public FriendCard()
        {
            InitializeComponent();
        }

        public void SetLobbyCard(string nickname, bool onlineStatus, int friendStatus)
        {
            lblNickname.Content = nickname;
            if (friendStatus == IS_FRIEND)
            {
                if (onlineStatus == true)
                {
                    ellipseOnlineStatus.Fill = System.Windows.Media.Brushes.Green;
                }
            }
            btnSendGameInvitation.Visibility = Visibility.Visible;
        }

        public void SetMainMenuCard(string nickname, bool onlineStatus, int friendStatus)
        {
            lblNickname.Content = nickname;
            if (friendStatus == IS_FRIEND)
            {
                if (onlineStatus == true)
                {
                    ellipseOnlineStatus.Fill = System.Windows.Media.Brushes.Green;
                }
                btnDeleteFriend.Visibility = Visibility.Visible;
            }
            else
            {
                ellipseOnlineStatus.Fill = System.Windows.Media.Brushes.Yellow;
                btnAcceptFriendRequest.Visibility = Visibility.Visible;
                btnDenyFriendRequest.Visibility = Visibility.Visible;
            }
        }

        private void BtnAcceptFriendRequestClick(object sender, RoutedEventArgs e)
        {
            string friendUsername = lblNickname.Content.ToString();

            if (_onlineStatus == true)
            {
                ellipseOnlineStatus.Fill = System.Windows.Media.Brushes.Green;
            }
            else
            {
                ellipseOnlineStatus.Fill = System.Windows.Media.Brushes.Red;
            }

            btnAcceptFriendRequest.Visibility = Visibility.Hidden;
            btnDenyFriendRequest.Visibility = Visibility.Hidden;

            ReplyToFriendRequestEvent?.Invoke(this, new Tuple<string, bool>(friendUsername, ACCEPTED_FRIEND));
        }

        private void BtnDeleteFriendClick(object sender, RoutedEventArgs e)
        {
            string friendUsername = lblNickname.Content.ToString();

            btnAcceptFriendRequest.Visibility = Visibility.Hidden;

            RemoveFriendEvent?.Invoke(this, friendUsername);
        }

        private void BtnDenyFriendRequestClick(object sender, RoutedEventArgs e)
        {
            string friendUsername = lblNickname.Content.ToString();

            if (_onlineStatus == true)
            {
                ellipseOnlineStatus.Fill = System.Windows.Media.Brushes.Green;
            }
            else
            {
                ellipseOnlineStatus.Fill = System.Windows.Media.Brushes.Red;
            }

            btnAcceptFriendRequest.Visibility = Visibility.Hidden;
            btnAcceptFriendRequest.Visibility = Visibility.Hidden;

            ReplyToFriendRequestEvent?.Invoke(this, new Tuple<string, bool>(friendUsername, DENIED_FRIEND));
        }

        private void BtnSendGameInvitationClick(object sender, RoutedEventArgs e)
        {
            string friendUsername = lblNickname.Content.ToString();

            btnSendGameInvitation.Visibility = Visibility.Hidden;

            SendGameInvitation?.Invoke(this, friendUsername);
        }
    }
}
