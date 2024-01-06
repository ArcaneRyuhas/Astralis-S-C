using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace DataAccessProject.Contracts
{
    [ServiceContract(CallbackContract =typeof(ILobbyManagerCallback))]
    public interface ILobbyManager
    {
        [OperationContract()]
        string CreateLobby(User user);

        [OperationContract()]
        int CanPlay(string nickname);

        [OperationContract()]
        bool GameExist(string gameId);

        [OperationContract()]
        bool GameIsNotFull(string gameId);

        [OperationContract()]
        bool IsBanned(string nickname);

        [OperationContract()]
        string SendFriendInvitation(string gameId, string userToSend);

        [OperationContract(IsOneWay = true)]
        void ConnectLobby(User user, string gameId);

        [OperationContract(IsOneWay = true)]
        void DisconnectLobby(User user);

        [OperationContract(IsOneWay = true)]
        void ChangeLobbyUserTeam(string userNickname, int team);

        [OperationContract(IsOneWay = true)]
        void SendMessage(string message, string gameId);

        [OperationContract(IsOneWay = true)]
        void StartGame(string gameId);

        [OperationContract(IsOneWay = true)]
        void KickUser(string userNickname);

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
        void ReceiveMessage(string message);

        [OperationContract]
        void StartClientGame();

        [OperationContract]
        void GetKicked();
    }
}
