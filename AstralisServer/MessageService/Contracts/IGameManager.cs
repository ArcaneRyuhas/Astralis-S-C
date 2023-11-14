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
        void DispenseCards(string nickname, int deckId);// Retornar un dicionario con el nickname y una lista de id's

        void DrawCard(string nickname, int cardId); // Este metodo se llama cuando un jugador roba una carta, este se encarga de decirle al aliado que carta agarro.

        void EndGame(int winnerTeam); //Solo el host manda esto

        void EndGameTurn(Dictionary<int, int> boardAfterTurn); //Es un diccionario del board completo

        void StartNewPhase(Dictionary<int, int> boardAfterPhase);//Solo el host manda esto

    }

    public interface IGameManagerCallback
    {
        void RecieveCards(int[] userDeck);

        void DrawCardClient (string nickname, int cardId);

        void PlayerEndedTurn(string player, Dictionary<int, int> boardAfterTurn); //Es un diccionario del board completo

        void EndPhase();

        void StartNewPhaseClient(Dictionary<int, int> boardAfterPhase);

        void EndGame(int winnerTeam);


        
    }
}
