using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using Astralis.Logic;
using System.Windows.Threading;
using System.Windows.Media.Animation;

namespace Astralis.Views.Game.GameLogic
{
    internal class GameManager
    {
        private const int ERROR_CARD_ID = 0;
        private const int COUNTDOWN_STARTING_VALUE = 20;
        private const int NO_MAGES = 0;
        private const int DRAW = 3;

        private GameBoard _gameBoard;
        private Dictionary<string, int> _usersTeam;
        private int _endTurnCounter = 0;
        private bool _isMyTurn = false;
        private Queue<int> _userDeckQueue = new Queue<int>();
        private Tuple<string, string> _firstPlayers = Tuple.Create<string, string>("", "");
        private string _myEnemy;
        private int _countdownValue = COUNTDOWN_STARTING_VALUE;
        private DispatcherTimer _timer;
        private ProgressBar _progressBarCounter;
        private bool _roundEnded = false;
        private List<Card> _userHand = new List<Card>();
        private Team _userTeam;
        private Team _enemyTeam;

        
        public Queue<int> UserDeckQueue { get { return _userDeckQueue; } set { _userDeckQueue = value; } }
        public bool IsMyTurn { get { return _isMyTurn; } }
        public Team UserTeam { get { return _userTeam; } set { _userTeam = value; } }
        public Dictionary<string, int> UsersTeam { get { return _usersTeam; } set { _usersTeam = value; } }
        public string MyEnemy { get { return _myEnemy; } }
        public Team EnemyTeam { get { return _enemyTeam; } set { _enemyTeam = value; } }
        public GameManager() { }

        public void SetGameBoard(GameBoard gameBoard)
        {
            this._gameBoard = gameBoard;
        }

        public void SetCounter(ProgressBar progressBarCounter)
        {
            this._progressBarCounter = progressBarCounter;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += TimerTick;

            progressBarCounter.Maximum = _countdownValue;
            progressBarCounter.Value = _countdownValue;
        }

        public int DrawCard()
        {
            int cardToDraw = ERROR_CARD_ID;

            if (_userHand.Count < 7)
            {
                cardToDraw = UserDeckQueue.Dequeue();
                Card card = CardManager.Instance().GetCard(cardToDraw);

                _userHand.Add(card);
                int indexOfDrawnCard = _userHand.IndexOf(card);

                _gameBoard.AddCardToHand(_userHand[indexOfDrawnCard]);
            }

            _gameBoard.EnemyDrawCard();

            return cardToDraw;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            _countdownValue--;
            _progressBarCounter.Value = _countdownValue;

            if (_countdownValue == 0)
            {
                _timer.Stop();

                EndTurn();
            }

            DoubleAnimation animation = new DoubleAnimation(_countdownValue, TimeSpan.FromSeconds(1));
            _progressBarCounter.BeginAnimation(ProgressBar.ValueProperty, animation);
        }

        public void EndTurn()
        {
            if (!_roundEnded && _isMyTurn)
            {
                _gameBoard.lblUserTurn.Content = "You have ended your turn";// MODIFICAR DESPUES
                _roundEnded = true;
                _endTurnCounter++;

                List<Card> removedCards = new List<Card>();

                foreach (Card playedCard in _gameBoard.PlayedCards)
                {
                    _userHand.Remove(playedCard);
                    removedCards.Add(playedCard);
                }

                foreach (Card cardToRemove in removedCards)
                {
                    _gameBoard.PlayedCards.Remove(cardToRemove);
                }

                _gameBoard.EndGameTurn();
                TurnCounter();
            }
        }

        public void PlayerEndedTurn(string player, Dictionary<int, int> boardAfterTurn, Grid gdEnemySlots, Grid gdPlayerSlots)
        {
            string myNickname = UserSession.Instance().Nickname;

            if(player != myNickname)
            {
                _endTurnCounter++;

                if (_usersTeam[player] != _usersTeam[myNickname])
                {
                    AddCardsToBoard(player, gdEnemySlots, boardAfterTurn);
                }
                else
                {
                    AddCardsToBoard(player, gdPlayerSlots, boardAfterTurn);
                }

                TurnCounter();
            }
        }

        public void AddCardsToBoard(string player, Grid gdSlots, Dictionary<int,int> boardAfterTurn)
        {
            int counter = 0;

            foreach (UIElement child in gdSlots.Children)
            {
                
                if (child is Grid grid)
                {
                    Grid childGrid = child as Grid;
                    counter++;

                    if (boardAfterTurn[counter] != ERROR_CARD_ID && childGrid.Children.Count < 1)
                    {
                        Card card = CardManager.Instance().GetCard(boardAfterTurn[counter]);
                        GraphicCard graphicCard = new GraphicCard();
                        Grid innerGrid = grid;

                        graphicCard.SetGraphicCard(card);
                        innerGrid.Children.Add(graphicCard);
                        _gameBoard.TakeCardOutOfHand(player, graphicCard, _usersTeam);
                    }
                }
            }
        }

        private void TurnCounter()
        {
            string myNickname = UserSession.Instance().Nickname;

            switch (_endTurnCounter)
            {
                case 2:
                    if (myNickname != _firstPlayers.Item1 && myNickname != _firstPlayers.Item2)
                    {
                        _isMyTurn = true;
                        _gameBoard.lblUserTurn.Content = "It's your turn";
                    }
                    else
                    {
                        _isMyTurn = false;
                        _gameBoard.lblUserTurn.Content = " Your turn has ended";
                    }
                    StartCountdown();
                    break;

                case 4:
                    string[] secondPlayers = _usersTeam.Keys.Where(name => name != _firstPlayers.Item2 && name != _firstPlayers.Item1).ToArray();
                    _firstPlayers = Tuple.Create(secondPlayers[0], secondPlayers[1]);

                    if (myNickname == _firstPlayers.Item1 || myNickname == _firstPlayers.Item2)
                    {
                        _isMyTurn = true;
                        _gameBoard.lblUserTurn.Content = "It's your turn";
                    }

                    ReviewEndGame();
                    break;
            }
        }

        private void ReviewEndGame()
        {
            StartCountdown();
            _endTurnCounter = 0;

            _userTeam.RoundMana++;
            _userTeam.Mana = _userTeam.RoundMana;
            
            AttackPhase();

            if (!HasGameEnded())
            {
                _gameBoard.DrawCard();
                _roundEnded = false;
            }
        }

        private bool HasGameEnded()
        {
            string myNickname = UserSession.Instance().Nickname;
            int winnerTeam = 0;
            bool gameEnded = false;

            if(_enemyTeam.Health < 1 || _userTeam.Health < 1)
            {
                gameEnded = true;

                if (_enemyTeam.Health < 1 && _userTeam.Health < 1)
                {
                    winnerTeam = DRAW;
                }
                else if (_enemyTeam.Health < 1)
                {
                    winnerTeam = _usersTeam[myNickname];

                }
                else if (_userTeam.Health < 1)
                {
                    winnerTeam = _usersTeam[_myEnemy];
                }
                _gameBoard.GameHasEnded(winnerTeam);
            }
            
            return gameEnded;
        }


        private void AttackPhase()
        {
            GraphicCard[] teamBoard = _gameBoard.GetAttackBoard(_gameBoard.gdPlayerSlots);
            GraphicCard[] enemyTeamBoard = _gameBoard.GetAttackBoard(_gameBoard.gdEnemySlots);

            int allyMagesCount = GetMagesCount(teamBoard);
            int enemyMagesCount = GetMagesCount(enemyTeamBoard);

            for(int cardsPosition = 0; cardsPosition < 5; cardsPosition++)
            {
                WarriorAttack(teamBoard[cardsPosition], enemyTeamBoard[cardsPosition]);
                OtherTypeAttack(teamBoard[cardsPosition], enemyTeamBoard[cardsPosition], allyMagesCount, enemyMagesCount);
            }
        }

        private int GetMagesCount(GraphicCard[] board)
        {
            int magesCount = 0;

            foreach(GraphicCard graphicCard in board)
            {
                if(graphicCard.Card.Type == Constants.MAGE)
                {
                    magesCount++;
                }
            }

            return magesCount;
        }

        private void WarriorAttack(GraphicCard allyGraphicCard, GraphicCard enemyGraphicCard) 
        {
            Card allyCard = allyGraphicCard.Card;
            Card enemyCard = enemyGraphicCard.Card;

            if (allyCard.Type == Constants.WARRIOR)
            {
                if(enemyCard.Type != Constants.NO_CLASS) 
                {
                    enemyCard.TakeDamage(allyCard.DealDamage(NO_MAGES));
                }
                else
                {
                    _enemyTeam.ReceiveDamage(allyCard.DealDamage(NO_MAGES));
                }
            }

            if(enemyCard.Type == Constants.WARRIOR)
            {
                if(allyCard.Type != Constants.NO_CLASS)
                {
                    allyCard.TakeDamage(enemyCard.DealDamage(NO_MAGES));
                }
                else
                {
                    _userTeam.ReceiveDamage(enemyCard.DealDamage(NO_MAGES));
                }
            }

            KillCards(allyGraphicCard, enemyGraphicCard);
        }

        private void OtherTypeAttack(GraphicCard allyGraphicCard, GraphicCard enemyGraphicCard, int allyMagesCount, int enemyMagesCount)
        {
            Card allyCard = allyGraphicCard.Card;
            Card enemyCard = enemyGraphicCard.Card;

            if (allyGraphicCard.Card.Type != Constants.WARRIOR && allyGraphicCard.Card.Type != Constants.NO_CLASS)
            {
                if(enemyCard.Type != Constants.NO_CLASS)
                {
                    enemyGraphicCard.Card.TakeDamage(allyCard.DealDamage(allyMagesCount));
                }
                else
                {
                    _enemyTeam.ReceiveDamage(allyCard.DealDamage(allyMagesCount));
                }
            }
            if (enemyGraphicCard.Card.Type != Constants.WARRIOR && enemyGraphicCard.Card.Type != Constants.NO_CLASS)
            {
                if (allyGraphicCard.Card.Type != Constants.NO_CLASS)
                {
                    allyGraphicCard.Card.TakeDamage(enemyCard.DealDamage(enemyMagesCount));
                }
                else
                {
                    _userTeam.ReceiveDamage(enemyCard.DealDamage(enemyMagesCount));
                }
            }

            KillCards(allyGraphicCard, enemyGraphicCard);
        }

        private void KillCards(GraphicCard allyGraphicCard, GraphicCard enemyGraphicCard)
        {
            if (allyGraphicCard.Card.Health < 1)
            {
                _gameBoard.DeleteGraphicCard(allyGraphicCard);
                allyGraphicCard.Card.Attack = Constants.DEAD_DAMAGE;
            }

            if(enemyGraphicCard.Card.Health < 1) 
            {
                _gameBoard.DeleteGraphicCard(enemyGraphicCard);
                enemyGraphicCard.Card.Attack = Constants.DEAD_DAMAGE;
            }
        }

        public void StartCountdown()
        {
            _countdownValue = COUNTDOWN_STARTING_VALUE;

            _timer.Start();
        }

        public void StartFirstPhaseClient(Tuple<string, string> firstPlayers)
        {
            this._firstPlayers = firstPlayers;
            string myNickname = UserSession.Instance().Nickname;

            if (firstPlayers.Item1 == myNickname)
            {
                _isMyTurn = true;
                _gameBoard.lblUserTurn.Content = "It's your turn";
                _myEnemy = firstPlayers.Item2;
            }
            else if (firstPlayers.Item2 == myNickname)
            {
                _isMyTurn = true;
                _gameBoard.lblUserTurn.Content = "It's your turn";
                _myEnemy = firstPlayers.Item1;
            }
            else
            {
                foreach (string nickname in _usersTeam.Keys)
                {
                    if (firstPlayers.Item1 != nickname && firstPlayers.Item2 != nickname && nickname != myNickname)
                    {
                        _isMyTurn = false;
                        _myEnemy = nickname;
                    }
                }
            }

            _roundEnded = false;
        }
    }
}
