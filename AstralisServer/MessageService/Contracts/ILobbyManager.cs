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
        [OperationContract]
        int CreateLobby(User user);

        [OperationContract(IsOneWay = true)]
        void ConnectLobby(User user, string gameId);

        [OperationContract(IsOneWay = true)]
        void DisconnectLobby(User user);

        [OperationContract(IsOneWay = true)]
        void ChangeLobbyUserTeam(User user, int team);
    }

    [ServiceContract]
    public interface ILobbyManagerCallback 
    {
        [OperationContract]
        void ShowConnectionInLobby(User user);

        [OperationContract]
        void ShowUsersInLobby(List<User> userList);

        [OperationContract]
        void ShowDisconnectionInLobby(string nickname);

        [OperationContract]
        void UpdateLobbyUserTeam(User user, int team);

        [OperationContract]
        void GiveLobbyId(string gameId);
    }
}
