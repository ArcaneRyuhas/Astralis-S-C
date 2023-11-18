using Astralis.Logic;
using System;
using System.Windows;

namespace Astralis.Views
{
    public partial class GameWindow : Window
    {
        MainMenu mainMenu;

        public GameWindow()
        {
            InitializeComponent();
            ImageManager.Instance();

            mainMenu = new MainMenu();
            mainMenu.CloseWindowEvent += CloseWindowEventHandler;
            mainFrame.Content = mainMenu;
        }


        private void CloseWindowEventHandler(object sender, EventArgs e)
        {
            mainMenu.Disconnect();

            Close();
        }
    }
}
