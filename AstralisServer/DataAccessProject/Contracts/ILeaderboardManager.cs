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
        public string Username { get; set; }
        public int GamesWonCount { get; set; }
    }
}
