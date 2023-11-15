using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessProject.DataAccess
{
    public class FriendAccess
    {
        private const int IS_FRIEND = 1;
        private const int IS_PENDING_FRIEND = 2;
        private const bool ONLINE = true;
        private const bool OFFLINE = false;
        private const bool ACCEPTED_FRIEND = true;

        public FriendAccess() { }  

        public bool RemoveFriend(string nickname, string nicknamefriendToRemove)
        {
            bool IsSucceded = false;

            using (var context = new AstralisDBEntities())
            {
                context.Database.Log = Console.WriteLine;

                var friendRelationship = context.UserFriend
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

        public bool SendFriendRequest(string nicknameSender, string nicknameReciever)
        {
            bool isSucceded = false;

            using (var context = new AstralisDBEntities())
            {
                context.Database.Log = Console.WriteLine;

                var existingRequest = context.UserFriend
                    .FirstOrDefault(f =>
                        (f.Nickname1 == nicknameSender && f.Nickname2 == nicknameReciever) ||
                        (f.Nickname1 == nicknameReciever && f.Nickname2 == nicknameSender) &&
                        f.FriendStatusId == IS_PENDING_FRIEND);

                if (existingRequest == null)
                {
                    var newFriendRequest = new UserFriend
                    {
                        Nickname1 = nicknameSender,
                        Nickname2 = nicknameReciever,
                        FriendStatusId = IS_PENDING_FRIEND
                    };

                    context.UserFriend.Add(newFriendRequest);
                    context.SaveChanges();

                    isSucceded = true;
                }
            }

            return isSucceded;
        }

        public int ReplyFriendRequest(string nicknameReciever, string nicknameSender, bool answer)
        {
            int result = 0;

            using (var context = new AstralisDBEntities())
            {
                context.Database.Log = Console.WriteLine;

                var existingRequest = context.UserFriend
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
                }
            }
            return result;
        }

        public Dictionary<string, Tuple<bool, int>> GetFriendList(string nickname, List<string> onlineUsers)
        {
            Dictionary<string, Tuple<bool, int>> friendList = new Dictionary<string, Tuple<bool, int>>();
            Tuple<bool, int> friendTuple;

            using (var context = new AstralisDBEntities())
            {
                context.Database.Log = Console.WriteLine;
                var databaseFriends = context.UserFriend.Where(databaseFriend => (databaseFriend.Nickname1 == nickname || databaseFriend.Nickname2 == nickname) && databaseFriend.FriendStatusId == IS_FRIEND).ToList();

                foreach (var friend in databaseFriends)
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

                    var pendingRequests = context.UserFriend.Where(f => (f.Nickname2 == nickname) && f.FriendStatusId == IS_PENDING_FRIEND).ToList();

                    foreach (var request in pendingRequests)
                    {
                        if (!friendList.ContainsKey(request.Nickname1))
                        {
                            friendTuple = new Tuple<bool, int>(OFFLINE, IS_PENDING_FRIEND);
                            friendList.Add(request.Nickname1, friendTuple);
                        }
                    }
                }
            }
            return friendList;
        }

    }
}
