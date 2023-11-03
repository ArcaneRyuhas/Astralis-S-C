using System;
using System.Collections.Generic;
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
        Dictionary<string, bool> GetFriendList(string nickname);
    }
}

    [ServiceContract]
    public interface IOnlineUserManagerCallback
    {
        [OperationContract]
        void ShowUserConected(string nickname);
        [OperationContract]
        void ShowUserDisconected(string nickname);
        [OperationContract]
        void ShowUsersOnline(List<String> nicknames);
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
        public int ImageId { get {  return imageId; } set {  imageId = value; } }

        [DataMember]
        public string Mail { get { return mail; } set { mail = value; } }

        [DataMember]
        public string Password { get { return password; } set { password = value; } }
    
    }
    
}
