using Astralis.Logic;
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

namespace Astralis.Views.Cards
{
    /// <summary>
    /// Interaction logic for EndGameUserCard.xaml
    /// </summary>
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
