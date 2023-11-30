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
        private const string CARD_HEALTH = "Health";
        private const string CARD_ATTACK = "Attack";
        private const string CARD_MANA = "Mana";

        private bool isSelected = false;
        public event EventHandler<bool> OnCardClicked;
        private Card card;

        public bool IsSelected { get { return isSelected; } set { isSelected = value; UpdateVisualState(); } }

        public Card Card { get { return card; } }

        public GraphicCard()
        {
            InitializeComponent();
        }

        private void UpdateVisualState()
        {
            rectangleCard.Stroke = isSelected ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B52727")) : Brushes.Black;
        }

        public void SetGraphicCard(Card card)
        {
            if (CardManager.Instance().GetCard(Constants.ENEMY_CARD) == card)
            {
                HideCard();
            }
            else
            {
                this.card = card;
                this.card.PropertyChanged += Card_PropertyChanged;

                lblHealth.Content = card.Health.ToString();
                lblAttack.Content = card.Attack.ToString();
                lblMana.Content = card.Mana.ToString();
                lblType.Content = card.Type.ToString();
            }
        }

        private void HideCard()
        {
            gdCard.Background = null;
            gdCard.Background = Brushes.Beige;

            lblHealth.Visibility = Visibility.Collapsed;
            lblAttack.Visibility = Visibility.Collapsed;
            lblMana.Visibility = Visibility.Collapsed;
            lblType.Visibility = Visibility.Collapsed;
        }

        private void Card_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case CARD_HEALTH:
                    lblHealth.Content = card.Health.ToString();
                    break;

                case CARD_ATTACK:
                    lblAttack.Content = card.Attack.ToString();
                    break;

                case CARD_MANA:
                    lblMana.Content = card.Mana.ToString();
                    break;
            }
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
