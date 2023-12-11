using Astralis.UserManager;
using System;
using System.Collections.Generic;
using System.Linq;
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
using Astralis.Properties;
using Astralis.Logic;

namespace Astralis.Views.Animations
{

    public partial class LobbyUserCard : UserControl
    {
        private const int TEAM_ONE = 1;
        private const int TEAM_TWO = 2;

        private List<string> ItemsList { get; set; }
        private string userNickname;
        private int team;
        public event EventHandler<Tuple<string, int>> TeamSelectionChanged;

        public string UserNickname { get { return userNickname; }}
        public int Team {get { return team; }}

        public LobbyUserCard()
        {
            InitializeComponent();
            ItemsList = new List<string> { Properties.Resources.cbxTeamOne, Properties.Resources.cbxTeamTwo};
            cbxTeam.ItemsSource = ItemsList;
        }

        public void setCard(User user, bool isHost)
        {
            
            userNickname = user.Nickname;
            lblNickname.Content = userNickname;
            imgUser.Source = ImageManager.Instance().GetImage(user.ImageId);

            if (!IsTheClientCard())
            {
                cbxTeam.IsEnabled = false;
            }

            if (!isHost)
            {
                btnKickout.Visibility = Visibility.Collapsed;

                
            }
            else
            {
                if (IsTheClientCard())
                {
                    btnKickout.Visibility = Visibility.Collapsed;
                }

            }
        }

        public void ChangeTeam(int team)
        {
            if (team == TEAM_ONE)
            {
                this.team = TEAM_ONE;
                cbxTeam.SelectedItem = Properties.Resources.cbxTeamOne;
            }
            else if (team == TEAM_TWO)
            {
                this.team = TEAM_TWO;
                cbxTeam.SelectedItem = Properties.Resources.cbxTeamTwo;
            }
        }

        private bool IsTheClientCard()
        {
            return userNickname == UserSession.Instance().Nickname;
        }

        private void cbxTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxTeam.SelectedValue != null && (IsTheClientCard()))
            {

                if(cbxTeam.SelectedValue.ToString() == Properties.Resources.cbxTeamOne)
                {
                    team = TEAM_ONE;
                    TeamSelectionChanged?.Invoke(this, new Tuple<string, int>(userNickname, TEAM_ONE));
                }
                else
                {
                    team = TEAM_ONE;
                    TeamSelectionChanged?.Invoke(this, new Tuple<string, int>(userNickname, TEAM_TWO));
                }
                
            }
        }
    }
}
