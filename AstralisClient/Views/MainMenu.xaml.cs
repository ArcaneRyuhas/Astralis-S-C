using Astralis.Logic;
using Astralis.UserManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Astralis.Views
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Page
    {
        public delegate void CloseWindowEventHandler(object sender, EventArgs e);
        public event CloseWindowEventHandler CloseWindowEvent;

        private Frame mainFrame;

        public MainMenu(Frame mainFrame)
        {
            InitializeComponent();
            this.mainFrame = mainFrame;
        }

        private void btnCreateGame_Click(object sender, RoutedEventArgs e)
        {
            Lobby lobby = new Lobby("host");
            NavigationService.Navigate(lobby);

           
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            CloseWindowEvent?.Invoke(this, EventArgs.Empty);
        }

        private void btnJoinGame_Click(object sender, RoutedEventArgs e)
        {
            string code = txtJoinCode.Text;

            Lobby lobby = new Lobby(code);
            NavigationService.Navigate(lobby);
        }
    }
}
