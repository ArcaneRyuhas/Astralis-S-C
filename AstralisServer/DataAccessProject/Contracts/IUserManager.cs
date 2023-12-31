﻿using System;
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
        User AddGuest();

        [OperationContract]
        int ConfirmUser(string nickname, string password);

        [OperationContract]
        int FindUserByNickname(string nickname);

        [OperationContract]
        User GetUserByNickname(string nickname);

        [OperationContract]
        int UpdateUser(User user);

        [OperationContract]
        bool UserOnline(string nickname);

    }

    [ServiceContract(CallbackContract = typeof(IOnlineUserManagerCallback))]
    public interface IOnlineUserManager
    {
        [OperationContract(IsOneWay = true)]
        void ConectUser(string nickname);

        [OperationContract(IsOneWay = true)]
        void DisconectUser(string nickname);

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
        void ShowUserConected(string nickname);
        
        [OperationContract]
        void ShowUserDisconected(string nickname);

        [OperationContract]
        void ShowOnlineFriends(Dictionary<string, Tuple<bool, int>> onlineFriends);

        [OperationContract]
        void ShowFriendRequest(string nickname);

        [OperationContract]
        void ShowFriendAccepted(string nickname);

        [OperationContract]
        void FriendDeleted(string nickname);
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