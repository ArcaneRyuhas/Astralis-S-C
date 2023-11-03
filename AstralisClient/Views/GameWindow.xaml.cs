using Astralis.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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

namespace Astralis.Views
{
    /// <summary>
    /// Interaction logic for GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window, UserManager.IOnlineUserManagerCallback
    {
        public GameWindow()
        {
            InitializeComponent();
            MainMenu mainMenu = new MainMenu(mainFrame);
            mainMenu.CloseWindowEvent += CloseWindowEventHandler;

            mainFrame.Content = mainMenu;
            Connect();
            ImageManager.Instance();
        }

        private void Connect()
        {
            InstanceContext context = new InstanceContext(this);
            UserManager.OnlineUserManagerClient client = new UserManager.OnlineUserManagerClient(context);

            client.ConectUser(UserSession.Instance().Nickname);
        }

        private void CloseWindowEventHandler(object sender, EventArgs e)
        {
            Close();
        }

        public void ShowUserConected(string nickname)
        {
            throw new NotImplementedException();
        }

        public void ShowUserDisconected(string nickname)
        {
            throw new NotImplementedException();
        }

        public void ShowUsersOnline(string[] nicknames)
        {
            throw new NotImplementedException();
        }
    }
}
