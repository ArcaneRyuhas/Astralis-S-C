﻿using System.Text;
using System.Windows;
using System.Windows.Input;
using Astralis.Logic;
using Astralis.UserManager;
using System.Security.Cryptography;
using Astralis.Views.Animations;
using System.Windows.Navigation;

namespace Astralis.Views
{
    public partial class LogIn : Window
    {
        public LogIn()
        {
            InitializeComponent();
        }

        private void btnLogIn_Click(object sender, RoutedEventArgs e)
        {
            string password = CreateSha2(pbPassword.Password);
            string nickname = tbNickname.Text;
            bool noEmptyFields = true;

            UserManager.UserManagerClient client = new UserManager.UserManagerClient();

            if(string.IsNullOrEmpty(password))
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

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            SignUp signUp = new SignUp();
            this.Close();
            signUp.Show();
        }

        private void EnterKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnLogIn_Click(sender, e);
            }
        }

        private void BtnJoinAsGuestClick(object sender, RoutedEventArgs e)
        {
            var guestInvitation = new GuestInvitation();

            guestInvitation.OnSubmit += guestInvitation_OnSubmit;

             var window = new Window
             {
                Content = guestInvitation,
                Title = "Join As Guest",
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize
             };
             window.ShowDialog();
        }

        private void guestInvitation_OnSubmit(object sender, string invitationCode)
        {
            UserManager.UserManagerClient client = new UserManager.UserManagerClient();
            if (client.AddGuest() > 0)
            {
                Lobby lobby = new Lobby();
                if (lobby.GameIsNotFull(invitationCode) && lobby.SetLobby(invitationCode))
                {
                    var gameWindow = Window.GetWindow(this) as GameWindow;

                    if (gameWindow != null)
                    {
                        gameWindow.mainFrame.Navigate(lobby);
                    }
                    Close();
                }
                else
                {
                    MessageBox.Show("msgGameIsFullOrLobbyDoesntExist", "titleLobbyDoesntExist", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}
