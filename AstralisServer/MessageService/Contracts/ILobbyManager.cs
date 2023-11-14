using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MessageService.Contracts
{
    [ServiceContract(CallbackContract =typeof(ILobbyManagerCallback))]
    public interface ILobbyManager
    {
        [OperationContract()]
        string CreateLobby(User user);

        [OperationContract()]
        bool GameExist(string gameId);

        [OperationContract()]
        bool GameIsNotFull(string gameId);

        [OperationContract(IsOneWay = true)]
        void ConnectLobby(User user, string gameId);

        [OperationContract(IsOneWay = true)]
        void DisconnectLobby(User user);

        [OperationContract(IsOneWay = true)]
        void ChangeLobbyUserTeam(string userNickname, int team);

        [OperationContract(IsOneWay = true)]
        void SendMessage(string message, string gameId);

        [OperationContract(IsOneWay = true)]
        void StartGame(string gameId); //Se guardan los usuarios en la tabla de Plays

    }

    [ServiceContract]
    public interface ILobbyManagerCallback 
    {
        [OperationContract]
        void ShowConnectionInLobby(User user);

        [OperationContract]
        void ShowUsersInLobby(List <Tuple<User, int>> users);

        [OperationContract]
        void ShowDisconnectionInLobby(User user);

        [OperationContract]
        void UpdateLobbyUserTeam(string userNickname, int team);

        [OperationContract]
        void GiveLobbyId(string gameId);

        [OperationContract]
        void ReceiveMessage(string message);

        [OperationContract]
        void StartClientGame(); //Se inicia su partida para cada cliente
    }
}
