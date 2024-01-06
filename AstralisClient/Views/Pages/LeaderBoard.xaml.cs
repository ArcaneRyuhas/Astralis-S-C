using Astralis.UserManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;


namespace Astralis.Views.Pages
{
    /// <summary>
    /// Interaction logic for LeaderBoard.xaml
    /// </summary>
    public partial class LeaderBoard : Page
    {
        private const int MAX_USER_LENGHT = 10;
        private const int INITIAL_LENGHT = 0;

        public LeaderBoard()
        {
            InitializeComponent();
            SetLeaderBoard();
        }

        private void SetLeaderBoard()
        {
            LeaderboardManagerClient client = new LeaderboardManagerClient();
            List<GamesWonInfo> gamesWonInfos = new List<GamesWonInfo> (client.GetLeaderboardInfo());

            SetUserList(gamesWonInfos);
        }

        private void SetUserList(List<GamesWonInfo> usersAndWins)
        {
            int rowNumber = INITIAL_LENGHT;

            foreach (GamesWonInfo userAndWins in usersAndWins)
            {
                if (rowNumber < MAX_USER_LENGHT)
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
        }

        private Label CreateTextBox(string textToAdd)
        {
            Label lblText = new Label
            {
                Content = textToAdd
            };

            lblText.FontSize = 22;
            lblText.VerticalContentAlignment = VerticalAlignment.Center;
            lblText.HorizontalContentAlignment = HorizontalAlignment.Center;

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
