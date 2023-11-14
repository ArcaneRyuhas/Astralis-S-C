using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MessageService.Contracts
{
    [ServiceContract(CallbackContract = typeof(IGameManagerCallback))]
    public interface IGameManager
    {
        [OperationContract(IsOneWay = true)]
        void DispenseCards(string nickname, int deckId);// Retornar un dicionario con el nickname y una lista de id's

        [OperationContract(IsOneWay = true)]
        void DrawCard(string nickname, int cardId); // Este metodo se llama cuando un jugador roba una carta, este se encarga de decirle al aliado que carta agarro.

        [OperationContract(IsOneWay = true)]
        void EndGame(int winnerTeam); //Solo el host manda esto

        [OperationContract(IsOneWay = true)]
        void EndGameTurn(Dictionary<int, int> boardAfterTurn); //Es un diccionario del board completo

        [OperationContract(IsOneWay = true)]
        void StartNewPhase(Dictionary<int, int> boardAfterPhase);//Solo el host manda esto

    }

    public interface IGameManagerCallback
    {
        [OperationContract]
        void RecieveCards(int[] userDeck);

        [OperationContract]
        void DrawCardClient (string nickname, int cardId);

        [OperationContract]
        void PlayerEndedTurn(string player, Dictionary<int, int> boardAfterTurn); //Es un diccionario del board completo

        [OperationContract]
        void EndPhase();

        [OperationContract]
        void StartNewPhaseClient(Dictionary<int, int> boardAfterPhase);

        [OperationContract]
        void EndGameClient(int winnerTeam);
        
    }
}
