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
        void DrawCard(string nickname, int cardId); // Este metodo se llama cuando un jugador roba una carta, este se encarga de decirle al aliado que carta agarro.

        [OperationContract(IsOneWay = true)]
        void EndGame(int winnerTeam); //Solo el host manda esto

        [OperationContract(IsOneWay = true)]
        void EndGameTurn(string nickname, Dictionary<int, int> boardAfterTurn);//Es un diccionario del board completo

        [OperationContract(IsOneWay = true)]
        void StartNewPhase(string hostNickname); //Solo el host manda esto

        [OperationContract(IsOneWay = true)]
        void StartFirstPhase(string hostNickname);

    }

    public interface IGameManagerCallback
    {

        [OperationContract]
        void ShowUserConnectedGame(string nickname, int team);

        [OperationContract]
        void ShowUsersInGame(Dictionary<string, int> users);

        [OperationContract]
        void DrawCardClient (string nickname, int cardId);

        [OperationContract]
        void PlayerEndedTurn(string nickname, Dictionary<int, int> boardAfterTurn); //Es un diccionario del board completo

        [OperationContract]
        void EndPhase();

        [OperationContract]
        void StartNewPhaseClient();

        [OperationContract]
        void StartFirstPhaseClien(Tuple<string, string> firstPlayers);

        [OperationContract]
        void EndGameClient(int winnerTeam);
        
    }
}
