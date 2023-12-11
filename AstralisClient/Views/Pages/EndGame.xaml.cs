using Astralis.Logic;
using Astralis.UserManager;
using Astralis.Views.Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Astralis.Views.Pages
{
    /// <summary>
    /// Interaction logic for EndGame.xaml
    /// </summary>
    public partial class EndGame : Page, IEndGameCallback
    {  
        public EndGame(int winnerTeam, int myTeam)
        {
            InitializeComponent();
            SetPlayersCard();
            SetWinnerTeam(winnerTeam, myTeam);
        }

        private void SetWinnerTeam(int winnerTeam, int myTeam)
        {
            if(winnerTeam == myTeam)
            {
                lblWinnerTeam.Content = "lblWinnerTeam = Has Ganado";
            }
            else
            {
                lblWinnerTeam.Content = "lblLoserTeam = Has perdido";
            }
        }

        private void SetPlayersCard()
        {
            string myNickname = UserSession.Instance().Nickname;
            InstanceContext context = new InstanceContext(this);
            EndGameClient client = new EndGameClient(context);

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

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            MainMenu mainMenu = new MainMenu();
            NavigationService.Navigate(mainMenu);
        }
    }
}
