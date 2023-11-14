using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MessageService.Contracts
{
    [ServiceContract]
    public interface IUserManager
    {
        [OperationContract]
        int AddUser(User user);

        [OperationContract]
        int ConfirmUser(string nickname, string password);

        [OperationContract]
        bool FindUserByNickname(string nickname);

        [OperationContract]
        User GetUserByNickname(string nickname);

        [OperationContract]
        int UpdateUser(User user);

    }

    [ServiceContract(CallbackContract = typeof(IOnlineUserManagerCallback))]
    public interface IOnlineUserManager
    {
        [OperationContract(IsOneWay = true)]
        void ConectUser(string nickname);

        [OperationContract(IsOneWay = true)]
        void DisconectUser(string nickname);

        [OperationContract]
        bool SendFriendRequest(string nickname, string nicknameFriend);

        [OperationContract]
        bool ReplyFriendRequest(string nickname, string nicknameRequest, bool answer);

        [OperationContract]
        bool RemoveFriend(string nickname, string nicknamefriendToRemove);

    }

    [ServiceContract]
    public interface IOnlineUserManagerCallback 
    {
        [OperationContract]
        void ShowUserConected(string nickname);
        
        [OperationContract]
        void ShowUserDisconected(string nickname);

        [OperationContract]
        void ShowOnlineFriends(Dictionary<string, Tuple<bool, int>> onlineFriends);

        [OperationContract]
        void ShowFriendRequest(string nickname);

        [OperationContract]
        void ShowFriendAccepted(string nickname);
    }


    [DataContract]
    public class User
    {
        private string nickname;
        private int imageId;
        private string mail;
        private string password;

        [DataMember]
        public string Nickname { get { return nickname; } set { nickname = value; } }

        [DataMember]
        public int ImageId { get { return imageId; } set { imageId = value; } }

        [DataMember]
        public string Mail { get { return mail; } set { mail = value; } }

        [DataMember]
        public string Password { get { return password; } set { password = value; } }

    }

}