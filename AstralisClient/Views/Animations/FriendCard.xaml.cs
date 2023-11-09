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
        public event EventHandler<string> ReplyToFriendRequestEvent;
        private const int IS_PENDING_FRIEND = 2;
        private const int IS_FRIEND = 1;
        private int friendStatus = 0;
        private bool onlineStatus = false; 

        public FriendCard()
        {
            InitializeComponent();
        }

        public void SetCard(string nickname, bool onlineStatus, int friendStatus)
        {
            lblNickname.Content = nickname;

            this.friendStatus = friendStatus;
            this.onlineStatus = onlineStatus;

            if (friendStatus == IS_FRIEND)
            {
                if (onlineStatus == true)
                {
                    ellipseOnlineStatus.Fill = System.Windows.Media.Brushes.Green;
                }

                btnActionFriend.Visibility = Visibility.Hidden;
            }
            else
            {
                ellipseOnlineStatus.Fill = System.Windows.Media.Brushes.Yellow;
            }
            
        }


        private void btnActionFriend_Click(object sender, RoutedEventArgs e)
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

            btnActionFriend.Visibility = Visibility.Hidden;

            ReplyToFriendRequestEvent?.Invoke(this, friendUsername);
        }
    }
}
