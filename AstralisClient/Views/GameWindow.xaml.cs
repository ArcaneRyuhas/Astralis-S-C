using Astralis.Logic;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Astralis.Views
{
    public partial class GameWindow : Window
    {
        private MainMenu _mainMenu;

        public GameWindow()
        {
            InitializeComponent();
            ImageManager.Instance();

            _mainMenu = new MainMenu(this);
            _mainMenu.CloseWindowEvent += CloseWindowEventHandler;
            mainFrame.Content = _mainMenu;
        }

        public void ChangePage(Page pageToSet)
        {
            mainFrame.Content = pageToSet;
        }
        public void CloseGameWindow()
        {
            this.Close();
        }

        private void CloseWindowEventHandler(object sender, EventArgs e)
        {
            Close();
        }
    }
}
