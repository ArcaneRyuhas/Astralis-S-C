using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;


namespace DataAccessProject.Contracts
{
    [ServiceContract]
    public interface ILeaderboardManager
    {
        [OperationContract]
        List<GamesWonInfo> GetLeaderboardInfo();

    }

    [DataContract]
    public class GamesWonInfo
    {
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public int GamesWonCount { get; set; }
    }
}
