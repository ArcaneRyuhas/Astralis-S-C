using Astralis.Logic;
using Astralis.UserManager;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        public FriendCard()
        {
            InitializeComponent();
        }

        public void SetCard(string nickname, bool onlineStatus, bool friendStatus)
        {
            lblNickname.Content = nickname;

            if (onlineStatus == true)
            {
                ellipseOnlineStatus.Fill = System.Windows.Media.Brushes.Green;
            }   

        }


        private void btnActionFriend_Click(object sender, RoutedEventArgs e)
        {
            string friendUsername = lblNickname.Content.ToString();

            using (UserManagerClient client = new UserManagerClient())
            {
                bool requestAccepted = client.ReplyFriendRequest(UserSession.Instance().Nickname, friendUsername, true);

                if (requestAccepted)
                {
                    MessageBox.Show($"Has aceptado la solicitud de amistad de {friendUsername}.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"No se pudo aceptar la solicitud de amistad de {friendUsername}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
