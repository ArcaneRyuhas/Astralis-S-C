using Astralis.Logic;
using Astralis.UserManager;
using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace Astralis.Views.Animations
{
    /// <summary>
    /// Interaction logic for FriendCard.xaml
    /// </summary>
    public partial class FriendCard : UserControl
    {
        public event EventHandler<Tuple<string, bool>> ReplyToFriendRequestEvent;
        public event EventHandler<string> RemoveFriendEvent;
        public event EventHandler<string> SendGameInvitation;
        private const int IS_PENDING_FRIEND = 2;
        private const int IS_FRIEND = 1;
        private int friendStatus = 0;
        private bool onlineStatus = false;
        private const bool ACCEPTED_FRIEND = true;
        private const bool DENIED_FRIEND = false;
        private const string LOBBY_WINDOW = "LOBBY";
        public FriendCard()
        {
            InitializeComponent();
        }


        public void SetLobbyCard(string nickname, bool onlineStatus, int friendStatus)
        {
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
            if (friendStatus == IS_FRIEND)
            {
                if (onlineStatus == true)
                {
                    ellipseOnlineStatus.Fill = System.Windows.Media.Brushes.Green;
                    btnDeleteFriend.Visibility = Visibility.Visible;
                }

            }
            else
            {
                ellipseOnlineStatus.Fill = System.Windows.Media.Brushes.Yellow;
                btnAcceptFriendRequest.Visibility = Visibility.Visible;
                btnDenyFriendRequest.Visibility = Visibility.Visible;
            }
        }

        private void btnAcceptFriendRequest_Click(object sender, RoutedEventArgs e)
        {
            string friendUsername = lblNickname.Content.ToString();

            if (onlineStatus == true)
            {
                ellipseOnlineStatus.Fill = System.Windows.Media.Brushes.Green;
            }
            else
            {
                ellipseOnlineStatus.Fill = System.Windows.Media.Brushes.Red;
            }

            btnAcceptFriendRequest.Visibility = Visibility.Hidden; //PREGUNTAR A MARIO POR ESTO

            ReplyToFriendRequestEvent?.Invoke(this, new Tuple<string, bool>(friendUsername, ACCEPTED_FRIEND));
        }

        private void btnDeleteFriend_Click(object sender, RoutedEventArgs e)
        {
            string friendUsername = lblNickname.Content.ToString();

            btnAcceptFriendRequest.Visibility = Visibility.Hidden;

            RemoveFriendEvent?.Invoke(this, friendUsername);
        }

        private void btnDenyFriendRequest_Click(object sender, RoutedEventArgs e)
        {
            string friendUsername = lblNickname.Content.ToString();

            if (onlineStatus == true)
            {
                ellipseOnlineStatus.Fill = System.Windows.Media.Brushes.Green;
            }
            else
            {
                ellipseOnlineStatus.Fill = System.Windows.Media.Brushes.Red;
            }

            btnAcceptFriendRequest.Visibility = Visibility.Hidden;

            ReplyToFriendRequestEvent?.Invoke(this, new Tuple<string, bool>(friendUsername, DENIED_FRIEND));
        }

        private void btnSendGameInvitation_Click(object sender, RoutedEventArgs e)
        {
            string friendUsername = lblNickname.Content.ToString();

            btnSendGameInvitation.Visibility = Visibility.Hidden;

            SendGameInvitation?.Invoke(this, friendUsername);
        }
    }
}
