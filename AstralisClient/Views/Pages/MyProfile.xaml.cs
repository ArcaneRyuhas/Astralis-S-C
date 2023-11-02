using Astralis.Logic;
using Astralis.UserManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

namespace Astralis.Views.Pages
{
    /// <summary>
    /// Interaction logic for MyProfile.xaml
    /// </summary>
    public partial class MyProfile : Page
    {
        public MyProfile()
        {
            InitializeComponent();
            SetGraphicElements();
        }

        private void SetGraphicElements()
        {
            txtNickname.Text = UserSession.Instance().Nickname;
            txtMail.Text = UserSession.Instance().Mail; 
            
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            string mail = txtMail.Text;
            string password = "";

            if(pbPassword==null)
            {
                password = CreateSha2(pbPassword.Password);
            }

            int imageId = 1;

            User user = new User();
            user.Nickname = txtNickname.Text;
            user.Mail = mail;
            user.Password = password;
            user.ImageId = imageId;

            UserManager.UserManagerClient client = new UserManager.UserManagerClient();
            if (client.UpdateUser(user)>0)
            {
                NavigationService.GoBack();

                App.RestartApplication();
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
