using Astralis.Logic;
using Astralis.Views.Game.GameLogic;
using Astralis.Views.Pages;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Astralis.UserManager;
using System.Linq;
using System.Windows.Input;

namespace Astralis.Views.Game
{
    public partial class GameBoard : Window, IGameManagerCallback
    {
        private const int GAME_MODE_STARTING_HEALT = 20;
        private const int GAME_MODE_STARTING_MANA = 2;
        private const string TEAM_HEALTH = "Health";
        private const string TEAM_MANA = "Mana";
        private const int ERROR_CARD_ID = 0;
        private const int ENEMY_CARD = -1;
        private const int ALL_USERS_CONNECTED = 4;
        private const int MAX_CHAT_LENGHT = 100;

        private GameManager _gameManager;
        private GameManagerClient _client;
        private GraphicCard _selectedCard;
        private List<Card> _playedCards = new List<Card>();
        private Team _userTeam;
        private Team _enemyTeam;

        public bool IsHost { get; set; }

        public List<Card> PlayedCards { get { return _playedCards; } }

        public GameBoard()
        {
            InitializeComponent();
            ConnectToGame();
            SetTeams();
            SetGameManager();

            lblMyNickname.Content = UserSession.Instance().Nickname;
            IsHost = false;
        }

        private void SetGameManager()
        {
            _gameManager = new GameManager(gdEnemySlots, gdPlayerSlots);

            _gameManager.SetGameBoard(this);
            _gameManager.SetGameCounters(progressBarCounter);

            _gameManager.UserTeam = _userTeam;
            _gameManager.EnemyTeam = _enemyTeam;

            _gameManager.StartExitTimer();
        }

        private void ConnectToGame()
        {
            try
            {
                InstanceContext context = new InstanceContext(this);
                _client = new GameManagerClient(context);

                _client.ConnectGame(UserSession.Instance().Nickname);
            }
            catch (CommunicationObjectFaultedException)
            {
                MessageBox.Show(Properties.Resources.msgPreviousConnectioLost, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
        }

        private void GetUserDeck()
        {
            int[] userDeck = _client.DispenseGameCards(UserSession.Instance().Nickname);
            _gameManager.UserDeckQueue = new Queue<int>(userDeck);
        }

        /*
         * We preferred to leave the DrawFourCards in this class because 
         * we need to make graphic cards for every card that is drawn and 
         * the communication between server and client.
         */

        private void DrawFourCards()
        {
            string myNickname = UserSession.Instance().Nickname;
            int[] drawnCard = new int[4];

            for (int cardsToDraw = 0; cardsToDraw < 4; cardsToDraw++)
            {
                drawnCard[cardsToDraw] = _gameManager.DrawCard();
            }

            try
            {
                _client.DrawGameCard(myNickname, drawnCard);
            }
            catch (CommunicationObjectFaultedException)
            {
                MessageBox.Show(Properties.Resources.msgPreviousConnectioLost, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
        }

        public void DrawCard()
        {
            string myNickname = UserSession.Instance().Nickname;
            int[] drawnCard = new int[1] { _gameManager.DrawCard()};

            try
            {
                _client.DrawGameCard(myNickname, drawnCard);
            }
            catch (CommunicationObjectFaultedException)
            {
                MessageBox.Show(Properties.Resources.msgPreviousConnectioLost, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
        }

        //We preferred to leave SetTeams method in this class because of the use of the PropertyChanges in the graphic manner.
        private void SetTeams()
        {
            _userTeam = new Team(GAME_MODE_STARTING_MANA, GAME_MODE_STARTING_HEALT);
            _userTeam.PropertyChanged += UserTeamPropertyChanged;
            _enemyTeam = new Team(GAME_MODE_STARTING_MANA, GAME_MODE_STARTING_HEALT);
            _enemyTeam.PropertyChanged += EnemyTeamPropertyChange;
        }

        public void EndGameTurn()
        {
            try
            {
                _client.EndGameTurn(UserSession.Instance().Nickname, GetBoardDictionary());
            }
            catch (CommunicationObjectFaultedException)
            {
                MessageBox.Show(Properties.Resources.msgPreviousConnectioLost, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
        }

        private Dictionary<int, int> GetBoardDictionary()
        {
            Dictionary<int, int> boardDictionary = new Dictionary<int, int>();
            int counter = 0;

            foreach (UIElement child in gdPlayerSlots.Children)
            {
                AddCardToBoardDictionary(child, ref counter, boardDictionary);
            }

            return boardDictionary;
        }

        private void AddCardToBoardDictionary(UIElement child, ref int counter, Dictionary<int, int> boardDictionary)
        {
            if (child is Grid grid)
            {
                counter++;
                Grid innerGrid = grid;
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

        public GraphicCard[] GetAttackBoard(Grid gdBoard)
        {
            GraphicCard[] attackBoard = new GraphicCard[5];
            int counter = 0;

            foreach (UIElement child in gdBoard.Children)
            {
                AddCardToAttackList(attackBoard, ref counter, child);
            }

            return attackBoard;
        }

        private void AddCardToAttackList(GraphicCard[] attackBoard, ref int counter, UIElement child)
        {
            if (child is Grid innerGrid)
            {
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

        private void UserTeamPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            string changedProperty = e.PropertyName;

            if (changedProperty == TEAM_HEALTH)
            {
                lblPlayerHealth.Content = _userTeam.Health;
            }
            else if (changedProperty == TEAM_MANA)
            {
                lblTurnMana.Content = _userTeam.Mana;
            }
        }

        private void EnemyTeamPropertyChange(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            string changedProperty = e.PropertyName;

            if (changedProperty == TEAM_HEALTH)
            {
                lblEnemyHealth.Content = _enemyTeam.Health;
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
            if (_gameManager.IsMyTurn)
            {
                GraphicCard clickedCard = sender as GraphicCard;

                if (leftClick)
                {
                    LeftClicOnCard(clickedCard);
                }
                else
                {
                    RightClicOnCard(clickedCard);
                }
            }
        }

        private void LeftClicOnCard(GraphicCard clickedCard)
        {
            if (_selectedCard != null)
            {
                _selectedCard.IsSelected = false;
            }

            _selectedCard = clickedCard;
            _selectedCard.IsSelected = true;
        }

        private void RightClicOnCard(GraphicCard clickedCard)
        {
            Grid currentCardParent = VisualTreeHelper.GetParent(clickedCard) as Grid;

            if (currentCardParent != null && currentCardParent != gdPlayerHand)
            {
                currentCardParent.Children.Remove(clickedCard);
                AddGraphicCardToGrid(clickedCard, gdPlayerHand);
                _playedCards.Remove(clickedCard.Card);

                _userTeam.Mana += clickedCard.Card.Mana;
            }
        }

        public void DeleteGraphicCard(GraphicCard graphicCardToRemove)
        {
            Grid currentCardParent = VisualTreeHelper.GetParent(graphicCardToRemove) as Grid;

            if (currentCardParent != null)
            {
                int columnIndex = Grid.GetColumn(graphicCardToRemove);

                currentCardParent.Children.Remove(graphicCardToRemove);
                RemoveGridColumn(currentCardParent, columnIndex);
            }
        }

        private void PlaceCardInGameSlot(object sender, MouseButtonEventArgs e)
        {
            if (_gameManager.IsMyTurn && _selectedCard != null)
            {
                Grid boardCardSlot = sender as Grid;
                Grid currentCardParent = VisualTreeHelper.GetParent(_selectedCard) as Grid;

                MoveCardFromBoardSlotToAnother(boardCardSlot, currentCardParent);
                MoveCardFromHandToBoard(boardCardSlot, currentCardParent);
            }
        }

        private void MoveCardFromHandToBoard(Grid boardCardSlot, Grid currentCardParent)
        {
            if (currentCardParent == gdPlayerHand && boardCardSlot.Children.Count == 0 && _userTeam.UseMana(_selectedCard.Card.Mana))
            {
                _selectedCard.IsSelected = false;
                int columnIndex = Grid.GetColumn(_selectedCard);

                currentCardParent.Children.Remove(_selectedCard);
                RemoveGridColumn(currentCardParent, columnIndex);
                boardCardSlot.Children.Add(_selectedCard);
                _playedCards.Add(_selectedCard.Card);

                _selectedCard.IsSelected = false;
                _selectedCard = null;
            }
        }

        private void MoveCardFromBoardSlotToAnother(Grid boardCardSlot, Grid currentCardParent)
        {
            if (currentCardParent != boardCardSlot && currentCardParent != gdPlayerHand && boardCardSlot.Children.Count == 0)
            {
                _selectedCard.IsSelected = false;

                currentCardParent.Children.Remove(_selectedCard);
                boardCardSlot.Children.Add(_selectedCard);

                _selectedCard.IsSelected = false;
                _selectedCard = null;
            }
        }

        private void RemoveGridColumn(Grid grid, int columnIndex)
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

        public void ShowCardDrawedInGame(string nickname, int [] cardId)
        {
            foreach(int Id in cardId) 
            {
                GraphicCard graphicCard = new GraphicCard();
                Card card = CardManager.Instance().GetCard(Id);

                graphicCard.SetGraphicCard(card);
                AddGraphicCardToGrid(graphicCard, gdAllyHand);
            }
        }

        public void GameHasEnded(int winnerTeam)
        {
            string myNickname = UserSession.Instance().Nickname;

            if (IsHost)
            {
                try
                {
                    _client.EndGame(winnerTeam, myNickname);
                }
                catch (CommunicationObjectFaultedException)
                {
                    MessageBox.Show(Properties.Resources.msgPreviousConnectioLost, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                    App.RestartApplication();
                }
                catch (CommunicationException)
                {
                    MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                    App.RestartApplication();
                }
                catch (TimeoutException)
                {
                    MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                    App.RestartApplication();
                }
            }
        }

        public void EndGameClient(int winnerTeam)
        {
            string myNickname = UserSession.Instance().Nickname;
            EndGame endGame = new EndGame(winnerTeam, _gameManager.UsersTeam[myNickname]);

            _gameManager.EndExitTimer();

            GameWindow gameWindow = new GameWindow();
            endGame.EndGameWindow = gameWindow;

            gameWindow.ChangePage(endGame);

            gameWindow.Visibility = Visibility.Visible;
            
            this.Close();
        }

        public void ShowGamePlayerEndedTurn(string nickname, Dictionary<int, int> boardAfterTurn)
        {
            _gameManager.PlayerEndedTurn(nickname, boardAfterTurn);
        }

        public void TakeCardOutOfHand(string nickname, GraphicCard graphicCardToRemove, Dictionary<string, int> usersTeam)
        {
            if (usersTeam[nickname] == usersTeam[UserSession.Instance().Nickname])
            {
                RemoveCardFromHand(gdAllyHand, graphicCardToRemove);
            }
            else if (nickname == _gameManager.MyEnemy)
            {
                RemoveEnemyCardFromHand(gdEnemyHand);
            }
            else
            {
                RemoveEnemyCardFromHand(gdEnemyAllyHand);
            }
        }

        private void RemoveCardFromHand(Grid gridToModify, GraphicCard graphicCardToRemove)
        {
            //The fact of using a foreach is to find the first graphicCard in the grid and theres no other found to do it succesufully.
            foreach (GraphicCard graphicCard in gridToModify.Children)
            {
                if (graphicCardToRemove.Card != null && graphicCard.Card != null && graphicCard.Card.Equals(graphicCardToRemove.Card))
                {
                        int columnIndex = Grid.GetColumn(graphicCard);

                        gdAllyHand.Children.Remove(graphicCard);
                        RemoveGridColumn(gdAllyHand, columnIndex);
                        break;
                }
            }
        }

        private void RemoveEnemyCardFromHand(Grid gdEnemyHand)
        {
            GraphicCard firstGraphicCard = gdEnemyHand.Children.OfType<GraphicCard>().FirstOrDefault();

            if (firstGraphicCard != null)
            {
                int columnIndex = Grid.GetColumn(firstGraphicCard);

                gdEnemyHand.Children.Remove(firstGraphicCard);
                RemoveGridColumn(gdEnemyHand, columnIndex);
            }
        }

        public void ShowUserConnectedGame(string nickname, int team)
        {
            _gameManager.UsersTeam.Add(nickname, team);

            _ = StartGameAsync();
        }

        public void ShowUsersInGame(Dictionary<string, int> users)
        {
            _gameManager.UsersTeam = users;
            lblUserTeam.Content += _gameManager.UsersTeam[UserSession.Instance().Nickname].ToString();
            _ = StartGameAsync();
        }

        public void StartFirstGamePhaseClient(Tuple<string, string> firstPlayers)
        {
            _gameManager.StartFirstPhaseClient(firstPlayers);
            DrawFourCards();
            _gameManager.StartCountdown();
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
            if(_gameManager.UsersTeam.Count == ALL_USERS_CONNECTED ) 
            {
                await Task.Delay(2000);
                GetUserDeck();
                
                if (IsHost)
                {
                    try
                    {
                        _client.StartFirstGamePhase(UserSession.Instance().Nickname);
                    }
                    catch (CommunicationObjectFaultedException)
                    {
                        MessageBox.Show(Properties.Resources.msgPreviousConnectioLost, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                        App.RestartApplication();
                    }
                    catch (CommunicationException)
                    {
                        MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                        App.RestartApplication();
                    }
                    catch (TimeoutException)
                    {
                        MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                        App.RestartApplication();
                    }
                }
            }
        }

        public void RecieveGameMessage(string message)
        {
            tbChat.Text = tbChat.Text + "\n" + message;
        }

        private void BtnMenuClick(object sender, RoutedEventArgs e)
        {
            lblUserTurn.Content = Properties.Resources.lblUserTurnFalse;

            _gameManager.EndTurn();
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

        private void BtnOpenChatClick(object sender, RoutedEventArgs e)
        {
            if (gdChat.IsVisible)
            {
                gdChat.Visibility = Visibility.Hidden;
            }
            else
            {
                gdChat.Visibility = Visibility.Visible;
            }
        }

        private void BtnSendMessageClick(object sender, RoutedEventArgs e)
        {

            string nickname = UserSession.Instance().Nickname;
            string message = nickname + ": " + txtChat.Text;
            txtChat.Text = Properties.Resources.txtChat;

            try
            {
                _client.SendMessageToGame(message, nickname);
            }
            catch (CommunicationObjectFaultedException)
            {
                MessageBox.Show(Properties.Resources.msgPreviousConnectioLost, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgConnectionError, "AstralisError", MessageBoxButton.OK, MessageBoxImage.Error);
                App.RestartApplication();
            }
        }

        private void TextLimiterForChat(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Length >= MAX_CHAT_LENGHT)
            {
                e.Handled = true;
            }
        }
    }
}
