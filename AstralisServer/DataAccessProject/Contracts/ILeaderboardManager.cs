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
        private string username;
        private int gamesWonCount;

        [DataMember]
        public string Username { get { return username; } set { username = value; } }

        [DataMember]
        public int GamesWonCount { get { return gamesWonCount; } set { gamesWonCount = value; } }
    }
}
