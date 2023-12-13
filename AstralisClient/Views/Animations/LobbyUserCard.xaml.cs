using Astralis.UserManager;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Astralis.Logic;
using System.ServiceModel;

namespace Astralis.Views.Animations
{

    public partial class LobbyUserCard : UserControl
    {
        private const int TEAM_ONE = 1;
        private const int TEAM_TWO = 2;

        private List<string> _itemsList { get; set; }
        private string _userNickname;
        private int _team;
        public event EventHandler<Tuple<string, int>> TeamSelectionChanged;

        public string UserNickname { get { return _userNickname; }}
        public int Team {get { return _team; }}

        public LobbyUserCard()
        {
            InitializeComponent();
            _itemsList = new List<string> { Properties.Resources.cbxTeamOne, Properties.Resources.cbxTeamTwo};
            cbxTeam.ItemsSource = _itemsList;
        }

        public void SetCard(User user, bool isHost)
        {
            
            _userNickname = user.Nickname;
            lblNickname.Content = _userNickname;
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
                this._team = TEAM_ONE;
                cbxTeam.SelectedItem = Properties.Resources.cbxTeamOne;
            }
            else if (team == TEAM_TWO)
            {
                this._team = TEAM_TWO;
                cbxTeam.SelectedItem = Properties.Resources.cbxTeamTwo;
            }
        }

        private bool IsTheClientCard()
        {
            return _userNickname == UserSession.Instance().Nickname;
        }

        private void cbxTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxTeam.SelectedValue != null && IsTheClientCard())
            {

                if(cbxTeam.SelectedValue.ToString() == Properties.Resources.cbxTeamOne)
                {
                    _team = TEAM_ONE;
                    TeamSelectionChanged?.Invoke(this, new Tuple<string, int>(_userNickname, TEAM_ONE));
                }
                else
                {
                    _team = TEAM_ONE;
                    TeamSelectionChanged?.Invoke(this, new Tuple<string, int>(_userNickname, TEAM_TWO));
                }
                
            }
        }

        private void BtnKickoutClick(object sender, RoutedEventArgs e)
        {
            InstanceContext context = new InstanceContext(this);
            UserManager.LobbyManagerClient client = new UserManager.LobbyManagerClient(context);

            client.KickUser(_userNickname);
        }
    }
}
