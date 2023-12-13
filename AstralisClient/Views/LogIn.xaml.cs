using System.Text;
using System.Windows;
using System.Windows.Input;
using Astralis.Logic;
using Astralis.UserManager;
using System.Security.Cryptography;
using Astralis.Views.Animations;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Astralis.Views
{
    public partial class LogIn : Window
    {
        private const string DELIMITER_NICKNAME_REGEX = @"^[a-zA-Z0-9]{0,30}$";
        private const int MAX_FIELDS_LENGHT = 39;

        public LogIn()
        {
            InitializeComponent();
        }

        private void BtnLogInClick(object sender, RoutedEventArgs e)
        {
            string password = CreateSha2(pbPassword.Password);
            string nickname = tbNickname.Text;
            bool noEmptyFields = true;

            UserManager.UserManagerClient client = new UserManager.UserManagerClient();

            if (string.IsNullOrEmpty(password))
            {
                noEmptyFields = false;
            }

            if (string.IsNullOrEmpty(nickname))
            {
                noEmptyFields = false;
            }

            if (noEmptyFields && client.ConfirmUser(nickname, password) == 1)
            {
                User user = client.GetUserByNickname(nickname);
                UserSession.Instance(user);

                GameWindow gameWindow = new GameWindow();
                this.Close();
                gameWindow.Show();
            }
            else
            {
                txbInvalidFields.Text = Astralis.Properties.Resources.txbInvalidFields;
                txbInvalidFields.Visibility = Visibility.Visible;
            }
        }

        private string CreateSha2(string password)
        {
            string hash = string.Empty;

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashValue = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                foreach (byte b in hashValue)
                {
                    hash += $"{b:X2}";
                }
            }

            return hash;
        }

        private void BtnRegisterClick(object sender, RoutedEventArgs e)
        {
            SignUp signUp = new SignUp();
            this.Close();
            signUp.Show();
        }

        private void EnterKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnLogInClick(sender, e);
            }
        }

        private void BtnJoinAsGuestClick(object sender, RoutedEventArgs e)
        {
            GuestInvitation guestInvitation = new GuestInvitation();

            guestInvitation.OnSubmit += GuestInvitationOnSubmit;

            guestInvitation.ShowDialog();
        }

        private void GuestInvitationOnSubmit(object sender, string invitationCode)
        {
            UserManager.UserManagerClient client = new UserManager.UserManagerClient();
            User user = client.AddGuest();

            if (user.Nickname != "ERROR")
            {
                UserSession.Instance(user);
                Lobby lobby = new Lobby();

                if (lobby.GameIsNotFull(invitationCode) && lobby.SetLobby(invitationCode))
                {
                    GameWindow gameWindow = new GameWindow();
                    this.Close();
                    gameWindow.ChangePage(lobby);
                    gameWindow.Show();
                }
                else
                {
                    MessageBox.Show("msgGameIsFullOrLobbyDoesntExist", "titleLobbyDoesntExist", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void TextFilterForNickname(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string fullString = textBox.Text + e.Text;

            if (!Regex.IsMatch(fullString, DELIMITER_NICKNAME_REGEX))
            {
                e.Handled = true;
            }

            if (textBox.Text.Length >= MAX_FIELDS_LENGHT)
            {
                e.Handled = true;
            }
        }

    }
}
