using Astralis.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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

namespace Astralis.Views.Animations
{
    /// <summary>
    /// Interaction logic for FriendWindow.xaml
    /// </summary>
    public partial class FriendWindow : UserControl
    {
        private const bool REQUEST_PENDING = false;
        private const bool IS_FRIEND = true;


        public FriendWindow()
        {
            InitializeComponent();
        }

        internal void SetFriendWindow(Dictionary<string, bool> friendList)
        {

            int cardsAddedRow = 0;

            foreach (var friendEntry in friendList)
            {
                FriendCard card = new FriendCard();
                card.SetCard(friendEntry.Key, friendEntry.Value, IS_FRIEND); //A CAMBIAR NO SE VAYA A OLVIDAR
                Grid.SetRow(card, cardsAddedRow);
                gdFriends.Children.Add(card);
                cardsAddedRow++;

                RowDefinition rowDefinition = new RowDefinition();
                rowDefinition.Height = GridLength.Auto;
                gdFriends.RowDefinitions.Add(rowDefinition);
            }

            RowDefinition lastRowDefinition =new RowDefinition();
            lastRowDefinition.Height = new GridLength(1, GridUnitType.Star);
            gdFriends.RowDefinitions.Add(lastRowDefinition);
        }
    }
}
