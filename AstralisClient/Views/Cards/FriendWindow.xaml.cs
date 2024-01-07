using Astralis.Logic;
using Astralis.UserManager;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;

namespace Astralis.Views.Cards
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

        private int _cardsAddedRow = 0;
        private string _typeFriendWindow = "";
        private Dictionary<string, Tuple<bool, int>> _friendList = new Dictionary<string, Tuple<bool, int>>();

        public event EventHandler<string> SendGameInvitation;

        public FriendWindow(string typeFriendWindow)
        {
            this._typeFriendWindow = typeFriendWindow;
            InitializeComponent();
            Connect();
            if(typeFriendWindow == LOBBY_WINDOW)
            {
                txtSearchUser.Visibility = Visibility.Collapsed;
                btnSendFriendRequest.Visibility = Visibility.Collapsed;
            }
        }

        private void Connect()
        {
            InstanceContext context = new InstanceContext(this);
            OnlineUserManagerClient client = new OnlineUserManagerClient(context);

            try
            {
                client.ConectUser(UserSession.Instance().Nickname);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
        }

        public void Disconnect()
        {
            InstanceContext context = new InstanceContext(this);
            OnlineUserManagerClient client = new OnlineUserManagerClient(context);

            try
            {
                client.DisconectUser(UserSession.Instance().Nickname);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
        }

        public void SetFriendWindow()
        {
            _cardsAddedRow = 0;
            gdFriends.Children.Clear();
            gdFriends.RowDefinitions.Clear();

            foreach (var friendEntry in _friendList)
            {
                if (friendEntry.Value.Item1 == IS_ONLINE && friendEntry.Value.Item2 == IS_FRIEND)
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

            if (_typeFriendWindow == LOBBY_WINDOW)
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
            
            if (_typeFriendWindow == LOBBY_WINDOW)
            {
                card.SetLobbyCard(friendOnlineKey, friendOnlineValue, friendStatus);
            }
            else
            {
                card.SetMainMenuCard(friendOnlineKey, friendOnlineValue, friendStatus);
            }

            Grid.SetRow(card, _cardsAddedRow);
            gdFriends.Children.Add(card);
            _cardsAddedRow++;

            RowDefinition rowDefinition = new RowDefinition();
            rowDefinition.Height = GridLength.Auto;
            gdFriends.RowDefinitions.Add(rowDefinition);
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
            SetFriendWindow();
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

        private void ReplyToFriendRequestEvent(object sender, Tuple<string, bool> e)
        {
            string friendUsername = e.Item1;
            bool reply = e.Item2;

            InstanceContext context = new InstanceContext(this);

            using (OnlineUserManagerClient client = new OnlineUserManagerClient(context))
            {
                try
                {
                    int requestReply = client.ReplyFriendRequest(UserSession.Instance().Nickname, friendUsername, reply);

                    if (requestReply == Constants.VALIDATION_SUCCESS)
                    {
                        if (reply == ACCEPTED_FRIEND)
                        {
                            Tuple<bool, int> friendTuple = new Tuple<bool, int>(_friendList[friendUsername].Item1, IS_FRIEND);
                            _friendList[friendUsername] = friendTuple;
                            SetFriendWindow();

                            MessageBox.Show(Properties.Resources.msgFriendRequestAccepted, "Astralis", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            RemoveFriendFromFriendList(friendUsername);
                            MessageBox.Show(Properties.Resources.msgFriendRequestDeclined, "Astralis", MessageBoxButton.OK, MessageBoxImage.Information);
                        }

                    }
                    else if (requestReply == Constants.ERROR)
                    {
                        MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                        Disconnect();
                        App.RestartApplication();
                    }
                }
                catch (CommunicationException)
                {
                    MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                    App.RestartApplication();
                }
                catch (TimeoutException)
                {
                    MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                    App.RestartApplication();
                }


            }
        }

        private void RemoveFriendEvent(object sender, string friendUsername)
        {
            InstanceContext context = new InstanceContext(this);

            using (OnlineUserManagerClient client = new OnlineUserManagerClient(context))
            {
                try
                {
                    int removedFriendAnswer = client.RemoveFriend(UserSession.Instance().Nickname, friendUsername);

                    if (removedFriendAnswer == Constants.VALIDATION_SUCCESS)
                    {
                        RemoveFriendFromFriendList(friendUsername);
                        MessageBox.Show(Properties.Resources.msgFriendRemoved, "Astralis", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (removedFriendAnswer == Constants.ERROR)
                    {
                        MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                        Disconnect();
                        App.RestartApplication();
                    }
                }
                catch (CommunicationException)
                {
                    MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                    App.RestartApplication();
                }
                catch (TimeoutException)
                {
                    MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                    App.RestartApplication();
                }
            }
        }

        private void SendGameInvitationEvent(object sender, string friendUsername)
        {
            SendGameInvitation?.Invoke(this, friendUsername);
        }

        public void FriendDeleted(string nickname)
        {
           RemoveFriendFromFriendList(nickname);
        }

        private void RemoveFriendFromFriendList(string nickname)
        {
            _friendList.Remove(nickname);
            SetFriendWindow();
        }

        private void BtnSendFriendRequestClick(object sender, RoutedEventArgs e)
        {
            string friendUsername = txtSearchUser.Text.Trim();

            if (friendUsername != UserSession.Instance().Nickname)
            {
                SendFriendRequest(friendUsername);
            }
            else
            {
                MessageBox.Show(Properties.Resources.msgUnableToSendFriendRequest, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SendFriendRequest(string friendUsername)
        {
            if (!string.IsNullOrEmpty(friendUsername))
            {
                InstanceContext context = new InstanceContext(this);

                using (OnlineUserManagerClient client = new OnlineUserManagerClient(context))
                {
                    try
                    {
                        int requestSent = client.SendFriendRequest(UserSession.Instance().Nickname, friendUsername);

                        if (requestSent == Constants.VALIDATION_SUCCESS)
                        {
                            MessageBox.Show(Properties.Resources.msgFriendRequestSuccessful, "Astralis", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else if (requestSent == Constants.ERROR)
                        {
                            MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                            Disconnect();
                            App.RestartApplication();
                        }
                        else
                        {
                            MessageBox.Show(Properties.Resources.msgUnableToSendFriendRequest, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (CommunicationException)
                    {
                        MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                        App.RestartApplication();
                    }
                    catch (TimeoutException)
                    {
                        MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                        App.RestartApplication();
                    }
                }
            }
            else
            {
                MessageBox.Show(Properties.Resources.msgUsernameMissing, "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
