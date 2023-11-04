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
using System.Windows.Shapes;
using Astralis.Properties;
using Astralis.Logic;
using Astralis.UserManager;
using System.Security.Cryptography;

namespace Astralis.Views
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    /// 

    public partial class LogIn : Window
    {
        public LogIn()
        {
            InitializeComponent();
        }


        private void Click_LogIn(object sender, RoutedEventArgs e)
        {
            string password = CreateSha2(pbPassword.Password);
            string nickname = tbNickname.Text;
            bool noEmptyFields = true;

            UserManager.UserManagerClient client = new UserManager.UserManagerClient();

            if(string.IsNullOrEmpty(password))
            {
                //AgregarToolTip
                noEmptyFields = false;
            }

            if (string.IsNullOrEmpty(nickname))
            {
                //Agregar ToolTip
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

        private void Click_Register(object sender, RoutedEventArgs e)
        {
            SignUp signUp = new SignUp();
            this.Close();
            signUp.Show();
        }

        private void EnterKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Click_LogIn(sender, e);
            }
        }
    }
}
