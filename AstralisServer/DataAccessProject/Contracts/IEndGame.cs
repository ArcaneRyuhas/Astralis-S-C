using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessProject.Contracts
{
    [ServiceContract(CallbackContract = typeof(IEndGameCallback))]
    public interface IEndGame
    {
        [OperationContract(IsOneWay = true)]
        void GetUsersWithTeam(string nickname);
    }

    public interface IEndGameCallback
    {
        [OperationContract()]
        void SetUsers(List<UserWithTeam> usersWithTeams);
    }

    [DataContract]
    public class UserWithTeam
    {
        private string nickname;
        private int imageId;
        private string mail;
        private int team;

        [DataMember]
        public string Nickname { get { return nickname; } set { nickname = value; } }

        [DataMember]
        public int ImageId { get { return imageId; } set { imageId = value; } }

        [DataMember]
        public string Mail { get { return mail; } set { mail = value; } }

        [DataMember]
        public int Team { get { return team; } set { team = value; } }

    }
}
