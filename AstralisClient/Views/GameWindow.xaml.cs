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
    public partial class GameWindow : Window
    {
        MainMenu mainMenu;

        public GameWindow()
        {
            InitializeComponent();
            mainMenu = new MainMenu();
            mainMenu.CloseWindowEvent += CloseWindowEventHandler;

            mainFrame.Content = mainMenu;
            ImageManager.Instance();
        }


        private void CloseWindowEventHandler(object sender, EventArgs e)
        {
            mainMenu.Disconnect();

            Close();
        }
    }
}
