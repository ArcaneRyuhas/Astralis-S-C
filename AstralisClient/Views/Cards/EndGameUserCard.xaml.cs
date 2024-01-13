using Astralis.Logic;
using Astralis.UserManager;
using System.Windows.Controls;


namespace Astralis.Views.Cards
{
    public partial class EndGameUserCard : UserControl
    {
        public EndGameUserCard()
        {
            InitializeComponent();
        }

        public void SetCard(UserWithTeam user, int team)
        {
            string userNickname = user.Nickname;
            string teamString = team.ToString();
            lblNickname.Content = userNickname;
            lblTeam.Content = teamString;
            imgUser.Source = ImageManager.Instance().GetImage(user.ImageId);
        }
    }
}
