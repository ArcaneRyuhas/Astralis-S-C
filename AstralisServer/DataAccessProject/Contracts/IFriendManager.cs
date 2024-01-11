using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessProject.Contracts
{
    [ServiceContract(CallbackContract = typeof(IOnlineUserManagerCallback))]
    public interface IFriendManager
    {
        [OperationContract(IsOneWay = true)]
        void SubscribeToFriendManager(string nickname);

        [OperationContract(IsOneWay = true)]
        void UnsubscribeToFriendManager(string nickname);

        [OperationContract]
        int SendFriendRequest(string nickname, string nicknameFriend);

        [OperationContract]
        int ReplyFriendRequest(string nickname, string nicknameRequest, bool answer);

        [OperationContract]
        int RemoveFriend(string nickname, string nicknamefriendToRemove);


    }

    [ServiceContract]
    public interface IOnlineUserManagerCallback
    {
        [OperationContract]
        void ShowUserSubscribedToFriendManager(string nickname);

        [OperationContract]
        void ShowUserUnsubscribedToFriendManager(string nickname);

        [OperationContract]
        void ShowFriends(Dictionary<string, Tuple<bool, int>> onlineFriends);

        [OperationContract]
        void ShowFriendRequest(string nickname);

        [OperationContract]
        void ShowFriendAccepted(string nickname);

        [OperationContract]
        void ShowFriendDeleted(string nickname);
    }

}

