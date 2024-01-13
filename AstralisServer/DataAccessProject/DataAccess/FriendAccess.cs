using System;
using System.Collections.Generic;
using System.Linq;

namespace DataAccessProject.DataAccess
{
    public class FriendAccess
    {
        private const int IS_FRIEND = 1;
        private const int IS_PENDING_FRIEND = 2;
        private const bool ONLINE = true;
        private const bool OFFLINE = false;
        private const bool ACCEPTED_FRIEND = true;
        private const int INT_VALIDATION_SUCCESS = 1;
        private const int INT_VALIDATION_FAILURE = 0;

        public FriendAccess() { }  

        public bool RemoveFriend(string nickname, string nicknamefriendToRemove)
        {
            bool IsSucceded = false;

            using (AstralisDBEntities context = new AstralisDBEntities())
            {
                context.Database.Log = Console.WriteLine;
                UserFriend friendRelationship = context.UserFriend
                    .FirstOrDefault(f =>
                        (f.Nickname1 == nickname && f.Nickname2 == nicknamefriendToRemove) ||
                        (f.Nickname1 == nicknamefriendToRemove && f.Nickname2 == nickname) &&
                        f.FriendStatusId == IS_FRIEND);

                if (friendRelationship != null)
                {
                    context.UserFriend.Remove(friendRelationship);
                    context.SaveChanges();

                    IsSucceded = true;
                }
            }

            return IsSucceded;
        }

        public bool FriendRequestExists(string nicknameSender, string nicknameReceiver)
        {
            using (AstralisDBEntities context = new AstralisDBEntities())
            {
                context.Database.Log = Console.WriteLine;
                UserFriend existingRequest = context.UserFriend
                    .FirstOrDefault(f =>
                        (f.Nickname1 == nicknameSender && f.Nickname2 == nicknameReceiver) ||
                        (f.Nickname1 == nicknameReceiver && f.Nickname2 == nicknameSender) &&
                        f.FriendStatusId == IS_PENDING_FRIEND);

                return existingRequest != null;
            }
        }

        public bool SendFriendRequest(string nicknameSender, string nicknameReceiver)
        {
            bool isSucceeded = false;

            if (!FriendRequestExists(nicknameSender, nicknameReceiver))
            {
                using (AstralisDBEntities context = new AstralisDBEntities())
                {
                    context.Database.Log = Console.WriteLine;
                    UserFriend newFriendRequest = new UserFriend
                    {
                        Nickname1 = nicknameSender,
                        Nickname2 = nicknameReceiver,
                        FriendStatusId = IS_PENDING_FRIEND
                    };

                    context.UserFriend.Add(newFriendRequest);
                    context.SaveChanges();

                    isSucceeded = true;
                }
            }

            return isSucceeded;
        }

        public int ReplyFriendRequest(string nicknameReciever, string nicknameSender, bool answer)
        {
            int result = INT_VALIDATION_FAILURE;

            using (AstralisDBEntities context = new AstralisDBEntities())
            {
                context.Database.Log = Console.WriteLine;
                UserFriend existingRequest = context.UserFriend
                    .FirstOrDefault(f =>
                        (f.Nickname1 == nicknameReciever && f.Nickname2 == nicknameSender) ||
                        (f.Nickname1 == nicknameSender && f.Nickname2 == nicknameReciever) &&
                        f.FriendStatusId == IS_PENDING_FRIEND);

                if (existingRequest != null)
                {
                    if (answer == ACCEPTED_FRIEND)
                    {
                        existingRequest.FriendStatusId = IS_FRIEND;
                    }
                    else
                    {
                        context.UserFriend.Remove(existingRequest);
                    }

                    result = context.SaveChanges();

                    if (result != INT_VALIDATION_FAILURE)
                    {
                        result = INT_VALIDATION_SUCCESS;
                    }
                }
            }

            return result;
        }

        public Dictionary<string, Tuple<bool, int>> GetFriendList(string nickname, List<string> onlineUsers)
        {
            Dictionary<string, Tuple<bool, int>> friendList = new Dictionary<string, Tuple<bool, int>>();
            Tuple<bool, int> friendTuple;

            AddFriends(nickname, onlineUsers, friendList);

            using (AstralisDBEntities context = new AstralisDBEntities())
            {
                List<UserFriend> pendingRequests = context.UserFriend.Where(f => (f.Nickname2 == nickname) && f.FriendStatusId == IS_PENDING_FRIEND).ToList();

                foreach (UserFriend request in pendingRequests)
                {
                    if (onlineUsers.Contains(request.Nickname1))
                    {
                        friendTuple = new Tuple<bool, int>(ONLINE, IS_PENDING_FRIEND);

                        friendList.Add(request.Nickname1, friendTuple);
                    }
                    else
                    {
                        friendTuple = new Tuple<bool, int>(OFFLINE, IS_PENDING_FRIEND);

                        friendList.Add(request.Nickname1, friendTuple);
                    }
                }
            }

            return friendList;
        }


        private void AddFriends(string nickname, List<string> onlineUsers, Dictionary<string, Tuple<bool, int>> friendList)
        {
            Tuple<bool, int> friendTuple;

            using (AstralisDBEntities context = new AstralisDBEntities())
            {
                context.Database.Log = Console.WriteLine;
                List<UserFriend> databaseFriends = context.UserFriend.Where(databaseFriend => (databaseFriend.Nickname1 == nickname || databaseFriend.Nickname2 == nickname) && databaseFriend.FriendStatusId == IS_FRIEND).ToList();

                foreach (UserFriend friend in databaseFriends)
                {
                    if (friend.Nickname1 != nickname)
                    {
                        if (onlineUsers.Contains(friend.Nickname1))
                        {
                            friendTuple = new Tuple<bool, int>(ONLINE, IS_FRIEND);

                            friendList.Add(friend.Nickname1, friendTuple);
                        }
                        else
                        {
                            friendTuple = new Tuple<bool, int>(OFFLINE, IS_FRIEND);

                            friendList.Add(friend.Nickname1, friendTuple);
                        }

                    }
                    else
                    {
                        if (onlineUsers.Contains(friend.Nickname2))
                        {
                            friendTuple = new Tuple<bool, int>(ONLINE, IS_FRIEND);

                            friendList.Add(friend.Nickname2, friendTuple);
                        }
                        else
                        {
                            friendTuple = new Tuple<bool, int>(OFFLINE, IS_FRIEND);

                            friendList.Add(friend.Nickname2, friendTuple);
                        }
                    }
                }
            }
        }
    }
}
