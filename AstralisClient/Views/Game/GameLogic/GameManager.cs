using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using Astralis.Logic;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Windows.Media;
using Astralis.UserManager;
using System.Reflection;

namespace Astralis.Views.Game.GameLogic
{
    internal class GameManager
    {
        private const int ERROR_CARD_ID = 0;
        private const int COUNTDOWN_STARTING_VALUE = 240; //RECORDAR REGRESARLO A 20
        private const int NO_MAGES = 0;
        private const int DRAW = 3;

        private GameBoard gameBoard;
        private Dictionary<string, int> usersTeam;
        private int endTurnCounter = 0;
        private bool isMyTurn = false; //CAMBIAR A FALSO
        private Queue<int> userDeckQueue = new Queue<int>();
        private Tuple<string, string> firstPlayers = Tuple.Create<string, string>("", "");
        private string myEnemy;
        private int countdownValue = COUNTDOWN_STARTING_VALUE;
        private DispatcherTimer timer;
        private ProgressBar progressBarCounter;
        private bool roundEnded = false;
        private List<Card> userHand = new List<Card>();
        private Team userTeam;
        private Team enemyTeam;

        
        public Queue<int> UserDeckQueue { get { return userDeckQueue; } set { userDeckQueue = value; } }
        public bool IsMyTurn { get { return isMyTurn; } }
        public Team UserTeam { get { return userTeam; } set { userTeam = value; } }
        public Dictionary<string, int> UsersTeam { get { return usersTeam; } set { usersTeam = value; } }
        public string MyEnemy { get { return myEnemy; } }
        public Team EnemyTeam { get { return enemyTeam; } set { enemyTeam = value; } }

        public GameManager() { }

        public void SetGameBoard(GameBoard gameBoard)
        {
            this.gameBoard = gameBoard;
        }

        public void SetCounter(ProgressBar progressBarCounter)
        {
            this.progressBarCounter = progressBarCounter;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += TimerTick;

            progressBarCounter.Maximum = countdownValue;
            progressBarCounter.Value = countdownValue;
        }

        public int DrawCard()
        {
            int cardToDraw = ERROR_CARD_ID;

            if (userHand.Count < 7)
            {
                cardToDraw = UserDeckQueue.Dequeue();
                Card card = CardManager.Instance().GetCard(cardToDraw);

                userHand.Add(card);
                int indexOfDrawnCard = userHand.IndexOf(card);

                gameBoard.AddCardToHand(userHand[indexOfDrawnCard]);
            }

            gameBoard.EnemyDrawCard();

            return cardToDraw;
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

        public void EndTurn()
        {
            if (!roundEnded && isMyTurn)
            {
                gameBoard.lblUserTurn.Content = "You have ended your turn";// MODIFICAR DESPUES
                roundEnded = true;
                endTurnCounter++;

                List<Card> removedCards = new List<Card>();

                foreach (Card playedCard in gameBoard.PlayedCards)
                {
                    userHand.Remove(playedCard);
                    removedCards.Add(playedCard);
                }

                foreach (Card cardToRemove in removedCards)
                {
                    gameBoard.PlayedCards.Remove(cardToRemove);
                }

                gameBoard.EndGameTurn();
                TurnCounter();
            }
        }

        public void PlayerEndedTurn(string player, Dictionary<int, int> boardAfterTurn, Grid gdEnemySlots, Grid gdPlayerSlots)
        {
            string myNickname = UserSession.Instance().Nickname;

            if(player != myNickname)
            {
                endTurnCounter++;

                if (usersTeam[player] != usersTeam[myNickname])
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
                
                if (child is Grid)
                {
                    Grid childGrid = child as Grid;
                    counter++;

                    if (boardAfterTurn[counter] != ERROR_CARD_ID && childGrid.Children.Count < 1)
                    {
                        Card card = CardManager.Instance().GetCard(boardAfterTurn[counter]);
                        GraphicCard graphicCard = new GraphicCard();
                        Grid innerGrid = (Grid)child;

                        graphicCard.SetGraphicCard(card);
                        innerGrid.Children.Add(graphicCard);
                        gameBoard.TakeCardOutOfHand(player, graphicCard, usersTeam);
                    }
                }
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
                        gameBoard.lblUserTurn.Content = "It's your turn";
                    }
                    else
                    {
                        isMyTurn = false;
                        gameBoard.lblUserTurn.Content = " Your turn has ended";
                    }
                    StartCountdown();
                    break;

                case 4:
                    string[] secondPlayers = usersTeam.Keys.Where(name => name != firstPlayers.Item2 && name != firstPlayers.Item1).ToArray();
                    firstPlayers = Tuple.Create(secondPlayers[0], secondPlayers[1]);

                    if (myNickname == firstPlayers.Item1 || myNickname == firstPlayers.Item2)
                    {
                        isMyTurn = true;
                        gameBoard.lblUserTurn.Content = "It's your turn";
                    }

                    
                    StartCountdown();
                    endTurnCounter = 0;

                    userTeam.RoundMana++;
                    userTeam.Mana = userTeam.RoundMana;

                    gameBoard.DrawCard();
                    AttackPhase();
                    HasGameEnded();
                    roundEnded = false;

                    break;
            }
        }

        private void HasGameEnded()
        {
            string myNickname = UserSession.Instance().Nickname;
            int winnerTeam;

            if(enemyTeam.Health < 1 && userTeam.Health < 1) 
            {
                winnerTeam = DRAW;
                gameBoard.GameHasEnded(winnerTeam);
            }
            else if(enemyTeam.Health < 1)
            {
                winnerTeam = usersTeam[myEnemy];
                gameBoard.GameHasEnded(winnerTeam);

            }
            else if(userTeam.Health < 1)
            {
                winnerTeam = usersTeam[myNickname];
                gameBoard.GameHasEnded(winnerTeam);
            }
        }


        private void AttackPhase()
        {
            GraphicCard[] teamBoard = gameBoard.GetAttackBoard(gameBoard.gdPlayerSlots);
            GraphicCard[] enemyTeamBoard = gameBoard.GetAttackBoard(gameBoard.gdEnemySlots);

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
                    enemyTeam.ReceiveDamage(allyCard.DealDamage(NO_MAGES));
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
                    userTeam.ReceiveDamage(enemyCard.DealDamage(NO_MAGES));
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
                    enemyTeam.ReceiveDamage(allyCard.DealDamage(allyMagesCount));
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
                    userTeam.ReceiveDamage(enemyCard.DealDamage(enemyMagesCount));
                }
            }

            KillCards(allyGraphicCard, enemyGraphicCard);
        }

        private void KillCards(GraphicCard allyGraphicCard, GraphicCard enemyGraphicCard)
        {
            if (allyGraphicCard.Card.Health < 1)
            {
                gameBoard.DeleteGraphicCard(allyGraphicCard);
                allyGraphicCard.Card.Attack = Constants.DEAD_DAMAGE;
            }

            if(enemyGraphicCard.Card.Health < 1) 
            {
                gameBoard.DeleteGraphicCard(enemyGraphicCard);
                enemyGraphicCard.Card.Attack = Constants.DEAD_DAMAGE;
            }
        }

        public void StartCountdown()
        {
            countdownValue = COUNTDOWN_STARTING_VALUE;

            timer.Start();
        }

        public void StartFirstPhaseClient(Tuple<string, string> firstPlayers)
        {
            this.firstPlayers = firstPlayers;
            string myNickname = UserSession.Instance().Nickname;

            if (firstPlayers.Item1 == myNickname)
            {
                isMyTurn = true;
                gameBoard.lblUserTurn.Content = "It's your turn";
                myEnemy = firstPlayers.Item2;
            }
            else if (firstPlayers.Item2 == myNickname)
            {
                isMyTurn = true;
                gameBoard.lblUserTurn.Content = "It's your turn";
                myEnemy = firstPlayers.Item1;
            }
            else
            {
                foreach (string nickname in usersTeam.Keys)
                {
                    if (firstPlayers.Item1 != nickname && firstPlayers.Item2 != nickname && nickname != myNickname)
                    {
                        isMyTurn = false;
                        myEnemy = nickname;
                    }
                }
            }

            roundEnded = false;
        }
    }
}
