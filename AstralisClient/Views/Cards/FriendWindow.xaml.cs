using Astralis.Logic;
using Astralis.UserManager;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Astralis.Views.Cards
{

    public partial class FriendWindow : UserControl, IFriendManagerCallback
    {
        private const int IS_PENDING_FRIEND = 2;
        private const int IS_FRIEND = 1;
        private const bool IS_ONLINE = true;
        private const bool OFFLINE = false;
        private const bool ONLINE = true;
        private const bool IS_OFFLINE = false;
        private const bool ACCEPTED_FRIEND = true;
        private const string LOBBY_WINDOW = "LOBBY";
        private const int STARTING_VALUE_FOR_ROWS = 0;
        private const int MAX_FIELDS_LENGHT = 30;


        private int _cardsAddedRow = 0;
        private readonly string _typeFriendWindow;
        private FriendManagerClient _client;
        private Dictionary<string, Tuple<bool, int>> _friendList = new Dictionary<string, Tuple<bool, int>>();

        public event EventHandler<string> SendGameInvitation;

        public FriendWindow(string typeFriendWindow)
        {
            this._typeFriendWindow = typeFriendWindow;

            InitializeComponent();
            SubscribeToFriendManager();

            if(typeFriendWindow == LOBBY_WINDOW)
            {
                txtSearchUser.Visibility = Visibility.Collapsed;
                btnSendFriendRequest.Visibility = Visibility.Collapsed;
            }
        }

        private void SubscribeToFriendManager()
        {
            InstanceContext context = new InstanceContext(this);
            _client = new FriendManagerClient(context);

            try
            {
                _client.SubscribeToFriendManager(UserSession.Instance().Nickname);
            }
            catch (CommunicationObjectFaultedException)
            {
                MessageBox.Show(Properties.Resources.msgPreviousConnectioLost, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Information);
                App.RestartApplication();
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

        public void UnsubscribeFromFriendManager()
        {
            try
            {
                _client.UnsubscribeToFriendManager(UserSession.Instance().Nickname);
            }
            catch (CommunicationObjectFaultedException)
            {
                MessageBox.Show(Properties.Resources.msgPreviousConnectioLost, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Information);
                App.RestartApplication();
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

        public void SetFriendWindow()
        {
            _cardsAddedRow = STARTING_VALUE_FOR_ROWS;

            gdFriends.Children.Clear();
            gdFriends.RowDefinitions.Clear();

            foreach (KeyValuePair<string, Tuple<bool, int>> friendEntry in _friendList)
            {
                if (friendEntry.Value.Item1 == IS_ONLINE && friendEntry.Value.Item2 == IS_FRIEND)
                {
                    AddFriendWindowCard(friendEntry.Key, friendEntry.Value.Item1, friendEntry.Value.Item2);
                }
            }

            foreach (KeyValuePair<string, Tuple<bool, int>> friendEntry in _friendList)
            {
                if (friendEntry.Value.Item1 == IS_OFFLINE && friendEntry.Value.Item2 == IS_FRIEND)
                {
                    AddFriendWindowCard(friendEntry.Key, friendEntry.Value.Item1, friendEntry.Value.Item2);
                }
            }

            foreach (KeyValuePair<string, Tuple<bool, int>> friendEntry in _friendList)
            {
                if (friendEntry.Value.Item2 == IS_PENDING_FRIEND)
                {
                    AddFriendWindowCard(friendEntry.Key, friendEntry.Value.Item1, friendEntry.Value.Item2);
                }
            }

            CreateLastFriendWindowRow();

            if (_typeFriendWindow == LOBBY_WINDOW)
            {
                txtSearchUser.Visibility = Visibility.Collapsed;
            }
        }

        private void CreateLastFriendWindowRow()
        {
            RowDefinition lastRowDefinition = new RowDefinition();
            lastRowDefinition.Height = new GridLength(1, GridUnitType.Star);

            gdFriends.RowDefinitions.Add(lastRowDefinition);
        }

        private void AddFriendWindowCard(string friendOnlineKey, bool friendOnlineValue, int friendStatus)
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

            AddFriendRow();
        }

        private void AddFriendRow()
        {
            RowDefinition rowDefinition = new RowDefinition();
            rowDefinition.Height = GridLength.Auto;

            gdFriends.RowDefinitions.Add(rowDefinition);
        }

        private void ReplyToFriendRequestEvent(object sender, Tuple<string, bool> friendReply)
        {
            string friendUsername = friendReply.Item1;
            bool reply = friendReply.Item2;

            try
            {
                int requestReply = _client.ReplyFriendRequest(UserSession.Instance().Nickname, friendUsername, reply);

                ValidateFriendRequest(requestReply, friendReply);
            }
            catch (CommunicationObjectFaultedException)
            {
                MessageBox.Show(Properties.Resources.msgPreviousConnectioLost, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
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

        private void ValidateFriendRequest(int requestReply, Tuple<string, bool> friendReply)
        {
            string friendUsername = friendReply.Item1;
            bool reply = friendReply.Item2;

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
                MessageBox.Show(Properties.Resources.msgUnableToAnswer, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                UnsubscribeFromFriendManager();
                App.RestartApplication();
            }
        }

        private void RemoveFriendEvent(object sender, string friendUsername)
        {
            try
            {
                int removedFriendAnswer = _client.RemoveFriend(UserSession.Instance().Nickname, friendUsername);

                ValidateFriendRemoved(friendUsername, removedFriendAnswer);
            }
            catch (CommunicationObjectFaultedException)
            {
                MessageBox.Show(Properties.Resources.msgPreviousConnectioLost, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
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

        private void ValidateFriendRemoved(string friendUsername, int removedFriendAnswer)
        {
            if (removedFriendAnswer == Constants.VALIDATION_SUCCESS)
            {
                RemoveFriendFromFriendList(friendUsername);
                MessageBox.Show(Properties.Resources.msgFriendRemoved, "Astralis", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (removedFriendAnswer == Constants.ERROR)
            {
                MessageBox.Show(Properties.Resources.msgUnableToAnswer, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                UnsubscribeFromFriendManager();
                App.RestartApplication();
            }
        }

        private void SendGameInvitationEvent(object sender, string friendUsername)
        {
            SendGameInvitation?.Invoke(this, friendUsername);
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
                try
                {
                    int requestSent = _client.SendFriendRequest(UserSession.Instance().Nickname, friendUsername);

                    ValidationRequestSent(requestSent);
                }
                catch (CommunicationObjectFaultedException)
                {
                    MessageBox.Show(Properties.Resources.msgPreviousConnectioLost, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                    App.RestartApplication();
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
            else
            {
                MessageBox.Show(Properties.Resources.msgUsernameMissing, "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ValidationRequestSent(int requestSent)
        {
            if (requestSent == Constants.VALIDATION_SUCCESS)
            {
                MessageBox.Show(Properties.Resources.msgFriendRequestSuccessful, "Astralis", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (requestSent == Constants.ERROR)
            {
                MessageBox.Show(Properties.Resources.msgUnableToAnswer, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                UnsubscribeFromFriendManager();
                App.RestartApplication();
            }
            else
            {
                MessageBox.Show(Properties.Resources.msgUnableToSendFriendRequest, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ShowUserSubscribedToFriendManager(string nickname)
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

        public void ShowUserUnsubscribedToFriendManager(string nickname)
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

        public void ShowFriends(Dictionary<string, Tuple<bool, int>> onlineFriends)
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

        public void ShowFriendDeleted(string nickname)
        {
            RemoveFriendFromFriendList(nickname);
        }

        private void TextLimiterForNickname(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Length >= MAX_FIELDS_LENGHT)
            {
                e.Handled = true;
            }
        }
    }
}
