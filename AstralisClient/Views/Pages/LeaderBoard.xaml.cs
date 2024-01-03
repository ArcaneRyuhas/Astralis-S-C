using Astralis.UserManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;


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

                    TextBox txtNickname = CreateTextBox(nickname);
                    TextBox txtWins = CreateTextBox(wins);

                    AddTextBoxToUserGrid(txtNickname, rowNumber);
                    AddTextBoxToWinsGrid(txtWins, rowNumber);

                    rowNumber++;
                }
            }
        }

        private void AddTextBoxToUserGrid(TextBox txtNickname, int rowNumber)
        {
            Grid.SetColumn(txtNickname, rowNumber);

            gdUsersName.Children.Add(txtNickname);
        }


        private void AddTextBoxToWinsGrid(TextBox txtWins, int rowNumber)
        {
            Grid.SetColumn(txtWins, rowNumber);

            gdUsersWins.Children.Add(txtWins);
        }

        
        private TextBox CreateTextBox(string textToAdd)
        {
            TextBox txtText = new TextBox
            {
                Text = textToAdd
            };

            return txtText;
        }

        public void btnExit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
