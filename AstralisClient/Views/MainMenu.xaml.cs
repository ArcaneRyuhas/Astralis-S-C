﻿using Astralis.Logic;
using Astralis.UserManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Astralis.Views.Pages;
using Astralis.Views.Animations;

namespace Astralis.Views
{
    public partial class MainMenu : Page
    {
       
        public delegate void CloseWindowEventHandler(object sender, EventArgs e);
        public event CloseWindowEventHandler CloseWindowEvent;

        private const string IS_HOST = "host";
        private const string MAIN_MENU_WINDOW = "MAIN_MENU";

        private FriendWindow friendWindow;

        public MainMenu()
        {
            InitializeComponent();
            friendWindow = new FriendWindow(MAIN_MENU_WINDOW);
            friendWindow.SetFriendWindow();
            gridFriendsWindow.Children.Add(friendWindow);
            btnMyProfile.Content = UserSession.Instance().Nickname;

        }

        private void btnCreateGame_Click(object sender, RoutedEventArgs e)
        {
            Lobby lobby = new Lobby();


            if (lobby.SetLobby(IS_HOST))
            {
                NavigationService.Navigate(lobby);
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            friendWindow.Disconnect();
            CloseWindowEvent?.Invoke(this, EventArgs.Empty);
        }

      

        private void btnJoinGame_Click(object sender, RoutedEventArgs e)
        {
            string code = txtJoinCode.Text;

            Lobby lobby = new Lobby();
            if (lobby.GameIsNotFull(code)  && lobby.SetLobby(code))
            {
                NavigationService.Navigate(lobby);
            }
            else
            {
                MessageBox.Show("msgGameIsFullOrLobbyDoesntExist", "titleLobbyDoesntExist", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnMyProfile_Click(object sender, RoutedEventArgs e)
        {
            MyProfile myProfile = new MyProfile(friendWindow);
            NavigationService.Navigate(myProfile);
        }


        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings();
            NavigationService.Navigate(settings);
        }

        private void btnFriends_Click(object sender, RoutedEventArgs e)
        {
            if (friendWindow.IsVisible == true)
            {
                friendWindow.Visibility = Visibility.Hidden;
            }

            else
            {
                friendWindow.SetFriendWindow();
                friendWindow.Visibility = Visibility.Visible;
            }
        }

       


        

        

        

      
    }
}
