using Astralis.Logic;
using Astralis.Views.Game.GameLogic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Astralis.Views.Game
{
    public partial class GameBoard : Window, UserManager.IGameManagerCallback
    {
        private Queue<int> userDeckQueue = new Queue<int>();
        private List <Card> userHand = new List<Card>();
        private Dictionary<string, int> users;
        private int columnCards = 0;
        private GraphicCard selectedCard;

        public GameBoard()
        {
            InitializeComponent();
            Connect();
            GetUserDeck();
            DrawFourCards();
        }

        private void Connect()
        {
            UserManager.GameManagerClient client = SetGameContext();
            client.ConnectGame(UserSession.Instance().Nickname);
        }

        private void GetUserDeck()
        {
            UserManager.GameManagerClient client = SetGameContext();
            int[] userDeck = client.DispenseCards(UserSession.Instance().Nickname);
            userDeckQueue = new Queue<int>(userDeck);
        }

        private void DrawFourCards()
        {
            UserManager.GameManagerClient client = SetGameContext();

            for (int cardsToDraw = 4; cardsToDraw > 0; cardsToDraw--)
            {
                int cardToDraw = userDeckQueue.Dequeue();
                Card card = CardManager.Instance().GetCard(cardToDraw);

                userHand.Add(card);
                int indexOfDrawnCard = userHand.IndexOf(card);

                AddCardToHand(userHand[indexOfDrawnCard]);
                client.DrawCard(UserSession.Instance().Nickname, cardToDraw);
            }
        }

        private void AddCardToHand(Card card)
        {
            GraphicCard graphicCard = new GraphicCard();
            graphicCard.SetGraphicCard(card);
            graphicCard.OnCardClicked += GraphicCardClickedHandler;

            AddGraphicCardToHand(graphicCard);
        }

        private void AddGraphicCardToHand(GraphicCard graphicCard)
        {
            Grid.SetColumn(graphicCard, columnCards);
            gdPlayerHand.Children.Add(graphicCard);
            columnCards++;

            ColumnDefinition columnDefinition = new ColumnDefinition();
            columnDefinition.Width = GridLength.Auto;
            gdPlayerHand.ColumnDefinitions.Add(columnDefinition);
        }

        private void GraphicCardClickedHandler(object sender, bool leftClick)
        {
            GraphicCard clickedCard = sender as GraphicCard;

            if (leftClick)
            {
                if (selectedCard != null)
                {
                    selectedCard.IsSelected = false;
                }

                selectedCard = clickedCard;
                selectedCard.IsSelected = true;
            }
            else
            {
                Grid currentCardParent = VisualTreeHelper.GetParent(clickedCard) as Grid;
                
                if(currentCardParent != null && currentCardParent != gdPlayerHand)
                {
                    currentCardParent.Children.Remove(clickedCard);

                    AddGraphicCardToHand(clickedCard);
                }
            }
            
        }

        private void PlaceCardInSlot(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (selectedCard != null)
            {
                Grid grid = sender as Grid;

                if (grid.Children.Count == 0)
                {
                    Grid currentCardParent = VisualTreeHelper.GetParent(selectedCard) as Grid;
                    if (currentCardParent != null)
                    {
                        selectedCard.IsSelected = false;
                        int columnIndex = Grid.GetColumn(selectedCard);
                        currentCardParent.Children.Remove(selectedCard);
                        RemoveColumn(currentCardParent, columnIndex);
                    }

                    grid.Children.Add(selectedCard);

                    selectedCard.IsSelected = false;
                    selectedCard = null;
                }
            }
        }

        private void RemoveColumn(Grid grid, int columnIndex)
        {
            if (columnIndex >= 0 && columnIndex < grid.ColumnDefinitions.Count)
            {
                grid.ColumnDefinitions.RemoveAt(columnIndex);
                foreach (UIElement child in grid.Children)
                {
                    int currentColumn = Grid.GetColumn(child);
                    if (currentColumn > columnIndex)
                    {
                        Grid.SetColumn(child, currentColumn - 1);
                    }
                }
                columnCards--;
            }

        }

        private UserManager.GameManagerClient SetGameContext()
        {
            InstanceContext context = new InstanceContext(this);
            UserManager.GameManagerClient client = new UserManager.GameManagerClient(context);

            return client;
        }

        public void DrawCardClient(string nickname, int cardId)
        {
            throw new NotImplementedException();
        }

        public void EndGameClient(int winnerTeam)
        {
            throw new NotImplementedException();
        }

        public void EndPhase()
        {
            throw new NotImplementedException();
        }

        public void PlayerEndedTurn(string player, Dictionary<int, int> boardAfterTurn)
        {
            throw new NotImplementedException();
        }

        public void StartNewPhaseClient(Dictionary<int, int> boardAfterPhase)
        {
            throw new NotImplementedException();
        }

        
    }
}
