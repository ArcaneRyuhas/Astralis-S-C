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

namespace Astralis.Views.Animations
{
    /// <summary>
    /// Interaction logic for FriendWindow.xaml
    /// </summary>
    public partial class FriendWindow : UserControl
    {
        private const bool REQUEST_PENDING = false;
        private const bool IS_FRIEND = true;
        private const bool IS_ONLINE = true;
        private const bool IS_OFFLINE = false;
        private int cardsAddedRow = 0;
        public event EventHandler<string> SendFriendRequestEvent;
        public event EventHandler<string> ReplyFriendRequestEvent;

        public FriendWindow()
        {
            InitializeComponent();
        }

        public void SetFriendWindow(Dictionary<string, Tuple<bool, int>> friendList)
        {
            cardsAddedRow = 0;
            gdFriends.Children.Clear();
            gdFriends.RowDefinitions.Clear();

            foreach (var friendEntry in friendList)
            {
                if(friendEntry.Value.Item1 == IS_ONLINE)
                {
                    AddFriendRow(friendEntry.Key, friendEntry.Value.Item1);
                }
            }

            foreach (var friendEntry in friendList)
            {
                if (friendEntry.Value.Item1 == IS_OFFLINE)
                {
                    AddFriendRow(friendEntry.Key, friendEntry.Value.Item1);
                }
            }


            RowDefinition lastRowDefinition =new RowDefinition();
            lastRowDefinition.Height = new GridLength(1, GridUnitType.Star);
            gdFriends.RowDefinitions.Add(lastRowDefinition);
        }

        private void AddFriendRow(string friendOnlineKey, bool friendOnlineValue)
        {
            FriendCard card = new FriendCard();
            card.ReplyToFriendRequestEvent += ReplyToFriendRequestEvent;
            card.SetCard(friendOnlineKey, friendOnlineValue, IS_FRIEND); //A CAMBIAR NO SE VAYA A OLVIDAR
            Grid.SetRow(card, cardsAddedRow);
            gdFriends.Children.Add(card);
            cardsAddedRow++;

            RowDefinition rowDefinition = new RowDefinition();
            rowDefinition.Height = GridLength.Auto;
            gdFriends.RowDefinitions.Add(rowDefinition);
        }

        private void btnSendFriendRequest_Click(object sender, RoutedEventArgs e)
        {
            string friendUsername = txtSearchUser.Text.Trim();

            SendFriendRequestEvent?.Invoke(this, friendUsername);
        }

        private void ReplyToFriendRequestEvent(object sender, string friendUsername)
        {
            ReplyFriendRequestEvent?.Invoke(this, friendUsername);
        }
    }
}
