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
        List<int> DispenseCards(string nickname);

        [OperationContract(IsOneWay = true)]
        void DrawCard(string nickname, int [] cardId); 

        [OperationContract(IsOneWay = true)]
        void EndGame(int winnerTeam, string nickname); //Solo el host manda esto

        [OperationContract(IsOneWay = true)]
        void EndGameTurn(string nickname, Dictionary<int, int> boardAfterTurn);

        [OperationContract(IsOneWay = true)]
        void StartFirstPhase(string hostNickname);

    }

    [ServiceContract]
    public interface IGameManagerCallback
    {

        [OperationContract]
        void ShowUserConnectedGame(string nickname, int team);

        [OperationContract]
        void ShowUsersInGame(Dictionary<string, int> users);

        [OperationContract]
        void DrawCardClient (string nickname, int [] cardId);

        [OperationContract]
        void PlayerEndedTurn(string nickname, Dictionary<int, int> boardAfterTurn);

        [OperationContract]
        void StartFirstPhaseClient(Tuple<string, string> firstPlayers);

        [OperationContract]
        void EndGameClient(int winnerTeam);
        
    }
}
