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

namespace Astralis.Windows
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }


        private void Click_LogIn(object sender, RoutedEventArgs e)
        {
            string password = pbPassword.Password;
            string nickname = tbNickname.Text;

            UserManager.UserManagerClient client = new UserManager.UserManagerClient();
            if(client.ConfirmUser(nickname, password) == 1)
            {
                GameWindow gameWindow = new GameWindow();
                this.Close();
                gameWindow.Show();
            }
        }

        private void Click_Register(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            this.Close();
            mainWindow.Show();
        }
    }
}
