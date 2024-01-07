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

        [OperationContract(IsOneWay = true)]
        void GameEnded(string nickname);
    }

    [ServiceContract]
    public interface IEndGameCallback
    {
        [OperationContract()]
        void SetUsers(List<UserWithTeam> usersWithTeams);
    }

    [DataContract]
    public class UserWithTeam
    {
        [DataMember]
        public string Nickname { get; set; }

        [DataMember]
        public int ImageId { get; set; }

        [DataMember]
        public string Mail { get; set; }

        [DataMember]
        public int Team { get; set; }

    }
}
