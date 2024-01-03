﻿using System.Text;
using System.Windows;
using System.Windows.Input;
using Astralis.Logic;
using Astralis.UserManager;
using System.Security.Cryptography;
using Astralis.Views.Animations;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.ServiceModel;
using System;

namespace Astralis.Views
{
    public partial class LogIn : Window
    {
        private const string DELIMITER_NICKNAME_REGEX = @"^[a-zA-Z0-9]{0,30}$";
        private const int MAX_FIELDS_LENGHT = 39;
        private const int VALIDATION_FAILURE = 0;
        private const int VALIDATION_SUCCES = 1;

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

            if (noEmptyFields)
            {
                bool userOnline = false;
                try
                {
                    userOnline = client.UserOnline(nickname);
                    int userConfirmed = client.ConfirmUser(nickname, password);

                    if (userOnline)
                    {
                        MessageBox.Show(Astralis.Properties.Resources.msgUserOnline, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (userConfirmed == VALIDATION_SUCCES)
                    {
                        User user = client.GetUserByNickname(nickname);
                        UserSession.Instance(user);

                        GameWindow gameWindow = new GameWindow();
                        this.Close();
                        gameWindow.Show();
                    }
                    else if(userConfirmed == VALIDATION_FAILURE)
                    {
                        txbInvalidFields.Text = Astralis.Properties.Resources.txbInvalidFields;
                        txbInvalidFields.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        MessageBox.Show(Astralis.Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (CommunicationException)
                {
                    MessageBox.Show(Astralis.Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (TimeoutException)
                {
                    MessageBox.Show(Astralis.Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Information);
                }
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

            try
            {
                User user = client.AddGuest();

                if (user.Nickname != "ERROR")
                {
                    UserSession.Instance(user);
                    GameWindow gameWindow = new GameWindow();
                    Lobby lobby = new Lobby(gameWindow);

                    if (lobby.GameIsNotFull(invitationCode) && lobby.SetLobby(invitationCode))
                    {
                        this.Close();
                        gameWindow.ChangePage(lobby);
                        gameWindow.Show();
                    }
                    else
                    {
                        MessageBox.Show(Astralis.Properties.Resources.msgLobbyError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show(Astralis.Properties.Resources.msgLobbyError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Astralis.Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Astralis.Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Information);
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
