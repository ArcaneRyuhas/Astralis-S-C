using Astralis.Logic;
using Astralis.UserManager;
using Astralis.Views.Cards;
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
        private EndGameClient _client;
        private GameWindow _endGameWindow;

        private const int GAME_ABORTED = 0;
        private const string GUEST_NAME = "Guest";

        public GameWindow EndGameWindow {get { return _endGameWindow;} set { _endGameWindow = value; } }

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
            _client = new EndGameClient(context);

            _client.GetUsersWithTeam(myNickname);
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

            endGameUserCard.SetCard(user, team);
            gridUsers.Children.Add(endGameUserCard);
            Grid.SetRow(endGameUserCard, gridRow);
        }

        private void BtnExitClick(object sender, RoutedEventArgs e)
        {
            if (!UserSession.Instance().Nickname.StartsWith(GUEST_NAME))
            {
                GameWindow gameWindow = new GameWindow();

                gameWindow.Show();
            }
            else
            {
                LogIn logIn = new LogIn();

                logIn.Show();

            }

            _endGameWindow.Close();
            _client.GameEnded(UserSession.Instance().Nickname);
        }
    }
}
