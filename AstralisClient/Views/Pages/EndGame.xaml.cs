using Astralis.Logic;
using Astralis.UserManager;
using Astralis.Views.Cards;
using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;


namespace Astralis.Views.Pages
{

    public partial class EndGame : Page, IEndGameCallback
    {
        private EndGameClient _client;
        private const int GAME_ABORTED = 0;
        private const string GUEST_NAME = "Guest";

        public GameWindow EndGameWindow { get; set; }

        public EndGame(int winnerTeam, int myTeam)
        {
            InitializeComponent();
            GetEndGameUsers();
            SetWinnerTeam(winnerTeam, myTeam);
        }

        private void SetWinnerTeam(int winnerTeam, int myTeam)
        {
            if (winnerTeam == myTeam)
            {
                lblWinnerTeam.Content = Properties.Resources.lblWinnerTeam;
            }
            else if (winnerTeam == GAME_ABORTED)
            {
                lblWinnerTeam.Content = Properties.Resources.lblGameAborted;
            }
            else
            {
                lblWinnerTeam.Content = Properties.Resources.lblLoserTeam;
            }
        }

        private void GetEndGameUsers()
        {
            string myNickname = UserSession.Instance().Nickname;
            InstanceContext context = new InstanceContext(this);

            try
            {
                _client = new EndGameClient(context);

                _client.GetEndGameUsers(myNickname);
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

        public void ShowEndGameUsers(UserWithTeam[] usersWithTeams)
        {
            int gridRow = 0;

            foreach (UserWithTeam userWithTeam in usersWithTeams)
            {
                AddEndGameUserCard(userWithTeam, userWithTeam.Team, gridRow);
            
                gridRow++;
            }
        }

        private void AddEndGameUserCard(UserWithTeam user, int team, int gridRow)
        {
            EndGameUserCard endGameUserCard = new EndGameUserCard();

            endGameUserCard.SetCard(user, team);
            gridUsers.Children.Add(endGameUserCard);
            Grid.SetRow(endGameUserCard, gridRow);
        }

        private void BtnExitClick(object sender, RoutedEventArgs e)
        {
            string nickname = UserSession.Instance().Nickname;

            ExitByUser(nickname);
            EndGameWindow.Close();
            _client.GameEnded(nickname);
        }

        private void ExitByUser(string nickname)
        {
            if (!nickname.StartsWith(GUEST_NAME))
            {
                GameWindow gameWindow = new GameWindow();

                gameWindow.Show();
            }
            else
            {
                App.RestartApplication();
            }
        }
    }
}
