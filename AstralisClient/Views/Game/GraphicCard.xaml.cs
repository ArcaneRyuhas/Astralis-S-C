using Astralis.Views.Game.GameLogic;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Astralis.Views.Game
{
    public partial class GraphicCard : UserControl
    {
        private const bool IS_LEFT_CLICKED = true;
        private const bool IS_RIGHT_CLICKED = false;
        private const string CARD_HEALTH = "Health";
        private const string CARD_ATTACK = "Attack";
        private const string CARD_MANA = "Mana";

        private bool _isSelected = false;
        private Card _card;
        public event EventHandler<bool> OnCardClicked;
        

        public bool IsSelected { 
            get 
            { 
                return _isSelected; 
            } 
            
            set 
            {
                _isSelected = value; UpdateVisualState(); 
            } 
        }

        public Card Card { get { return _card; } }

        public GraphicCard()
        {
            InitializeComponent();
        }

        private void UpdateVisualState()
        {
            rectangleCard.Stroke = _isSelected ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B52727")) : Brushes.Black;
        }

        public void SetGraphicCard(Card card)
        {
            if (card.Mana != 0)
            {
                _card = card;
                _card.PropertyChanged += CardPropertyChanged;

                lblHealth.Content = card.Health.ToString();
                lblAttack.Content = card.Attack.ToString();
                lblMana.Content = card.Mana.ToString();
                lblType.Content = card.Type.ToString();
            }
            else
            {
                HideCard();
            }
        }

        private void HideCard()
        {
            gdCard.Background = null;

            lblHealth.Visibility = Visibility.Collapsed;
            lblAttack.Visibility = Visibility.Collapsed;
            lblMana.Visibility = Visibility.Collapsed;
            lblType.Visibility = Visibility.Collapsed;
        }

        private void CardPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case CARD_HEALTH:
                    lblHealth.Content = _card.Health.ToString();
                    break;

                case CARD_ATTACK:
                    lblAttack.Content = _card.Attack.ToString();
                    break;

                case CARD_MANA:
                    lblMana.Content = _card.Mana.ToString();
                    break;
            }
        }

        private void GraphicCardOnLeftClick(object sender, MouseButtonEventArgs e)
        {
            OnCardClicked?.Invoke(this, IS_LEFT_CLICKED);
        }

        private void GraphicCardOnRightClick(object sender, MouseButtonEventArgs e)
        {
            OnCardClicked?.Invoke(this, IS_RIGHT_CLICKED);
        }
    }
}
