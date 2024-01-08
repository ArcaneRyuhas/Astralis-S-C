using DataAccessProject.DataAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Entity.Core;
using DataAccessProject;
using User = DataAccessProject.Contracts.User;
using System.Collections.Generic;
using System;
using System.Configuration;
using System.ServiceModel;

namespace DataAccessTestProject
{

    [TestClass]
    public class UserAccessTest
    {
        private const int INT_VALIDATION_SUCCESS = 1;
        private const int INT_VALIDATION_FAILURE = 0;
        private const int ERROR = -1;
        private const string USER_NOT_FOUND = "UserNotFound";
        private static UserAccess userAccess = new UserAccess();

        [TestMethod]
        public void SuccesfullyCreateUser()
        {
            User userToAdd = new User()
            {
                Nickname = "AddUserTest",
                ImageId = 1,
                Mail = "mariom.portilla@hotmail.com",
                Password = "password"
            };

            Assert.IsTrue(userAccess.CreateUser(userToAdd) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnSuccesfullyCreateUser()
        {
            User userToAdd = new User()
            {
                Nickname = "UserRepeated",
                ImageId = 1,
                Mail = "userRepeated@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(userToAdd);

            Assert.IsTrue(userAccess.CreateUser(userToAdd) == INT_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccesfullyCreateGuest()
        {
            User guestToAdd = new User()
            {
                Nickname = "CreateGuestTest",
                ImageId = 1,
                Mail = "GuestTest@hotmail.com",
                Password = "password"
            };

            Assert.IsTrue(userAccess.CreateGuest(guestToAdd) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnSuccesfullyCreateGuest()
        {
            User guestToAdd = new User()
            {
                Nickname = "CreateGuestReapetedTest",
                ImageId = 1,
                Mail = "GuestTest@hotmail.com",
                Password = "password"
            };

            userAccess.CreateGuest(guestToAdd);

            Assert.IsTrue(userAccess.CreateGuest(guestToAdd) == INT_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccesfullyGetHigherGuest()
        {
            Assert.IsTrue(userAccess.GetHigherGuests() > INT_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccesfullyUpdateUser()
        {
            User userToUpdate = new User()
            {
                Nickname = "UpdateUserTest",
                ImageId = 1,
                Mail = "UpdateUserTest@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(userToUpdate);

            userToUpdate.Mail = "EmailUpdated@hotmail.com";
            userToUpdate.Password = "passwordUpdated";

            Assert.IsTrue(userAccess.UpdateUser(userToUpdate) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnSuccesfullyUpdateUser()
        {
            User userToUpdate = new User()
            {
                Nickname = "UpdateUserUnsuccessTest",
                ImageId = 1,
                Mail = "UpdateUserTest@hotmail.com",
                Password = "password"
            };

            Assert.IsTrue(userAccess.UpdateUser(userToUpdate) == INT_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccesfullyGetUserByNickname()
        {
            User userToGet = new User()
            {
                Nickname = "GetUserTest",
                ImageId = 1,
                Mail = "GetUserTest@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(userToGet);

            Assert.IsTrue(userAccess.GetUserByNickname(userToGet.Nickname).Nickname == userToGet.Nickname);
        }

        [TestMethod]
        public void UnSuccesfullyGetUserByNickname()
        {
            User userToGet = new User()
            {
                Nickname = "GetUserUnsuccessTest",
                ImageId = 1,
                Mail = "GetUserTest@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(userToGet);

            userToGet.Nickname = "NewNickname";

            Assert.IsTrue(userAccess.GetUserByNickname(userToGet.Nickname).Nickname == USER_NOT_FOUND);
        }

        [TestMethod]
        public void SuccesfullyFindUserByNickname()
        {
            User userToFind = new User()
            {
                Nickname = "FindUserTest",
                ImageId = 1,
                Mail = "FindUserTest@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(userToFind);

            Assert.IsTrue(userAccess.FindUserByNickname(userToFind.Nickname) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnSuccesfullyFindUserByNickname()
        {
            User userToFind = new User()
            {
                Nickname = "FindUserUnsuccessTest",
                ImageId = 1,
                Mail = "FindUserTest@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(userToFind);

            userToFind.Nickname = "NewNickname";

            Assert.IsTrue(userAccess.FindUserByNickname(userToFind.Nickname) == INT_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccesfullyConfirmUser()
        {
            User userToConfirm = new User()
            {
                Nickname = "ConfirmUserTest",
                ImageId = 1,
                Mail = "ConfirmUserTest@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(userToConfirm);

            Assert.IsTrue(userAccess.ConfirmUser(userToConfirm.Nickname, userToConfirm.Password) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnSuccesfullyConfirmUser()
        {
            User userToConfirm = new User()
            {
                Nickname = "ConfirmUserUnsuccessTest",
                ImageId = 1,
                Mail = "ConfirmUserTest@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(userToConfirm);

            userToConfirm.Password = "newPassword";

            Assert.IsTrue(userAccess.ConfirmUser(userToConfirm.Nickname, userToConfirm.Password) == INT_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccesfullyDeleteUser()
        {
            User userToDelete = new User()
            {
                Nickname = "DeleteUserTest",
                ImageId = 1,
                Mail = "DeleteUserTest@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(userToDelete);

            Assert.IsTrue(userAccess.DeleteUser(userToDelete.Nickname) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnSuccesfullyDeleteUser()
        {
            User userToDelete = new User()
            {
                Nickname = "DeleteUserTest",
                ImageId = 1,
                Mail = "DeleteUserTest@hotmail.com",
                Password = "password"
            };

            Assert.IsTrue(userAccess.DeleteUser(userToDelete.Nickname) == INT_VALIDATION_FAILURE);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            userAccess.DeleteUser("AddUserTest");
            userAccess.DeleteUser("UserRepeated");
            userAccess.DeleteUser("CreateGuestTest");
            userAccess.DeleteUser("CreateGuestReapetedTest");
            userAccess.DeleteUser("UpdateUserTest");
            userAccess.DeleteUser("GetUserTest");
            userAccess.DeleteUser("FindUserTest");
            userAccess.DeleteUser("ConfirmUserTest");
            userAccess.DeleteUser("ConfirmUserUnsuccessTest");
            userAccess.DeleteUser("FindUserUnsuccessTest");
            userAccess.DeleteUser("GetUserUnsuccessTest");
        }
    }

    [TestClass]
    public class UserAccessTestExceptions
    {
        private static UserAccess userAccess = new UserAccess();


        [TestMethod]
        public void CreatUserEntityException()
        {
            User userToAdd = new User()
            {
                Nickname = "AddUserTest",
                ImageId = 1,
                Mail = "mariom.portilla@hotmail.com",
                Password = "password"
            };


            Assert.ThrowsException<EntityException>(() =>
            {
                userAccess.CreateUser(userToAdd);
            });
        }

        [TestMethod]
        public void CreatGuestEntityException()
        {
            User guestToAdd = new User()
            {
                Nickname = "CreateGuestTest",
                ImageId = 1,
                Mail = "GuestTest@hotmail.com",
                Password = "password"
            };


            Assert.ThrowsException<EntityException>(() =>
            {
                userAccess.CreateGuest(guestToAdd);
            });
        }

        [TestMethod]
        public void GetHigherGuestEntityException()
        {
            Assert.ThrowsException<EntityException>(() =>
            {
                userAccess.GetHigherGuests();
            });
        }

        [TestMethod]
        public void UpdateUserEntityException()
        {

            User userToUpdate = new User()
            {
                Nickname = "UpdateUserTest",
                ImageId = 1,
                Mail = "UpdateUserTest@hotmail.com",
                Password = "password"
            };

            Assert.ThrowsException<EntityException>(() =>
            {
                userAccess.UpdateUser(userToUpdate);
            });
        }

        [TestMethod]
        public void GetUserByNicknameEntityException()
        {
            User userToFind = new User()
            {
                Nickname = "FindUserTest",
                ImageId = 1,
                Mail = "GetUserTest@hotmail.com",
                Password = "password"
            };

            Assert.ThrowsException<EntityException>(() =>
            {
                userAccess.FindUserByNickname(userToFind.Nickname);
            });
        }

        [TestMethod]
        public void FindUserByNicknameEntityException()
        {
            User userToGet = new User()
            {
                Nickname = "GetUserTest",
                ImageId = 1,
                Mail = "FindUserTest@hotmail.com",
                Password = "password"
            };

            Assert.ThrowsException<EntityException>(() =>
            {
                userAccess.GetUserByNickname(userToGet.Nickname);
            });
        }

        [TestMethod]
        public void ConfirmUserEntityException()
        {
            User userToConfirm = new User()
            {
                Nickname = "ConfirmUserTest",
                ImageId = 1,
                Mail = "ConfirmUserTest@hotmail.com",
                Password = "password"
            };

            Assert.ThrowsException<EntityException>(() =>
            {
                userAccess.ConfirmUser(userToConfirm.Nickname, userToConfirm.Password);
            });
        }

        [TestMethod]
        public void DeleteUserEntityException()
        {
            User userToDelete = new User()
            {
                Nickname = "DeleteUserTest",
                ImageId = 1,
                Mail = "DeleteUserTest@hotmail.com",
                Password = "password"
            };

            Assert.ThrowsException<EntityException>(() =>
            {
                userAccess.DeleteUser(userToDelete.Nickname);
            });
        }
    }

    [TestClass]
    public class FriendAccessTests
    {
        private const int INT_VALIDATION_SUCCESS = 1;
        private const int INT_VALIDATION_FAILURE = 0;
        private const bool BOOL_VALIDATION_SUCCESS = true;
        private const bool BOOL_VALIDATION_FAILURE = false;
        private static UserAccess userAccess = new UserAccess();
        private static FriendAccess friendAccess = new FriendAccess();

        [TestMethod]
        public void SuccesfullySendFriendRequest()
        {
            User user1 = new User()
            {
                Nickname = "User1SuccessSendTest",
                ImageId = 1,
                Mail = "FrienAccesTestusr1@hotmail.com",
                Password = "password"
            };

            User user2 = new User()
            {
                Nickname = "User2SuccessSendTest",
                ImageId = 1,
                Mail = "FrienAccesTestusr2@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(user1);
            userAccess.CreateUser(user2);

            friendAccess.ReplyFriendRequest(user1.Nickname, user2.Nickname, false);

            Assert.IsTrue(friendAccess.SendFriendRequest(user1.Nickname, user2.Nickname) == BOOL_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnSuccesfullySendFriendRequest()
        {
            User user1 = new User()
            {
                Nickname = "User1UnsuccessSendTest",
                ImageId = 1,
                Mail = "FrienAccesTestusr1@hotmail.com",
                Password = "password"
            };

            User user2 = new User()
            {
                Nickname = "User2UnsuccessSendTest",
                ImageId = 1,
                Mail = "FrienAccesTestusr2@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(user1);
            userAccess.CreateUser(user2);
            friendAccess.SendFriendRequest(user1.Nickname, user2.Nickname);

            Assert.IsTrue(friendAccess.SendFriendRequest(user1.Nickname, user2.Nickname) == BOOL_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccesfullyRemoveFriend()
        {
            User user1 = new User()
            {
                Nickname = "User1SuccessRemoveTest",
                ImageId = 1,
                Mail = "FrienAccesTestusr1@hotmail.com",
                Password = "password"
            };

            User user2 = new User()
            {
                Nickname = "User2SuccessRemoveTest",
                ImageId = 1,
                Mail = "FrienAccesTestusr2@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(user1);
            userAccess.CreateUser(user2);
            friendAccess.SendFriendRequest(user1.Nickname, user2.Nickname);
            friendAccess.ReplyFriendRequest(user1.Nickname, user2.Nickname, true);

            Assert.IsTrue(friendAccess.RemoveFriend(user1.Nickname, user2.Nickname) == BOOL_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnSuccesfullyRemoveFriend()
        {
            User user1 = new User()
            {
                Nickname = "User1UnsuccessRemoveTest",
                ImageId = 1,
                Mail = "FrienAccesTestusr1@hotmail.com",
                Password = "password"
            };

            User user2 = new User()
            {
                Nickname = "User2UnsuccessRemoveTest",
                ImageId = 1,
                Mail = "FrienAccesTestusr2@hotmail.com",
                Password = "password"
            };

            Assert.IsTrue(friendAccess.RemoveFriend(user1.Nickname, user2.Nickname) == BOOL_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccesfullyReplyFriendRequest()
        {
            User user1 = new User()
            {
                Nickname = "User1SuccessReplyTest",
                ImageId = 1,
                Mail = "FrienAccesTestusr1@hotmail.com",
                Password = "password"
            };

            User user2 = new User()
            {
                Nickname = "User2SuccessReplyTest",
                ImageId = 1,
                Mail = "FrienAccesTestusr2@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(user1);
            userAccess.CreateUser(user2);
            friendAccess.SendFriendRequest(user1.Nickname, user2.Nickname);

            Assert.IsTrue(friendAccess.ReplyFriendRequest(user1.Nickname, user2.Nickname, true) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnSuccesfullyReplyFriendRequest()
        {
            User user1 = new User()
            {
                Nickname = "User1UnSuccessReplyTest",
                ImageId = 1,
                Mail = "FrienAccesTestusr1@hotmail.com",
                Password = "password"
            };

            User user2 = new User()
            {
                Nickname = "User2UnSuccessReplyTest",
                ImageId = 1,
                Mail = "FrienAccesTestusr2@hotmail.com",
                Password = "password"
            };

            Assert.IsTrue(friendAccess.ReplyFriendRequest(user1.Nickname, user2.Nickname, true) == INT_VALIDATION_FAILURE);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            friendAccess.ReplyFriendRequest("User1SuccessSendTest", "User2SuccessSendTest", false);
            friendAccess.ReplyFriendRequest("User1UnsuccessSendTest", "User2UnsuccessSendTest", false);
            friendAccess.RemoveFriend("User1SuccessReplyTest", "User2SuccessReplyTest");

            userAccess.DeleteUser("User1SuccessSendTest");
            userAccess.DeleteUser("User2SuccessSendTest");
            userAccess.DeleteUser("User1UnsuccessSendTest");
            userAccess.DeleteUser("User2UnsuccessSendTest");
            userAccess.DeleteUser("User1SuccessRemoveTest");
            userAccess.DeleteUser("User2SuccessRemoveTest");
            userAccess.DeleteUser("User1SuccessReplyTest");
            userAccess.DeleteUser("User2SuccessReplyTest");
        }
    }

    [TestClass]
    public class FriendAccessTestExceptions
    {

        private static FriendAccess friendAccess = new FriendAccess();
        private static UserAccess userAccess = new UserAccess();


        [TestMethod]
        public void SendFriendRequestEntityException()
        {
            User user1 = new User()
            {
                Nickname = "User1",
                ImageId = 1,
                Mail = "FrienAccesTestusr1@hotmail.com",
                Password = "password"
            };

            User user2 = new User()
            {
                Nickname = "User2",
                ImageId = 1,
                Mail = "FrienAccesTestusr2@hotmail.com",
                Password = "password"
            };


            Assert.ThrowsException<EntityException>(() =>
            {
                friendAccess.SendFriendRequest(user1.Nickname, user2.Nickname);
            });
        }


        [TestMethod]
        public void RemoveFriendEntityException()
        {
            User user1 = new User()
            {
                Nickname = "User1",
                ImageId = 1,
                Mail = "FrienAccesTestusr1@hotmail.com",
                Password = "password"
            };

            User user2 = new User()
            {
                Nickname = "User2",
                ImageId = 1,
                Mail = "FrienAccesTestusr2@hotmail.com",
                Password = "password"
            };


            Assert.ThrowsException<EntityException>(() =>
            {
                friendAccess.RemoveFriend(user1.Nickname, user2.Nickname);
            });
        }

        [TestMethod]
        public void ReplyFriendRequestEntityException()
        {
            User user1 = new User()
            {
                Nickname = "User1",
                ImageId = 1,
                Mail = "FrienAccesTestusr1@hotmail.com",
                Password = "password"
            };

            User user2 = new User()
            {
                Nickname = "User2",
                ImageId = 1,
                Mail = "FrienAccesTestusr2@hotmail.com",
                Password = "password"
            };


            Assert.ThrowsException<EntityException>(() =>
            {
                friendAccess.ReplyFriendRequest(user1.Nickname, user2.Nickname, true);
            });
        }

    }

    [TestClass]
    public class DeckAccessTests
    {
        private const int INT_VALIDATION_SUCCESS = 1;
        private const int STARTING_CARD_COUNT = 30;
        private static UserAccess userAccess = new UserAccess();
        private static DeckAccess deckAccess = new DeckAccess();

        [TestMethod]
        public void SuccessfullyCreateDefaultDeck()
        {
            User userDefaultDeck = new User()
            {
                Nickname = "UserDefaultDeck",
                ImageId = 1,
                Mail = "UserDefaultDeck@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(userDefaultDeck);
            var context = new AstralisDBEntities();

            Assert.IsTrue(deckAccess.CreateDefaultDeck(context, userDefaultDeck.Nickname) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnsuccessfullyCreateDefaultDeck()
        {
            User userDefaultDeck = new User()
            {
                Nickname = "UserDefaultDeck",
                ImageId = 1,
                Mail = "UserDefaultDeck@hotmail.com",
                Password = "password"
            };

            var context = new AstralisDBEntities();

            Assert.IsFalse(deckAccess.CreateDefaultDeck(context, userDefaultDeck.Nickname) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void SuccessfullyGetDeckByNickname()
        {
            User user = new User()
            {
                Nickname = "TestUser",
                ImageId = 1,
                Mail = "testuser@example.com",
                Password = "password"
            };

            userAccess.CreateUser(user);

            var context = new AstralisDBEntities();
            deckAccess.CreateDefaultDeck(context, user.Nickname);

            List<int> cardList = deckAccess.GetDeckByNickname(user.Nickname);

            Assert.IsNotNull(cardList);
            Assert.AreEqual(STARTING_CARD_COUNT, cardList.Count);
        }

        [TestMethod]
        public void UnsuccessfullyGetDeckByNickname()
        {
            User user = new User()
            {
                Nickname = "TestUser",
                ImageId = 1,
                Mail = "testuser@example.com",
                Password = "password"
            };

            userAccess.CreateUser(user);

            var context = new AstralisDBEntities();

            List<int> cardList = deckAccess.GetDeckByNickname(user.Nickname);

            Assert.IsNull(cardList);
        }


        [ClassCleanup]
        public static void ClassCleanup()
        {
            userAccess.DeleteUser("UserDefaultDeck");
            userAccess.DeleteUser("UserRelationDeck");
            userAccess.DeleteUser("TestUser");
        }
    }

    [TestClass]
    public class DeckAccessTestExceptions
    {

        private static DeckAccess deckAccess = new DeckAccess();


        [TestMethod]
        public void CreateDefaultDeckEntityException()
        {
            User userDefaultDeck = new User()
            {
                Nickname = "UserDefaultDeck",
                ImageId = 1,
                Mail = "UserDefaultDeck@hotmail.com",
                Password = "password"
            };

            var context = new AstralisDBEntities();

            Assert.ThrowsException<EntityException>(() =>
            {
                deckAccess.CreateDefaultDeck(context, userDefaultDeck.Nickname);
            });
        }

        [TestMethod]
        public void GetDeckByNicknameEntityException()
        {
            User userDefaultDeck = new User()
            {
                Nickname = "UserDefaultDeck",
                ImageId = 1,
                Mail = "UserDefaultDeck@hotmail.com",
                Password = "password"
            };

            Assert.ThrowsException<EntityException>(() =>
            {
                deckAccess.GetDeckByNickname(userDefaultDeck.Nickname);
            });
        }


    }

    [TestClass]
    public class GameAccessTest
    {
        private const int INT_VALIDATION_SUCCESS = 1;
        private const int INT_VALIDATION_FAILURE = 0;
        private const bool BOOL_VALIDATION_FAILURE = false;
        private const string GAME_ID = "TestGameId"; 
        private const int WINNER_TEAM_TEST = 1;
        private const string USER_NICKNAME = "TestUser";
        private static GameAccess gameAccess = new GameAccess();
        private static UserAccess userAccess = new UserAccess();
        


        [TestMethod]
        public void SuccessfullyCreateGame()
        {
            gameAccess.CleanupGame(GAME_ID);
            Assert.IsTrue(gameAccess.CreateGame(GAME_ID));
        }

        [TestMethod]
        public void UnSuccessfullyCreateGame()
        {
            gameAccess.CleanupGame(GAME_ID);
            gameAccess.CreateGame(GAME_ID);

            Assert.IsTrue(gameAccess.CreateGame(GAME_ID) == BOOL_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccessfullyCreatePlayRelation()
        {
            gameAccess.CleanupGame(GAME_ID);
            gameAccess.CreateGame(GAME_ID);
            Assert.IsTrue(gameAccess.CreatePlaysRelation(USER_NICKNAME, GAME_ID, 1) == INT_VALIDATION_SUCCESS);

        }

        [TestMethod]
        public void SuccessfullyGameIdIsReapeted()
        {
            gameAccess.CleanupGame(GAME_ID);
            gameAccess.CreateGame(GAME_ID);
            Assert.IsTrue(gameAccess.GameIdIsRepeated(GAME_ID));
        }

        [TestMethod]
        public void UnSuccessfullyGameIdIsReapeted()
        {
            gameAccess.CleanupGame(GAME_ID);
            Assert.IsTrue(gameAccess.GameIdIsRepeated(GAME_ID) == BOOL_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccessfullyEndGame()
        {
            gameAccess.CleanupGame(GAME_ID);
            gameAccess.CreateGame(GAME_ID);
            Assert.IsTrue(gameAccess.EndGame(WINNER_TEAM_TEST, GAME_ID) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnSuccessfullyEndGame()
        {
            gameAccess.CleanupGame(GAME_ID);
            Assert.IsTrue(gameAccess.EndGame(WINNER_TEAM_TEST, "NotGameIdValid") == INT_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccessfullyBanUser()
        {
            User userToBan = new User()
            {
                Nickname = "UserToBan",
                ImageId = 1,
                Mail = "UserToBan@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(userToBan);

            Assert.IsTrue(gameAccess.BanUser(userToBan.Nickname) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnSuccessfullyBanUser()
        {
            Assert.IsTrue(gameAccess.BanUser("NotValidNickname") == INT_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccessfullyCanPlay()
        {
            User userCanPlay = new User()
            {
                Nickname = "UserCanPlay",
                ImageId = 1,
                Mail = "UserCanPlay@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(userCanPlay);

            Assert.IsTrue(gameAccess.CanPlay(userCanPlay.Nickname) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnSuccessfullyCanPlay()
        {
            User userCanPlay = new User()
            {
                Nickname = "UserCanPlayUnSuccess",
                ImageId = 1,
                Mail = "UserCanPlay@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(userCanPlay);
            gameAccess.BanUser(userCanPlay.Nickname);

            Assert.IsTrue(gameAccess.CanPlay(userCanPlay.Nickname) == INT_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccessfullyCleanUpGame()
        {
            gameAccess.CreateGame(GAME_ID);

            Assert.IsTrue(gameAccess.CleanupGame(GAME_ID) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnSuccessfullyCleanUpGame()
        {
            gameAccess.CleanupGame(GAME_ID);
            Assert.IsTrue(gameAccess.CleanupGame(GAME_ID) == INT_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccessfullyRemoveBan()
        {

            User userBanToRemove = new User()
            {
                Nickname = "UserBanToRemove",
                ImageId = 1,
                Mail = "UserBanToRemove@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(userBanToRemove);
            gameAccess.BanUser(userBanToRemove.Nickname);

            Assert.IsTrue(gameAccess.RemoveBan(userBanToRemove.Nickname) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnSuccessfullyRemoveBan()
        {

            User userBanToRemove = new User()
            {
                Nickname = "UserBanToRemove",
                ImageId = 1,
                Mail = "UserBanToRemove@hotmail.com",
                Password = "password"
            };

            Assert.IsTrue(gameAccess.RemoveBan(userBanToRemove.Nickname) == INT_VALIDATION_FAILURE);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            gameAccess.RemoveBan("UserToBan");
            gameAccess.RemoveBan("UserCanPlayUnSuccess");

            userAccess.DeleteUser("UserToBan");
            userAccess.DeleteUser("UserToBanUn");
            userAccess.DeleteUser("UserCanPlay");
            userAccess.DeleteUser("UserCanPlayUnSuccess");
            userAccess.DeleteUser("UserBanToRemove");
        }


    }

    [TestClass]
    public class GamekAccessTestExceptions
    {
        private const string GAME_ID = "TestGameId";
        private const string USER_NICKNAME = "TestUser";
        private const int WINNER_TEAM_TEST = 1;
        private static GameAccess gameAccess = new GameAccess();

        [TestMethod]
        public void CreateGameEntityException()
        {
            Assert.ThrowsException<EntityException>(() =>
            {
                gameAccess.CreateGame(GAME_ID);
            });
        }

        [TestMethod]
        public void CreatePLayRelationEntityException()
        {
            Assert.ThrowsException<EntityException>(() =>
            {
                gameAccess.CreatePlaysRelation(USER_NICKNAME, GAME_ID, 1);
            });
        }

        [TestMethod]
        public void GameIdIsReaetedEntityException()
        {
            Assert.ThrowsException<EntityException>(() =>
            {
                gameAccess.GameIdIsRepeated(GAME_ID);
            });
        }

        [TestMethod]
        public void EndGameEntityException()
        {
            Assert.ThrowsException<EntityException>(() =>
            {
                gameAccess.EndGame(WINNER_TEAM_TEST, GAME_ID);
            });
        }

        [TestMethod]
        public void BanUserEntityException()
        {
            Assert.ThrowsException<EntityException>(() =>
            {
                gameAccess.BanUser(USER_NICKNAME);
            });
        }

        [TestMethod]
        public void CanPlayEntityException()
        {
            Assert.ThrowsException<EntityException>(() =>
            {
                gameAccess.CanPlay(USER_NICKNAME);
            });
        }

        [TestMethod]
        public void CleanUpGameEntityException()
        {
            Assert.ThrowsException<EntityException>(() =>
            {
                gameAccess.CleanupGame(GAME_ID);
            });
        }

        [TestMethod]
        public void RemoveBanEntityException()
        {
            Assert.ThrowsException<EntityException>(() =>
            {
                gameAccess.RemoveBan(USER_NICKNAME);
            });
        }
    }


}



