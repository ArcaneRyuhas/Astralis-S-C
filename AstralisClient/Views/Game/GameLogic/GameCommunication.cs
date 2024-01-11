using Astralis.Logic;
using Astralis.UserManager;
using Astralis.Views.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Astralis.Views.Game.GameLogic
{
    public class GameCommunication : IGameManagerCallback
    {
        private GameManagerClient _client;
        private readonly GameBoard _gameBoard;
        private  GameManager _gameManager;
        private GameManager gameManager;

        public GameCommunication(GameBoard gameBoard) 
        {
            Connect();
            _gameBoard = gameBoard;
        }

        public GameCommunication(GameBoard gameBoard, GameManager gameManager) : this(gameBoard)
        {
            this.gameManager = gameManager;
        }

        private void Connect()
        {
            _client = SetGameContext();
            try
            {
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

        private GameManagerClient SetGameContext()
        {
            InstanceContext context = new InstanceContext(this);
            GameManagerClient client = new GameManagerClient(context);

            return client;
        }

        public void DrawGameCard(string myNickname, int[] drawnCard)
        {
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

        public void EndGameTurn()
        {
            try
            {
                _client.EndGameTurn(UserSession.Instance().Nickname, _gameBoard.GetBoardDictionary());
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

        public void ShowGameDrawedCard(string nickname, int[] cardId)
        {
            foreach (int Id in cardId)
            {
                GraphicCard graphicCard = new GraphicCard();
                Card card = CardManager.Instance().GetCard(Id);

                graphicCard.SetGraphicCard(card);
                _gameBoard.AddGraphicCardToGrid(graphicCard, _gameBoard.gdAllyHand);
            }
        }

        public void ShowGamePlayerEndedTurn(string nickname, Dictionary<int, int> boardAfterTurn)
        {
            throw new NotImplementedException();
        }

        public void ShowUserConnectedGame(string nickname, int team)
        {
            throw new NotImplementedException();
        }

        public void ShowUsersInGame(Dictionary<string, int> users)
        {
            throw new NotImplementedException();
        }

        public void StartFirstGamePhaseClient(Tuple<string, string> firstPlayers)
        {
            throw new NotImplementedException();
        }
    }
}
