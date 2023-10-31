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
    /// <summary>
    /// Interaction logic for LobbyUserCard.xaml
    /// </summary>
    public partial class LobbyUserCard : UserControl
    {
        public List<string> ItemsList { get; set; }

        public LobbyUserCard()
        {
            InitializeComponent();
            ItemsList = new List<string> { Properties.Resources.cbxTeamOne, Properties.Resources.cbxTeamTwo};
            cbxTeam.ItemsSource = ItemsList;
        }

        public void setCard(User user)
        {
            ImageManager imageManager = new ImageManager();
            
            lblNickname.Content = user.Nickname;
            imgUser.Source = imageManager.GetImage(user.ImageId);
        }
    }
}
