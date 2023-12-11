using Astralis.Logic;
using Astralis.Views.Game.GameLogic;
using Astralis.Views.Pages;
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
        private const int GAME_MODE_STARTING_MANA = 2;
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
        private GameWindow gameWindow;

        public bool IsHost { get { return isHost; } set { isHost = value; } }

        public List<Card> PlayedCards { get { return playedCards; } }

        public Label LblTurnMana { get { return lblTurnMana; } }

        public GameBoard(GameWindow gameWindow)
        {
            InitializeComponent();
            InitializeGame();
            this.gameWindow = gameWindow;
        }

        private void InitializeGame()
        {
            Connect();
            SetTeams();
            gameManager = new GameManager();
            lblMyNickname.Content = UserSession.Instance().Nickname;

            gameManager.SetGameBoard(this);
            gameManager.SetCounter(progressBarCounter);
            gameManager.UserTeam = userTeam;
            gameManager.EnemyTeam = enemyTeam;
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

                    foreach (GraphicCard innerCard in innerGrid.Children)
                    {
                        innerCard.OnCardClicked -= GraphicCardClickedHandler;
                        cardId = CardManager.Instance().GetCardId(innerCard.Card.Clone());
                        break;
                    }

                    boardDictionary.Add(counter, cardId);
                }
            }

            return boardDictionary;
        }

        public GraphicCard[] GetAttackBoard(Grid gdBoard)
        {
            GraphicCard[] attackBoard = new GraphicCard[5];
            int counter = 0;

            foreach (UIElement child in gdBoard.Children)
            {
                if (child is Grid)
                {
                    Grid innerGrid = (Grid)child;
                    GraphicCard graphicCard = new GraphicCard();
                    graphicCard.SetGraphicCard(CardManager.Instance().GetCard(ERROR_CARD_ID));

                    foreach (GraphicCard innerCard in innerGrid.Children)
                    {
                        graphicCard = innerCard;
                        break;
                    }

                    attackBoard[counter] = graphicCard;
                    counter++;
                }
            }

            return attackBoard;
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
                lblEnemyHealth.Content = enemyTeam.Health;
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

        public void DeleteGraphicCard(GraphicCard graphicCardToRemove)
        {
            Grid currentCardParent = VisualTreeHelper.GetParent(graphicCardToRemove) as Grid;

            if (currentCardParent != null)
            {
                int columnIndex = Grid.GetColumn(graphicCardToRemove);
                currentCardParent.Children.Remove(graphicCardToRemove);
                RemoveColumn(currentCardParent, columnIndex);
            }
        }

        private void PlaceCardInSlot(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (gameManager.IsMyTurn && selectedCard != null)
            {
                Grid boardCardSlot = sender as Grid;
                Grid currentCardParent = VisualTreeHelper.GetParent(selectedCard) as Grid;

                if (currentCardParent != boardCardSlot && currentCardParent != gdPlayerHand && boardCardSlot.Children.Count == 0)
                {
                    selectedCard.IsSelected = false;

                    currentCardParent.Children.Remove(selectedCard);
                    boardCardSlot.Children.Add(selectedCard);

                    selectedCard.IsSelected = false;
                    selectedCard = null;
                }

                if (currentCardParent == gdPlayerHand && boardCardSlot.Children.Count == 0 && userTeam.UseMana(selectedCard.Card.Mana))
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

        public void GameHasEnded(int winnerTeam)
        {
            string myNickname = UserSession.Instance().Nickname;

            if (isHost)
            {
                client.EndGame(winnerTeam, myNickname);
            }
        }

        public void EndGameClient(int winnerTeam)
        {
            string myNickname = UserSession.Instance().Nickname;
            EndGame endGame = new EndGame(winnerTeam, gameManager.UsersTeam[myNickname]);

            gameWindow.ChangePage(endGame);
            gameWindow.Visibility = Visibility.Visible;
            
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
                RemoveCardFromHand(gdAllyHand, graphicCardToRemove);
            }
            else if (nickname == gameManager.MyEnemy)
            {
                RemoveCardFromHand(gdEnemyHand, graphicCardToRemove);
            }
            else
            {
                RemoveCardFromHand(gdEnemyAllyHand, graphicCardToRemove);
            }
        }

        private void RemoveCardFromHand(Grid gridToModify, GraphicCard graphicCardToRemove)
        {
            foreach (GraphicCard graphicCard in gridToModify.Children)
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

        public void ShowUserConnectedGame(string nickname, int team)
        {
            gameManager.UsersTeam.Add(nickname, team);

            _ = StartGameAsync();
        }

        public void ShowUsersInGame(Dictionary<string, int> users)
        {
            gameManager.UsersTeam = users;

            lblUserTeam.Content += gameManager.UsersTeam[UserSession.Instance().Nickname].ToString(); //CAMBIAR DESPUES DE PROBAR

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
