using Astralis.Logic;
using System;
using System.Windows;
using System.Windows.Controls;

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

        public void ChangePage(Page pageToSet)
        {
            mainFrame.Content = pageToSet;
        }

        private void CloseWindowEventHandler(object sender, EventArgs e)
        {
            Close();
        }
    }
}
