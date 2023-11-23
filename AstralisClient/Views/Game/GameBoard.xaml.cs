using Astralis.Logic;
using Astralis.UserManager;
using Astralis.Views.Game.GameLogic;
using System;
using System.Collections.Generic;
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
        private const int GAME_MODE_STARTING_MANA = 10;
        private const int COUNTDOWN_STARTING_VALUE = 20;
        private const string TEAM_HEALTH = "Health";
        private const string TEAM_MANA = "Mana";

        private Dictionary<string, int> users;
        private Queue<int> userDeckQueue = new Queue<int>();
        private GraphicCard selectedCard;
        private int userColumnCards = 0;
        private int allyColumnCards = 0;
        private DispatcherTimer timer;
        private int countdownValue = COUNTDOWN_STARTING_VALUE;

        private List<Card> userHand = new List<Card>();
        private Team userTeam;
        private Team enemyTeam;


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

            countdownValue --;
            progressBarCounter.Value = countdownValue;

            if(countdownValue == 0) 
            {
                timer.Stop();
            }

            DoubleAnimation animation = new DoubleAnimation(countdownValue, TimeSpan.FromSeconds(1));
            progressBarCounter.BeginAnimation(ProgressBar.ValueProperty, animation);
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
                lblPlayerMana.Content = userTeam.Mana;
            }
        }

        private void EnemyTeam_PropertyChange(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            string changedProperty = e.PropertyName;

            if (changedProperty == TEAM_HEALTH)
            {
                lblEnemyHealth.Content = userTeam.Health;
            }
            else if (changedProperty == TEAM_MANA)
            {
                lblEnemyMana.Content = userTeam.Mana;
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
            Grid.SetColumn(graphicCard, userColumnCards);
            gdPlayerHand.Children.Add(graphicCard);
            userColumnCards++;

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

                    userTeam.Mana += clickedCard.Card.Mana;
                }
            }
        }

        private void PlaceCardInSlot(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (selectedCard != null)
            {
                Grid grid = sender as Grid;
                Grid currentCardParent = VisualTreeHelper.GetParent(selectedCard) as Grid;

                if(currentCardParent != null)
                {
                    if (currentCardParent != grid && currentCardParent != gdPlayerHand)
                    {
                        selectedCard.IsSelected = false;

                        currentCardParent.Children.Remove(selectedCard);
                        grid.Children.Add(selectedCard);

                        selectedCard.IsSelected = false;
                        selectedCard = null;
                    }

                    if (currentCardParent == gdPlayerHand && userTeam.UseMana(selectedCard.Card.Mana))
                    {
                        if (grid.Children.Count == 0)
                        {
                            selectedCard.IsSelected = false;

                            int columnIndex = Grid.GetColumn(selectedCard);
                            currentCardParent.Children.Remove(selectedCard);
                            RemoveColumn(currentCardParent, columnIndex);

                            grid.Children.Add(selectedCard);

                            selectedCard.IsSelected = false;
                            selectedCard = null;
                        }
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
                userColumnCards--;
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
            if (users[nickname] == users[UserSession.Instance().Nickname])
            {
                Card card = CardManager.Instance().GetCard(cardId);

                AddCardToAllyHand(card);
            }
        }

        private void AddCardToAllyHand(Card card)
        {
            GraphicCard graphicCard = new GraphicCard();
            graphicCard.SetGraphicCard(card);

            AddGraphicCardToAllyHand(graphicCard);
        }

        private void AddGraphicCardToAllyHand(GraphicCard graphicCard)
        {
            Grid.SetColumn(graphicCard, allyColumnCards);
            gdAllyHand.Children.Add(graphicCard);
            allyColumnCards++;

            ColumnDefinition columnDefinition = new ColumnDefinition();
            columnDefinition.Width = GridLength.Auto;
            gdAllyHand.ColumnDefinitions.Add(columnDefinition);
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

        public void ShowUserConnectedGame(string nickname, int team)
        {
            users.Add(nickname, team);

            _ = StartGameAsync();
        }

        public void ShowUsersInGame(Dictionary<string, int> users)
        {
            this.users = users;

            _ = StartGameAsync();
        }

        private async Task StartGameAsync()
        {

            lblEnemyHealth.Content = users.Count.ToString();

            if(users.Count == 4 ) 
            {
                await Task.Delay(10000);
                DrawFourCards();
            }
        }

        private void btnMenu_Click(object sender, RoutedEventArgs e)
        {
            StartCountdown();
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
