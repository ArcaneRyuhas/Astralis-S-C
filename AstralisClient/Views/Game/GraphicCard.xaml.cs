using Astralis.Views.Game.GameLogic;
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

namespace Astralis.Views.Game
{
    public partial class GraphicCard : UserControl
    {
        private const bool IS_LEFT_CLICKED = true;
        private const bool IS_RIGHT_CLICKED = false;

        private bool isSelected = false;
        public event EventHandler<bool> OnCardClicked;
        private Card card;

        public bool IsSelected { get { return isSelected; } set { isSelected = value; UpdateVisualState(); } }

        public GraphicCard()
        {
            InitializeComponent();
        }

        private void UpdateVisualState()
        {
            lblType.Foreground = isSelected ? Brushes.Black : Brushes.Red;
        }

        public void SetGraphicCard(Card card)
        {
            lblHealth.Content = card.Health.ToString();
            lblAttack.Content = card.Attack.ToString();
            lblMana.Content = card.Mana.ToString();
            lblType.Content = card.Type.ToString();
        }

        private void GraphicCardOnLeftClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OnCardClicked?.Invoke(this, IS_LEFT_CLICKED);
        }

        private void GraphicCardOnRightClick(object sender, MouseButtonEventArgs e)
        {
            OnCardClicked?.Invoke(this, IS_RIGHT_CLICKED);
        }
    }
}
