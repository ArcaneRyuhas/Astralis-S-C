using Astralis.Logic;
using Astralis.Views.Game.GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Astralis.Views.Game
{
    public partial class GameBoard : Window, UserManager.IGameManagerCallback
    {
        private const int GAME_MODE_STARTING_HEALT = 20;
        private const int GAME_MODE_STARTING_MANA = 10; // CAMBIAR DESPUES DE PROBAR A 2
        private const string TEAM_HEALTH = "Health";
        private const string TEAM_MANA = "Mana";
        private const int ERROR_CARD_ID = 0;
        private const int ENEMY_CARD = -1;
        private const int ALL_USERS_CONNECTED = 4;

        private GameManager gameManager;
        UserManager.GameManagerClient client;
        private GraphicCard selectedCard;
        private bool isHost = false;
        private List<Card> playedCards = new List<Card>();
        private Team userTeam;
        private Team enemyTeam;

        public bool IsHost { get { return isHost; } set { isHost = value; } }

        public List<Card> PlayedCards { get { return playedCards; } }

        public Label LblTurnMana { get { return lblTurnMana; } }

        public GameBoard()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            Connect();
            SetTeams();
            gameManager = new GameManager();

            gameManager.SetGameBoard(this);
            gameManager.SetCounter(progressBarCounter);
            gameManager.UserTeam = userTeam;
        }

        private void Connect()
        {
            client = SetGameContext();
            client.ConnectGame(UserSession.Instance().Nickname);
        }

        private UserManager.GameManagerClient SetGameContext()
        {
            InstanceContext context = new InstanceContext(this);
            UserManager.GameManagerClient client = new UserManager.GameManagerClient(context);

            return client;
        }

        private void GetUserDeck()
        {
            int[] userDeck = client.DispenseCards(UserSession.Instance().Nickname);
            gameManager.UserDeckQueue = new Queue<int>(userDeck);
        }


        //We preferred to leave the DrawFourCards in this class because we need the Await and here was found to be more easy to implement and to understand.
        private void DrawFourCards()
        {
            string myNickname = UserSession.Instance().Nickname;

            int[] drawnCard = new int[4];

            for (int cardsToDraw = 0; cardsToDraw < 4; cardsToDraw++)
            {
                drawnCard[cardsToDraw] = gameManager.DrawCard();
            }

            client.DrawCard(myNickname, drawnCard);
        }

        public void DrawCard()
        {
            string myNickname = UserSession.Instance().Nickname;
            int[] drawnCard = new int[1] { gameManager.DrawCard()};

            client.DrawCard(myNickname, drawnCard);
        }

        //We preferred to leave SetTeams method in this class because of the use of the PropertyChanges.
        private void SetTeams()
        {
            userTeam = new Team(GAME_MODE_STARTING_MANA, GAME_MODE_STARTING_HEALT);
            userTeam.PropertyChanged += UserTeam_PropertyChanged;

            enemyTeam = new Team(GAME_MODE_STARTING_MANA, GAME_MODE_STARTING_HEALT);
            enemyTeam.PropertyChanged += EnemyTeam_PropertyChange;
        }

        public void EndGameTurn()
        {
            client.EndGameTurn(UserSession.Instance().Nickname, GetBoardDictionary());
        }

        private Dictionary<int, int> GetBoardDictionary()
        {
            Dictionary<int, int> boardDictionary = new Dictionary<int, int>();
            int counter = 0;

            foreach (UIElement child in gdPlayerSlots.Children)
            {
                if (child is Grid)
                {
                    counter++;

                    Grid innerGrid = (Grid)child;
                    int cardId = ERROR_CARD_ID;

                    if (innerGrid.Children.Count == 1 && innerGrid.Children[0] is GraphicCard)
                    {
                        GraphicCard graphicCard = innerGrid.Children[0] as GraphicCard;
                        cardId = CardManager.Instance().GetCardId(graphicCard.Card.Clone());
                    }

                    boardDictionary.Add(counter, cardId);
                }
            }

            return boardDictionary;
        }

        private void UserTeam_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            string changedProperty = e.PropertyName;

            if (changedProperty == TEAM_HEALTH)
            {
                lblPlayerHealth.Content = userTeam.Health;
            }
            else if (changedProperty == TEAM_MANA)
            {
                lblTurnMana.Content = userTeam.Mana;
            }
        }

        private void EnemyTeam_PropertyChange(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            string changedProperty = e.PropertyName;

            if (changedProperty == TEAM_HEALTH)
            {
                lblEnemyHealth.Content = userTeam.Health;
            }
        }

        public void AddCardToHand(Card card)
        {
            GraphicCard graphicCard = new GraphicCard();
            graphicCard.SetGraphicCard(card);
            graphicCard.OnCardClicked += GraphicCardClickedHandler;

            AddGraphicCardToGrid(graphicCard, gdPlayerHand);
        }

        private void AddGraphicCardToGrid(GraphicCard graphicCard, Grid gridToAddCard)
        {
            int numberOfColumns = gridToAddCard.ColumnDefinitions.Count;

            Grid.SetColumn(graphicCard, numberOfColumns);
            gridToAddCard.Children.Add(graphicCard);

            ColumnDefinition columnDefinition = new ColumnDefinition();
            columnDefinition.Width = GridLength.Auto;
            gridToAddCard.ColumnDefinitions.Add(columnDefinition);
        }

        private void GraphicCardClickedHandler(object sender, bool leftClick)
        {
            if (gameManager.IsMyTurn)
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

                    if (currentCardParent != null && currentCardParent != gdPlayerHand)
                    {
                        currentCardParent.Children.Remove(clickedCard);
                        AddGraphicCardToGrid(clickedCard, gdPlayerHand);
                        playedCards.Remove(clickedCard.Card);

                        userTeam.Mana += clickedCard.Card.Mana;
                    }
                }
            }
        }

        private void PlaceCardInSlot(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (gameManager.IsMyTurn && selectedCard != null)
            {
                Grid boardCardSlot = sender as Grid;
                Grid currentCardParent = VisualTreeHelper.GetParent(selectedCard) as Grid;

                if (currentCardParent != boardCardSlot && currentCardParent != gdPlayerHand)
                {
                    selectedCard.IsSelected = false;

                    currentCardParent.Children.Remove(selectedCard);
                    boardCardSlot.Children.Add(selectedCard);

                    selectedCard.IsSelected = false;
                    selectedCard = null;
                }

                if (currentCardParent == gdPlayerHand && userTeam.UseMana(selectedCard.Card.Mana))
                {
                    if (boardCardSlot.Children.Count == 0)
                    {
                        selectedCard.IsSelected = false;

                        int columnIndex = Grid.GetColumn(selectedCard);
                        currentCardParent.Children.Remove(selectedCard);
                        RemoveColumn(currentCardParent, columnIndex);

                        boardCardSlot.Children.Add(selectedCard);
                        playedCards.Add(selectedCard.Card);

                        selectedCard.IsSelected = false;
                        selectedCard = null;
                    }
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
            }
        }

        public void DrawCardClient(string nickname, int [] cardsId)
        {
            foreach(int cardId in cardsId) 
            {
                GraphicCard graphicCard = new GraphicCard();
                Card card = CardManager.Instance().GetCard(cardId);

                graphicCard.SetGraphicCard(card);
                AddGraphicCardToGrid(graphicCard, gdAllyHand);
            }
        }

        public void EndGameClient(int winnerTeam)
        {
            //MOSTRAR EN PANTALLA GANADOR

            //ABRIR VENTANA DE PARTIDA ACABADA
            this.Close();
        }

        public void PlayerEndedTurn(string player, Dictionary<int, int> boardAfterTurn)
        {
            gameManager.PlayerEndedTurn(player, boardAfterTurn, gdEnemySlots, gdPlayerSlots);
        }

        public void TakeCardOutOfHand(string nickname, GraphicCard graphicCardToRemove, Dictionary<string, int> usersTeam)
        {
            if (usersTeam[nickname] == usersTeam[UserSession.Instance().Nickname])
            {
                foreach (GraphicCard graphicCard in gdAllyHand.Children)
                {
                    if (graphicCard.Card.Equals(graphicCardToRemove.Card))
                    {
                        int columnIndex = Grid.GetColumn(graphicCard);
                        gdAllyHand.Children.Remove(graphicCard);
                        RemoveColumn(gdAllyHand, columnIndex);
                        break;
                    }
                }
            }
            else if (nickname == gameManager.MyEnemy)
            {
                foreach (GraphicCard graphicCard in gdEnemyHand.Children)
                {
                    int columnIndex = Grid.GetColumn(graphicCard);
                    gdEnemyHand.Children.Remove(graphicCard);
                    RemoveColumn(gdEnemyHand, columnIndex);
                    break;
                }
            }
            else
            {
                foreach (GraphicCard graphicCard in gdEnemyAllyHand.Children)
                {
                    int columnIndex = Grid.GetColumn(graphicCard);
                    gdEnemyAllyHand.Children.Remove(graphicCard);
                    RemoveColumn(gdEnemyAllyHand, columnIndex);
                    break;
                }
            }
        }

        public void ShowUserConnectedGame(string nickname, int team)
        {
            gameManager.UsersTeam.Add(nickname, team);

            _ = StartGameAsync();
        }

        public void ShowUsersInGame(Dictionary<string, int> users)
        {
            gameManager.UsersTeam = users;

            lblEnemyHealth.Content = gameManager.UsersTeam[UserSession.Instance().Nickname].ToString(); //CAMBIAR DESPUES DE PROBAR

            _ = StartGameAsync();
        }

        public void StartFirstPhaseClient(Tuple<string, string> firstPlayers)
        {
            gameManager.StartFirstPhaseClient(firstPlayers);
            DrawFourCards();
            gameManager.StartCountdown();
        }

        public void EnemyDrawCard ()
        {
            Card card = CardManager.Instance().GetCard(ENEMY_CARD);
            GraphicCard graphicCardOne = new GraphicCard();

            graphicCardOne.SetGraphicCard(card);

            GraphicCard graphicCardTwo = new GraphicCard();

            graphicCardTwo.SetGraphicCard(card);

            AddGraphicCardToGrid(graphicCardOne, gdEnemyHand);
            AddGraphicCardToGrid(graphicCardTwo, gdEnemyAllyHand);
        }

        private async Task StartGameAsync()
        {
            if(gameManager.UsersTeam.Count == ALL_USERS_CONNECTED ) 
            {
                await Task.Delay(2000);
                GetUserDeck();
                await Task.Delay(5000);
                
                if (isHost)
                {
                    client.StartFirstPhase(UserSession.Instance().Nickname);
                }

            }
        }

        private void BtnMenuClick(object sender, RoutedEventArgs e)
        {
            gameManager.EndTurn();
        }

        private void BtnChangeViewClick(object sender, RoutedEventArgs e)
        {
            if(gdPlayerHand.IsVisible)
            {
                gdPlayerHand.Visibility = Visibility.Collapsed;
                gdAllyHand.Visibility = Visibility.Visible;
                gdEnemyHand.Visibility = Visibility.Collapsed;
                gdEnemyAllyHand.Visibility = Visibility.Visible;
            }
            else
            {
                gdPlayerHand.Visibility = Visibility.Visible;
                gdAllyHand.Visibility = Visibility.Collapsed;
                gdEnemyHand.Visibility = Visibility.Visible;
                gdEnemyAllyHand.Visibility = Visibility.Collapsed;
            }
        }


    }
}
