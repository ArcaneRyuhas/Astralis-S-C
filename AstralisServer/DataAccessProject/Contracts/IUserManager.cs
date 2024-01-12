using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;


namespace DataAccessProject.Contracts
{
    [ServiceContract]
    public interface IUserManager
    {
        [OperationContract]
        int AddUser(User user);

        [OperationContract]
        User AddGuestUser();

        [OperationContract]
        int ConfirmUserCredentials(string nickname, string password);

        [OperationContract]
        int FindUserByNickname(string nickname);

        [OperationContract]
        User GetUserByNickname(string nickname);

        [OperationContract]
        int UpdateUser(User user);

        [OperationContract]
        bool IsUserOnline(string nickname);

    }


    [DataContract]
    public partial class User
    {

        [DataMember]
        public string Nickname { get; set; }

        [DataMember]
        public int ImageId { get; set; }

        [DataMember]
        public string Mail { get; set; }

        [DataMember]
        public string Password { get; set; }

    }

}