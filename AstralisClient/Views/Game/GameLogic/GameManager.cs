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

        public Queue<int> UserDeckQueue { get { return userDeckQueue; } set { userDeckQueue = value; } }
        public bool IsMyTurn { get { return isMyTurn; } }
        public bool RoundEnded { get { return roundEnded; } }
        public Team UserTeam { get { return userTeam; } set { userTeam = value; } }
        public Dictionary<string, int> UsersTeam { get { return usersTeam; } set { usersTeam = value; } }
        public string MyEnemy { get { return myEnemy; } }


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
                gameBoard.LblTurnMana.Foreground = Brushes.Green;// MODIFICAR DESPUES
                roundEnded = true;

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
            }
        }

        public void PlayerEndedTurn(string player, Dictionary<int, int> boardAfterTurn, Grid gdEnemySlots, Grid gdPlayerSlots)
        {
            endTurnCounter++;
            string myNickname = UserSession.Instance().Nickname;

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

        public void AddCardsToBoard(string player, Grid gdSlots, Dictionary<int,int> boardAfterTurn)
        {
            int counter = 0;

            foreach (UIElement child in gdSlots.Children)
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
                    }
                    StartCountdown();
                    break;

                case 4:
                    string[] secondPlayers = usersTeam.Keys.Where(name => name != firstPlayers.Item2 && name != firstPlayers.Item1).ToArray();

                    firstPlayers = Tuple.Create(secondPlayers[0], secondPlayers[1]);
                    StartCountdown();
                    endTurnCounter = 0;
                    userTeam.RoundMana++;
                    gameBoard.DrawCard();

                    break;
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

            if (firstPlayers.Item1 == UserSession.Instance().Nickname)
            {
                isMyTurn = true;
                gameBoard.LblTurnMana.Foreground = Brushes.Yellow;
                myEnemy = firstPlayers.Item2;
            }
            else if (firstPlayers.Item2 == UserSession.Instance().Nickname)
            {
                isMyTurn = true;
                gameBoard.LblTurnMana.Foreground = Brushes.Yellow;
                myEnemy = firstPlayers.Item1;
            }
            else
            {
                foreach (string nickname in usersTeam.Keys)
                {
                    if (firstPlayers.Item1 != nickname && firstPlayers.Item2 != nickname && nickname != UserSession.Instance().Nickname)
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
