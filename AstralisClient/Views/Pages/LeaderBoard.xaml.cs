using Astralis.UserManager;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using System.ServiceModel;

namespace Astralis.Views.Pages
{
    public partial class LeaderBoard : Page
    {
        private const int INITIAL_LENGHT = 0;

        public LeaderBoard()
        {
            InitializeComponent();
            SetLeaderBoard();
        }

        private void SetLeaderBoard()
        {
            try
            {
                LeaderboardManagerClient client = new LeaderboardManagerClient();
                List<GamesWonInfo> gamesWonInfos = new List<GamesWonInfo>(client.GetLeaderboardInfo());

                SetUserList(gamesWonInfos);
            }
            catch (CommunicationObjectFaultedException)
            {
                MessageBox.Show(Properties.Resources.msgPreviousConnectioLost, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
        }

        private void SetUserList(List<GamesWonInfo> usersAndWins)
        {
            int rowNumber = INITIAL_LENGHT;

            foreach (GamesWonInfo userAndWins in usersAndWins)
            {
                string nickname = userAndWins.Username;
                string wins = userAndWins.GamesWonCount.ToString();
                Label lblNickname = CreateTextBox(nickname);
                Label lblWins = CreateTextBox(wins);

                AddTextBoxToUserGrid(lblNickname, rowNumber);
                AddTextBoxToWinsGrid(lblWins, rowNumber);

                rowNumber++;
            }
        }

        private Label CreateTextBox(string textToAdd)
        {
            Label lblText = new Label
            {
                Content = textToAdd,
                FontSize = 22,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };

            return lblText;
        }

        private void AddTextBoxToUserGrid(Label lblNickname, int rowNumber)
        {
            Grid.SetRow(lblNickname, rowNumber);
            gdUsersName.Children.Add(lblNickname);
        }

        private void AddTextBoxToWinsGrid(Label lblWins, int rowNumber)
        {
            Grid.SetRow(lblWins, rowNumber);
            gdUsersWins.Children.Add(lblWins);
        }

        public void BtnExitClick(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
