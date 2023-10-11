﻿using Astralis.UserManager;
using System;
using System.Collections.Generic;
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
using System.Security.Cryptography;
using Astralis.Windows;
using System.Text.RegularExpressions;
using Astralis.Properties;

namespace Astralis
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private const string NICKNAME_REGEX = @"[a-zA-Z0-9]+";
        private const string MAIL_REGEX = @"^.+@[^\.].*\.[a-z]{2,}$";
        private const string PASSWORD_REGEX = @"^((?=\S*?[A-Z])(?=\S*?[a-z])(?=\S*?[0-9]).{6,})\S$";
        private const int MAX_FIELDS_LENGHT = 49;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Cancel(object sender, RoutedEventArgs e)
        {
            Window1 window1 = new Window1();
            this.Close();
            window1.Show();
        }

        private void Button_Register(object sender, RoutedEventArgs e)
        {
            UserManager.User user = new UserManager.User();

            HideErrorLabels();
            SetUserInformation(user);

            if (ValidFields(user))
            {
               
                UserManager.UserManagerClient client = new UserManager.UserManagerClient();
                
                if (!client.FindUserByNickname(user.Nickname))
                {
                    if (client.AddUser(user) > 0)
                    {
                        MessageBox.Show("msgUserAddedSucceed", "titleUserAdded", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    lblErrorNickname.Visibility = Visibility.Visible;
                    lblErrorNickname.Content = Properties.Resources.lblErrorNicknameRepeated;
                }
            }
        }

        private bool ValidFields(User user)
        {
            bool band = true;

            if (!Regex.IsMatch(user.Nickname, NICKNAME_REGEX))
            {
                lblErrorNickname.Visibility = Visibility.Visible;
                lblErrorNickname.Content = Properties.Resources.lblErrorNickname;
                band = false;
            }

            if(!Regex.IsMatch(user.Mail, MAIL_REGEX))
            {
                lblErrorMail.Visibility = Visibility.Visible;
                lblErrorMail.Content = Properties.Resources.lblErrorMail;
                band = false;
            }

            if(!Regex.IsMatch(user.Password, PASSWORD_REGEX))
            {
                lblErrorPassword.Visibility = Visibility.Visible;
                lblErrorPassword.Content = Properties.Resources.lblErrorPassword;
                band = false;
            }

            if (pbPassword.Password == pbConfirmPassword.Password)
            {
                lblErrorPassword.Visibility = Visibility.Visible;
                lblErrorPassword.Content = Properties.Resources.lblErrorPasswordsNoMatch;
            }

            return band;
        }

        private void HideErrorLabels()
        {
            lblErrorMail.Visibility = Visibility.Collapsed;
            lblErrorNickname.Visibility = Visibility.Collapsed;
            lblErrorPassword.Visibility = Visibility.Collapsed;
        }

        private void TextFilterForNickname(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string fullString= textBox.Text + e.Text;

            if (!Regex.IsMatch(fullString, NICKNAME_REGEX))
            {
                e.Handled = true;
            }

            if (textBox.Text.Length >= MAX_FIELDS_LENGHT)
            {
                e.Handled = true;
            }
        }

        private void TextLimiterForMail (object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Length >= MAX_FIELDS_LENGHT)
            {
                e.Handled = true;
            }
        }

        private User SetUserInformation(User user)
        {
            user.Nickname = txtNickname.Text;
            user.Password = CreateSha2(pbPassword.Password);
            user.ImageId = 1;
            user.Mail = txtMail.Text;

            return user;
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
    }
}
