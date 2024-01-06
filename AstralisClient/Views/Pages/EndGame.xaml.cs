using Astralis.Logic;
using Astralis.UserManager;
using Astralis.Views.Animations;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;


namespace Astralis.Views.Pages
{
    /// <summary>
    /// Interaction logic for EndGame.xaml
    /// </summary>
    public partial class EndGame : Page, IEndGameCallback
    {

        private EndGameClient client;
        private const int GAME_ABORTED = 0;

        public EndGame(int winnerTeam, int myTeam)
        {
            InitializeComponent();
            SetPlayersCard();
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

        private void SetPlayersCard()
        {
            string myNickname = UserSession.Instance().Nickname;
            InstanceContext context = new InstanceContext(this);
            client = new EndGameClient(context);

            client.GetUsersWithTeam(myNickname);
        }

        public void SetUsers(UserWithTeam[] usersWithTeams)
        {
            int gridRow = 0;

            foreach (UserWithTeam userWithTeam in usersWithTeams)
            {
                AddCard(userWithTeam, userWithTeam.Team, gridRow);
                gridRow++;
            }
        }

        private void AddCard(UserWithTeam user, int team, int gridRow)
        {
            EndGameUserCard endGameUserCard = new EndGameUserCard();

            endGameUserCard.setCard(user, team);
            gridUsers.Children.Add(endGameUserCard);
            Grid.SetRow(endGameUserCard, gridRow);
        }

        private void BtnExitClick(object sender, RoutedEventArgs e)
        {
            GameWindow gameWindow = new GameWindow();
            MainMenu mainMenu = new MainMenu(gameWindow);
            NavigationService.Navigate(mainMenu);

            client.GameEnded(UserSession.Instance().Nickname);
        }
    }
}
