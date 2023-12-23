using Astralis.Logic;
using Astralis.UserManager;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;

namespace Astralis.Views.Animations
{

    public partial class FriendWindow : UserControl, UserManager.IOnlineUserManagerCallback
    {
        private const int IS_PENDING_FRIEND = 2;
        private const int IS_FRIEND = 1;
        private const bool IS_ONLINE = true;
        private const bool OFFLINE = false;
        private const bool ONLINE = true;
        private const bool IS_OFFLINE = false;
        private const bool ACCEPTED_FRIEND = true;
        private const string LOBBY_WINDOW = "LOBBY";
        private const string MAIN_MENU_WINDOW = "MAIN_MENU";
        private int _cardsAddedRow = 0;
        private string typeFriendWindow = "";

        private Dictionary<string, Tuple<bool, int>> _friendList = new Dictionary<string, Tuple<bool, int>>();

        public FriendWindow(string typeFriendWindow)
        {
            this.typeFriendWindow = typeFriendWindow;
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
            _cardsAddedRow = 0;
            gdFriends.Children.Clear();
            gdFriends.RowDefinitions.Clear();

            foreach (var friendEntry in _friendList)
            {
                if(friendEntry.Value.Item1 == IS_ONLINE && friendEntry.Value.Item2 == IS_FRIEND)
                {
                    AddFriendRow(friendEntry.Key, friendEntry.Value.Item1, friendEntry.Value.Item2);
                }
            }

            foreach (var friendEntry in _friendList)
            {
                if (friendEntry.Value.Item1 == IS_OFFLINE && friendEntry.Value.Item2 == IS_FRIEND)
                {
                    AddFriendRow(friendEntry.Key, friendEntry.Value.Item1, friendEntry.Value.Item2);
                }
            }

            foreach (var friendEntry in _friendList)
            {
                if (friendEntry.Value.Item2 == IS_PENDING_FRIEND)
                {
                    AddFriendRow(friendEntry.Key, friendEntry.Value.Item1, friendEntry.Value.Item2);
                }
            }

            RowDefinition lastRowDefinition = new RowDefinition();
            lastRowDefinition.Height = new GridLength(1, GridUnitType.Star);
            gdFriends.RowDefinitions.Add(lastRowDefinition);

            if(typeFriendWindow == LOBBY_WINDOW)
            {
                txtSearchUser.Visibility = Visibility.Collapsed;
            }
        }

        private void AddFriendRow(string friendOnlineKey, bool friendOnlineValue, int friendStatus)
        {
            FriendCard card = new FriendCard();
            card.ReplyToFriendRequestEvent += ReplyToFriendRequestEvent;
            card.RemoveFriendEvent += RemoveFriendEvent;
            card.SendGameInvitation += SendGameInvitationEvent;
            card.SetCard(friendOnlineKey, friendOnlineValue, friendStatus, typeFriendWindow);
            Grid.SetRow(card, _cardsAddedRow);
            gdFriends.Children.Add(card);
            _cardsAddedRow++;

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
            if (_friendList.ContainsKey(nickname))
            {

                if (_friendList[nickname].Item2 == IS_FRIEND)
                {
                    Tuple<bool, int> friendTuple = new Tuple<bool, int>(ONLINE, IS_FRIEND);
                    _friendList[nickname] = friendTuple;

                    SetFriendWindow();

                }
                else
                {
                    Tuple<bool, int> friendTuple = new Tuple<bool, int>(ONLINE, IS_PENDING_FRIEND);
                    _friendList[nickname] = friendTuple;

                    SetFriendWindow();

                }
            }
        }

        public void ShowUserDisconected(string nickname)
        {
            if (_friendList.ContainsKey(nickname))
            {

                if (_friendList[nickname].Item2 == IS_FRIEND)
                {
                    Tuple<bool, int> friendTuple = new Tuple<bool, int>(OFFLINE, IS_FRIEND);
                    _friendList[nickname] = friendTuple;

                    SetFriendWindow();
                }
                else
                {
                    Tuple<bool, int> friendTuple = new Tuple<bool, int>(OFFLINE, IS_PENDING_FRIEND);
                    _friendList[nickname] = friendTuple;

                    SetFriendWindow();

                }
            }
        }

        public void ShowOnlineFriends(Dictionary<string, Tuple<bool, int>> onlineFriends)
        {
            _friendList = onlineFriends;
        }

        public void ShowFriendRequest(string nickname)
        {
            Tuple<bool, int> friendTuple = new Tuple<bool, int>(ONLINE, IS_PENDING_FRIEND);

            _friendList.Add(nickname, friendTuple);

            SetFriendWindow();
        }

        public void ShowFriendAccepted(string nickname)
        {
            Tuple<bool, int> friendTuple = new Tuple<bool, int>(ONLINE, IS_FRIEND);

            _friendList.Add(nickname, friendTuple);

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

        private void ReplyToFriendRequestEvent(object sender, Tuple<string, bool> e)
        {
            string friendUsername = e.Item1;
            bool reply = e.Item2;

            InstanceContext context = new InstanceContext(this);

            using (OnlineUserManagerClient client = new OnlineUserManagerClient(context))
            {
                bool requestReplySucceded = client.ReplyFriendRequest(UserSession.Instance().Nickname, friendUsername, reply);

                if (requestReplySucceded && reply == ACCEPTED_FRIEND)
                {
                    Tuple<bool, int> friendTuple = new Tuple<bool, int>(_friendList[friendUsername].Item1, IS_FRIEND);
                    _friendList[friendUsername] = friendTuple;

                    MessageBox.Show($"Has aceptado la solicitud de amistad de {friendUsername}.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"No se pudo aceptar la solicitud de amistad de {friendUsername}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                if (!requestReplySucceded && reply != ACCEPTED_FRIEND)
                {
                    MessageBox.Show($"No se pudo rechazar la solicitud de amistad de {friendUsername}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RemoveFriendEvent(object sender, string friendUsername)
        {
            InstanceContext context = new InstanceContext(this);

            using (OnlineUserManagerClient client = new OnlineUserManagerClient(context))
            {
                bool removedFriendSucceded = client.RemoveFriend(UserSession.Instance().Nickname, friendUsername);

                if (removedFriendSucceded)
                {
                    MessageBox.Show($"Has eliminado de tus amigos a {friendUsername}.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void SendGameInvitationEvent(object sender, string e)
        {
            throw new NotImplementedException();
        }
    }
}
