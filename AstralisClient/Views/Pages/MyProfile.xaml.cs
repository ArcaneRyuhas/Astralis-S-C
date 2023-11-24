using Astralis.Logic;
using Astralis.UserManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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

namespace Astralis.Views.Pages
{
    /// <summary>
    /// Interaction logic for MyProfile.xaml
    /// </summary>
    public partial class MyProfile : Page
    {
        private const string MAIL_REGEX = @"^.+@[^\.].*\.[a-z]{2,}$";
        private const string DELIMITER_PASSWORD_REGEX = @"^[a-zA-Z0-9\S]{0,40}$";
        private const string PASSWORD_REGEX = @"^((?=\S*?[A-Z])(?=\S*?[a-z])(?=\S*?[0-9])(?=\S*?[!@#$%^&*_-]).{6,40})\S$";
        private const int MAX_FIELDS_LENGHT = 39;
        private const int MAX_GRID_COLUMN = 3;

        public MyProfile()
        {
            InitializeComponent();
            SetGraphicElements();
        }

        private void SetGraphicElements()
        {
            txtNickname.Text = UserSession.Instance().Nickname;
            txtMail.Text = UserSession.Instance().Mail; 
            SetImages();
        }

        private void SetImages()
        {
            int imageCount = ImageManager.Instance().GetImageCount();
            int imagesCreated = 1;

            int gridRow = 0;
            int gridColumn = 0;

            while (imagesCreated <= imageCount)
            {
                RadioButton radioButton = new RadioButton();
                radioButton.Content = imagesCreated.ToString();

                if (imagesCreated == UserSession.Instance().ImageId)
                {
                    radioButton.IsChecked = true;
                }

                Image image = new Image();
                image.Width = 100;
                image.Height = 100;
                image.Source = ImageManager.Instance().GetImage(imagesCreated);

                Grid.SetRow(image, gridRow);
                Grid.SetColumn(image, gridColumn);

                Grid.SetRow(radioButton, gridRow);
                Grid.SetColumn(radioButton, gridColumn);

                gdImages.Children.Add(image);
                gdImages.Children.Add(radioButton);

                imagesCreated++;

                if (gridColumn == MAX_GRID_COLUMN)
                {
                    gridColumn = 0;
                    gridRow++;
                }
                else
                {
                    gridColumn++;
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private int GetChossenImage()
        {
            int imageId = 1;

            foreach (UIElement element in gdImages.Children)
            {
                if (element is RadioButton radioButton && radioButton.IsChecked == true)
                {
                    imageId = int.Parse(radioButton.Content.ToString());
                    break; 
                }
            }

            return imageId;
        }

        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            string mail = txtMail.Text;

            User user = new User();
            SetUserInformation(user);

            if (ValidFields(user))
            {
                UserManager.UserManagerClient client = new UserManager.UserManagerClient();
                if (client.UpdateUser(user) > 0)
                {
                    App.RestartApplication();
                }
            }
            

        }

        private User SetUserInformation(User user)
        {
            user.Nickname = txtNickname.Text;
            user.Password = "";
            user.ImageId = GetChossenImage();
            user.Mail = txtMail.Text;

            if (pbPassword.Password != String.Empty)
            {
                user.Password = CreateSha2(pbPassword.Password);
            }

            return user;
        }


        private bool ValidFields(User user)
        {
            bool band = true;

            if (!Regex.IsMatch(user.Mail, MAIL_REGEX))
            {
                lblErrorMail.Visibility = Visibility.Visible;
                lblErrorMail.Content = Properties.Resources.lblErrorMail;
                band = false;
            }

            if (!Regex.IsMatch(pbPassword.Password, PASSWORD_REGEX) && pbPassword.Password != string.Empty)
            {
                lblErrorPassword.Visibility = Visibility.Visible;
                lblErrorPassword.Content = Properties.Resources.lblErrorPassword;
                band = false;
            }

            return band;
        }

        private void HideErrorLabels()
        {
            lblErrorMail.Visibility = Visibility.Collapsed;
            lblErrorPassword.Visibility = Visibility.Collapsed;
        }

        private void TextLimiterForMail(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Length >= MAX_FIELDS_LENGHT)
            {
                e.Handled = true;
            }
        }

        private void TextLimeterForPassword(object sender, TextCompositionEventArgs e)
        {
            PasswordBox passwordBox = (PasswordBox)sender;
            String passwordString = passwordBox.Password;

            if (!Regex.IsMatch(passwordString, DELIMITER_PASSWORD_REGEX))
            {
                e.Handled = true;
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
    }
}
