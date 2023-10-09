using Astralis.UserManager;
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

namespace Astralis
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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

            if (pbPassword.Password == pbConfirmPassword.Password)
            {
                user.Nickname = txtNickname.Text;
                user.Password = CreateSha2(pbPassword.Password);
                user.ImageId = 1;
                user.Mail = txtMail.Text;

                UserManager.UserManagerClient client = new UserManager.UserManagerClient();

                if (!client.FindUserByNickname(user.Nickname))
                {
                    if (client.AddUser(user) > 0)
                    {
                        //Mensaje de registro exitoso
                    }
                }
                else
                {
                    lblErrorNickname.Visibility = Visibility.Visible;
                }
            }
            else
            {
                //TODO
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
