using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessProject.Contracts
{
    [ServiceContract(CallbackContract = typeof(IGameManagerCallback))]
    public interface IGameManager
    {
        [OperationContract(IsOneWay = true)]
        void ConnectGame(string nickname);

        [OperationContract]
        List<int> DispenseGameCards(string nickname);

        [OperationContract(IsOneWay = true)]
        void DrawGameCard(string nickname, int [] cardId); 

        [OperationContract(IsOneWay = true)]
        void EndGame(int winnerTeam, string nickname);

        [OperationContract(IsOneWay = true)]
        void EndGameTurn(string nickname, Dictionary<int, int> boardAfterTurn);

        [OperationContract(IsOneWay = true)]
        void StartFirstGamePhase(string hostNickname);

        [OperationContract(IsOneWay = true)]
        void SendMessageToGame(string message, string nickname);
    }

    [ServiceContract]
    public interface IGameManagerCallback: IMessageManagerCallback
    {

        [OperationContract]
        void ShowUserConnectedGame(string nickname, int team);

        [OperationContract]
        void ShowUsersInGame(Dictionary<string, int> users);

        [OperationContract]
        void ShowCardDrawedInGame (string nickname, int [] cardId);

        [OperationContract]
        void ShowGamePlayerEndedTurn(string nickname, Dictionary<int, int> boardAfterTurn);

        [OperationContract]
        void StartFirstGamePhaseClient(Tuple<string, string> firstPlayers);

        [OperationContract]
        void EndGameClient(int winnerTeam);

        
    }
}
