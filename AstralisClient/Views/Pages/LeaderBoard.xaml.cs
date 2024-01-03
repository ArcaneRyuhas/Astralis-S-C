using System;
using System.Collections.Generic;
using System.Windows.Controls;


namespace Astralis.Views.Pages
{
    /// <summary>
    /// Interaction logic for LeaderBoard.xaml
    /// </summary>
    public partial class LeaderBoard : Page
    {
        private const int MAX_USER_LENGHT = 10;

        public LeaderBoard()
        {
            InitializeComponent();
            SetLeaderBoard();
        }

        private void SetLeaderBoard()
        {
            throw new NotImplementedException();
        }

        private void SetUserList(List<Tuple<string, int>> usersAndWins)
        {
            int rowNumber = 0;

            foreach (Tuple<string, int> userAndWins in usersAndWins)
            {
                if (rowNumber < MAX_USER_LENGHT) { }
                {
                    string nickname = userAndWins.Item1;
                    string wins = userAndWins.Item2.ToString();

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
            TextBox txtText = new TextBox();
            txtText.Text = textToAdd;

            return txtText;
        }

        public void btnExit_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
