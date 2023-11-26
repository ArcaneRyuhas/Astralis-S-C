using Astralis.Logic;
using Astralis.UserManager;
using Astralis.Views.Game.GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Astralis.Views.Game
{
    public partial class GameBoard : Window, UserManager.IGameManagerCallback
    {
        private const int GAME_MODE_STARTING_HEALT = 20;
        private const int GAME_MODE_STARTING_MANA = 2;
        private const int COUNTDOWN_STARTING_VALUE = 50; //RECORDAR REGRESARLO A 20
        private const string TEAM_HEALTH = "Health";
        private const string TEAM_MANA = "Mana";
        private const int ERROR_CARD_ID = 0;
        private const int ENEMY_CARD = -1;

        UserManager.GameManagerClient client;
        private Dictionary<string, int> usersTeam;
        private Queue<int> userDeckQueue = new Queue<int>();
        private GraphicCard selectedCard;
        private bool isHost = false;
        private bool isMyTurn = false;
        private DispatcherTimer timer;
        private int countdownValue = COUNTDOWN_STARTING_VALUE;
        private Tuple<string, string> firstPlayers = new Tuple<string, string>("", "");
        private int endTurnCounter = 0;
        private bool roundEnded = false;
        private List<Card> playedCards = new List<Card>();
        private string myEnemy;

        private List<Card> userHand = new List<Card>();
        private Team userTeam;
        private Team enemyTeam;

        public bool IsHost { get { return isHost; } set { isHost = value; } }

        public GameBoard()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            Connect();
            GetUserDeck();
            SetTeams();
            SetCounter();
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
            userDeckQueue = new Queue<int>(userDeck);
        }

        private void DrawFourCards()
        {
            for (int cardsToDraw = 4; cardsToDraw > 0; cardsToDraw--)
            {
                DrawCard();
            }
        }

        private void DrawCard()
        {
            if (userHand.Count < 7)
            {
                int cardToDraw = userDeckQueue.Dequeue();
                Card card = CardManager.Instance().GetCard(cardToDraw);

                userHand.Add(card);
                int indexOfDrawnCard = userHand.IndexOf(card);

                AddCardToHand(userHand[indexOfDrawnCard]);
                client.DrawCard(UserSession.Instance().Nickname, cardToDraw);
            }
        }

        private void SetTeams()
        {
            userTeam = new Team(GAME_MODE_STARTING_MANA, GAME_MODE_STARTING_HEALT);
            userTeam.PropertyChanged += UserTeam_PropertyChanged;

            enemyTeam = new Team(GAME_MODE_STARTING_MANA, GAME_MODE_STARTING_HEALT);
            enemyTeam.PropertyChanged += EnemyTeam_PropertyChange;
        }

        private void SetCounter()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += TimerTick;

            progressBarCounter.Maximum = countdownValue;
            progressBarCounter.Value = countdownValue;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            countdownValue--;
            progressBarCounter.Value = countdownValue;

            if (countdownValue == 0)
            {
                timer.Stop();

                EndTurn();
            }

            DoubleAnimation animation = new DoubleAnimation(countdownValue, TimeSpan.FromSeconds(1));
            progressBarCounter.BeginAnimation(ProgressBar.ValueProperty, animation);
        }

        private void EndTurn()
        {
            if (!roundEnded && isMyTurn)
            {
                roundEnded = true;

                foreach (Card playedCard in playedCards)
                {
                    userHand.Remove(playedCard);
                    playedCards.Remove(playedCard);
                }

                client.EndGameTurn(UserSession.Instance().Nickname, GetBoardDictionary());
            }
        }

        private Dictionary<int, int> GetBoardDictionary()
        {
            Dictionary<int, int> boardDictionary = new Dictionary<int, int>();
            int counter = 0;

            foreach (UIElement child in gdEnemySlots.Children)
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

        private void StartCountdown()
        {
            countdownValue = COUNTDOWN_STARTING_VALUE;

            timer.Start();
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

        //DE AQUI PARA ARRIBA YA REVISAMOS

        private void AddCardToHand(Card card)
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
            if (isMyTurn)
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

            if (isMyTurn && selectedCard != null)
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

        public void DrawCardClient(string nickname, int cardId)
        {
            Card card = CardManager.Instance().GetCard(ENEMY_CARD);
            GraphicCard graphicCard = new GraphicCard();
            graphicCard.SetGraphicCard(card);

            if (usersTeam[nickname] == usersTeam[UserSession.Instance().Nickname])
            {
                card = CardManager.Instance().GetCard(cardId);

                graphicCard.SetGraphicCard(card);
                AddGraphicCardToGrid(graphicCard, gdAllyHand);
            }
            else if (nickname == myEnemy)
            {
                AddGraphicCardToGrid(graphicCard, gdEnemyHand);
            }
            else
            {
                AddGraphicCardToGrid(graphicCard, gdEnemyAllyHand);
            }
        }

        public void EndGameClient(int winnerTeam)
        {
            throw new NotImplementedException();
        }

        public void PlayerEndedTurn(string player, Dictionary<int, int> boardAfterTurn)
        {
            endTurnCounter++;
            int counter = 0;

            foreach (UIElement child in gdEnemySlots.Children)
            {
                if (child is Grid)
                {
                    counter++;

                    if (boardAfterTurn[counter] != ERROR_CARD_ID)
                    {
                        Card card = CardManager.Instance().GetCard(boardAfterTurn[counter]);
                        GraphicCard graphicCard = new GraphicCard();
                        Grid innerGrid = (Grid)child;

                        graphicCard.SetGraphicCard(card);
                        innerGrid.Children.Add(graphicCard);
                        TakeCardOutOfHand(player, graphicCard);
                    }
                }
            }

            foreach (UIElement child in gdPlayerSlots.Children)
            {
                if (child is Grid)
                {
                    counter++;

                    if (boardAfterTurn[counter] != ERROR_CARD_ID)
                    {
                        Card card = CardManager.Instance().GetCard(boardAfterTurn[counter]);
                        GraphicCard graphicCard = new GraphicCard();
                        Grid innerGrid = (Grid)child;

                        graphicCard.SetGraphicCard(card);
                        innerGrid.Children.Add(graphicCard);
                        TakeCardOutOfHand(player, graphicCard);
                    }
                }
            }

            TurnCounter();
        }

        private void TakeCardOutOfHand(string nickname, GraphicCard graphicCardToRemove)
        {
            if(usersTeam[nickname] == usersTeam[UserSession.Instance().Nickname])
            {
                foreach(GraphicCard graphicCard in gdAllyHand.Children)
                {
                    if(graphicCard.Card == graphicCardToRemove.Card)
                    {
                        int columnIndex = Grid.GetColumn(graphicCard);
                        gdAllyHand.Children.Remove(graphicCard);
                        RemoveColumn(gdAllyHand, columnIndex);
                        break;
                    }
                }
            }
            else if(nickname == myEnemy)
            {
                int lastChildren = gdEnemyHand.Children.Count - 1;
                gdEnemyHand.Children.Remove(gdEnemyHand.Children[lastChildren]);
            }
            else
            {
                int lastChildren = gdEnemyAllyHand.Children.Count - 1;
                gdEnemyAllyHand.Children.Remove(gdEnemyAllyHand.Children[lastChildren]);
            }
        }

        private void TurnCounter()
        {
            string myNickname = UserSession.Instance().Nickname;

            switch (endTurnCounter)
            {
                case 2:
                    if (myNickname != firstPlayers.Item1 && myNickname != firstPlayers.Item2)
                    {
                        isMyTurn = true;
                    }
                    break;

                case 4:
                    string[] secondPlayers = usersTeam.Keys.Where(name => name != firstPlayers.Item2 && name != firstPlayers.Item1).ToArray();

                    firstPlayers = Tuple.Create(secondPlayers[0], secondPlayers[1]);
                    break;
            }
        }

        public void ShowUserConnectedGame(string nickname, int team)
        {
            usersTeam.Add(nickname, team);

            _ = StartGameAsync();
        }

        public void ShowUsersInGame(Dictionary<string, int> users)
        {
            this.usersTeam = users;

            _ = StartGameAsync();
        }

        public void StartFirstPhaseClient(Tuple<string, string> firstPlayers)
        {
            this.firstPlayers = firstPlayers;

            if (firstPlayers.Item1 == UserSession.Instance().Nickname)
            {
                isMyTurn = true;
                lblTurnMana.Foreground = Brushes.Yellow;
                myEnemy = firstPlayers.Item2;
            }
            else if (firstPlayers.Item2 == UserSession.Instance().Nickname)
            {
                isMyTurn = true;
                lblTurnMana.Foreground = Brushes.Yellow;
                myEnemy = firstPlayers.Item1;
            }
            else
            {
                foreach (string nickname in usersTeam.Keys)
                {
                    if (firstPlayers.Item1 != nickname && firstPlayers.Item2 != nickname && nickname != UserSession.Instance().Nickname)
                    {
                        myEnemy = nickname;
                    }
                }
            }

            roundEnded = false;

            DrawFourCards();
            StartCountdown();
        }

        private async Task StartGameAsync()
        {
            if(usersTeam.Count == 4 ) 
            {
                await Task.Delay(2000);
                
                if (isHost)
                {
                    client.StartFirstPhase(UserSession.Instance().Nickname);
                }

                await Task.Delay(2000);
            }
        }

        private void btnMenu_Click(object sender, RoutedEventArgs e)
        {
            EndTurn();
        }

        private void btnChangeView_Click(object sender, RoutedEventArgs e)
        {
            if(gdPlayerHand.IsVisible)
            {
                gdPlayerHand.Visibility = Visibility.Collapsed;
                gdAllyHand.Visibility = Visibility.Visible;
            }
            else
            {
                gdPlayerHand.Visibility = Visibility.Visible;
                gdAllyHand.Visibility = Visibility.Collapsed;
            }
        }


    }
}
