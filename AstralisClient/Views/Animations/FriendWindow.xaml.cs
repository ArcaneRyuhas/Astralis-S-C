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
    public partial class FriendWindow : UserControl, UserManager.IOnlineUserManagerCallback
    {
        private const int IS_PENDING_FRIEND = 2;
        private const int IS_FRIEND = 1;
        private const bool IS_ONLINE = true;
        private const bool OFFLINE = false;
        private const bool ONLINE = true;
        private const bool IS_OFFLINE = false;
        private int cardsAddedRow = 0;
        private Dictionary<string, Tuple<bool, int>> friendList = new Dictionary<string, Tuple<bool, int>>();

        public FriendWindow()
        {
            InitializeComponent();
            Connect();
        }

        private void Connect()
        {
            InstanceContext context = new InstanceContext(this);
            UserManager.OnlineUserManagerClient client = new UserManager.OnlineUserManagerClient(context);

            client.ConectUser(UserSession.Instance().Nickname);
        }

        public void Disconnect()
        {
            InstanceContext context = new InstanceContext(this);
            UserManager.OnlineUserManagerClient client = new UserManager.OnlineUserManagerClient(context);

            client.DisconectUser(UserSession.Instance().Nickname);
        }

        public void SetFriendWindow()
        {
            cardsAddedRow = 0;
            gdFriends.Children.Clear();
            gdFriends.RowDefinitions.Clear();

            foreach (var friendEntry in friendList)
            {
                if(friendEntry.Value.Item1 == IS_ONLINE && friendEntry.Value.Item2 == IS_FRIEND)
                {
                    AddFriendRow(friendEntry.Key, friendEntry.Value.Item1, friendEntry.Value.Item2);
                }
            }

            foreach (var friendEntry in friendList)
            {
                if (friendEntry.Value.Item1 == IS_OFFLINE && friendEntry.Value.Item2 == IS_FRIEND)
                {
                    AddFriendRow(friendEntry.Key, friendEntry.Value.Item1, friendEntry.Value.Item2);
                }
            }

            foreach (var friendEntry in friendList)
            {
                if (friendEntry.Value.Item2 == IS_PENDING_FRIEND)
                {
                    AddFriendRow(friendEntry.Key, friendEntry.Value.Item1, friendEntry.Value.Item2);
                }
            }

            RowDefinition lastRowDefinition =new RowDefinition();
            lastRowDefinition.Height = new GridLength(1, GridUnitType.Star);
            gdFriends.RowDefinitions.Add(lastRowDefinition);
        }

        private void AddFriendRow(string friendOnlineKey, bool friendOnlineValue, int friendStatus)
        {
            FriendCard card = new FriendCard();
            card.ReplyToFriendRequestEvent += ReplyToFriendRequestEvent;
            card.SetCard(friendOnlineKey, friendOnlineValue, friendStatus);
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
            
            if(friendUsername != UserSession.Instance().Nickname)
            {
                SendFriendRequest(friendUsername);
            }
            else
            {
                MessageBox.Show($"No andes loqueando {friendUsername} chavito.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ShowUserConected(string nickname)
        {
            if (friendList.ContainsKey(nickname))
            {

                if (friendList[nickname].Item2 == IS_FRIEND)
                {
                    Tuple<bool, int> friendTuple = new Tuple<bool, int>(ONLINE, IS_FRIEND);
                    friendList[nickname] = friendTuple;

                    SetFriendWindow();

                }
                else
                {
                    Tuple<bool, int> friendTuple = new Tuple<bool, int>(ONLINE, IS_PENDING_FRIEND);
                    friendList[nickname] = friendTuple;

                    SetFriendWindow();

                }
            }
        }

        public void ShowUserDisconected(string nickname)
        {
            if (friendList.ContainsKey(nickname))
            {

                if (friendList[nickname].Item2 == IS_FRIEND)
                {
                    Tuple<bool, int> friendTuple = new Tuple<bool, int>(OFFLINE, IS_FRIEND);
                    friendList[nickname] = friendTuple;

                    SetFriendWindow();
                }
                else
                {
                    Tuple<bool, int> friendTuple = new Tuple<bool, int>(OFFLINE, IS_PENDING_FRIEND);
                    friendList[nickname] = friendTuple;

                    SetFriendWindow();

                }
            }
        }

        public void ShowOnlineFriends(Dictionary<string, Tuple<bool, int>> onlineFriends)
        {
            friendList = onlineFriends;
        }

        public void ShowFriendRequest(string nickname)
        {
            Tuple<bool, int> friendTuple = new Tuple<bool, int>(OFFLINE, IS_PENDING_FRIEND);

            friendList.Add(nickname, friendTuple);

            SetFriendWindow();
        }

        public void ShowFriendAccepted(string nickname)
        {
            Tuple<bool, int> friendTuple = new Tuple<bool, int>(ONLINE, IS_FRIEND);

            friendList.Add(nickname, friendTuple);

            SetFriendWindow();
        }

        private void SendFriendRequest(string friendUsername)
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

        private void ReplyToFriendRequestEvent(object sender, string friendUsername)
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
