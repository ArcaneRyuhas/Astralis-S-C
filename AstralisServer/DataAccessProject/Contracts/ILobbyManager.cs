using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace DataAccessProject.Contracts
{
    [ServiceContract(CallbackContract =typeof(ILobbyManagerCallback))]
    public interface ILobbyManager: IMessageManager
    {
        [OperationContract()]
        string CreateLobby(User user);

        [OperationContract()]
        int CanAccessToLobby(string nickname);

        [OperationContract()]
        bool LobbyExist(string gameId);

        [OperationContract()]
        bool LobbyIsNotFull(string gameId);

        [OperationContract()]
        string SendInvitationToLobby(string gameId, string userToSend);

        [OperationContract(IsOneWay = true)]
        void ConnectToLobby(User user, string gameId);

        [OperationContract(IsOneWay = true)]
        void DisconnectFromLobby(User user);

        [OperationContract(IsOneWay = true)]
        void ChangeLobbyUserTeam(string userNickname, int team);

        [OperationContract(IsOneWay = true)]
        void SendUsersFromLobbyToGame(string gameId);

        [OperationContract(IsOneWay = true)]
        void KickUserFromLobby(string userNickname);

    }

    [ServiceContract]
    public interface ILobbyManagerCallback: IMessageManagerCallback
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
        void SendUserFromLobbyToGame();

        [OperationContract]
        void GetKickedFromLobby();
    }
}
