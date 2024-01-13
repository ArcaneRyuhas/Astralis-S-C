using DataAccessProject.DataAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Entity.Core;
using DataAccessProject;
using User = DataAccessProject.Contracts.User;
using System.Collections.Generic;
using System;
using System.Configuration;
using System.ServiceModel;
using System.Globalization;

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
        private const int GUEST_CREATED_FOR_TEST = 1;


        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            GetConnectionString();

            User userToAdd = new User()
            {
                Nickname = "UserRepeated",
                ImageId = 1,
                Mail = "userRepeated@hotmail.com",
                Password = "password"
            };
            userAccess.CreateUser(userToAdd);

            User guestToAdd = new User()
            {
                Nickname = "CreateGuestReapetedTest",
                ImageId = 1,
                Mail = "GuestTest@hotmail.com",
                Password = "password"
            };
            userAccess.CreateGuest(guestToAdd);

            User userToUpdate = new User()
            {
                Nickname = "UpdateUserTest",
                ImageId = 1,
                Mail = "UpdateUserTest@hotmail.com",
                Password = "password"
            };
            userAccess.CreateUser(userToUpdate);

            User userToGet = new User()
            {
                Nickname = "GetUserTest",
                ImageId = 1,
                Mail = "GetUserTest@hotmail.com",
                Password = "password"
            };
            userAccess.CreateUser(userToGet);

            User userToFind = new User()
            {
                Nickname = "FindUserTest",
                ImageId = 1,
                Mail = "FindUserTest@hotmail.com",
                Password = "password"
            };
            userAccess.CreateUser(userToFind);

            User userToConfirm = new User()
            {
                Nickname = "ConfirmUserTest",
                ImageId = 1,
                Mail = "ConfirmUserTest@hotmail.com",
                Password = "password"
            };
            userAccess.CreateUser(userToConfirm);

            User userToDelete = new User()
            {
                Nickname = "DeleteUserTest",
                ImageId = 1,
                Mail = "DeleteUserTest@hotmail.com",
                Password = "password"
            };
            userAccess.CreateUser(userToDelete);
        }


        public static void GetConnectionString()
        {
            string connectionString = Environment.GetEnvironmentVariable("ASTRALIS");
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var connectionStringSection = config.ConnectionStrings.ConnectionStrings["AstralisDBEntities"];

            if (connectionStringSection != null)
            {
                connectionStringSection.ConnectionString = connectionString;

                config.Save(ConfigurationSaveMode.Modified);

                ConfigurationManager.RefreshSection("connectionStrings");

            }
        }

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

            Assert.IsTrue(userAccess.CreateGuest(guestToAdd) == INT_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccesfullyGetHigherGuest()
        {
            Assert.IsTrue(userAccess.GetHigherGuests() > GUEST_CREATED_FOR_TEST);
        }

        [TestMethod]
        public void SuccesfullyUpdateUser()
        {
            User userToUpdate = new User()
            {
                Nickname = "UpdateUserTest",
                ImageId = 2,
                Mail = "EmailUpdated@hotmail.com",
                Password = "passwordUpdated"
            };

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
            string nickname = "GetUserTest";

            Assert.IsTrue(userAccess.GetUserByNickname(nickname).Nickname == nickname);
        }

        [TestMethod]
        public void UnSuccesfullyGetUserByNickname()
        {
            string nickname = "NewNickname";

            Assert.IsTrue(userAccess.GetUserByNickname(nickname).Nickname == USER_NOT_FOUND);
        }

        [TestMethod]
        public void SuccesfullyFindUserByNickname()
        {
            string nickname = "FindUserTest";

            Assert.IsTrue(userAccess.FindUserByNickname(nickname) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnSuccesfullyFindUserByNickname()
        {
            string nickname = "InvalidNickname";

            Assert.IsTrue(userAccess.FindUserByNickname(nickname) == INT_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccesfullyConfirmUser()
        {
            string nickname = "ConfirmUserTest";
            string password = "password";

            Assert.IsTrue(userAccess.ConfirmUserCredentials(nickname, password) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnSuccesfullyConfirmUser()
        {
            string nickname = "ConfirmUserTest";
            string password = "InvalidPassword";

            Assert.IsTrue(userAccess.ConfirmUserCredentials(nickname, password) == INT_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccesfullyDeleteUser()
        {
            string nickname = "DeleteUserTest";

            Assert.IsTrue(userAccess.DeleteUser(nickname) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnSuccesfullyDeleteUser()
        {
            string nickname = "InvalidNickname";

            Assert.IsTrue(userAccess.DeleteUser(nickname) == INT_VALIDATION_FAILURE);
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

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            GetConnectionString();
        }

        public static void GetConnectionString()
        {
            string connectionString = Environment.GetEnvironmentVariable("ASTRALIS");
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var connectionStringSection = config.ConnectionStrings.ConnectionStrings["AstralisDBEntities"];

            if (connectionStringSection != null)
            {
                connectionStringSection.ConnectionString = connectionString;

                config.Save(ConfigurationSaveMode.Modified);

                ConfigurationManager.RefreshSection("connectionStrings");

            }
        }

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
            string nickname = "ExceptionNickname";

            Assert.ThrowsException<EntityException>(() =>
            {
                userAccess.FindUserByNickname(nickname);
            });
        }

        [TestMethod]
        public void FindUserByNicknameEntityException()
        {
            string nickname = "ExceptionNickname";

            Assert.ThrowsException<EntityException>(() =>
            {
                userAccess.GetUserByNickname(nickname);
            });
        }

        [TestMethod]
        public void ConfirmUserEntityException()
        {
            string nickname = "ExceptionNickname";
            string password = "ExceptionPassword";


            Assert.ThrowsException<EntityException>(() =>
            {
                userAccess.ConfirmUserCredentials(nickname, password);
            });
        }

        [TestMethod]
        public void DeleteUserEntityException()
        {
            string nickname = "ExceptionNickname";

            Assert.ThrowsException<EntityException>(() =>
            {
                userAccess.DeleteUser(nickname);
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
        private const bool ACCEPT_FRIEND_REQUEST = true;
        private static UserAccess userAccess = new UserAccess();
        private static FriendAccess friendAccess = new FriendAccess();
        

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            GetConnectionString();

            User user1SuccessSendTest = new User()
            {
                Nickname = "User1SuccessSendTest",
                ImageId = 1,
                Mail = "FrienAccesTestusr1@hotmail.com",
                Password = "password"
            };
            User user2SuccessSendTest = new User()
            {
                Nickname = "User2SuccessSendTest",
                ImageId = 1,
                Mail = "FrienAccesTestusr2@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(user1SuccessSendTest);
            userAccess.CreateUser(user2SuccessSendTest);


            User user1UnsuccessSendTest = new User()
            {
                Nickname = "User1UnsuccessSendTest",
                ImageId = 1,
                Mail = "FrienAccesTestusr1@hotmail.com",
                Password = "password"
            };
            User user2UnsuccessSendTest = new User()
            {
                Nickname = "User2UnsuccessSendTest",
                ImageId = 1,
                Mail = "FrienAccesTestusr2@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(user1UnsuccessSendTest);
            userAccess.CreateUser(user2UnsuccessSendTest);
            friendAccess.SendFriendRequest(user1UnsuccessSendTest.Nickname, user2UnsuccessSendTest.Nickname);


            User user1SuccessRemoveTest = new User()
            {
                Nickname = "User1SuccessRemoveTest",
                ImageId = 1,
                Mail = "FrienAccesTestusr1@hotmail.com",
                Password = "password"
            };
            User user2SuccessRemoveTest = new User()
            {
                Nickname = "User2SuccessRemoveTest",
                ImageId = 1,
                Mail = "FrienAccesTestusr2@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(user1SuccessRemoveTest);
            userAccess.CreateUser(user2SuccessRemoveTest);
            friendAccess.SendFriendRequest(user1SuccessRemoveTest.Nickname, user2SuccessRemoveTest.Nickname);
            friendAccess.ReplyFriendRequest(user1SuccessRemoveTest.Nickname, user2SuccessRemoveTest.Nickname, true);


            User user1SuccessReplyTest = new User()
            {
                Nickname = "User1SuccessReplyTest",
                ImageId = 1,
                Mail = "FrienAccesTestusr1@hotmail.com",
                Password = "password"
            };
            User user2SuccessReplyTest = new User()
            {
                Nickname = "User2SuccessReplyTest",
                ImageId = 1,
                Mail = "FrienAccesTestusr2@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(user1SuccessReplyTest);
            userAccess.CreateUser(user2SuccessReplyTest);
            friendAccess.SendFriendRequest(user1SuccessReplyTest.Nickname, user2SuccessReplyTest.Nickname);
        }

        public static void GetConnectionString()
        {
            string connectionString = Environment.GetEnvironmentVariable("ASTRALIS");
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var connectionStringSection = config.ConnectionStrings.ConnectionStrings["AstralisDBEntities"];

            if (connectionStringSection != null)
            {
                connectionStringSection.ConnectionString = connectionString;

                config.Save(ConfigurationSaveMode.Modified);

                ConfigurationManager.RefreshSection("connectionStrings");

            }
        }

        [TestMethod]
        public void SuccesfullySendFriendRequest()
        {
            string nickname1 = "User1SuccessSendTest";
            string nickname2 = "User2SuccessSendTest";

            Assert.IsTrue(friendAccess.SendFriendRequest(nickname1, nickname2) == BOOL_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnSuccesfullySendFriendRequest()
        {
            string nickname1 = "User1UnsuccessSendTest";
            string nickname2 = "User2UnsuccessSendTest";

            Assert.IsTrue(friendAccess.SendFriendRequest(nickname1, nickname2) == BOOL_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccesfullyRemoveFriend()
        {
            string nickname1 = "User1SuccessRemoveTest";
            string nickname2 = "User2SuccessRemoveTest";

            Assert.IsTrue(friendAccess.RemoveFriend(nickname1, nickname2) == BOOL_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnSuccesfullyRemoveFriend()
        {
            string nickname1 = "User1UnsuccessRemoveTest";
            string nickname2 = "User2UnsuccessRemoveTest";

            Assert.IsTrue(friendAccess.RemoveFriend(nickname1, nickname2) == BOOL_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccesfullyReplyFriendRequest()
        {
            string nickname1 = "User1SuccessReplyTest";
            string nickname2 = "User2SuccessReplyTest";

            Assert.IsTrue(friendAccess.ReplyFriendRequest(nickname1, nickname2, ACCEPT_FRIEND_REQUEST) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnSuccesfullyReplyFriendRequest()
        {
            string nickname1 = "User1UnSuccessReplyTest";
            string nickname2 = "User2UnSuccessReplyTest";

            Assert.IsTrue(friendAccess.ReplyFriendRequest(nickname1, nickname2, ACCEPT_FRIEND_REQUEST) == INT_VALIDATION_FAILURE);
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
        private const bool ACCEPT_FRIEND_REQUEST = true;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            GetConnectionString();
        }

        public static void GetConnectionString()
        {
            string connectionString = Environment.GetEnvironmentVariable("ASTRALIS");
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var connectionStringSection = config.ConnectionStrings.ConnectionStrings["AstralisDBEntities"];

            if (connectionStringSection != null)
            {
                connectionStringSection.ConnectionString = connectionString;

                config.Save(ConfigurationSaveMode.Modified);

                ConfigurationManager.RefreshSection("connectionStrings");

            }
        }

        [TestMethod]
        public void SendFriendRequestEntityException()
        {
            string nickname1 = "NicknameException1";
            string nickname2 = "NicknameException2";

            Assert.ThrowsException<EntityException>(() =>
            {
                friendAccess.SendFriendRequest(nickname1, nickname2);
            });
        }


        [TestMethod]
        public void RemoveFriendEntityException()
        {
            string nickname1 = "NicknameException1";
            string nickname2 = "NicknameException2";

            Assert.ThrowsException<EntityException>(() =>
            {
                friendAccess.RemoveFriend(nickname1, nickname2);
            });
        }

        [TestMethod]
        public void ReplyFriendRequestEntityException()
        {
            string nickname1 = "NicknameException1";
            string nickname2 = "NicknameException2";

            Assert.ThrowsException<EntityException>(() =>
            {
                friendAccess.ReplyFriendRequest(nickname1, nickname2, ACCEPT_FRIEND_REQUEST);
            });
        }

    }

    [TestClass]
    public class GameAccessTest
    {
        private const int INT_VALIDATION_SUCCESS = 1;
        private const int INT_VALIDATION_FAILURE = 0;
        private const bool BOOL_VALIDATION_FAILURE = false;
        private const int WINNER_TEAM_TEST = 1;
        private static GameAccess gameAccess = new GameAccess();
        private static UserAccess userAccess = new UserAccess();

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            GetConnectionString();
            gameAccess.CreateGame("GameId2");
            gameAccess.CreateGame("GameId3");
            gameAccess.CreateGame("GameId5");

            User userToBan = new User()
            {
                Nickname = "UserToBan",
                ImageId = 1,
                Mail = "UserToBan@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(userToBan);

            User userCanPlay = new User()
            {
                Nickname = "UserCanPlay",
                ImageId = 1,
                Mail = "UserCanPlay@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(userCanPlay);

            User userCanPlayUnsuccessfully = new User()
            {
                Nickname = "UserCanPlayUnSuccess",
                ImageId = 1,
                Mail = "UserCanPlay@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(userCanPlayUnsuccessfully);
            gameAccess.BanUser(userCanPlayUnsuccessfully.Nickname);
            gameAccess.CreateGame("GameId7");

            User userBanToRemove = new User()
            {
                Nickname = "UserBanToRemove",
                ImageId = 1,
                Mail = "UserBanToRemove@hotmail.com",
                Password = "password"
            };

            userAccess.CreateUser(userBanToRemove);
            gameAccess.BanUser(userBanToRemove.Nickname);
        }

        public static void GetConnectionString()
        {
            string connectionString = Environment.GetEnvironmentVariable("ASTRALIS");
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var connectionStringSection = config.ConnectionStrings.ConnectionStrings["AstralisDBEntities"];

            if (connectionStringSection != null)
            {
                connectionStringSection.ConnectionString = connectionString;

                config.Save(ConfigurationSaveMode.Modified);

                ConfigurationManager.RefreshSection("connectionStrings");

            }
        }

        [TestMethod]
        public void SuccessfullyCreateGame()
        {
            string gameId = "GameId1";
            Assert.IsTrue(gameAccess.CreateGame(gameId));
        }

        [TestMethod]
        public void UnSuccessfullyCreateGame()
        {
            string gameId = "GameId2";
            Assert.IsTrue(gameAccess.CreateGame(gameId) == BOOL_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccessfullyGameIdIsReapeted()
        {
            string gameId = "GameId3";

            Assert.IsTrue(gameAccess.GameIdIsRepeated(gameId));
        }

        [TestMethod]
        public void UnSuccessfullyGameIdIsReapeted()
        {
            string gameId = "GameId4";

            Assert.IsTrue(gameAccess.GameIdIsRepeated(gameId) == BOOL_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccessfullyEndGame()
        {
            string gameId = "GameId5";

            Assert.IsTrue(gameAccess.EndGame(WINNER_TEAM_TEST, gameId) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnSuccessfullyEndGame()
        {
            string gameId = "GameId6";
            Assert.IsTrue(gameAccess.EndGame(WINNER_TEAM_TEST, gameId) == INT_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccessfullyBanUser()
        {
            string nickname = "UserToBan";

            Assert.IsTrue(gameAccess.BanUser(nickname) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnSuccessfullyBanUser()
        {
            string nickname = "UserToBanUnsuccessfuly";

            Assert.IsTrue(gameAccess.BanUser(nickname) == INT_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccessfullyCanPlay()
        {
            string nickname = "UserCanPlay";

            Assert.IsTrue(gameAccess.CanPlay(nickname) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnSuccessfullyCanPlay()
        {
            string nickname = "UserCanPlayUnSuccess";

            Assert.IsTrue(gameAccess.CanPlay(nickname) == INT_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccessfullyCleanUpGame()
        {
            string gameId = "GameId7";

            Assert.IsTrue(gameAccess.CleanupGame(gameId) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnSuccessfullyCleanUpGame()
        {
            string gameId = "GameId8";

            Assert.IsTrue(gameAccess.CleanupGame(gameId) == INT_VALIDATION_FAILURE);
        }

        [TestMethod]
        public void SuccessfullyRemoveBan()
        {
            string nickname = "UserBanToRemove";

            Assert.IsTrue(gameAccess.RemoveBan(nickname) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void UnSuccessfullyRemoveBan()
        {

            string nickname = "InvalidNickname";

            Assert.IsTrue(gameAccess.RemoveBan(nickname) == INT_VALIDATION_FAILURE);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            gameAccess.CleanupGame("GameId1");
            gameAccess.CleanupGame("GameId2");
            gameAccess.CleanupGame("GameId3");
            gameAccess.CleanupGame("GameId5");

            gameAccess.RemoveBan("UserToBan");
            userAccess.DeleteUser("UserToBan");

            userAccess.DeleteUser("UserCanPlay");

            gameAccess.RemoveBan("UserCanPlayUnSuccess");
            userAccess.DeleteUser("UserCanPlayUnSuccess");

            userAccess.DeleteUser("UserBanToRemove");
        }
    }

    [TestClass]
    public class GameAccessTestExceptions
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            GetConnectionString();
        }

        public static void GetConnectionString()
        {
            string connectionString = Environment.GetEnvironmentVariable("ASTRALIS");
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var connectionStringSection = config.ConnectionStrings.ConnectionStrings["AstralisDBEntities"];

            if (connectionStringSection != null)
            {
                connectionStringSection.ConnectionString = connectionString;

                config.Save(ConfigurationSaveMode.Modified);

                ConfigurationManager.RefreshSection("connectionStrings");

            }
        }

        private const int WINNER_TEAM_TEST = 1;
        private static GameAccess gameAccess = new GameAccess();

        [TestMethod]
        public void CreateGameEntityException()
        {
            string gameId = "GameidException";

            Assert.ThrowsException<EntityException>(() =>
            {
                gameAccess.CreateGame(gameId);
            });
        }

        [TestMethod]
        public void CreatePLayRelationEntityException()
        {
            string nickname = "NicknameException";
            string gameId = "GameidException";

            Assert.ThrowsException<EntityException>(() =>
            {
                gameAccess.CreatePlaysRelation(nickname, gameId, 1);
            });
        }

        [TestMethod]
        public void GameIdIsReaetedEntityException()
        {
            string gameId = "GameIdException";

            Assert.ThrowsException<EntityException>(() =>
            {
                gameAccess.GameIdIsRepeated(gameId);
            });
        }

        [TestMethod]
        public void EndGameEntityException()
        {
            string gameId = "GameIdException";

            Assert.ThrowsException<EntityException>(() =>
            {
                gameAccess.EndGame(WINNER_TEAM_TEST, gameId);
            });
        }

        [TestMethod]
        public void BanUserEntityException()
        {
            string nickname = "NicknameException";

            Assert.ThrowsException<EntityException>(() =>
            {
                gameAccess.BanUser(nickname);
            });
        }

        [TestMethod]
        public void CanPlayEntityException()
        {
            string nickname = "NicknameException";

            Assert.ThrowsException<EntityException>(() =>
            {
                gameAccess.CanPlay(nickname);
            });
        }

        [TestMethod]
        public void CleanUpGameEntityException()
        {
            string gameId = "GameIdException";

            Assert.ThrowsException<EntityException>(() =>
            {
                gameAccess.CleanupGame(gameId);
            });
        }

        [TestMethod]
        public void RemoveBanEntityException()
        {
            string nickname = "NicknameException";

            Assert.ThrowsException<EntityException>(() =>
            {
                gameAccess.RemoveBan(nickname);
            });
        }
    }

    [TestClass]
    public class DeckAccessTests
    {
        private const int INT_VALIDATION_SUCCESS = 1;
        private const int STARTING_CARD_COUNT = 30;
        private static UserAccess _userAccess = new UserAccess();
        private static DeckAccess _deckAccess = new DeckAccess();
        private static AstralisDBEntities _context = new AstralisDBEntities();

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            GetConnectionString();

            User userDefaultDeck = new User()
            {
                Nickname = "UserDefaultDeck",
                ImageId = 1,
                Mail = "UserDefaultDeck@hotmail.com",
                Password = "password"
            };
            _userAccess.CreateUser(userDefaultDeck);

            User userToGetDeck = new User()
            {
                Nickname = "UserToGetDeck",
                ImageId = 1,
                Mail = "testuser@example.com",
                Password = "password"
            };
            _userAccess.CreateUser(userToGetDeck);
            _deckAccess.CreateDefaultDeck(_context, userToGetDeck.Nickname);
        }

        public static void GetConnectionString()
        {
            string connectionString = Environment.GetEnvironmentVariable("ASTRALIS");
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var connectionStringSection = config.ConnectionStrings.ConnectionStrings["AstralisDBEntities"];

            if (connectionStringSection != null)
            {
                connectionStringSection.ConnectionString = connectionString;

                config.Save(ConfigurationSaveMode.Modified);

                ConfigurationManager.RefreshSection("connectionStrings");

            }
        }

        [TestMethod]
        public void SuccessfullyCreateDefaultDeck()
        {
            string nickname = "UserDefaultDeck";

            Assert.IsTrue(_deckAccess.CreateDefaultDeck(_context, nickname) == INT_VALIDATION_SUCCESS);
        }

        [TestMethod]
        public void SuccessfullyGetDeckByNickname()
        {
            string nickname = "UserToGetDeck";
            List<int> cardList = _deckAccess.GetDeckByNickname(nickname);

            Assert.IsNotNull(cardList);
            Assert.AreEqual(STARTING_CARD_COUNT, cardList.Count);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _userAccess.DeleteUser("UserDefaultDeck");
            _userAccess.DeleteUser("UserToGetDeck");
        }
    }

    [TestClass]
    public class DeckAccessTestExceptions
    {
        private static DeckAccess deckAccess = new DeckAccess();
        private static AstralisDBEntities _context = new AstralisDBEntities();

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            GetConnectionString();
        }

        public static void GetConnectionString()
        {
            string connectionString = Environment.GetEnvironmentVariable("ASTRALIS");
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var connectionStringSection = config.ConnectionStrings.ConnectionStrings["AstralisDBEntities"];

            if (connectionStringSection != null)
            {
                connectionStringSection.ConnectionString = connectionString;

                config.Save(ConfigurationSaveMode.Modified);

                ConfigurationManager.RefreshSection("connectionStrings");

            }
        }

        [TestMethod]
        public void CreateDefaultDeckEntityException()
        {
            string nickname = "NicknameException";

            Assert.ThrowsException<EntityException>(() =>
            {
                deckAccess.CreateDefaultDeck(_context, nickname);
            });
        }

        [TestMethod]
        public void GetDeckByNicknameEntityException()
        {
            string nickname = "NicknameException";


            Assert.ThrowsException<EntityException>(() =>
            {
                deckAccess.GetDeckByNickname(nickname);
            });
        }


    }

}



