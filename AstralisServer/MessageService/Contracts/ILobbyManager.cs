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
        [OperationContract(IsOneWay =true)]
        void CreateLobby(User user);

        [OperationContract(IsOneWay = true)]
        void JoinLobby(User user);

        [OperationContract(IsOneWay = true)]
        void ChangeLobbyUserTeam(User user, int team);
    }

    [ServiceContract]
    public interface ILobbyManagerCallback 
    {
        [OperationContract]
        void UpdateLobby(User user);

        [OperationContract]
        void UpdateLobbyUserTeam(User user, int team);
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
