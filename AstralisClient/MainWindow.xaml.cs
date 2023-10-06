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
            //BORRAR ESTO DESPUES, InitializeNames me sirve para llenar los componentes de TXT
            InitializeNames();
        }

        private void Button_Cancel(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Register(object sender, RoutedEventArgs e)
        {

        }

        private void InitializeNames()
        {
            //BORRAR ESTO DESPUES, Creo un objeto de tipo cliente y lo uso para llamar a los metodos, despues creo un objeto segun el servicio para guardar el objeto.
            UserManager.UserManagerClient client = new UserManager.UserManagerClient();
            UserManager.User user = client.GetUserByNickname("");

            txtNickname.Text = user.Nickname;
            txtMail.Text = user.Mail;
            txtPassword.Text = user.Password;
            txtConfirmPassword.Text = user.Password;
        }
    }
}
